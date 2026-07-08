namespace TeleportNative.Editor
{
    /// <summary>
    /// Entry point CI (GitHub Actions / game-ci).
    /// CLI: -executeMethod TeleportNative.Editor.BuildiOS.Build
    /// </summary>
    public static class BuildiOS
    {
        public static void Build() => ExportIosProject.Export();
    }
}
