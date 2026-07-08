# GitHub Actions — Build iOS (IPA assinado)

Pipeline: **Unity 6000.5.2f1 → Xcode → archive → exportArchive → TeleportNative.ipa**

Arquivo: `.github/workflows/ios-build.yml`

---

## Por que o erro 0xe8008019?

O IPA anterior era **unsigned** (zip manual do Payload). O iPhone exige assinatura válida em:

- App principal
- `UnityRuntime.framework`
- `UnityFramework.framework`
- Demais frameworks embarcados

A pipeline agora usa o fluxo oficial Apple: `xcodebuild archive` + `xcodebuild -exportArchive` + validação `codesign --verify`.

---

## Secrets Unity (obrigatórias)

| Secret | Como obter |
|--------|------------|
| `UNITY_LICENSE` | Conteúdo de `C:\ProgramData\Unity\Unity_lic.ulf` |
| `UNITY_EMAIL` | Email conta Unity |
| `UNITY_PASSWORD` | Senha conta Unity |

---

## Secrets Apple (obrigatórias para IPA instalável)

Repositório → **Settings → Secrets and variables → Actions → New repository secret**

### 1. `APPLE_CERTIFICATE_BASE64`

Certificado **Apple Development** (.p12):

1. Mac: Keychain Access → certificado "Apple Development: …" → Exportar → `.p12`
2. Windows (sem Mac): [developer.apple.com](https://developer.apple.com) → Certificates → baixe e converta, ou use Mac emprestado uma vez
3. Encode:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\caminho\dev.p12"))
```

Ou: `.\scripts\setup-apple-secrets.ps1 -CertPath "..." -ProfilePath "..."`

Cole **toda** a string base64 na secret.

### 2. `APPLE_CERTIFICATE_PASSWORD`

Senha definida ao exportar o `.p12`.

### 3. `APPLE_PROVISIONING_PROFILE_BASE64`

Profile **iOS App Development** para `com.teleportnative.app`:

1. [developer.apple.com](https://developer.apple.com) → Profiles → **+** → iOS App Development
2. App ID: `com.teleportnative.app` (habilite ARKit se pedido)
3. Selecione seu certificado Development
4. Baixe `.mobileprovision`
5. Encode:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\caminho\Teleport.mobileprovision"))
```

### 4. `APPLE_TEAM_ID`

Team ID de 10 caracteres (ex: `AB12CD34EF`):

- [developer.apple.com/account](https://developer.apple.com/account) → Membership → **Team ID**

### 5. `APPLE_SIGNING_IDENTITY`

Nome **exato** do certificado no Keychain:

```
Apple Development: Seu Nome (XXXXXXXXXX)
```

No Mac: Keychain Access → My Certificates → copie o nome completo.

### 6. `KEYCHAIN_PASSWORD`

String aleatória para keychain temporária do CI (ex: `ci-keychain-a8f3b2c1`). Não precisa ser a senha Apple.

---

## Executar o workflow

1. Configure **todas** as secrets acima
2. [Actions → iOS Build → Run workflow](https://github.com/Igorrv/teleport-native/actions)
3. Aguarde ~20–90 min
4. Baixe artifact **`TeleportNative-ipa`** → `TeleportNative.ipa`

---

## Instalar no iPhone

IPA **já assinado** — instale via:

- **Xcode** → Window → Devices → drag IPA (Mac)
- **Apple Configurator** (Mac)
- **TestFlight** (futuro, conta paga)

No Windows, perfil Development expira em 7 dias; reinstale ou use TestFlight.

---

## Validação no CI

O script `scripts/ios/sign-and-export.sh` executa:

```bash
codesign --verify --deep --strict --verbose=2 Payload/*.app
codesign --verify --verbose=2 Payload/*.app/Frameworks/UnityRuntime.framework
```

Se falhar, o workflow para antes de publicar o artifact.

---

## Arquivos do pipeline

| Arquivo | Função |
|---------|--------|
| `Assets/Scripts/Editor/IosCiPostProcessor.cs` | Team ID + signing no Xcode exportado |
| `Assets/Scripts/Editor/TeleportNativeIosExport.cs` | Export Unity → ios/ |
| `scripts/ios/sign-and-export.sh` | Keychain, archive, export, validação |
| `ios/ExportOptions.plist` | Template development (CI preenche team/profile) |
