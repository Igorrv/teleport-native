using System.Collections;
using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Capture;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Captura AR: overlay transparente, camera visivel, permissoes iOS, botoes grandes.</summary>
    public sealed class CaptureScreen : AppScreen
    {
        private Text _count;
        private Text _coverage;
        private Text _hint;
        private Text _status;
        private Image _ring;
        private Image _recDot;
        private Text _actionLabel;
        private Button _actionBtn;
        private ToastView _toast;
        private GameObject _noArPanel;
        private GameObject _permPanel;
        private float _coverage01;
        private int _keyframes;
        private bool _armed;
        private bool _subscribed;
        private bool _cameraOk;

        protected override void Build()
        {
            // Fundo transparente — camera AR visivel no centro
            var bg = Root.gameObject.AddComponent<Image>();
            bg.color = Color.clear;
            bg.raycastTarget = false;

            UiChrome.Header(Root, "Captura AR", "Aponte e gire 360°");

            // Anel de cobertura
            var top = new GameObject("top", typeof(RectTransform));
            var trt = (RectTransform)top.transform;
            trt.SetParent(Root, false);
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(240, 240);
            trt.anchoredPosition = new Vector2(0, 80);

            var track = new GameObject("track", typeof(Image));
            var krt = (RectTransform)track.transform;
            krt.SetParent(trt, false);
            Stretch(krt);
            var trackImg = track.GetComponent<Image>();
            trackImg.sprite = UIFactory.White();
            trackImg.color = new Color(0, 0, 0, 0.35f);
            trackImg.raycastTarget = false;

            var ringGo = new GameObject("ring", typeof(Image));
            var rrt = (RectTransform)ringGo.transform;
            rrt.SetParent(trt, false);
            Stretch(rrt);
            rrt.offsetMin = rrt.offsetMax = new Vector2(14, 14);
            _ring = ringGo.GetComponent<Image>();
            _ring.sprite = UIFactory.White();
            _ring.color = T.Primary;
            _ring.type = Image.Type.Filled;
            _ring.fillMethod = Image.FillMethod.Radial360;
            _ring.fillAmount = 0;
            _ring.raycastTarget = false;

            _coverage = UIFactory.Text(trt, "0%", 36f, T.Text, TextAnchor.MiddleCenter, fit: false);
            Stretch(_coverage.rectTransform);
            _coverage.raycastTarget = false;

            _count = UIFactory.Text(Root, "0 fotos", T.BodySize, T.Accent, TextAnchor.MiddleCenter, fit: false);
            _count.rectTransform.anchorMin = _count.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _count.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _count.rectTransform.anchoredPosition = new Vector2(0, -30);
            _count.rectTransform.sizeDelta = new Vector2(340, 40);
            _count.raycastTarget = false;

            // Dock inferior (nao cobre tela inteira)
            var sheet = UiChrome.BottomSheet(Root, MobileUi.IsPhone ? 260f : 200f);

            var sheetCol = UIFactory.Column(sheet, "col", T.SpaceM, T.SpaceM);
            UIFactory.Stretch(sheetCol);

            _status = UIFactory.Text(sheetCol, "Pronto para capturar", T.CaptionSize, T.Success, TextAnchor.MiddleCenter, fit: false);
            _hint = UIFactory.Text(sheetCol, CaptureFlowLogic.Hint(true, false, 0, 0, null), T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true, fit: false);

            var btn = UIFactory.Button(sheetCol, "Iniciar captura", OnAction, true);
            btn.sizeDelta = new Vector2(0, T.ButtonHeight);
            _actionLabel = btn.GetComponentInChildren<Text>();
            _actionBtn = btn.GetComponent<Button>();

            var backBtn = UIFactory.Button(Root, "‹ Voltar", () => Cancel(), false);
            backBtn.SetInset(new Vector2(T.SpaceM, -T.SpaceM - T.TouchTarget), new Vector2(120, T.TouchTarget));

            var rec = new GameObject("rec", typeof(RectTransform), typeof(Image));
            var rrt2 = (RectTransform)rec.transform;
            rrt2.SetParent(Root, false);
            rrt2.SetInset(new Vector2(T.SpaceL + 130, -T.SpaceM - T.TouchTarget), new Vector2(14, 14));
            _recDot = rec.GetComponent<Image>();
            _recDot.sprite = UIFactory.White();
            _recDot.color = T.Danger;
            _recDot.raycastTarget = false;
            rec.SetActive(false);

            _toast = ToastView.Attach(Root);
            BuildPermissionPanel();
            BuildNoArPanel();
        }

        private void BuildPermissionPanel()
        {
            _permPanel = new GameObject("perm", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)_permPanel.transform;
            rt.SetParent(Root, false);
            UIFactory.Stretch(rt);
            var img = _permPanel.GetComponent<Image>();
            img.sprite = UIFactory.White();
            img.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);

            var col = UIFactory.Column(rt, "col", T.SpaceXL, T.SpaceL);
            UIFactory.Stretch(col);
            var vlg = col.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleCenter;

            UIFactory.Text(col, "Acesso à câmera", T.TitleSize, T.Text, TextAnchor.MiddleCenter);
            UIFactory.Text(col,
                "O Teleport precisa da câmera para capturar o ambiente em AR.\n\nToque abaixo — o iOS vai pedir permissão.",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            UIFactory.Button(col, "Permitir câmera", () => StartCoroutine(RequestPermissionFlow()), true)
                .sizeDelta = new Vector2(0, T.ButtonHeight);
        }

        private void BuildNoArPanel()
        {
            _noArPanel = new GameObject("noAr", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)_noArPanel.transform;
            rt.SetParent(Root, false);
            UIFactory.Stretch(rt);
            _noArPanel.GetComponent<Image>().sprite = UIFactory.White();
            _noArPanel.GetComponent<Image>().color = T.SurfaceRaised;

            var col = UIFactory.Column(rt, "col", T.SpaceL, T.SpaceM);
            UIFactory.Stretch(col);
            UIFactory.Text(col, "AR indisponível", T.HeadingSize, T.Text, TextAnchor.MiddleCenter);
            UIFactory.Text(col,
                "O módulo de captura AR não foi encontrado neste build.\nReinstale o IPA gerado pelo workflow iOS Test (Sideloadly).",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            UIFactory.Button(col, "Voltar à biblioteca", () => Ctx.Flow.Request(ScreenId.Library), true)
                .sizeDelta = new Vector2(0, T.ButtonHeight);
        }

        public override void OnShow()
        {
            _armed = false;
            _coverage01 = 0f;
            _keyframes = 0;
            ApplyRoomHeader();
            RefreshUi();

            var hasAr = Ctx.Capture != null;
            _noArPanel.SetActive(!hasAr);
            if (!hasAr) return;

            StartCoroutine(RequestPermissionFlow());
        }

        private IEnumerator RequestPermissionFlow()
        {
            yield return CameraPermissionHelper.EnsureCamera(ok => _cameraOk = ok);

            _permPanel.SetActive(!_cameraOk);
            if (_actionBtn != null)
                _actionBtn.gameObject.SetActive(_cameraOk);

            if (!_cameraOk)
            {
                _status.text = "Permissão de câmera necessária";
                _status.color = T.Warning;
                _hint.text = "Ajustes → Privacidade → Câmera → ative Teleport Native.";
                yield break;
            }

            if (Ctx.ArRig != null)
            {
                var starter = Ctx.ArRig.GetComponent<ArSessionStarter>();
                if (starter == null)
                    starter = Ctx.ArRig.AddComponent<ArSessionStarter>();
                starter.EnsureRunning();
            }

            if (!_subscribed)
            {
                Ctx.Capture.KeyframeCaptured += OnKeyframe;
                Ctx.Capture.CoverageUpdated += OnCoverage;
                _subscribed = true;
            }

            RefreshUi();
        }

        public override void OnHide()
        {
            if (_armed && Ctx.Capture != null) Ctx.Capture.StopCapture();
            _armed = false;
            if (_recDot != null) _recDot.transform.parent.gameObject.SetActive(false);
        }

        private void OnAction()
        {
            if (Ctx.Capture == null || !_cameraOk) return;
            Ctx.Haptics.Trigger(HapticType.ImpactLight);
            if (!_armed)
            {
                Ctx.Capture.StartCapture();
                _armed = true;
                _recDot.transform.parent.gameObject.SetActive(true);
                RefreshUi();
            }
            else
            {
                var frames = Ctx.Capture.StopCapture();
                _armed = false;
                _recDot.transform.parent.gameObject.SetActive(false);
                _keyframes = frames.Count;

                var block = CaptureFlowLogic.FinishBlockReason(frames.Count, _coverage01, ActiveRoom());
                if (block != null)
                {
                    _toast.Show(block, T.Warning, 4f);
                    RefreshUi();
                    return;
                }
                Ctx.LastCapture = frames;
                Ctx.Flow.Request(ScreenId.Processing);
            }
        }

        private void Cancel()
        {
            if (_armed) Ctx.Capture?.StopCapture();
            _armed = false;
            Ctx.Flow.Request(ScreenId.Library);
        }

        private void OnKeyframe(CapturedFrame f)
        {
            _keyframes = Ctx.Capture.KeyframeCount;
            RefreshUi();
        }

        private void OnCoverage(CoverageState c)
        {
            _coverage01 = c.Coverage01;
            _ring.fillAmount = c.Coverage01;
            _ring.color = c.IsComplete ? T.Success : (c.Coverage01 >= CaptureFlowLogic.MinCoverage ? T.Accent : T.Primary);
            if (c.IsComplete) Ctx.Haptics.Trigger(HapticType.Success);
            RefreshUi();
        }

        private void ApplyRoomHeader()
        {
            var header = Root.Find("header");
            if (header == null) return;
            var texts = header.GetComponentsInChildren<Text>(true);
            if (texts.Length < 2) return;

            var d = Ctx.RealtyDraft;
            if (d != null && d.HasNext)
            {
                var info = RoomCatalog.Of(d.NextType);
                texts[0].text = info.Label;
                texts[1].text = $"{d.Done + 1}/{d.Total} · {d.Title} — gire 360°";
            }
            else
            {
                texts[0].text = "Captura AR";
                texts[1].text = "Aponte e gire 360°";
            }
        }

        private RoomType? ActiveRoom()
        {
            var d = Ctx.RealtyDraft;
            return d != null && d.HasNext ? d.NextType : (RoomType?)null;
        }

        private void RefreshUi()
        {
            var room = ActiveRoom();
            int minPhotos = CaptureFlowLogic.EffectiveMinKeyframes(room);
            if (_coverage != null)
                _coverage.text = Mathf.RoundToInt(_coverage01 * 100f) + "%";
            if (_count != null)
                _count.text = $"{_keyframes} fotos · meta {minPhotos}+";
            if (_hint != null && _cameraOk)
                _hint.text = CaptureFlowLogic.Hint(Ctx.Capture != null, _armed, _keyframes, _coverage01, room);
            if (_actionLabel != null)
            {
                _actionLabel.text = _armed ? "Finalizar captura" : "Iniciar captura";
                _actionLabel.color = _armed ? Color.black : Color.white;
            }

            if (!_cameraOk) return;

            if (!_armed)
                _status.text = "Pronto para capturar";
            else if (CaptureFlowLogic.CanFinish(_keyframes, _coverage01, room))
                _status.text = "Pode finalizar";
            else
                _status.text = "Capturando...";
            _status.color = CaptureFlowLogic.CanFinish(_keyframes, _coverage01, room) ? T.Success : T.Warning;
        }

        private void OnDestroy()
        {
            if (Ctx != null && _subscribed && Ctx.Capture != null)
            {
                Ctx.Capture.KeyframeCaptured -= OnKeyframe;
                Ctx.Capture.CoverageUpdated -= OnCoverage;
            }
        }
    }

    internal static class RtExtensions
    {
        public static RectTransform SetInset(this RectTransform rt, Vector2 min, Vector2 size)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = min;
            rt.sizeDelta = size;
            return rt;
        }
    }
}
