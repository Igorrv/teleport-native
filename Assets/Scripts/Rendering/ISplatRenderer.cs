using TeleportNative.Core;
using UnityEngine;

namespace TeleportNative.Rendering
{
    /// <summary>
    /// Ponte para o UnityGaussianSplatting (Aras-p). Isolada por reflection: compila mesmo
    /// antes do pacote resolver, e "acende" quando o renderer (GaussianSplatRenderer) esta na
    /// cena com um asset. BudgetFraction = LOD/cutout pelo tier; SetPaused = economia de bateria.
    /// </summary>
    public interface ISplatRenderer
    {
        bool IsAvailable { get; }   // pacote + renderer presente na cena?
        bool HasAsset { get; }
        void ApplyEditorAsset(Object asset);
        Awaitable<Result<bool>> LoadFromPathAsync(string localPath);
        void SetBudgetFraction(float fraction);
        void SetPaused(bool paused);
    }
}
