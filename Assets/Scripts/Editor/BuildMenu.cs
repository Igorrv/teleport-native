using System.IO;
using UnityEditor;
using UnityEngine;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Atalhos de menu p/ preparar o projeto para publish e gerar o config.json. Abra em
    /// Unity > menu "Teleport". Ajuste bundle id / chave / backend antes do build.
    /// </summary>
    public static class BuildMenu
    {
        private const string BundleId = "com.teleportnative.app";
        private const string CameraUsage = "Usamos a camera para capturar o ambiente e criar espacos 3D.";

        [MenuItem("Teleport/1. Preparar iOS")]
        public static void PrepIOS()
        {
            PlayerSettings.companyName = "Teleport";
            PlayerSettings.productName = "Teleport Native";
            PlayerSettings.applicationIdentifier = BundleId;
            PlayerSettings.iOS.cameraUsageDescription = CameraUsage;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            SetIosRequireArKit(true);
            try { PlayerSettings.iOS.targetOSVersionString = "14.0"; } catch { /* versao de API antiga */ }
            Debug.Log("[Teleport] iOS preparado (camera + ARKit). ARKit e ligado automaticamente pelo menu 4b / export iOS.");
        }

        private static void SetIosRequireArKit(bool required)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("iOSRequireARKit");
            if (prop != null)
            {
                prop.intValue = required ? 1 : 0;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        [MenuItem("Teleport/2. Preparar Android")]
        public static void PrepAndroid()
        {
            PlayerSettings.companyName = "Teleport";
            PlayerSettings.productName = "Teleport Native";
            PlayerSettings.applicationIdentifier = BundleId;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26; // ARCore exige API 24+; 26 p/ segura
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)0; // Auto
            // CAMERA + AR: o pacote ARCore ja adiciona a permissao CAMERA e o <uses-feature> AR.
            Debug.Log("[Teleport] Android preparado. Habilite ARCore em XR Plug-in Management (Android tab) + exporte como AAB assinado.");
        }

        [MenuItem("Teleport/3. Gerar Resources/config.json")]
        public static void GenConfig()
        {
            string dir = "Assets/Resources";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string path = dir + "/config.json";
            string json =
"{\n" +
"  \"BackendBaseUrl\": \"https://api.exemplo.com\",\n" +
"  \"ReconstructionProvider\": \"luma\",\n" +
"  \"ProviderApiKey\": \"COLE_SUA_CHAVE\",\n" +
"  \"BundleId\": \"" + BundleId + "\",\n" +
"  \"AccountEnabled\": false,\n" +
"  \"DevHudEnabled\": true\n" +
"}\n";
            File.WriteAllText(path, json);
            AssetDatabase.ImportAsset(path);
            Debug.Log("[Teleport] config.json gerado em " + path + " — edite backend/provider/chave antes do build.");
        }
    }
}
