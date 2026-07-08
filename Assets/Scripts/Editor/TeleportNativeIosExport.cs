using System.IO;
using UnityEditor;
using UnityEditor.iOS;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Exporta projeto Xcode localmente para ios/ (Codemagic compila IPA sem Unity).
    /// macOS + modulo iOS obrigatorios.
    /// CLI: Unity -batchmode -quit -projectPath . -executeMethod TeleportNative.Editor.ExportIosProject.Export
    /// </summary>
    public static class ExportIosProject
    {
        private const string BundleId = "com.teleportnative.app";
        private const string MainScene = "Assets/Main.unity";
        private const string BuildDir = "Build/iOS";
        private const string IosDir = "ios";

        [MenuItem("Teleport/8. Export iOS (Xcode -> ios/)")]
        public static void Export()
        {
            if (Application.platform != RuntimePlatform.OSXEditor && !Application.isBatchMode)
            {
                Debug.LogError("[Teleport] Export iOS exige Unity macOS (modulo iOS). " +
                               "No Windows use CI (GitHub Actions / Codemagic).");
                ExitBatch(1);
                return;
            }

            PrepareProject();
            ApplyIosPlayerSettings();
            SwitchToIos();

            if (!File.Exists(MainScene))
            {
                Debug.LogError("[Teleport] Cena Main ausente. Rode Teleport > 4. Montar cena Main.");
                ExitBatch(1);
                return;
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MainScene, true) };

            if (Directory.Exists(BuildDir))
                Directory.Delete(BuildDir, true);
            Directory.CreateDirectory(BuildDir);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { MainScene },
                locationPathName = BuildDir,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.None
            };

            Debug.Log("[Teleport] Exportando Xcode -> " + BuildDir + " ...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[Teleport] Export iOS falhou: " + report.summary.result);
                ExitBatch(1);
                return;
            }

            SyncToIosFolder();
            WriteCodemagicFiles(IosDir);

            Debug.Log("[Teleport] Export OK -> ios/Unity-iPhone.xcodeproj");
            ExitBatch(0);
        }

        private static void ExitBatch(int code)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(code);
        }

        private static void PrepareProject()
        {
            if (!File.Exists("Assets/Resources/config.json"))
                BuildMenu.GenConfig();

            if (!File.Exists(MainScene))
                SceneBuilder.BuildMainScene();
            else
                ArSetupMenu.CompleteArRigInScene();
        }

        private static void ApplyIosPlayerSettings()
        {
            BuildMenu.PrepIOS();

            PlayerSettings.applicationIdentifier = BundleId;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, OSArchitecture.ARM64);

            try { PlayerSettings.iOS.targetOSVersionString = "15.0"; } catch { /* API antiga */ }

            EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;
            EditorUserBuildSettings.symlinkSources = false;

            Debug.Log("[Teleport] iOS: IL2CPP, ARM64, min 15.0, Release, bundle=" + BundleId);
        }

        private static void SwitchToIos()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }

        private static void SyncToIosFolder()
        {
            if (Directory.Exists(IosDir))
                Directory.Delete(IosDir, true);
            CopyDirectory(BuildDir, IosDir);
            Debug.Log("[Teleport] Copiado " + BuildDir + " -> " + IosDir);
        }

        private static void WriteCodemagicFiles(string iosRoot)
        {
            var podfile = Path.Combine(iosRoot, "Podfile");
            if (!File.Exists(podfile) || File.ReadAllText(podfile).IndexOf("platform :ios", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                File.WriteAllText(podfile, PodfileTemplate);
            }
            else
            {
                var text = File.ReadAllText(podfile);
                if (!text.Contains("15.0"))
                    File.WriteAllText(podfile, PodfileTemplate + "\n# Unity Podfile original substituido — ajuste se necessario.\n");
            }

            File.WriteAllText(Path.Combine(iosRoot, "ExportOptions.plist"), ExportOptionsPlist);
        }

        private static void CopyDirectory(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dir.Replace(src, dst));

            foreach (var file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".meta")) continue;
                var dest = file.Replace(src, dst);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, true);
            }
        }

        private const string PodfileTemplate = @"source 'https://cdn.cocoapods.org/'

platform :ios, '15.0'

target 'Unity-iPhone' do
end

target 'UnityFramework' do
end

post_install do |installer|
  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = '15.0'
    end
  end
end
";

        private const string ExportOptionsPlist = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
  <key>method</key>
  <string>development</string>
  <key>signingStyle</key>
  <string>automatic</string>
  <key>compileBitcode</key>
  <false/>
</dict>
</plist>
";
    }
}
