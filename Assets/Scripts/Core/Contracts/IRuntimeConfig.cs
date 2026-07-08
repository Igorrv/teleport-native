namespace TeleportNative.Core
{
    /// <summary>
    /// Configuracao de runtime (URLs, chaves, feature flags). Unico ponto de troca entre
    /// dev/prod; pode ser sobrescrito por Resources/config.json ou PlayerPrefs (sem recompilar).
    /// </summary>
    public interface IRuntimeConfig
    {
        string BackendBaseUrl { get; }      // gateway publico HTTPS do HomeView (ex.: https://api.teleport.app)
        string ReconstructionProvider { get; } // luma | worldlabs | meshy | external
        string ProviderApiKey { get; }      // chave do provider 3DGS (nao embarcar em texto no binario final)
        string BundleId { get; }            // ex.: com.teleportnative.app
        bool AccountEnabled { get; }        // feature flag de conta/sincronismo (opcional, atrasado)
        bool DevHudEnabled { get; }         // mostra FPS/budget em build de dev
    }
}
