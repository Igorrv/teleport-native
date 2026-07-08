# Build iOS — Unity local + Codemagic (Xcode only)

Pipeline em **duas etapas**: Unity exporta Xcode no **Mac**; Codemagic **so compila IPA** (sem Unity, sem licenca no CI).

---

## 1. Exportar Xcode no Mac (local)

Requisitos:
- macOS
- Unity **6000.5.2f1** + modulo **iOS Build Support**
- Xcode instalado

### Via menu Unity

1. Abrir o projeto no Unity Hub
2. **Teleport → 8. Export iOS (Xcode -> ios/)**

### Via linha de comando

```bash
/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -nographics \
  -projectPath "$(pwd)" \
  -executeMethod TeleportNative.Editor.ExportIosProject.Export \
  -logFile Build/export-ios.log
```

### O que o export faz

| Config | Valor |
|--------|--------|
| Bundle ID | `com.teleportnative.app` |
| Backend | IL2CPP |
| Arquitetura | ARM64 |
| iOS minimo | 15.0 |
| Config | Release |
| Saida | `Build/iOS` → copiado para **`ios/`** |

---

## 2. Pods (no Mac, apos export)

```bash
cd ios
pod install
```

Gera `ios/Unity-iPhone.xcworkspace`.

---

## 3. Enviar para GitHub

```bash
git add ios/
git commit -m "chore(ios): export Xcode para Codemagic"
git push origin main
```

A pasta `ios/` deve conter pelo menos:
- `Unity-iPhone.xcodeproj`
- `Unity-iPhone/`
- `Podfile`
- `ExportOptions.plist`
- `Unity-iPhone.xcworkspace` (apos `pod install`)

---

## 4. Codemagic — workflow `ios-ipa`

1. [codemagic.io](https://codemagic.io) → app **teleport-native**
2. Workflow: **`ios-ipa`**
3. **Sem** variaveis Unity (`UNITY_LICENSE`, serial, etc.)
4. Configurar **Apple Developer Portal** (Team settings) para assinatura development
5. Start build → baixar artefato `*.ipa`

O CI executa apenas:

```bash
cd ios && pod install
xcode-project use-profiles
xcode-project build-ipa --workspace ios/Unity-iPhone.xcworkspace ...
```

---

## 5. Instalar no iPhone (Windows)

```powershell
.\scripts\install-iphone.ps1 -IpaPath "C:\Downloads\teleport-native.ipa"
```

Sideloadly + Apple ID + cabo USB.

---

## Windows (seu PC)

O Unity **no Windows nao exporta iOS**. Opcoes:

- Mac emprestado / Mac na nuvem (MacinCloud, etc.)
- Amigo com Mac roda o export (5–15 min)
- Pacote leve do codigo: `.\scripts\pack-for-mac.ps1` → enviar ZIP para quem tem Mac

---

## Troubleshooting

| Erro | Acao |
|------|------|
| `ios/Unity-iPhone.xcodeproj nao encontrado` | Rode export no Mac e de push em `ios/` |
| `pod install` falha | Xcode Command Line Tools: `xcode-select --install` |
| Codemagic signing | Vincule Apple ID em Team settings |
| Export falha no Mac | XR Plug-in Management → ARKit habilitado (iOS) |

---

## Arquivos do pipeline

| Arquivo | Funcao |
|---------|--------|
| `Assets/Scripts/Editor/TeleportNativeIosExport.cs` | Export local |
| `ios/Podfile` | CocoaPods iOS 15+ |
| `ios/ExportOptions.plist` | IPA development |
| `codemagic.yaml` | Workflow `ios-ipa` (Xcode only) |
