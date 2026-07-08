namespace TeleportNative.Core
{
    /// <summary>Telas/estados da maquina de aplicacao. Onboarding -> Library -> Capture -> Processing -> Viewer -> Share.</summary>
    public enum AppScreen
    {
        Onboarding,   // permissoes + explicacao (1o uso)
        Library,      // Home: espacos capturados
        Capture,      // captura AR guiada
        Processing,   // upload -> reconstrucao -> gerando 3D
        Viewer,       // navegacao livre do splat
        Share         // link/QR/export do espaco
    }
}
