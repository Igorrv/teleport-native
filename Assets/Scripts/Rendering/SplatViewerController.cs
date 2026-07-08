using System;
using TeleportNative.Core;
using TeleportNative.Performance;
using UnityEngine;

namespace TeleportNative.Rendering
{
    /// <summary>
    /// Conductor do viewer (o "motor" de exibicao): aplica budget do tier (LOD/cutout), reage a
    /// thermal/FPS, mede frame pacing p/ HUD de dev, e carrega o splat (editor asset ou runtime).
    /// Delega a renderizacao real ao ISplatRenderer (adapter do UnityGaussianSplatting).
    /// </summary>
    public interface ISplatViewer
    {
        bool IsActive { get; }
        void Bind(IDeviceProfiler profiler, FramePacer pacer, ISplatRenderer renderer);
        void SetActive(bool active);
        Awaitable<Result<bool>> LoadPathAsync(string localPath);
        event Action<int, float> FpsUpdated; // (fps, ms)
    }

    public sealed class SplatViewerController : MonoBehaviour, ISplatViewer
    {
        [SerializeField] private Transform _splatHost;
        [SerializeField] private UnityEngine.Object _editorSplatAsset; // M1: GaussianSplatAsset pre-importado (sem tipo p/ nao acoplar o pacote)
        [SerializeField] private float _idleSecondsToThrottle = 3f;
        [SerializeField] private float _idleBudgetFraction = 0.6f;

        private IDeviceProfiler _profiler;
        private FramePacer _pacer;
        private ISplatRenderer _renderer;
        private float _lastMoveTime;
        private Vector3 _lastCamPos;
        private Quaternion _lastCamRot;

        public bool IsActive { get; private set; }
        public event Action<int, float> FpsUpdated;

        private Camera _camera;

        private void Awake() => _camera = GetComponentInChildren<Camera>(true);

        public void Bind(IDeviceProfiler profiler, FramePacer pacer, ISplatRenderer renderer)
        {
            _profiler = profiler;
            _pacer = pacer;
            _renderer = renderer ?? new UnityGaussianSplatAdapter(_splatHost);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            if (_camera != null) _camera.gameObject.SetActive(active);
            _renderer?.SetPaused(!active);
            if (active)
            {
                _renderer?.ApplyEditorAsset(_editorSplatAsset);
                _lastMoveTime = Time.time;
            }
        }

        public async Awaitable<Result<bool>> LoadPathAsync(string localPath)
        {
            if (_renderer == null) return Result<bool>.Fail("renderer nao vinculado");
            return await _renderer.LoadFromPathAsync(localPath);
        }

        private void Update()
        {
            if (!IsActive || _profiler == null) return;

            float frameMs = Time.unscaledDeltaTime * 1000f;
            _profiler.Tick(Time.unscaledDeltaTime, frameMs);
            if (_pacer != null)
            {
                _pacer.Tick(Time.unscaledDeltaTime, out int fps, out float ms);
                FpsUpdated?.Invoke(fps, ms);
            }

            // Idle: se a camera parou, reduz GPU/budget (bateria/calor); ao mover, restaura.
            bool moved = transform.position != _lastCamPos || transform.rotation != _lastCamRot;
            if (moved) { _lastMoveTime = Time.time; _lastCamPos = transform.position; _lastCamRot = transform.rotation; }

            float fraction = _profiler.IsThrottled ? _profiler.ThermalFactor
                          : (Time.time - _lastMoveTime > _idleSecondsToThrottle ? _idleBudgetFraction : 1f);
            _renderer?.SetBudgetFraction(fraction);
        }
    }
}
