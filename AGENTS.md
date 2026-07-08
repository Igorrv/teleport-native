# teleport-native — App nativo 3D (estilo Teleport 360)

> Briefing canonico do projeto. Leia isto primeiro. Nao re-derive decisoes ja tomadas aqui (economiza tokens). Skills relacionadas: `mobile-native-3d`, `homeview-reference`, `token-economy`.

## Objetivo
App nativo **iOS + Android**, publicavel nas lojas, que captura um ambiente real, reconstroi em **3D Gaussian Splatting** e permite navegacao livre fluida no celular — equivalente ao Teleport (Varjo) / `teleport-360-camera`. Baseado no sistema web existente `HomeView`.

## Decisoes travadas (NAO reabrir sem pedido)
- **Engine/stack**: Unity (LTS) + **AR Foundation** (ARKit no iOS, ARCore no Android) + **UnityGaussianSplatting** (Aras-p) para render GPU dos splats. Um codebase para as duas lojas.
- **Reconstrucao**: **provider externo via API** (Luma / World Labs / Meshy). Treino de 3DGS roda na nuvem, NUNCA no celular.
- **Backend**: reaproveitar o `HomeView` (Express) como API de orquestracao de scan/reconstrucao/storage e contrato `scene`/`splat_url`. Ver skill `homeview-reference`.
- **Local do projeto**: `C:/xampp/htdocs/GLM-5.2/teleport-native`.

## Arquitetura (3 camadas)
1. **Captura nativa** (Unity + AR Foundation): camera + pose (extrinsics) + intrinsics por frame; selecao de keyframes (anti-blur, cobertura). 
2. **Reconstrucao na nuvem** (provider externo): upload frames+poses -> job 3DGS -> recebe `.splat/.ply/.ksplat` + `scene.json`.
3. **Viewer GPU on-device** (UnityGaussianSplatting): carrega o splat, LOD adaptativo, sort em GPU, streaming progressivo. Este e o "motor" de desempenho.

## Contrato de dados (reuso do HomeView)
`scene = { version, type:'walkthrough'|'splat', provider, capture:{fov,ultrawide}, points[], links[], splat_url, measurements, guided_tour, exports }`.
O app nativo consome principalmente `scene.splat_url`. Para o MVP do viewer, usar um `.splat` de exemplo fixo antes de plugar a captura/nuvem.

## Orcamento de desempenho (budget de splats por tier)
- Low (~3GB RAM): ~1.5M splats. Mid: ~4M. High (Pro/flagship): ~8M+.
- LOD por distancia/tela; dynamic resolution; reduzir budget no thermal throttling; pausar render ocioso; nao re-sortear se a camera nao moveu.

## Milestones (ordem; valide cada um antes do proximo)
- **M0 Setup**: projeto Unity, AR Foundation + plugins ARKit/ARCore, UnityGaussianSplatting instalado, build vazio rodando em 1 device.
- **M1 Viewer**: carregar e navegar um `.splat` de exemplo com LOD; atingir 60fps no device alvo. (Ver `Assets/Scripts/Rendering`.)
- **M2 Captura**: sessao AR capturando frames+pose+intrinsics, selecao de keyframes, preview de cobertura. (`Assets/Scripts/Capture`.)
- **M3 Nuvem**: upload p/ provider externo, polling de status, download do splat resultante. (`Assets/Scripts/Network`.)
- **M4 Loop completo**: capturar -> reconstruir -> visualizar dentro do app.
- **M5 Lojas**: permissoes/Info.plist (iOS), data safety (Android), politica de privacidade, TestFlight + Play internal testing.

## Regras de trabalho (token-eficiente)
- Itere o viewer contra um asset `.splat` estatico; nao reprocesse na nuvem a cada teste.
- Reuse backend/contratos do HomeView; nao reescreva o que ja existe.
- Diffs cirurgicos, sem reformatar codigo nao relacionado. Siga a skill `token-economy`.
- Faca commits pequenos e logicos por milestone.

## Pendencias a confirmar com o usuario quando chegar a hora
- Qual provider externo exato (Luma vs World Labs vs Meshy) + chave de API.
- Versao do Unity LTS a fixar.
- Bundle id / Application id e contas de loja (Apple Developer, Google Play Console).

---

## Implementacao atual (codigo completo, pronto p/ abrir no Unity)

Arquitetura limpa em **8 assemblies** (`Assets/Scripts/`, `.asmdef` por modulo):
```
Core          # dominio (SceneContract, Space, ReconstructionStatus), AppFlow (state machine pura),
              # ILogger/IRuntimeConfig/IHaptics, Result<T>, DesignTokens (dark-first), Composition root
Performance   # IDeviceProfiler (substitui o static DeviceTier), SplatBudget, FramePacer, thermal por FPS
Capture       # ICaptureSession/ARCaptureSession (XRCpuImage->JPEG+pose+intrinsics), IFrameSelector,
              # BlurDetector (Laplaciano), CoverageTracker (guia 360) — logica pura testavel
Network       # RestClient (Awaitable + retry/backoff/JSON), IReconstructionProvider + HomeViewReconstructionProvider,
              # ISplatCache, ISceneRepository (library.json), IStorage, ReconstructionClient (orquestra o pipeline)
Rendering     # ISplatViewer/SplatViewerController (motor: budget/LOD/idle/HUD), SplatCameraController (orbit/walk),
              # UnityGaussianSplatAdapter (reflection p/ UnityGaussianSplatting), SplatData (parser .splat)
UI            # UIFactory (uGUI em codigo), componentes, 6 telas (Onboarding/Library/Capture/Processing/Viewer/Share),
              # ScreenManager, AppContext + AppBootstrap (composition root)
Editor/Tests  # BuildMenu (prep publish + config.json), EditMode tests (AppFlow/FrameSelector/SplatBudget/Blur/Coverage/SplatData)
```

Decisoes travadas adicionais (NAO reabrir):
- **UI = uGUI** (Canvas Screen Space-Overlay), UI construida em codigo via UIFactory (design tokens dark-first). Polimento: TextMeshPro + cantos arredondados (fase futura).
- **State machine** `AppFlow` pura/testavel; `ScreenManager` (UI) liga transicoes a telas e alterna rigs (AR x viewer).
- **DI leve** sem framework: `AppContext` (holder unico) montado por `AppBootstrap`; modulos recebem interfaces.
- **Provider default = Luma**, roteado pelo backend HomeView (`/api/scan` -> provider externo). Trocavel por IReconstructionProvider.
- **Runtime splat loader**: o pacote oficial NAO carrega .ply/.splat em runtime (issue #157). Adapter usa reflection; path de **editor (.ksplat) = M1/60fps**; **runtime (.splat) = M3** via SplatData parser + CreateAsset reflection (fallback honesto). Ver `BUILD.md §Runtime Splat Loader`.

Estado dos milestones: M0/M1 a M5 com codigo implementado; falta a **abertura no Unity** (Library/, resolucao do pacote git), montagem da cena "Main" e **validacao no device** (60fps, captura real, cloud). Ver `BUILD.md`.

Riscos tecnicos vigentes: (1) signature exata de `GaussianSplatAsset.CreateAsset` para runtime import; (2) custo por scan (FrameSelector agressivo); (3) thermal (captura+render); (4) backend self-signed inacessivel no device -> gateway publico HTTPS.
