using Newtonsoft.Json;
using UnityEngine;

namespace TeleportNative.Core
{
    /// <summary>
    /// Implementacao padrao de IRuntimeConfig. Defaults seguros + override via
    /// PlayerPrefs (debug) ou Resources/config.json (empacotado). Usa Newtonsoft (private setters).
    /// Nunca logar a chave.
    /// </summary>
    public sealed class RuntimeConfig : IRuntimeConfig
    {
        public string BackendBaseUrl { get; private set; } = "https://api.exemplo.com";
        public string ReconstructionProvider { get; private set; } = "luma";
        public string ProviderApiKey { get; private set; } = "";
        public string BundleId { get; private set; } = "com.teleportnative.app";
        public bool AccountEnabled { get; private set; } = false;
        public bool DevHudEnabled { get; private set; } = true;

        public static IRuntimeConfig Load()
        {
            RuntimeConfig c;
            var tex = Resources.Load<TextAsset>("config");
            if (tex != null)
            {
                try { c = JsonConvert.DeserializeObject<RuntimeConfig>(tex.text); }
                catch { c = new RuntimeConfig(); /* config invalido: mantem defaults */ }
            }
            else c = new RuntimeConfig();

            // Override de debug em runtime (nao persiste entre reinstalacoes).
            if (PlayerPrefs.HasKey("tn_backend")) c.BackendBaseUrl = PlayerPrefs.GetString("tn_backend");
            if (PlayerPrefs.HasKey("tn_provider")) c.ReconstructionProvider = PlayerPrefs.GetString("tn_provider");
            if (PlayerPrefs.HasKey("tn_key")) c.ProviderApiKey = PlayerPrefs.GetString("tn_key");
            return c;
        }

        public void OverrideForDebug(string backend, string provider, string key)
        {
            BackendBaseUrl = backend; ReconstructionProvider = provider; ProviderApiKey = key;
        }
    }
}
