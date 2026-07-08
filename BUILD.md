# BUILD.md — teleport-native (montagem, validacao e publicacao)

Este projeto e **codigo-completo**, mas partes exigem o **Unity Editor** e um **dispositivo** que nao rodam num terminal. Este guia cobre o caminho do "abrir no Unity" ate a loja.

---

## 0. Fronteiras honestas (o que NAO foi feito aqui)
Nao foi possivel neste ambiente (CLI Windows, sem Unity/dispositivo):
- Gerar `Library/` e resolver o pacote git `UnityGaussianSplatting` (o Unity faz no 1o open).
- Compilar/validar reflection contra a API real do pacote (ver §Runtime Splat Loader).
- Rodar captura AR, reconstrucao na nuvem e medir 60fps no device.
- Assinar/fazer upload para TestFlight/Play.

Tudo isso esta preparado no codigo; falta a passada no Unity/dispositivo abaixo.

---

## 1. Abrir no Unity
1. Unity Hub > **Add > Add project from disk** > esta pasta.
2. Confirme a versao `6000.0.x` (`ProjectSettings/ProjectVersion.txt`). Se o seu patch diferir, ajuste o arquivo ou use **Switch version** no Hub.
3. Abrir. O Unity resolve `Packages/manifest.json`: AR Foundation 6 + ARKit + ARCore + **UnityGaussianSplatting** (git UPM) + uGUI + test-framework. **Precisa de internet** para o pacote git.
4. Se pedir **Active Input Handling**: escolha **Both** (a camera do viewer usa o `Input` classico; o AR Foundation usa o novo). Em *Project Settings > Player > Active Input Handling = Both*.
5. Aguarde a compilacao. Deve compilar limpo (os 8 assemblies). Eventuais erros de nome de referencia em algum `.asmdef` sao de 1 linha (ver §8).

## 2. XR + Player Settings
1. **Project Settings > XR Plug-in Management > Install**. Habilite **ARKit** (aba iOS) e **ARCore** (aba Android).
2. Menu **Teleport > 1. Preparar iOS** e **Teleport > 2. Preparar Android** (preenche bundle id, camera usage, min SDK).
3. **Teleport > 3. Gerar Resources/config.json**. Edite `Assets/Resources/config.json`:
   - `BackendBaseUrl`: seu gateway HTTPS publico do HomeView.
   - `ReconstructionProvider`: `luma` (ou `worldlabs`/`meshy`).
   - `ProviderApiKey`: sua chave. *(Nao versione a chave real; em prod use um endpoint de auth.)*
4. Render Pipeline: o projeto usa **URP**. Garanta que a camera do viewer usa o renderer URP (AR Camera background: componente `ARCameraBackground`).

## 3. Cena "Main" (obrigatorio montar)
**Atalho:** rode o menu **Teleport > 4. Montar cena Main (auto)**. Ele cria `Assets/Main.unity` com
`AppBootstrap` + `EventSystem` + `Canvas` + Viewer Rig + AR Rig e ja vincula a maioria das referencias
do `AppBootstrap` (UI Root, Viewer, Camera, AR Rig, Viewer Rig). Falta apenas completar o **AR Rig**
(XR Origin) e o **Splat Host** (asset de exemplo) — ver o log que o menu imprime no Console.

Alternativamente, monte a mao a hierarquia:

```
Main
├─ AppBootstrap            (UI/Composition/AppBootstrap) — o entry point
├─ EventSystem             (uGUI; criar com UI > Event System)
├─ Canvas                  (Screen Space - Overlay, Canvas Scaler: Scale With Screen Size 1080x1920, Match 0.5)
│     (as 6 telas sao criadas em runtime pelo ScreenManager, filhas deste Canvas)
├─ AR Rig (GameObject)     <- ativa so na tela Capture
│     ├─ AR Session
│     ├─ AR Session Origin (com AR Camera + ARCameraManager + ARCameraBackground + Camera)
│     └─ [ARCaptureSession]  no AR Camera ( componente Capture/ARCaptureSession )
└─ Viewer Rig (GameObject) <- ativa so na tela Viewer
      ├─ Camera (Camera URP)
      ├─ SplatCameraController (no Camera; alvo _target = Splat Host)
      └─ Splat Host (Transform vazio)
            └─ [GaussianSplatRenderer]  (componente do pacote Aras-p; adicione depois do pacote resolver)
```

No **AppBootstrap** (Inspector), arraste:
- `UI Root` = o **Canvas**.
- `Capture` = o **ARCaptureSession**.
- `Viewer` = o **SplatViewerController** (adicione o componente `Rendering/SplatViewerController` ao Camera do Viewer Rig; nele, `Splat Host` = o Splat Host). *Adicione tambem `SplatCameraController` ao mesmo Camera.*
- `Camera` = o **SplatCameraController** acima; no `SplatCameraController`, `Target` = o **Splat Host**.
- `AR Rig` = o GameObject **AR Rig**; `Viewer Rig` = o GameObject **Viewer Rig**.

Coloque **Main** em **Build Settings** (unica cena) e marque **iOS** ou **Android** como plataforma.

---

