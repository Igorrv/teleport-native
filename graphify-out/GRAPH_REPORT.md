# Graph Report - teleport-native  (2026-07-07)

## Corpus Check
- 84 files · ~20,436 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 953 nodes · 1580 edges · 43 communities
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 38 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_TeleportNative.Core|TeleportNative.Core]]
- [[_COMMUNITY_Result|Result]]
- [[_COMMUNITY_LibraryScreen|LibraryScreen]]
- [[_COMMUNITY_TeleportNative.UI|TeleportNative.UI]]
- [[_COMMUNITY_Space|Space]]
- [[_COMMUNITY_BUILD.md — teleport-native (montagem, validacao e publicacao)|BUILD.md — teleport-native (montagem, validacao e publicacao)]]
- [[_COMMUNITY_JoystickView|JoystickView]]
- [[_COMMUNITY_UnityGaussianSplatAdapter|UnityGaussianSplatAdapter]]
- [[_COMMUNITY_BuildMenu|BuildMenu]]
- [[_COMMUNITY_.SendAsync|.SendAsync]]
- [[_COMMUNITY_dependencies|dependencies]]
- [[_COMMUNITY_ARCaptureSession|ARCaptureSession]]
- [[_COMMUNITY_SplatData|SplatData]]
- [[_COMMUNITY_FrameSelector|FrameSelector]]
- [[_COMMUNITY_SceneContract.cs|SceneContract.cs]]
- [[_COMMUNITY_SplatCameraController|SplatCameraController]]
- [[_COMMUNITY_.SharpnessFromTexture|.SharpnessFromTexture]]
- [[_COMMUNITY_CoverageState|CoverageState]]
- [[_COMMUNITY_IDeviceProfiler|IDeviceProfiler]]
- [[_COMMUNITY_CapturedFrame|CapturedFrame]]
- [[_COMMUNITY_SplatViewerController|SplatViewerController]]
- [[_COMMUNITY_CaptureScreen|CaptureScreen]]
- [[_COMMUNITY_.ProcessFrame|.ProcessFrame]]
- [[_COMMUNITY_SplatBudget|SplatBudget]]
- [[_COMMUNITY_ISplatRenderer|ISplatRenderer]]
- [[_COMMUNITY_ScreenManager|ScreenManager]]
- [[_COMMUNITY_AppBootstrap|AppBootstrap]]
- [[_COMMUNITY_TeleportNative.Capture|TeleportNative.Capture]]
- [[_COMMUNITY_ISplatViewer|ISplatViewer]]
- [[_COMMUNITY_teleport-native — convencoes do app Unity|teleport-native — convencoes do app Unity]]
- [[_COMMUNITY_FramePacer|FramePacer]]
- [[_COMMUNITY_Result|Result]]
- [[_COMMUNITY_AppFlow|AppFlow]]
- [[_COMMUNITY_IFrameSelector|IFrameSelector]]
- [[_COMMUNITY_AppScreen|AppScreen]]
- [[_COMMUNITY_IRuntimeConfig|IRuntimeConfig]]
- [[_COMMUNITY_com.unity.nuget.newtonsoft-json|com.unity.nuget.newtonsoft-json]]
- [[_COMMUNITY_AppScreen|AppScreen]]
- [[_COMMUNITY_Instalar no iPhone (Windows + iPhone, sem Mac)|Instalar no iPhone (Windows + iPhone, sem Mac)]]

## God Nodes (most connected - your core abstractions)
1. `TeleportNative.Core` - 36 edges
2. `Result` - 28 edges
3. `ARCaptureSession` - 26 edges
4. `AppContext` - 23 edges
5. `UnityGaussianSplatAdapter` - 21 edges
6. `CaptureScreen` - 20 edges
7. `SplatViewerController` - 18 edges
8. `TeleportNative.UI` - 18 edges
9. `Space` - 17 edges
10. `AppScreen` - 16 edges

## Surprising Connections (you probably didn't know these)
- `AppBootstrap` --references--> `ARCaptureSession`  [EXTRACTED]
  Assets/Scripts/UI/Composition/AppBootstrap.cs → Assets/Scripts/Capture/ARCaptureSession.cs
- `FrameSelector` --implements--> `IFrameSelector`  [EXTRACTED]
  Assets/Scripts/Capture/FrameSelector.cs → Assets/Scripts/Capture/IFrameSelector.cs
- `AppContext` --references--> `ICaptureSession`  [EXTRACTED]
  Assets/Scripts/UI/Composition/AppContext.cs → Assets/Scripts/Capture/ICaptureSession.cs
