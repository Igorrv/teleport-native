using System;
using System.Collections.Generic;
using UnityEngine;

namespace TeleportNative.Capture
{
    /// <summary>Um keyframe capturado, pronto p/ upload (JPEG + pose + intrinsics).</summary>
    public struct CapturedFrame
    {
        public byte[] Jpeg;          // imagem comprimida
        public Matrix4x4 Pose;       // camera -> world (extrinsics)
        public Vector4 Intrinsics;   // (fx, fy, cx, cy) em pixels
        public double Timestamp;
    }

    /// <summary>Abstracao da sessao de captura AR (ARKit/ARCore). Trocavel p/ testes/fake.</summary>
    public interface ICaptureSession
    {
        bool IsCapturing { get; }
        int KeyframeCount { get; }
        IReadOnlyList<CapturedFrame> Keyframes { get; }
        CoverageState Coverage { get; }

        event Action<CapturedFrame> KeyframeCaptured;
        event Action<CoverageState> CoverageUpdated;

        void StartCapture();
        IReadOnlyList<CapturedFrame> StopCapture();
    }
}