## 4. Runtime Splat Loader (o ponto critico — ler com atencao)
O pacote oficial do Aras **nao importa `.ply`/`.splat` em runtime** (issue #157). O app tem **dois caminhos**:

### 4a. M1 — editor asset (validar 60fps AGORA)
1. Importe um splat de exemplo pelo menu do pacote (`.ply`/`.splat`/`.ksplat`) -> gera um **GaussianSplatAsset** no projeto.
2. No **SplatViewerController**, arraste esse asset para o campo **`Editor Splat Asset`**.
3. Adicione `GaussianSplatRenderer` ao **Splat Host** (pacote) e, no Start, o adapter atribui o asset por reflection.
4. Rode no device: **Library > (espaco de teste) > Viewer** e meça **60fps** no HUD. Ajuste `SplatBudget`/LOD pelo tier.

### 4b. M3 — runtime (.splat baixado da nuvem)
`UnityGaussianSplatAdapter.LoadFromPathAsync` parseia `.splat` (`SplatData`) e tenta criar o asset via `GaussianSplatAsset.CreateAsset` por **reflection**. Como a assinatura varia entre versoes do pacote:
- Se a versao instalada do pacote tiver `CreateAsset(...)` publica com a assinatura esperada (`int count, NativeArray pos/scale/rot/color...`), funciona.
- Caso contrario, o adapter devolve `Fail` com mensagem clara. Solucao recomendada: **converta para `.ksplat` no editor (ou no backend)** e ajuste UM metodo (`TryCreateRuntimeAsset`) para a API da sua versao, ou use um **fork** que ja importe em runtime (ver issue #157 / soumi-akira fork).

> A arquitetura isola todo o pacote em `UnityGaussianSplatAdapter`; o restante do app nao muda ao trocar a estrategia de carga.

---

## 5. Backend (HomeView) acessivel pelo device
- O `https://SEU:3443` **self-signed** do HomeView **nao funciona** em iOS/Android (sem certificado confiavel). Exponha um **gateway HTTPS publico com cert valido** (Let's Encrypt / reverse proxy / tunel) apontando para o HomeView e ponha a URL em `config.json > BackendBaseUrl`.
- Confirme os endpoints usados: `POST /api/scan` (multipart), `GET /api/scan/{id}/status`, `GET /api/scan/{id}/result` (devolve `scene.splat_url`). Ajuste em `HomeViewReconstructionProvider` se o seu HomeView diferir.

## 6. Validacao no device (M1 -> M4)
- Build pra **1 plataforma** (iOS recomendado p/ estabilidade ARKit). Plugue o device, Build and Run.
- M1: viewer a 60fps com asset de exemplo (§4a).
- M2: Capture -> gera keyframes (veja contagem/cobertura no overlay).
- M3: Processing -> se o loader runtime (§4b) nao estiver pronto, valide o **fluxo** apontando o resultado para um `.ksplat` de teste; valide 60fps do splat real depois.
- Termico: deixe rodar 5-10 min; o `DeviceProfiler` deve reduzir budget/FPS e manter estavel.

## 7. Publicar (M5)
**iOS**
- Player Settings > Signing: Time/Provisioning (conta Apple Developer).
- `Info.plist`: `NSCameraUsageDescription` (ja setado pelo menu) + `NSMotionUsageDescription` (motion) — adicione manualmente se o menu nao cobrir.
- Build -> Xcode -> archive -> **TestFlight**.
- App Privacy: declare uso de camera e dados (ambientes capturados). Politica de privacidade obrigatoria.

**Android**
- **Build System = Gradle**, **Export = AAB**. Keystore assinado (Player > Publishing Settings).
- ARCore `"Required"` + permissao `CAMERA` (o pacote ARCore ja adiciona).
- Google Play Console: **Data safety form**, politica de privacidade, **internal testing**.

## 7b. Testar no iPhone (sem Mac proprio)
iOS **so compila em macOS** (regra da Apple). Se voce so tem iPhone + Windows, o Mac precisa entrar na cadeia de alguma forma:

**Caminho A — Mac de emprestado/na nuvem (mais simples p/ teste)**
1. Num Mac com **Unity 6 LTS (modulo iOS)** + **Xcode**, abra o projeto e rode **Teleport > 6. Build iOS** (gera `Build/Xcode`).
2. Abra `Build/Xcode` no Xcode, em **Signing & Capabilities** escolha seu **Apple ID** (Time Pessoal, gratuito).
3. Plugue o iPhone (USB) ou emparelhe **wireless** (mesma rede); selecione o iPhone no topo e **Run**. O app instala e abre.
4. No iPhone: **Ajustes > Geral > VPN e Gerenciamento de Dispositivo** > confie no seu perfil de desenvolvedor.

**Caminho B — sem Mac algum: CI na nuvem + Sideloadly (Windows)**
1. Use um CI com runner macOS (ex.: **Codemagic**, **Bitrise** ou **GitHub Actions**) com Unity 6 LTS: ele builda o **IPA nao assinado** (ou assinado) e voce baixa no Windows.
2. No Windows, instale **Sideloadly**, abra o IPA + seu **Apple ID gratuito** -> ele assina e instala no iPhone via cabo USB.
3. Confie no perfil (passo 4 acima). Conta gratuita: app expira em **7 dias**, max. 3 apps; renove reaplicando o IPA por semana.
   - Com conta **Apple Developer paga ($99/ano)**: instala Ad Hoc com o **UDID** do iPhone e o app dura 1 ano.

> O iPhone 17 Pro Max e um dos melhores dispositivos de teste para este app (ARKit de ponta): a captura AR e o viewer a 60fps rodarao otimo. A unica barreira e o Mac para compilar.

## 8. Troubleshooting rapido
- **Erro de asmdef "reference not found"** (ex.: `Unity.XR.ARFoundation`): confira o nome da referencia no `.asmdef`; nomes mudam entre versoes.
- **Captura nao gera keyframes**: o AR Camera precisa de `ARCameraManager` + AR ativo + luz suficiente; `TryAcquireLatestCpuImage` exige suporte da plataforma (nao funciona no Editor sem webcam AR).
- **Viewer preto**: `GaussianSplatRenderer` sem asset, ou camera URP sem `ARCameraBackground` removido no viewer. Use o caminho de editor (§4a).
- **Input nao responde no device**: confirme *Active Input Handling = Both*.
