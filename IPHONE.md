# Instalar no iPhone (Windows + iPhone, sem Mac)

> iOS **nao compila no Windows**. Fluxo: **macOS gera .ipa ou Xcode** → **Sideloadly instala no iPhone**.

## O projeto ja e compacto

GitHub: **~470 arquivos (~1 MB)** — sem `Library/`, sem `Build/`.

```powershell
.\scripts\pack-for-mac.ps1
# -> Build\teleport-native-ios-src.zip (~0.15 MB)
```

---

## Por que Codemagic falhou (checklist)

| Problema | Solucao |
|---|---|
| Licenca Unity | Group `unity_credentials`: `UNITY_LICENSE` (.ulf) **ou** `UNITY_SERIAL`+`UNITY_EMAIL`+`UNITY_PASSWORD` |
| Unity 6.5.2f1 | `codemagic.yaml` instala via Unity Hub automaticamente |
| Assinatura iOS | Apple Developer Portal no Codemagic **ou** workflow `ios-xcode-only` + Mac/Xcode |
| Primeiro build lento | Pacote git (Gaussian Splatting) baixa na hora — espere 20-40 min |

---

## Opcao A — Codemagic

1. [codemagic.io](https://codemagic.io) → **Igorrv/teleport-native**
2. Variables group **`unity_credentials`**
3. Workflows:
   - **`ios-sideload`** → IPA
   - **`ios-xcode-only`** → so Xcode (fallback confiavel)
4. `.\scripts\install-iphone.ps1 -IpaPath ...`

---

## Opcao B — GitHub Actions

Secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`  
Actions → **Build iOS IPA** → modo `xcode` ou `ipa`

---

## Opcao C — Mac

Unity 6000.5.2f1 → **Teleport > 8. Preparar export iOS** → Xcode → Run no iPhone

---

## Sideloadly (Windows)

```powershell
.\scripts\install-iphone.ps1 -IpaPath "C:\Downloads\teleport-native.ipa"
```

Ajustes → Geral → VPN e Gerenciamento de Dispositivo → confiar perfil.
