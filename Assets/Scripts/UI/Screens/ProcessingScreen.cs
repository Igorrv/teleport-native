using System;
using System.Threading;
using T = TeleportNative.Core.DesignTokens;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;
using TeleportNative.Network;

namespace TeleportNative.UI
{
    /// <summary>
    /// Tela de processamento: dispara ReconstructionClient.RunAsync, mostra progresso
    /// (ENVIADO -> PROCESSANDO -> GERANDO_3D -> FINALIZADO) com retry/cancel. Vai ao Viewer ao pronto.
    /// </summary>
    public sealed class ProcessingScreen : AppScreen
    {
        private Text _status;
        private ProgressBarView _bar;
        private Text _pct;
        private Text _error;
        private RectTransform _actions;
        private CancellationTokenSource _cts;

        protected override void Build()
        {
            UIFactory.Panel(Root, "bg", T.Background);
            var c = UIFactory.ScreenLayout(Root, "col", TextAnchor.MiddleCenter, T.SpaceM, T.SpaceXL);

            UIFactory.Text(c, "Reconstruindo seu espaco", T.HeadingSize, T.Text, TextAnchor.MiddleCenter);
            UIFactory.Spacer(c, T.SpaceS);
            _status = UIFactory.Text(c, "Enviando fotos...", T.BodySize, T.TextMuted, TextAnchor.MiddleCenter);
            UIFactory.Spacer(c, T.SpaceM);
            _bar = ProgressBarView.Create(c);
            UIFactory.Spacer(c, T.SpaceS);
            _pct = UIFactory.Text(c, "0%", T.CaptionSize, T.TextMuted, TextAnchor.MiddleCenter);
            UIFactory.Spacer(c, T.SpaceL);
            UIFactory.Text(c, "Isso pode levar alguns minutos. Voce pode minimizar o app.",
                T.CaptionSize, T.TextMuted, TextAnchor.MiddleCenter, true);

            _error = UIFactory.Text(c, "", T.BodySize, T.Danger, TextAnchor.MiddleCenter, true);
            _actions = UIFactory.Column(c, "actions", 0, T.SpaceS);
            UIFactory.Button(_actions, "Tentar novamente", () => Ctx.Flow.Request(ScreenId.Capture), false);
            UIFactory.Button(_actions, "Voltar a biblioteca", () => Ctx.Flow.Request(ScreenId.Library), true);
            HideError();
        }

        public override async void OnShow()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            HideError();
            _status.text = "Enviando fotos...";
            _bar.Value = 0; _pct.text = "0%";

            var req = new ReconstructionRequest
            {
                Frames = Ctx.LastCapture,
                BackendBaseUrl = Ctx.Config.BackendBaseUrl,
                ProviderKey = Ctx.Config.ReconstructionProvider,
                ApiKey = Ctx.Config.ProviderApiKey
            };
            var progress = new Progress<ReconstructionStatus>(OnStatus);

            try
            {
                var res = await Ctx.Reconstruction.RunAsync(req, Ctx.PendingName, progress, _cts.Token);
                if (res.IsSuccess) { Ctx.CurrentSpace = res.Value; Ctx.Flow.Request(ScreenId.Viewer); }
                else ShowError(res.Error);
            }
            catch (OperationCanceledException) { /* tela trocada */ }
            catch (Exception e) { ShowError(e.Message); }
        }

        public override void OnHide() => _cts?.Cancel();

        private void OnStatus(ReconstructionStatus s)
        {
            _status.text = ReconstructionStatus.Label(s.State);
            _bar.Value = s.Progress;
            _pct.text = Mathf.RoundToInt(s.Progress * 100f) + "%";
        }

        private void ShowError(string msg)
        {
            _error.text = "Nao foi possivel concluir: " + msg;
            _error.gameObject.SetActive(true);
            _actions.gameObject.SetActive(true);
        }

        private void HideError()
        {
            if (_error != null) _error.gameObject.SetActive(false);
            if (_actions != null) _actions.gameObject.SetActive(false);
        }
    }
}
