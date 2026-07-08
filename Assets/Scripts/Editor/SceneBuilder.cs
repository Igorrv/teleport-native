using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TeleportNative.UI;
using TeleportNative.Rendering;

namespace TeleportNative.Editor
{
    /// <summary>
    /// Monta automaticamente a cena "Main" (AppBootstrap + EventSystem + Canvas + Viewer Rig +
    /// AR Rig) e vincula as referencias do AppBootstrap. As telas sao criadas em runtime pelo
    /// ScreenManager. Restam passadas manuais (ver log no console) que dependem de pacotes XR
    /// e do pacote de Gaussian Splatting, feitas pelo Inspector depois do pacote resolver.
    /// Menu: Teleport > 4. Montar cena Main (auto).
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenePath = "Assets/Main.unity";

        [MenuItem("Teleport/4. Montar cena Main (auto)")]
        public static void BuildMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // EventSystem (uGUI)
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // Canvas (Screen Space - Overlay, scaler responsivo)
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // AR Rig (ativo so em Capture). Apenas ARSession aqui; a AR Camera (XROrigin) e
            // adicionada depois pelo menu do Unity (GameObject > XR > XR Origin (Mobile AR)).
            var arRig = new GameObject("AR Rig");
            AddByTypeName(arRig, "UnityEngine.XR.ARFoundation.ARSession");

            // Viewer Rig (ativo so em Viewer)
            var viewerRig = new GameObject("Viewer Rig");
            var vcamGo = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            vcamGo.transform.SetParent(viewerRig.transform, false);
            var vcam = vcamGo.GetComponent<Camera>();
            vcam.tag = "MainCamera";
            vcam.clearFlags = CameraClearFlags.SolidColor;
            vcam.backgroundColor = new Color(0.043f, 0.051f, 0.075f, 1f); // DesignTokens.Background
            vcam.nearClipPlane = 0.05f;
            vcam.farClipPlane = 200f;
            var viewer = vcamGo.AddComponent<SplatViewerController>();
            var camCtrl = vcamGo.AddComponent<SplatCameraController>();

            var hostGo = new GameObject("Splat Host");
            hostGo.transform.SetParent(viewerRig.transform, false);

            SetSerializedField(viewer, "_splatHost", hostGo.transform);
            SetSerializedField(camCtrl, "_target", hostGo.transform);

            // Entry point
            var bootstrapGo = new GameObject("AppBootstrap");
            var bootstrap = bootstrapGo.AddComponent<AppBootstrap>();
            SetSerializedField(bootstrap, "_uiRoot", canvas);
            SetSerializedField(bootstrap, "_viewer", viewer);
            SetSerializedField(bootstrap, "_camera", camCtrl);
            SetSerializedField(bootstrap, "_arRig", arRig);
            SetSerializedField(bootstrap, "_viewerRig", viewerRig);
            // _capture fica null: exige XR Origin + ARCameraManager (montados manualmente depois).

            ArSetupMenu.CompleteArRigInScene();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log(
                "[Teleport] Cena 'Main' criada em " + ScenePath + " (AR Rig + XR Origin + ARCaptureSession).\n" +
                "Opcional (M1 Viewer): adicione 'GaussianSplatRenderer' ao 'Splat Host' e atribua um GaussianSplatAsset de exemplo.\n" +
                "iPhone: rode Codemagic (IPHONE.md) e instale o .ipa com scripts/install-iphone.ps1.");
        }

        private static Component AddByTypeName(GameObject go, string fullTypeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullTypeName);
                if (t != null) return go.AddComponent(t);
            }
            Debug.LogWarning("[Teleport] Tipo nao encontrado (pacote ainda nao resolvido?): " + fullTypeName);
            return null;
        }

        private static void SetSerializedField(Component c, string fieldName, UnityEngine.Object value)
        {
            var so = new SerializedObject(c);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[Teleport] Campo nao encontrado: " + fieldName + " em " + c.GetType().Name);
            }
        }
    }
}
