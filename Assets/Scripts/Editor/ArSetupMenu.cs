using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;
using TeleportNative.Capture;
using TeleportNative.UI;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Liga XR Origin + ARCaptureSession na cena Main e habilita ARKit (iOS).
    /// Necessario para camera AR no iPhone.
    /// </summary>
    public static class ArSetupMenu
    {
        private const string ScenePath = "Assets/Main.unity";
        private const string ArKitLoader = "UnityEngine.XR.ARKit.ARKitLoader, Unity.XR.ARKit";

        [MenuItem("Teleport/4b. Completar AR Rig (camera iPhone)")]
        public static void CompleteArRigMenu()
        {
            if (!File.Exists(ScenePath))
                SceneBuilder.BuildMainScene();
            else
                EditorSceneManager.OpenScene(ScenePath);

            CompleteArRigInScene();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[Teleport] AR Rig pronto para iPhone (XR Origin + ARCaptureSession + ARKit).");
        }

        /// <summary>Chamado por BuildMainScene, IosCiMenu e CI Codemagic.</summary>
        public static void CompleteArRigInScene()
        {
            EnableArKitForIos();
            BuildMenu.PrepIOS();

            var arRig = GameObject.Find("AR Rig");
            if (arRig == null)
            {
                Debug.LogError("[Teleport] GameObject 'AR Rig' nao encontrado na cena.");
                return;
            }

            var originTf = arRig.transform.Find("XR Origin");
            XROrigin origin;
            if (originTf == null)
                origin = CreateMobileArOrigin(arRig.transform);
            else
            {
                origin = originTf.GetComponent<XROrigin>();
                if (origin == null)
                    origin = originTf.gameObject.AddComponent<XROrigin>();
            }

            var cam = origin.Camera;
            if (cam == null)
            {
                Debug.LogError("[Teleport] Camera AR nao encontrada no XR Origin.");
                return;
            }

            FixViewerCameraConflict();

            var capture = cam.GetComponent<ARCaptureSession>();
            if (capture == null)
                capture = cam.gameObject.AddComponent<ARCaptureSession>();

            var bootstrap = Object.FindFirstObjectByType<AppBootstrap>();
            if (bootstrap != null)
                SetCaptureRef(bootstrap, capture);
            else
                Debug.LogWarning("[Teleport] AppBootstrap nao encontrado — ligue Capture manualmente no Inspector.");

            arRig.SetActive(false);
        }

        public static void EnableArKitForIos()
        {
            var perBuildType = typeof(UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget);
            var getOrCreate = perBuildType.GetMethod("GetOrCreate", BindingFlags.NonPublic | BindingFlags.Static);
            var perBuild = getOrCreate?.Invoke(null, null);
            if (perBuild == null)
            {
                Debug.LogWarning("[Teleport] XRGeneralSettingsPerBuildTarget nao disponivel.");
                return;
            }

            var ios = (BuildTargetGroup)BuildTargetGroup.iOS;
            if (!(bool)perBuildType.GetMethod("HasSettingsForBuildTarget")!.Invoke(perBuild, new object[] { ios }))
                perBuildType.GetMethod("CreateDefaultSettingsForBuildTarget")!.Invoke(perBuild, new object[] { ios });

            var gs = perBuildType.GetMethod("SettingsForBuildTarget")!.Invoke(perBuild, new object[] { ios });
            gs!.GetType().GetProperty("InitManagerOnStart")!.SetValue(gs, true);

            if (!(bool)perBuildType.GetMethod("HasManagerSettingsForBuildTarget")!.Invoke(perBuild, new object[] { ios }))
                perBuildType.GetMethod("CreateDefaultManagerSettingsForBuildTarget")!.Invoke(perBuild, new object[] { ios });

            var manager = perBuildType.GetMethod("ManagerSettingsForBuildTarget")!.Invoke(perBuild, new object[] { ios }) as XRManagerSettings;
            if (manager != null && !XRPackageMetadataStore.IsLoaderAssigned(ArKitLoader, BuildTargetGroup.iOS))
                XRPackageMetadataStore.AssignLoader(manager, ArKitLoader, BuildTargetGroup.iOS);

            EditorUtility.SetDirty(perBuild as Object);
            AssetDatabase.SaveAssets();
        }

        private static XROrigin CreateMobileArOrigin(Transform parent)
        {
            var originGo = new GameObject("XR Origin");
            originGo.transform.SetParent(parent, false);
            var origin = originGo.AddComponent<XROrigin>();

            var offsetGo = new GameObject("Camera Offset");
            offsetGo.transform.SetParent(originGo.transform, false);

            var cameraGo = new GameObject("Main Camera",
                typeof(Camera),
                typeof(AudioListener),
                typeof(ARCameraManager),
                typeof(ARCameraBackground));
            cameraGo.transform.SetParent(offsetGo.transform, false);

            var camera = cameraGo.GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;

            var tpdType = System.Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");
            if (tpdType != null)
                cameraGo.AddComponent(tpdType);

            origin.CameraFloorOffsetObject = offsetGo;
            origin.Camera = camera;
            return origin;
        }

        private static void FixViewerCameraConflict()
        {
            var viewerRig = GameObject.Find("Viewer Rig");
            if (viewerRig == null) return;

            foreach (var cam in viewerRig.GetComponentsInChildren<Camera>(true))
            {
                if (cam.CompareTag("MainCamera"))
                    cam.tag = "Untagged";
                var listener = cam.GetComponent<AudioListener>();
                if (listener != null)
                    Object.DestroyImmediate(listener);
            }
        }

        private static void SetCaptureRef(AppBootstrap bootstrap, ARCaptureSession capture)
        {
            var so = new SerializedObject(bootstrap);
            var prop = so.FindProperty("_capture");
            if (prop == null)
            {
                Debug.LogWarning("[Teleport] Campo _capture nao encontrado em AppBootstrap.");
                return;
            }
            prop.objectReferenceValue = capture;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
