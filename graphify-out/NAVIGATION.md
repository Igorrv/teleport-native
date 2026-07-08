# teleport-native — mapa do grafo (use isto antes de ler arquivos)

> Grafo: 609 nós · 1079 arestas · 31 comunidades · AST-only (2026-07-07)
> Benchmark: ~40.600 tokens (ler tudo) → ~900–5.000 tokens por query (**8–36x**)

## God nodes (comece por aqui)

| Nó | Papel | Arquivo principal |
|---|---|---|
| `TeleportNative.Core` | Domínio, AppFlow, contratos | `Assets/Scripts/Core/` |
| `ARCaptureSession` | Captura AR (frames+pose) | `Assets/Scripts/Capture/ARCaptureSession.cs` |
| `AppBootstrap` | Composition root / DI | `Assets/Scripts/UI/Composition/AppBootstrap.cs` |
| `AppContext` | Estado compartilhado da UI | `Assets/Scripts/UI/Composition/AppContext.cs` |
| `ScreenManager` | Navegação entre telas | `Assets/Scripts/UI/Composition/ScreenManager.cs` |
| `UnityGaussianSplatAdapter` | Bridge p/ UnityGaussianSplatting | `Assets/Scripts/Rendering/UnityGaussianSplatAdapter.cs` |
| `SplatViewerController` | Motor do viewer (LOD/budget) | `Assets/Scripts/Rendering/SplatViewerController.cs` |
| `HomeViewReconstructionProvider` | API HomeView / nuvem | `Assets/Scripts/Network/HomeViewReconstructionProvider.cs` |
| `Result` | Result<T> pattern | `Assets/Scripts/Core/Common/Result.cs` |

## Módulos (8 assemblies)

```
Core/         AppFlow, SceneContract, Space, Result, DesignTokens, RuntimeConfig
Capture/      ARCaptureSession, FrameSelector, BlurDetector, CoverageTracker
Network/      RestClient, ReconstructionClient, SplatCache, SceneRepository
Rendering/    SplatViewerController, SplatCameraController, UnityGaussianSplatAdapter
Performance/  DeviceProfiler, SplatBudget, FramePacer
UI/           Screens/*, UIFactory, AppBootstrap, ScreenManager
Tests/        EditMode/AppFlowTests.cs
```

## Fluxos principais (caminhos no grafo)

| De | Para | Hops | Como |
|---|---|---:|---|
| `CaptureScreen` | `SplatViewerController` | 4 | UI → Core → Rendering |
| `ARCaptureSession` | `SplatViewerController` | 2 | via `AppBootstrap` |
| `TeleportNative.Core` | `HomeViewReconstructionProvider` | 1 | import direto |

## Comandos graphify (rodar na raiz do projeto)

```bash
graphify query "pergunta em ingles" --budget 1200
graphify explain "ARCaptureSession"
graphify path "CaptureScreen" "SplatViewerController"
graphify affected "SplatViewerController" --depth 2
graphify update .    # após editar .cs — 0 tokens de API
```

## Perguntas-modelo (copie no chat)

- `graphify query "how does ARCaptureSession connect to frame selection and coverage?"`
- `graphify query "what calls SplatViewerController and ISplatRenderer?"`
- `graphify path "AppBootstrap" "HomeViewReconstructionProvider"`
- `graphify explain "UnityGaussianSplatAdapter"`

## O que NÃO reler

- `AGENTS.md` / `BUILD.md` — decisões já travadas; só consulte se o usuário pedir mudança de arquitetura
- Pastas inteiras (`Assets/Scripts/**`) — use graphify query + Read só nos arquivos citados
- `Packages/manifest.json` — grafo já mapeia dependências Unity

## Decisões travadas (1 linha cada)

Unity + AR Foundation · reconstrução na nuvem (nunca no device) · backend HomeView · viewer com `.splat` estático no M1 · diffs cirúrgicos por milestone M0–M5
