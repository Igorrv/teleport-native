using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Aplica Team ID e assinatura manual no projeto Xcode exportado (CI GitHub Actions).
    /// Variaveis de ambiente: APPLE_TEAM_ID, APPLE_SIGNING_IDENTITY, APPLE_PROVISIONING_PROFILE_UUID
    /// </summary>
    public sealed class IosCiPostProcessor : IPostprocessBuildWithReport
    {
        private const string BundleId = "com.teleportnative.app";

        public int callbackOrder => 999;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
                return;

            var teamId = Environment.GetEnvironmentVariable("APPLE_TEAM_ID");
            if (string.IsNullOrWhiteSpace(teamId))
            {
                Debug.Log("[Teleport] APPLE_TEAM_ID ausente — signing sera feito no xcodebuild CI.");
                return;
            }

            var identity = Environment.GetEnvironmentVariable("APPLE_SIGNING_IDENTITY") ?? "Apple Development";
            var profileUuid = Environment.GetEnvironmentVariable("APPLE_PROVISIONING_PROFILE_UUID");

            var projPath = PBXProject.GetPBXProjectPath(report.summary.outputPath);
            var proj = new PBXProject();
            proj.ReadFromFile(projPath);

            ApplySigning(proj, proj.GetUnityMainTargetGuid(), teamId, identity, profileUuid);
            ApplySigning(proj, proj.GetUnityFrameworkTargetGuid(), teamId, identity, profileUuid);

            var mainTarget = proj.GetUnityMainTargetGuid();
            proj.SetBuildProperty(mainTarget, "IPHONEOS_DEPLOYMENT_TARGET", "15.0");
            proj.SetBuildProperty(proj.GetUnityFrameworkTargetGuid(), "IPHONEOS_DEPLOYMENT_TARGET", "15.0");

            proj.WriteToFile(projPath);
            WriteExportOptions(report.summary.outputPath, teamId, profileUuid);

            Debug.Log($"[Teleport] iOS CI signing: team={teamId}, identity={identity}, profile={profileUuid ?? "auto"}");
        }

        private static void ApplySigning(PBXProject proj, string targetGuid, string teamId, string identity, string profileUuid)
        {
            if (string.IsNullOrEmpty(targetGuid))
                return;

            proj.SetTeamId(targetGuid, teamId);
            proj.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM", teamId);
            proj.SetBuildProperty(targetGuid, "CODE_SIGN_STYLE", "Manual");
            proj.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY", identity);
            proj.SetBuildProperty(targetGuid, "DEVELOPMENT_TEAM[sdk=iphoneos*]", teamId);
            proj.SetBuildProperty(targetGuid, "CODE_SIGN_IDENTITY[sdk=iphoneos*]", identity);

            if (!string.IsNullOrWhiteSpace(profileUuid))
            {
                proj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE_SPECIFIER", profileUuid);
                proj.SetBuildProperty(targetGuid, "PROVISIONING_PROFILE", profileUuid);
            }

            proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
        }

        private static void WriteExportOptions(string iosRoot, string teamId, string profileUuid)
        {
            var path = Path.Combine(iosRoot, "ExportOptions.plist");
            var profileEntry = string.IsNullOrWhiteSpace(profileUuid)
                ? ""
                : $@"  <key>provisioningProfiles</key>
  <dict>
    <key>{BundleId}</key>
    <string>{profileUuid}</string>
    <key>{BundleId}.UnityFramework</key>
    <string>{profileUuid}</string>
  </dict>
";

            var plist = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
  <key>method</key>
  <string>development</string>
  <key>teamID</key>
  <string>{teamId}</string>
  <key>signingStyle</key>
  <string>manual</string>
  <key>compileBitcode</key>
  <false/>
  <key>thinning</key>
  <string>&lt;none&gt;</string>
{profileEntry}</dict>
</plist>
";
            File.WriteAllText(path, plist);
        }
    }
}
