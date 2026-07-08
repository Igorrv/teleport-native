# Sideloadly — corrigir erro de certificado (7252)

Erro tipico:
```
There is no 'ios' certificate with serial number '...' on this team
```

## Causa

1. **IPA antigo** em `Downloads` (antes do strip de assinaturas)
2. **Certificado cache** do Sideloadly revogado/expirado na conta Apple
3. Bundle ID truncado (`com.teleportnative.ap` em vez de `com.teleportnative.app`)

---

## Passo 1 — Baixar IPA NOVO (obrigatorio)

**NAO** use `C:\Users\Admin\Downloads\TeleportNative-unsigned.ipa` antigo.

1. https://github.com/Igorrv/teleport-native/actions/workflows/ios-test-sideload.yml
2. Ultimo run **verde** → Artifacts → **`TeleportNative-sideload-ipa`**
3. Extraia para pasta nova, ex: `C:\TeleportBuild\TeleportNative-unsigned.ipa`

Run de referencia (sucesso): https://github.com/Igorrv/teleport-native/actions/runs/28925411702

---

## Passo 2 — Limpar certificados Apple (7252)

1. https://developer.apple.com/account/resources/certificates/list
2. Login: **igor_venhadozzi@icloud.com**
3. Revogue todos **Apple Development** antigos (deixe 0 ou 1)
4. Feche o Sideloadly completamente
5. Apague cache (PowerShell):
   ```powershell
   Remove-Item -Recurse -Force "$env:LOCALAPPDATA\Sideloadly" -ErrorAction SilentlyContinue
   Remove-Item -Recurse -Force "$env:APPDATA\Sideloadly" -ErrorAction SilentlyContinue
   ```
6. Abra Sideloadly de novo → login Apple ID de novo (cria certificado novo)

---

## Passo 3 — Configuracao Sideloadly

| Campo | Valor |
|-------|--------|
| IPA | Arquivo **novo** do artifact GitHub |
| Apple ID | igor_venhadozzi@icloud.com |
| Use automatic bundle ID | **DESMARCADO** |
| Bundle ID | `com.teleportnative.app` (completo!) |
| Cydia Substrate | **DESMARCADO** |
| Change app name | opcional |

---

## Passo 4 — iPhone

1. Cabo USB, confiar no computador
2. Apos instalar: **Ajustes → Geral → VPN e Gerenciamento** → confiar perfil

---

## Alternativa: AltStore

Se Sideloadly continuar falhando:
1. https://altstore.io — AltServer no PC + AltStore no iPhone (mesma rede Wi-Fi)
2. Use o mesmo IPA novo

---

## Disparar build IPA novo

Actions → **iOS Test (Sideloadly)** → Run workflow (~20 min)
