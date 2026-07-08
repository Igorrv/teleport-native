# Graphify auto-improve — teleport-native

> Rodado automaticamente antes de cada ciclo de melhoria.
> Grafo: `graphify-out/graph.json` · ~609 nós

## Prompt (copie ou use via `scripts/graphify-auto-improve.ps1`)

```
Projeto: teleport-native (Unity 6 + AR Foundation + Gaussian Splatting)
Contexto: graphify-out/NAVIGATION.md + graphify query — NÃO ler o repo inteiro
Milestone: M1–M2 (viewer + captura UX)

Tarefa graphify-first:
1. graphify query "what screens lack error handling or user feedback?"
2. graphify path "CaptureScreen" "SplatViewerController"
3. graphify explain "ARCaptureSession"

Melhorias alvo (diff mínimo):
- Feedback visível em Capture/Viewer/Processing (toast, banner, validação)
- Modo demo no Editor para testar viewer sem AR device
- Script run-local.ps1 + menu Teleport > 7 Build Windows

Restrições: não reabrir decisões do AGENTS.md; graphify update . após editar .cs
```

## Resultado desta execução (2026-07-07)

| Melhoria | Arquivo |
|---|---|
| Toast de validação (min 5 fotos, cobertura 35%) | `CaptureScreen.cs` |
| Banner carregamento/erro no viewer | `ViewerScreen.cs` |
| Botão Demo viewer (Editor) | `LibraryScreen.cs` |
| Componente Toast reutilizável | `ToastView.cs` |
| Build Windows local | `LocalRunMenu.cs` + `scripts/run-local.ps1` |

## Próximo ciclo (sugestão graphify)

```bash
graphify query "what connects ProcessingScreen to ReconstructionClient errors?"
graphify affected "UnityGaussianSplatAdapter" --depth 2
```
