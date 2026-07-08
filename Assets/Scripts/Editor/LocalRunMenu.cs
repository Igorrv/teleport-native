using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Setup + build Windows Standalone para testar UI/viewer no PC (sem AR).
    /// CLI: Unity.exe -batchmode -quit -projectPath . -executeMethod TeleportNative.Editor.LocalRunMenu.SetupAndBuildWindows
    /// Menu: Teleport > 7. Setup + Build Windows (teste local).
    /// </summary>
    public static class LocalRunMenu
    {
        private const string ExePath = "Build/Windows/teleport-native.exe";

        [MenuItem("Teleport/7. Setup + Build Windows (teste local)")]
        public static void SetupAndBuildWindows()
        {
            if (!File.Exists("Assets/Resources/config.json"))
                BuildMenu.GenConfig();

            if (!File.Exists("Assets/Main.unity"))
                SceneBuilder.BuildMainScene();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Main.unity", true)
            };

            Directory.CreateDirectory("Build/Windows");
            var abs = Path.GetFullPath(ExePath);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Main.unity" },
                locationPathName = abs,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log("[Teleport] Build Windows -> " + abs);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[Teleport] Build OK. Iniciando: " + abs);
                Process.Start(new ProcessStartInfo(abs) { WorkingDirectory = Path.GetDirectoryName(abs) });
            }
            else
            {
                Debug.LogError("[Teleport] Build Windows falhou: " + report.summary.result);
            }
        }
    }
}
