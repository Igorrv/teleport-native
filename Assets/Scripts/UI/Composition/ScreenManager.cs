using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeleportNative.Core;
using ScreenId = TeleportNative.Core.AppScreen;

namespace TeleportNative.UI
{
    /// <summary>
    /// Constroi as 6 telas em runtime (filhas do Canvas), mostra/oculta conforme AppFlow e
    /// alterna os "rigs" de mundo (camera AR x viewer) por estado.
    /// </summary>
    public sealed class ScreenManager
    {
        private readonly AppContext _ctx;
        private readonly Canvas _root;
        private readonly Dictionary<ScreenId, AppScreen> _map = new();

        public ScreenManager(AppContext ctx, Canvas root)
        {
            _ctx = ctx;
            _root = root;

            var safeGo = new GameObject("SafeArea", typeof(RectTransform));
            var safeRt = (RectTransform)safeGo.transform;
            safeRt.SetParent(_root.transform, false);
            UIFactory.Stretch(safeRt);
            safeGo.AddComponent<SafeAreaFitter>();

            Add<OnboardingScreen>(ScreenId.Onboarding, safeRt);
            Add<LibraryScreen>(ScreenId.Library, safeRt);
            Add<CaptureScreen>(ScreenId.Capture, safeRt);
            Add<ProcessingScreen>(ScreenId.Processing, safeRt);
            Add<ViewerScreen>(ScreenId.Viewer, safeRt);
            Add<ShareScreen>(ScreenId.Share, safeRt);

            foreach (var kv in _map) kv.Value.Hide();

            var start = PlayerPrefs.GetInt("tn_onboarded", 0) == 1 ? ScreenId.Library : ScreenId.Onboarding;
            _ctx.Flow.ResetTo(start);
            _ctx.Flow.Transitioned += OnTransition;

            _map[start].Show();
            ToggleRigs(start);
        }

        private T Add<T>(ScreenId key, RectTransform parent) where T : AppScreen
        {
            var go = new GameObject(key.ToString(), typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            UIFactory.Stretch(rt);
            var comp = go.AddComponent<T>();
            comp.Init(_ctx);
            _map[key] = comp;
            return comp;
        }

        private void OnTransition(ScreenId from, ScreenId to)
        {
            if (_map.TryGetValue(from, out var f)) f.Hide();
            _map[to].Show();
            ToggleRigs(to);
            _ctx.Haptics.Trigger(HapticType.Selection);
        }

        private void ToggleRigs(ScreenId to)
        {
            if (_ctx.ArRig != null) _ctx.ArRig.SetActive(to == ScreenId.Capture);
            if (_ctx.ViewerRig != null) _ctx.ViewerRig.SetActive(to == ScreenId.Viewer);
        }
    }
}