- `AppContext` --references--> `AppFlow`  [EXTRACTED]
  Assets/Scripts/UI/Composition/AppContext.cs → Assets/Scripts/Core/App/AppFlow.cs
- `AppContext` --references--> `Space`  [EXTRACTED]
  Assets/Scripts/UI/Composition/AppContext.cs → Assets/Scripts/Core/Domain/Space.cs

## Import Cycles
- None detected.

## Communities (43 total, 0 thin omitted)

### Community 0 - "TeleportNative.Core"
Cohesion: 0.05
Nodes (22): AppFlow, AppScreen, Color, float, DesignTokens, ILogger, UnityLogger, double (+14 more)

### Community 1 - "Result"
Cohesion: 0.05
Nodes (34): string, Result, float, string, ReconstructionState, ReconstructionStatus, Awaitable, CancellationToken (+26 more)

### Community 2 - "LibraryScreen"
Cohesion: 0.06
Nodes (34): AppScreen, Color, RectTransform, Text, UnityAction, LibraryScreen, Transform, OnboardingScreen (+26 more)

### Community 3 - "TeleportNative.UI"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 4 - "Space"
Cohesion: 0.14
Nodes (9): long, string, Space, IReadOnlyList, ISceneRepository, IReadOnlyList, List, string (+1 more)

### Community 5 - "BUILD.md — teleport-native (montagem, validacao e publicacao)"
Cohesion: 0.06
Nodes (29): Arquitetura (3 camadas), Contrato de dados (reuso do HomeView), Decisoes travadas (NAO reabrir sem pedido), Implementacao atual (codigo completo, pronto p/ abrir no Unity), Milestones (ordem; valide cada um antes do proximo), Objetivo, Orcamento de desempenho (budget de splats por tier), Pendencias a confirmar com o usuario quando chegar a hora (+21 more)

### Community 6 - "JoystickView"
Cohesion: 0.11
Nodes (14): Color, RectTransform, Text, Vector2, ViewerScreen, RectTransform, Vector2, JoystickView (+6 more)

### Community 7 - "UnityGaussianSplatAdapter"
Cohesion: 0.07
Nodes (19): Array, Color32, int, Quaternion, Vector3, SplatData, Awaitable, Color32 (+11 more)

### Community 8 - "BuildMenu"
Cohesion: 0.07
Nodes (23): MenuItem, string, Transform, ArSetupMenu, MenuItem, string, BuildMenu, MenuItem (+15 more)

### Community 9 - ".SendAsync"
Cohesion: 0.19
Nodes (16): Awaitable, bool, byte, CancellationToken, Dictionary, float, int, IProgress (+8 more)

### Community 10 - "dependencies"
Cohesion: 0.08
Nodes (23): dependencies, com.unity.ide.visualstudio, com.unity.inputsystem, com.unity.modules.adaptiveperformance, com.unity.modules.androidjni, com.unity.modules.audio, com.unity.modules.imageconversion, com.unity.modules.imgui (+15 more)

### Community 11 - "ARCaptureSession"
Cohesion: 0.12
Nodes (20): depth, source, version, dependencies, depth, source, version, dependencies (+12 more)

### Community 12 - "SplatData"
Cohesion: 0.12
Nodes (17): dependencies, depth, source, url, version, depth, source, url (+9 more)

### Community 13 - "FrameSelector"
Cohesion: 0.24
Nodes (6): bool, float, Matrix4x4, FrameSelector, Test, FrameSelectorTests

### Community 14 - "SceneContract.cs"
Cohesion: 0.27
Nodes (12): bool, float, int, List, string, CaptureInfo, SceneContract, SceneExports (+4 more)

### Community 15 - "SplatCameraController"
Cohesion: 0.26
Nodes (4): float, Transform, Vector2, SplatCameraController

### Community 16 - ".SharpnessFromTexture"
Cohesion: 0.23
Nodes (5): Color, Texture2D, BlurDetector, Test, BlurDetectorTests

### Community 17 - "CoverageState"
Cohesion: 0.06
Nodes (28): ARCameraFrameEventArgs, ARCameraManager, bool, Camera, int, IReadOnlyList, List, Texture2D (+20 more)

### Community 18 - "IDeviceProfiler"
Cohesion: 0.05
Nodes (25): float, DeviceProfiler, Tier, float, int, FramePacer, IDeviceProfiler, int (+17 more)

### Community 19 - "CapturedFrame"
Cohesion: 0.13
Nodes (17): depth, source, version, dependencies, depth, source, url, version (+9 more)

### Community 20 - "SplatViewerController"
Cohesion: 0.10
Nodes (21): dependencies, depth, source, version, dependencies, depth, source, version (+13 more)

