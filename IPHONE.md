# Instalar no iPhone — pipeline Xcode-only

> Unity exporta **no Mac** → pasta **`ios/`** no Git → **Codemagic compila IPA** (sem Unity no CI).

Guia completo: **[BUILD_IOS.md](BUILD_IOS.md)**

## Resumo rapido

1. **Mac:** Unity → **Teleport → 8. Export iOS** → `cd ios && pod install`
2. **Git:** `git add ios/ && git push`
3. **Codemagic:** workflow **`ios-ipa`** → baixar `.ipa`
4. **Windows:** `.\scripts\install-iphone.ps1 -IpaPath ...`

Sem `UNITY_LICENSE`, serial, ALF ou ULF no Codemagic.
