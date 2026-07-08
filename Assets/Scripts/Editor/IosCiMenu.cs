using System.IO;
using UnityEditor;
using UnityEngine;

namespace TeleportNative.Editor
{
    /// <summary>Atalho legado — delega para ExportIosProject (Xcode -> ios/).</summary>
    public static class IosCiMenu
    {
        [MenuItem("Teleport/9. Preparar export iOS (legado -> menu 8)")]
        public static void PrepareForIosExport()
        {
            Debug.Log("[Teleport] Use Teleport > 8. Export iOS (Xcode -> ios/). Redirecionando...");
            ExportIosProject.Export();
        }
    }
}
