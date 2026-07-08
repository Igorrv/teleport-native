using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Build do APK de desenvolvimento (sem loja). Gera um APK num caminho estavel
    /// (Build/teleport-native.apk) para que possa ser instalado por `adb install` num
    /// emulador ou device. Batchmode-safe: sem AutoRunPlayer, p/ rodar via CLI:
    ///   Unity.exe -batchmode -quit -projectPath . -executeMethod TeleportNative.Editor.DeviceBuildMenu.BuildApk
    /// Menu: Teleport > 5. Build APK (Android).
    /// </summary>
    public static class DeviceBuildMenu
    {
        private const string ApkPath = "Build/teleport-native.apk";

        [MenuItem("Teleport/5. Build APK (Android)")]
        public static void BuildApk()
        {
            if (!File.Exists("Assets/Main.unity"))
            {
                Debug.LogError("[Teleport] Rode antes o menu 'Teleport > 4. Montar cena Main (auto)'.");
                return;
            }

            // Troca para Android se necessario.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // APK de desenvolvimento (nao AAB). Keystore de debug e automatico.
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            // Cena unica no build.
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Main.unity", true)
            };

            Directory.CreateDirectory("Build");
            var absPath = Path.GetFullPath(ApkPath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Main.unity" },
                locationPathName = absPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development // sem AutoRunPlayer: instalacao via adb (funciona em batchmode)
            };

            Debug.Log("[Teleport] Compilando APK -> " + absPath + " ...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Teleport] APK OK -> {absPath}. Instale com: adb install \"{absPath}\"");
            }
            else
            {
                Debug.LogError($"[Teleport] Build {summary.result}. Causas comuns: ARCore nao habilitado no XR Plug-in Management (aba Android); SDK/NDK faltando; cena sem o rig AR.");
            }
        }

        /// <summary>
        /// Exporta o projeto Xcode (iOS). SO FUNCIONA EM macOS (regra da Apple: iOS nao
        /// compila em Windows). Rode num Mac com Unity 6 LTS + Xcode:
        ///   Unity -batchmode -quit -projectPath . -executeMethod TeleportNative.Editor.DeviceBuildMenu.BuildIos
        /// Depois abra Build/Xcode no Xcode, sign com seu Apple ID e de Run no iPhone conectado.
        /// Menu: Teleport > 6. Build iOS (projeto Xcode).
        /// </summary>
        [MenuItem("Teleport/6. Build iOS (projeto Xcode)")]
        public static void BuildIos()
        {
            if (!File.Exists("Assets/Main.unity"))
            {
                Debug.LogError("[Teleport] Rode antes o menu 'Teleport > 4. Montar cena Main (auto)'.");
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Main.unity", true)
            };

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Main.unity" },
                locationPathName = "Build/Xcode",
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.Development
            };

            Debug.Log("[Teleport] Exportando projeto Xcode -> Build/Xcode ...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[Teleport] Projeto Xcode OK -> Build/Xcode. Abra no Xcode, sign com seu Apple ID e rode no iPhone.");
            }
            else
            {
                Debug.LogError($"[Teleport] Build iOS {summary.result}. Observacao: iOS so compila em macOS. No Windows este metodo falha por design.");
            }
        }
    }
}
