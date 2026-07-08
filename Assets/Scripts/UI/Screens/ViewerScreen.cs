using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Viewer 3D: renderer, joystick, HUD, CTA próximo cômodo e título do espaço.</summary>
    public sealed class ViewerScreen : AppScreen
    {
        private Text _hud;
        private Text _banner;
        private Text _titleChip;
        private JoystickView _joy;
        private RectTransform _nextRoomBtn;

        protected override void Build()
        {
            _banner = UIFactory.Text(Root, "", T.BodySize, T.Text, TextAnchor.MiddleCenter, wrap: true);
            _banner.rectTransform.anchorMin = new Vector2(0.1f, 0.12f);
            _banner.rectTransform.anchorMax = new Vector2(0.9f, 0.12f);
            _banner.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _banner.rectTransform.sizeDelta = new Vector2(0, 120);
            _banner.gameObject.SetActive(false);

            _hud = UIFactory.Text(Root, "", T.CaptionSize, T.Success, TextAnchor.UpperLeft);
            AnchorTopLeft(_hud.rectTransform, new Vector2(T.SpaceM, -T.SpaceM), new Vector2(260, 60));
            _hud.gameObject.SetActive(false);

            var back = UIFactory.Button(Root, "‹  Biblioteca", () => Ctx.Flow.Request(ScreenId.Library), false);
            AnchorTopLeft(back, new Vector2(T.SpaceM, -T.SpaceM - 50), new Vector2(160, 44));

            var share = UIFactory.Button(Root, "Compartilhar  ›", () => Ctx.Flow.Request(ScreenId.Share), true);
            AnchorTopRight(share, new Vector2(-T.SpaceM, -T.SpaceM - 50), new Vector2(180, 44));

            _titleChip = UIFactory.Text(Root, "", T.CaptionSize, T.Text, TextAnchor.MiddleCenter, fit: false);
            _titleChip.rectTransform.anchorMin = new Vector2(0.15f, 1f);
            _titleChip.rectTransform.anchorMax = new Vector2(0.85f, 1f);
            _titleChip.rectTransform.pivot = new Vector2(0.5f, 1f);
            _titleChip.rectTransform.anchoredPosition = new Vector2(0, -T.SpaceM - 100f);
            _titleChip.rectTransform.sizeDelta = new Vector2(0, 28);

            var reset = UIFactory.Button(Root, "⟲", () => Ctx.Camera.ResetView(), false);
            AnchorBottomRight(reset, new Vector2(-T.SpaceM, T.SpaceM), new Vector2(56, 56));

            _nextRoomBtn = UIFactory.Button(Root, "Próximo cômodo ›", OnNextRoom, true);
            _nextRoomBtn.anchorMin = new Vector2(0.12f, 0f);
            _nextRoomBtn.anchorMax = new Vector2(0.88f, 0f);
            _nextRoomBtn.pivot = new Vector2(0.5f, 0f);
            _nextRoomBtn.anchoredPosition = new Vector2(0, T.SpaceM + 72f);
            _nextRoomBtn.sizeDelta = new Vector2(0, T.ButtonHeight);
            _nextRoomBtn.gameObject.SetActive(false);

            _joy = JoystickView.Create(Root, new Vector2(0.02f, 0.02f), new Vector2(0.42f, 0.42f));
        }

        public override async void OnShow()
        {
            Ctx.Viewer.SetActive(true);
            _hud.gameObject.SetActive(Ctx.Config.DevHudEnabled);
            Ctx.Viewer.FpsUpdated += OnFps;
            RefreshTitle();
            ShowBanner("Carregando espaço 3D...", T.TextMuted);

            if (Ctx.CurrentSpace != null && !string.IsNullOrEmpty(Ctx.CurrentSpace.SplatPath))
            {
                var r = await Ctx.Viewer.LoadPathAsync(Ctx.CurrentSpace.SplatPath);
                if (r.IsSuccess) HideBanner();
                else ShowBanner("Splat indisponível. Atribua Editor Splat Asset (M1) ou use .ksplat.", T.Danger);
            }
            else
            {
                HideBanner();
            }

            RefreshNextRoomCta();
        }

        public override void OnHide()
        {
            Ctx.Viewer.FpsUpdated -= OnFps;
            Ctx.Viewer.SetActive(false);
            if (_nextRoomBtn != null) _nextRoomBtn.gameObject.SetActive(false);
        }

        private void RefreshTitle()
        {
            if (_titleChip == null) return;
            var name = Ctx.CurrentSpace?.Name;
            if (string.IsNullOrEmpty(name))
            {
                _titleChip.gameObject.SetActive(false);
                return;
            }
            _titleChip.text = name;
            _titleChip.gameObject.SetActive(true);
        }

        private void RefreshNextRoomCta()
        {
            if (_nextRoomBtn == null) return;
            var d = Ctx.RealtyDraft;
            var hasNext = d != null && d.HasNext;
            _nextRoomBtn.gameObject.SetActive(hasNext);
            if (!hasNext) return;
            var label = _nextRoomBtn.GetComponentInChildren<Text>();
            if (label != null)
                label.text = $"Capturar {RoomCatalog.Of(d.NextType).Label}  ·  {d.Done}/{d.Total}";
        }

        private void OnNextRoom()
        {
            var d = Ctx.RealtyDraft;
            if (d == null || !d.HasNext) return;
            Ctx.PendingName = d.NextLabel();
            Ctx.Haptics.Trigger(HapticType.ImpactLight);
            Ctx.Flow.Request(ScreenId.Capture);
        }

        private void OnFps(int fps, float ms)
        {
            float m = Ctx.Profiler.SplatBudget / 1_000_000f;
            _hud.text = $"{fps} fps\n{m:F1}M splats";
        }

        private void ShowBanner(string msg, Color color)
        {
            _banner.text = msg;
            _banner.color = color;
            _banner.gameObject.SetActive(true);
        }

        private void HideBanner() => _banner.gameObject.SetActive(false);

        private void Update()
        {
            if (_joy != null && _joy.Value.sqrMagnitude > 0.01f)
                Ctx.Camera.Walk(_joy.Value);
        }

        private static void AnchorTopLeft(RectTransform rt, Vector2 pos, Vector2 size)
        { rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = pos; rt.sizeDelta = size; }

        private static void AnchorTopRight(RectTransform rt, Vector2 pos, Vector2 size)
        { rt.anchorMin = rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(1, 1); rt.anchoredPosition = pos; rt.sizeDelta = size; }

        private static void AnchorBottomRight(RectTransform rt, Vector2 pos, Vector2 size)
        { rt.anchorMin = rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(1, 0); rt.anchoredPosition = pos; rt.sizeDelta = size; }
    }
}
