---
name: teleport-native-app
description: Use ao trabalhar no projeto teleport-native (Unity + AR Foundation + UnityGaussianSplatting, app iOS/Android estilo Teleport 360). Convencoes Unity/C#, estrutura, e o fluxo de build/verificacao. Acione ao editar scripts, cenas, pacotes ou config deste app.
---

# teleport-native — convencoes do app Unity

Leia `AGENTS.md` na raiz do projeto para o briefing e milestones. Decisoes ja travadas la (Unity, AR Foundation, UnityGaussianSplatting, recon externa).

## Estrutura
- `Assets/Scripts/Capture` — sessao AR, captura de frames+pose+intrinsics, selecao de keyframes.
- `Assets/Scripts/Network` — cliente da API de reconstrucao (upload, polling, download do splat).
- `Assets/Scripts/Rendering` — controle do viewer de Gaussian Splatting + LOD/perf.
- `Assets/Scripts/Performance` — tiering de device e budget de splats.
- `Packages/manifest.json` — pacotes UPM (AR Foundation 6, ARKit, ARCore, UnityGaussianSplatting via git).

## Convencoes C#/Unity
- Namespaces `TeleportNative.*`. PascalCase para metodos/propriedades, camelCase para campos privados (`_serializeField`).
- Prefira componentes pequenos e testaveis; evite `Update()` pesado (cache de referencias, evite GetComponent no loop).
- Use `UnityWebRequest` (ja no manifest) para rede; async/await com `Awaitable` ou coroutines.
- Nao bloqueie o main thread em IO/decodificacao; use Job System/threads quando pesado.

## UnityGaussianSplatting (Aras-p)
- Pacote `org.nesnausk.gaussian-splatting`. Componente principal: `GaussianSplatRenderer` + `GaussianSplatAsset`.
- Importacao normal de `.ply` -> asset e feita no editor. Para carregar `.splat` em runtime (vindo da nuvem) e preciso um importer de runtime — tratar como tarefa explicita (M1/M3), nao assumir API pronta.
- Perf: sort em GPU, LOD por distancia, budget por tier (ver `Performance/DeviceTier`).

## Build / verificacao (economico)
- Abrir no Unity LTS (fixar versao em `ProjectSettings/ProjectVersion.txt`).
- iOS: build Xcode; precisa `NSCameraUsageDescription` + motion no Info.plist; device com ARKit.
- Android: AAB assinado; permissao CAMERA + ARCore; Min API conforme ARCore.
- Valide M1 (viewer com .splat de exemplo) ANTES de integrar captura/nuvem. Nao reprocessar na nuvem a cada teste.
