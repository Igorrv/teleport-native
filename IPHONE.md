# Instalar no iPhone (Windows + iPhone, sem Mac)

> iOS **nao compila no Windows**. Fluxo: **CI macOS gera .ipa** → **Sideloadly instala no iPhone**.

## Status do projeto

- Build Windows OK: `Build\Windows\teleport-native.exe` (testar UI no PC)
- Build iOS: requer macOS (Codemagic / GitHub Actions / Mac)

## Passo a passo (15–30 min)

### 1. Gerar o `.ipa` na nuvem (Codemagic — recomendado)

1. Crie conta em [codemagic.io](https://codemagic.io)
2. **Add application** → conecte o repositório git deste projeto
3. O arquivo `codemagic.yaml` já está na raiz (workflow `ios-sideload`)
4. Em **Team settings → Integrations → Apple Developer Portal**: vincule seu Apple ID
5. Rode o build → baixe o artefato `*.ipa`

### 2. Instalar no iPhone (Windows)

```powershell
cd C:\xampp\htdocs\GLM-5.2\teleport-native
.\scripts\install-iphone.ps1 -IpaPath "C:\Downloads\teleport-native.ipa"
```

- Conecte o iPhone via **USB**
- Desbloqueie e toque **Confiar neste computador**
- No Sideloadly: informe seu **Apple ID** (conta gratuita funciona; app expira em 7 dias)

### 3. Confiar no app (iPhone)

**Ajustes → Geral → VPN e Gerenciamento de Dispositivo** → confie no perfil de desenvolvedor.

## Alternativa: Mac emprestado

```
Unity > Teleport > 8. Preparar export iOS
Xcode > Run no iPhone conectado
```

## Graphify

```powershell
graphify query "iOS build iPhone Windows sideload" --graph graphify-out\graph.json
```
