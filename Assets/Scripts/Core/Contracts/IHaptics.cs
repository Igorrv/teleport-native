namespace TeleportNative.Core
{
    public enum HapticType { Selection, ImpactLight, ImpactMedium, Success, Warning, Error }

    /// <summary>Feedback haptico cruzado (iOS/Android) com fallback no-op.</summary>
    public interface IHaptics
    {
        void Trigger(HapticType type);
    }
}
