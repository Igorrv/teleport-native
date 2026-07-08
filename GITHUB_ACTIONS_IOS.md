# GitHub Actions — Build iOS (IPA)

Pipeline: **GitHub → macOS runner → Unity 6000.5.2f1 → Xcode → IPA artifact**

Arquivo: `.github/workflows/ios-build.yml`

---

## Análise do projeto (iOS)

| Item | Valor |
|------|--------|
| Unity | **6000.5.2f1** (`eb73d3b415a1`) |
| Bundle ID | `com.teleportnative.app` |
| iOS mínimo | 15.0 |
| Backend scripting | IL2CPP + ARM64 |
| Cena principal | `Assets/Main.unity` |

### Dependências nativas / XR

| Pacote | Uso |
|--------|-----|
| `com.unity.xr.arkit` 6.5.0 | ARKit (câmera AR) |
| `com.unity.xr.arfoundation` 6.5.0 | AR Foundation |
| `com.unity.xr.management` | XR plug-in loader |
| `org.nesnausk.gaussian-splatting` (git) | Viewer splats — **requer internet no CI** |
| URP 17.5.0 | Render pipeline |

### Frameworks iOS (via ARKit / Unity)

- **ARKit**, **AVFoundation**, **CoreMotion** (câmera + tracking)
- Permissão câmera: `cameraUsageDescription` em ProjectSettings

### Correções aplicadas

- Removidos `com.unity.ai.assistant` e `com.unity.ai.inference` (não usados, pre-release)
- Entry point CI: `TeleportNative.Editor.BuildiOS.Build`
- Export: `ExportIosProject` → pasta `ios/`

### Riscos conhecidos

| Risco | Mitigação |
|-------|-----------|
| Pacote git Gaussian Splatting | Runner precisa de rede; primeiro build ~40–90 min |
| Licença Unity | Secrets `UNITY_LICENSE` + email/password |
| IPA no device | Fase 1: IPA **unsigned** → Sideloadly assina no Windows |
| ARKit no simulador | Teste real só em iPhone físico |

---

## Secrets no GitHub

Repositório → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

### Obrigatórias (Unity)

| Secret | Como obter |
|--------|------------|
| `UNITY_LICENSE` | Rode `.\scripts\setup-unity-license.ps1` → copie **todo** o `.ulf` |
| `UNITY_EMAIL` | Email da conta Unity (Personal grátis) |
| `UNITY_PASSWORD` | Senha da conta Unity |

> **Não** precisa de serial/key — conta Personal basta.

### Opcionais (IPA já assinado)

| Secret | Conteúdo |
|--------|----------|
| `APPLE_CERTIFICATE` | Arquivo `.p12` codificado em **base64** |
| `APPLE_CERTIFICATE_PASSWORD` | Senha do `.p12` |
| `APPLE_PROVISIONING_PROFILE` | `.mobileprovision` em **base64** |
| `KEYCHAIN_PASSWORD` | Senha aleatória para keychain temporário |

---

## Executar o workflow

1. [github.com/Igorrv/teleport-native/actions](https://github.com/Igorrv/teleport-native/actions)
2. **iOS Build** → **Run workflow**
3. **signed**: `false` (padrão) → IPA unsigned para Sideloadly
4. Aguarde conclusão (~40–90 min no 1º run)

---

## Baixar o IPA

1. Workflow concluído (verde)
2. **Summary** → **Artifacts**
3. Baixe `teleport-native-unsigned-ipa` ou `teleport-native-signed-ipa`
4. Extraia o `.ipa`

---

## Instalar no iPhone (Windows)

```powershell
cd C:\xampp\htdocs\GLM-5.2\teleport-native
.\scripts\install-iphone.ps1 -IpaPath "C:\Downloads\TeleportNative-unsigned.ipa"
```

- Cabo USB + Sideloadly + Apple ID
- iPhone: confiar perfil em Ajustes → Geral → VPN e Gerenciamento

---

## Build local (Mac)

```bash
chmod +x scripts/build_ios.sh
./scripts/build_ios.sh
```

Ou:

```bash
Unity -batchmode -quit -executeMethod TeleportNative.Editor.BuildiOS.Build
```

---

## Próximos passos

1. Configurar secrets Unity no GitHub
2. Rodar workflow **iOS Build**
3. Baixar artifact e instalar via Sideloadly
4. (Opcional) Adicionar secrets Apple e rodar com **signed = true**
