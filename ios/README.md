# Pasta ios/ — projeto Xcode para Codemagic

Esta pasta e **preenchida pelo Unity no Mac** (nao commite vazia para CI).

## Gerar conteudo

No **Mac** com Unity 6000.5.2f1 + modulo iOS:

```
Unity > Teleport > 8. Export iOS (Xcode -> ios/)
```

Ou batchmode:

```bash
/Applications/Unity/Hub/Editor/6000.5.2f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -projectPath . \
  -executeMethod TeleportNative.Editor.ExportIosProject.Export \
  -logFile Build/export-ios.log
```

Depois:

```bash
cd ios && pod install
git add ios/
git commit -m "chore(ios): export Xcode"
git push
```

Codemagic workflow **ios-ipa** compila o IPA (sem Unity).

Veja **BUILD_IOS.md** na raiz do projeto.
