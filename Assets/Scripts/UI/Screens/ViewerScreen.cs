using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Viewer 3D: ativa o renderer, carrega o splat do espaco atual, joystick p/ caminhar,
    /// HUD de FPS/budget (dev), e saida para Share/Library.</summary>
    public sealed class ViewerScreen : AppScreen
    {
        private Text _hud;
        private Text _banner;
        private JoystickView _joy;

        protected override void Build()
        {
            _banner = UIFactory.Text(Root, "", T.BodySize, T.Text, TextAnchor.MiddleCenter, wrap: true);
            _banner.rectTransform.anchorMin = new Vector2(0.1f, 0.12f);
            _banner.rectTransform.anchorMax = new Vector2(0.9f, 0.12f);
            _banner.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _banner.rectTransform.sizeDelta = new Vector2(0, 120);
            _banner.gameObject.SetActive(false);
            // HUD topo-esquerda (FPS / budget — dev)
            _hud = UIFactory.Text(Root, "", T.CaptionSize, T.Success, TextAnchor.UpperLeft);
            AnchorTopLeft(_hud.rectTransform, new Vector2(T.SpaceM, -T.SpaceM), new Vector2(260, 60));
            _hud.gameObject.SetActive(false);

            // Biblioteca (topo-esquerda, abaixo do HUD)
            var back = UIFactory.Button(Root, "‹  Biblioteca", () => Ctx.Flow.Request(ScreenId.Library), false);
            AnchorTopLeft(back, new Vector2(T.SpaceM, -T.SpaceM - 50), new Vector2(160, 44));

            // Compartilhar (topo-direita)
            var share = UIFactory.Button(Root, "Compartilhar  ›", () => Ctx.Flow.Request(ScreenId.Share), true);
            AnchorTopRight(share, new Vector2(-T.SpaceM, -T.SpaceM - 50), new Vector2(180, 44));

            // Reset (baixo-direita)
            var reset = UIFactory.Button(Root, "⟲", () => Ctx.Camera.ResetView(), false);
            AnchorBottomRight(reset, new Vector2(-T.SpaceM, T.SpaceM), new Vector2(56, 56));

            // Joystick (caminhar) baixo-esquerda
            _joy = JoystickView.Create(Root, new Vector2(0.02f, 0.02f), new Vector2(0.42f, 0.42f));
        }

        public override async void OnShow()
        {
            Ctx.Viewer.SetActive(true);
            _hud.gameObject.SetActive(Ctx.Config.DevHudEnabled);
            Ctx.Viewer.FpsUpdated += OnFps;
            ShowBanner("Carregando espaco 3D...", T.TextMuted);

            if (Ctx.CurrentSpace != null && !string.IsNullOrEmpty(Ctx.CurrentSpace.SplatPath))
            {
                var r = await Ctx.Viewer.LoadPathAsync(Ctx.CurrentSpace.SplatPath);
                if (r.IsSuccess) HideBanner();
                else ShowBanner("Splat indisponivel. Atribua Editor Splat Asset (M1) ou use .ksplat.", T.Danger);
            }
            else
            {
                // M1: splat de editor no SplatViewerController (sem path runtime).
                HideBanner();
            }
        }

        public override void OnHide()
        {
            Ctx.Viewer.FpsUpdated -= OnFps;
            Ctx.Viewer.SetActive(false);
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
