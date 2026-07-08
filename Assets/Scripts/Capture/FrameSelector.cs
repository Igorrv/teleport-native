using UnityEngine;

namespace TeleportNative.Capture
{
    /// <summary>Implementacao padrao de IFrameSelector (logica pura, testavel).</summary>
    public sealed class FrameSelector : IFrameSelector
    {
        private readonly float _minTranslationMeters;
        private readonly float _minRotationDegrees;
        private readonly float _blurThreshold;

        private Matrix4x4 _lastPose;
        private bool _hasLast;

        public FrameSelector(float minTranslationMeters = 0.10f,
                             float minRotationDegrees = 8f,
                             float blurThreshold = 100f)
        {
            _minTranslationMeters = minTranslationMeters;
            _minRotationDegrees = minRotationDegrees;
            _blurThreshold = blurThreshold;
        }

        public void Reset() { _hasLast = false; _lastPose = Matrix4x4.identity; }

        public bool IsPoseNovel(Matrix4x4 pose)
        {
            if (!_hasLast) return true;
            float dist = Vector3.Distance(pose.GetColumn(3), _lastPose.GetColumn(3));
            float ang = Quaternion.Angle(pose.rotation, _lastPose.rotation);
            return dist >= _minTranslationMeters || ang >= _minRotationDegrees;
        }

        public bool ShouldKeep(Matrix4x4 pose, float sharpness)
        {
            if (sharpness < _blurThreshold) return false; // borrado

            if (_hasLast)
            {
                float dist = Vector3.Distance(pose.GetColumn(3), _lastPose.GetColumn(3));
                float ang = Quaternion.Angle(pose.rotation, _lastPose.rotation);
                if (dist < _minTranslationMeters && ang < _minRotationDegrees)
                    return false; // pose redundante
            }

            _lastPose = pose;
            _hasLast = true;
            return true;
        }
    }
}
