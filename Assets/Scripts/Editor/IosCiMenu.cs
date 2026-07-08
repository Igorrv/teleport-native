using System.IO;
using UnityEditor;
using UnityEngine;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Prepara o projeto iOS para CI (macOS) ou export Xcode local.
    /// Windows: use scripts/install-iphone.ps1 apos baixar o IPA do Codemagic/GitHub Actions.
    /// macOS CLI:
    ///   Unity -batchmode -quit -projectPath . -executeMethod TeleportNative.Editor.IosCiMenu.PrepareForIosExport
    /// </summary>
    public static class IosCiMenu
    {
        [MenuItem("Teleport/8. Preparar export iOS (CI/Xcode)")]
        public static void PrepareForIosExport()
        {
            if (!File.Exists("Assets/Resources/config.json"))
                BuildMenu.GenConfig();

            if (!File.Exists("Assets/Main.unity"))
                SceneBuilder.BuildMainScene();
            else
                ArSetupMenu.CompleteArRigInScene();

            BuildMenu.PrepIOS();
            DeviceBuildMenu.BuildIos();
        }
    }
}
