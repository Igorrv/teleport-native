using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace TeleportNative.Capture
{
    /// <summary>
    /// Sessao de captura AR (ARKit/ARCore via AR Foundation). Implementa ICaptureSession.
    /// Por frame: pega a pose (camera->world) + intrinsics; se a pose for nova, adquire
    /// XRCpuImage, mede nitidez (Laplaciano em downscale) e, se aceita, gera JPEG + keyframe.
    /// Atualiza cobertura 360 para guiar o usuario. Imagem/encode no main (M2: mover p/ thread).
    /// </summary>
    [RequireComponent(typeof(ARCameraManager))]
    public sealed class ARCaptureSession : MonoBehaviour, ICaptureSession
    {
        [SerializeField] private Camera _arCamera;
        [SerializeField] private int _sharpnessTargetW = 96;
        [SerializeField] private int _jpegMaxWidth = 1920;
        [SerializeField] private int _jpegQuality = 82;

        private ARCameraManager _cameraManager;
        private IFrameSelector _selector = new FrameSelector();
        private readonly CoverageTracker _coverage = new(12);
        private readonly List<CapturedFrame> _frames = new();
        private Texture2D _sharpTex;
        private Texture2D _fullTex;
        private bool _capturing;

        public bool IsCapturing => _capturing;
        public int KeyframeCount => _frames.Count;
        public IReadOnlyList<CapturedFrame> Keyframes => _frames;
        public CoverageState Coverage => _coverage.Current;

        public event Action<CapturedFrame> KeyframeCaptured;
        public event Action<CoverageState> CoverageUpdated;

        private void Awake()
        {
            _cameraManager = GetComponent<ARCameraManager>();
            if (_arCamera == null)
                _arCamera = GetComponent<Camera>();
            _selector.Reset();
        }

        private void OnEnable()
        {
            if (_cameraManager != null) _cameraManager.frameReceived += OnFrameReceived;
        }

        private void OnDisable()
        {
            if (_cameraManager != null) _cameraManager.frameReceived -= OnFrameReceived;
        }

        public void StartCapture()
        {
            _frames.Clear();
            _selector.Reset();
            _coverage.Reset();
            _capturing = true;
        }

        public IReadOnlyList<CapturedFrame> StopCapture()
        {
            _capturing = false;
            return _frames;
        }

        private void OnFrameReceived(ARCameraFrameEventArgs args)
        {
            if (!_capturing) return;
            if (!_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage cpuImage)) return;
            try { ProcessFrame(cpuImage, args); }
            catch (Exception e) { Debug.LogWarning($"[ARCapture] frame descartado: {e.Message}"); }
            finally { cpuImage.Dispose(); }
        }

        private void ProcessFrame(XRCpuImage image, ARCameraFrameEventArgs args)
        {
            Matrix4x4 pose = _arCamera != null
                ? _arCamera.transform.localToWorldMatrix
                : Matrix4x4.identity;

            // Checape barato antes de qualquer trabalho de imagem.
            if (!_selector.IsPoseNovel(pose)) return;

            Vector4 intr = new(1, 1, 0.5f, 0.5f);
            if (_cameraManager.TryGetIntrinsics(out XRCameraIntrinsics k))
                intr = new Vector4(k.focalLength.x, k.focalLength.y, k.principalPoint.x, k.principalPoint.y);

            float sharpness = ComputeSharpness(image);
            if (!_selector.ShouldKeep(pose, sharpness)) return; // blur ou (raro) redundancia

            byte[] jpeg = EncodeJpeg(image);
            double ts = args.timestampNs.HasValue ? args.timestampNs.Value * 1e-9 : Time.timeAsDouble;

            var frame = new CapturedFrame
            {
                Jpeg = jpeg,
                Pose = pose,
                Intrinsics = intr,
                Timestamp = ts
            };
            _frames.Add(frame);

            var coverage = _coverage.Register(_arCamera != null ? _arCamera.transform.forward : pose.MultiplyVector(Vector3.forward));
            KeyframeCaptured?.Invoke(frame);
            CoverageUpdated?.Invoke(coverage);
        }

        private float ComputeSharpness(XRCpuImage image)
        {
            int w = _sharpnessTargetW;
            int h = Mathf.RoundToInt(w * (float)image.height / image.width);
            var p = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(w, h),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };
            byte[] data = ConvertToBytes(image, p);
            if (_sharpTex == null || _sharpTex.width != w || _sharpTex.height != h)
                _sharpTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            _sharpTex.LoadRawTextureData(data);
            _sharpTex.Apply();
            return BlurDetector.SharpnessFromTexture(_sharpTex, w);
        }

        private byte[] EncodeJpeg(XRCpuImage image)
        {
            float scale = Mathf.Min(1f, _jpegMaxWidth / (float)image.width);
            int w = Mathf.Max(2, Mathf.RoundToInt(image.width * scale));
            int h = Mathf.Max(2, Mathf.RoundToInt(image.height * scale));
            var p = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(w, h),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };
            byte[] data = ConvertToBytes(image, p);
            if (_fullTex == null || _fullTex.width != w || _fullTex.height != h)
                _fullTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            _fullTex.LoadRawTextureData(data);
            _fullTex.Apply();
            return _fullTex.EncodeToJPG(_jpegQuality);
        }

        private static byte[] ConvertToBytes(XRCpuImage image, XRCpuImage.ConversionParams p)
        {
            int size = image.GetConvertedDataSize(p);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            try
            {
                image.Convert(p, buffer);
                return buffer.ToArray();
            }
            finally
            {
                buffer.Dispose();
            }
        }
    }
}