### Community 21 - "CaptureScreen"
Cohesion: 0.08
Nodes (17): float, int, CaptureFlowLogic, bool, float, GameObject, Image, int (+9 more)

### Community 22 - ".ProcessFrame"
Cohesion: 0.05
Nodes (42): dependencies, depth, source, version, dependencies, depth, source, version (+34 more)

### Community 23 - "SplatBudget"
Cohesion: 0.13
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 24 - "ISplatRenderer"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 25 - "ScreenManager"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 26 - "AppBootstrap"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.searcher

### Community 27 - "TeleportNative.Capture"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, url (+8 more)

### Community 28 - "ISplatViewer"
Cohesion: 0.20
Nodes (10): dependencies, depth, source, url, version, depth, source, version (+2 more)

### Community 29 - "teleport-native — convencoes do app Unity"
Cohesion: 0.33
Nodes (5): Build / verificacao (economico), Convencoes C#/Unity, Estrutura, teleport-native — convencoes do app Unity, UnityGaussianSplatting (Aras-p)

### Community 30 - "FramePacer"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.unitywebrequestwww

### Community 31 - "Result"
Cohesion: 0.22
Nodes (8): TeleportNative.Capture, TeleportNative.Core, TeleportNative.Editor, TeleportNative.Network, TeleportNative.Performance, TeleportNative.Rendering, TeleportNative.Tests.EditMode, TeleportNative.UI

### Community 32 - "AppFlow"
Cohesion: 0.08
Nodes (27): dependencies, depth, source, url, version, depth, source, version (+19 more)

### Community 33 - "IFrameSelector"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.visualstudio

### Community 34 - "AppScreen"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.vectorgraphics

### Community 35 - "IRuntimeConfig"
Cohesion: 0.12
Nodes (17): dependencies, dependencies, depth, source, url, version, depth, dependencies (+9 more)

### Community 38 - "com.unity.nuget.newtonsoft-json"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.nuget.newtonsoft-json

### Community 45 - "AppScreen"
Cohesion: 0.10
Nodes (11): HapticType, IHaptics, IRuntimeConfig, RuntimeConfig, GameObject, IReadOnlyList, string, AppContext (+3 more)

### Community 46 - "Instalar no iPhone (Windows + iPhone, sem Mac)"
Cohesion: 0.22
Nodes (8): 1. Gerar o `.ipa` na nuvem (Codemagic — recomendado), 2. Instalar no iPhone (Windows), 3. Confiar no app (iPhone), Alternativa: Mac emprestado, Graphify, Instalar no iPhone (Windows + iPhone, sem Mac), Passo a passo (15–30 min), Status do projeto

## Knowledge Gaps
- **247 isolated node(s):** `SceneType`, `com.unity.ide.visualstudio`, `com.unity.inputsystem`, `com.unity.nuget.newtonsoft-json`, `com.unity.render-pipelines.universal` (+242 more)
  These have ≤1 connection - possible missing edges or undocumented components.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `TeleportNative.Core` connect `TeleportNative.Core` to `Result`, `Space`, `UnityGaussianSplatAdapter`, `AppScreen`, `SceneContract.cs`, `IDeviceProfiler`?**
  _High betweenness centrality (0.082) - this node is a cross-community bridge._
- **Why does `AppContext` connect `AppScreen` to `TeleportNative.Core`, `Result`, `Space`, `.SendAsync`, `SplatCameraController`, `CoverageState`, `IDeviceProfiler`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **Why does `dependencies` connect `ARCaptureSession` to `AppFlow`, `IFrameSelector`, `AppScreen`, `TeleportNative.UI`, `IRuntimeConfig`, `com.unity.nuget.newtonsoft-json`, `SplatData`, `CapturedFrame`, `SplatViewerController`, `.ProcessFrame`, `SplatBudget`, `ISplatRenderer`, `ScreenManager`, `AppBootstrap`, `TeleportNative.Capture`, `ISplatViewer`, `FramePacer`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **What connects `SceneType`, `com.unity.ide.visualstudio`, `com.unity.inputsystem` to the rest of the system?**
  _247 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `TeleportNative.Core` be split into smaller, more focused modules?**
  _Cohesion score 0.05045045045045045 - nodes in this community are weakly interconnected._
- **Should `Result` be split into smaller, more focused modules?**
  _Cohesion score 0.05017543859649123 - nodes in this community are weakly interconnected._
- **Should `LibraryScreen` be split into smaller, more focused modules?**
  _Cohesion score 0.057124310288867254 - nodes in this community are weakly interconnected._