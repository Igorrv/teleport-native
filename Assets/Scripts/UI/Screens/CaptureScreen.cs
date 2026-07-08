using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Capture;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>Captura AR guiada: overlay sobre camera AR, cobertura 360, keyframes, feedback.</summary>
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
        private float _coverage01;
        private int _keyframes;
        private bool _armed;
        private bool _subscribed;

        protected override void Build()
        {
            UiChrome.Header(Root, "Captura AR", "Camera + movimento 360");

            // Anel de cobertura (centro-superior)
            var top = new GameObject("top", typeof(RectTransform));
            var trt = (RectTransform)top.transform;
            trt.SetParent(Root, false);
            trt.anchorMin = new Vector2(0.5f, 0.5f); trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(200, 200);
            trt.anchoredPosition = new Vector2(0, 120);

            var track = new GameObject("track", typeof(Image));
            var krt = (RectTransform)track.transform;
            krt.SetParent(trt, false); Stretch(krt);
            track.GetComponent<Image>().sprite = UIFactory.White();
            track.GetComponent<Image>().color = new Color(0, 0, 0, 0.45f);

            var ringGo = new GameObject("ring", typeof(Image));
            var rrt = (RectTransform)ringGo.transform;
            rrt.SetParent(trt, false); Stretch(rrt); rrt.offsetMin = rrt.offsetMax = new Vector2(12, 12);
            _ring = ringGo.GetComponent<Image>();
            _ring.sprite = UIFactory.White(); _ring.color = T.Primary;
            _ring.type = Image.Type.Filled; _ring.fillMethod = Image.FillMethod.Radial360; _ring.fillAmount = 0;

            _coverage = UIFactory.Text(trt, "0%", 32f, T.Text, TextAnchor.MiddleCenter, fit: false);
            Stretch(_coverage.rectTransform);

            _count = UIFactory.Text(Root, "0 fotos", T.BodySize, T.Accent, TextAnchor.MiddleCenter, fit: false);
            _count.rectTransform.anchorMin = _count.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _count.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _count.rectTransform.anchoredPosition = new Vector2(0, -20);
            _count.rectTransform.sizeDelta = new Vector2(300, 36);

            // Chips de passos
            var steps = new GameObject("steps", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var srt = (RectTransform)steps.transform;
            srt.SetParent(Root, false);
            srt.anchorMin = new Vector2(0, 0.5f); srt.anchorMax = new Vector2(1, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.sizeDelta = new Vector2(-T.SpaceL * 2, 32);
            srt.anchoredPosition = new Vector2(0, -70);
            var hlg = steps.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = T.SpaceS; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true; hlg.childForceExpandWidth = true;
            UiChrome.StepChip(steps.transform, "1. Apontar", true);
            UiChrome.StepChip(steps.transform, "2. Girar 360", false);
            UiChrome.StepChip(steps.transform, "3. Finalizar", false);

            // Painel inferior
            var sheet = UiChrome.BottomSheet(Root, 200);
            sheet.anchorMin = new Vector2(0, 0); sheet.anchorMax = new Vector2(1, 0);
            sheet.pivot = new Vector2(0.5f, 0);
            sheet.sizeDelta = new Vector2(0, 200);
            sheet.anchoredPosition = Vector2.zero;

            var sheetCol = UIFactory.Column(sheet, "col", T.SpaceM, T.SpaceS);
            UIFactory.Stretch(sheetCol);

            _status = UIFactory.Text(sheetCol, "Pronto para capturar", T.CaptionSize, T.Success, TextAnchor.MiddleCenter, fit: false);
            _hint = UIFactory.Text(sheetCol, CaptureFlowLogic.Hint(true, false, 0, 0), T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true, fit: false);

            var btn = UIFactory.Button(sheetCol, "Iniciar captura", OnAction, true);
            btn.sizeDelta = new Vector2(0, T.ButtonHeight);
            _actionLabel = btn.GetComponentInChildren<Text>();
            _actionBtn = btn.GetComponent<Button>();

            UIFactory.Button(Root, "‹  Voltar", () => Cancel(), false)
                .SetInset(new Vector2(T.SpaceM, -T.SpaceM - 44), new Vector2(100, 44));

            // Indicador gravando
            var rec = new GameObject("rec", typeof(RectTransform), typeof(Image));
            var rrt2 = (RectTransform)rec.transform;
            rrt2.SetParent(Root, false);
            rrt2.SetInset(new Vector2(T.SpaceL + 110, -T.SpaceM - 44), new Vector2(12, 12));
            _recDot = rec.GetComponent<Image>();
            _recDot.sprite = UIFactory.White(); _recDot.color = T.Danger;
            rec.SetActive(false);

            _toast = ToastView.Attach(Root);
            BuildNoArPanel();
        }

        private void BuildNoArPanel()
        {
            _noArPanel = new GameObject("noAr", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)_noArPanel.transform;
            rt.SetParent(Root, false);
            UIFactory.Stretch(rt);
            rt.offsetMin = new Vector2(T.SpaceM, 220); rt.offsetMax = new Vector2(-T.SpaceM, -100);
            _noArPanel.GetComponent<Image>().sprite = UIFactory.White();
            _noArPanel.GetComponent<Image>().color = T.SurfaceRaised;

            var col = UIFactory.Column(rt, "col", T.SpaceL, T.SpaceM);
            UIFactory.Stretch(col);
            UIFactory.Text(col, "Camera AR no iPhone", T.HeadingSize, T.Text, TextAnchor.MiddleCenter);
            UIFactory.Text(col,
                "No Windows nao ha camera AR. Para capturar ambientes reais:\n\n" +
                "1. Gere o .ipa no Codemagic (veja IPHONE.md)\n" +
                "2. Instale com Sideloadly + cabo USB\n" +
                "3. Confie o perfil em Ajustes do iPhone",
                T.BodySize, T.TextMuted, TextAnchor.MiddleCenter, wrap: true);
            UIFactory.Button(col, "Abrir guia IPHONE.md", () =>
                Application.OpenURL("file:///" + Application.dataPath + "/../IPHONE.md"), false);
            UIFactory.Button(col, "Voltar a biblioteca", () => Ctx.Flow.Request(ScreenId.Library), true);
        }

        public override void OnShow()
        {
            _armed = false;
            _coverage01 = 0f;
            _keyframes = 0;
            RefreshUi();
            var hasAr = Ctx.Capture != null;
            _noArPanel.SetActive(!hasAr);
            if (_actionBtn != null) _actionBtn.gameObject.SetActive(hasAr);
            if (!hasAr) return;

            if (_actionBtn != null) _actionBtn.interactable = true;
            if (!_subscribed)
            {
                Ctx.Capture.KeyframeCaptured += OnKeyframe;
                Ctx.Capture.CoverageUpdated += OnCoverage;
                _subscribed = true;
            }
#if UNITY_IOS
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif
        }

        public override void OnHide()
        {
            if (_armed && Ctx.Capture != null) Ctx.Capture.StopCapture();
            _armed = false;
            if (_recDot != null) _recDot.transform.parent.gameObject.SetActive(false);
        }

        private void OnAction()
        {
            if (Ctx.Capture == null) return;
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

                var block = CaptureFlowLogic.FinishBlockReason(frames.Count, _coverage01);
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

        private void RefreshUi()
        {
            _coverage.text = Mathf.RoundToInt(_coverage01 * 100f) + "%";
            _count.text = $"{_keyframes} fotos · meta {CaptureFlowLogic.MinKeyframes}+";
            _hint.text = CaptureFlowLogic.Hint(Ctx.Capture != null, _armed, _keyframes, _coverage01);
            _actionLabel.text = _armed ? "Finalizar captura" : "Iniciar captura";
            _actionLabel.color = _armed ? Color.black : Color.white;

            if (! _armed)
                _status.text = "Pronto para capturar";
            else if (CaptureFlowLogic.CanFinish(_keyframes, _coverage01))
                _status.text = "Pode finalizar";
            else
                _status.text = "Capturando...";
            _status.color = CaptureFlowLogic.CanFinish(_keyframes, _coverage01) ? T.Success : T.Warning;
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
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = min; rt.sizeDelta = size;
            return rt;
        }
    }
}
