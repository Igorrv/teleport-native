# PRODUCT.md — Guia para construir um app premium de Tour Virtual Imobiliário

> Inspirado **apenas no conceito de captura simplificada** do Teleport 360. **Não copiamos UI/design.**
> Produto SaaS premium, foco em **imobiliárias/corretores/construtoras/arquitetos** no Brasil (e internacional).
> Este doc = "instruções para criar um app parecido e superior".

---

## 1. Visão & posicionamento
- **Um corretor escaneia o imóvel inteiro só com o iPhone** → a plataforma gera um **tour virtual profissional** (3D + 360°) + **página pública** + **compartilhamento** (link/QR/embed) em poucos toques.
- Diferencial de mercado: **IA imobiliária** (descrição, staging, remoção de objetos, HDR), **planta baixa/medidas via LiDAR**, **analytics por cômodo**, **integração com CRMs** e **geração de leads**.
- Acabamento visual: **minimalista premium** (Apple/Linear/Arc/Notion/Airbnb) — dark mode, glassmorphism discreto, cards arredondados, animações fluidas.

## 2. Design system (já iniciado em `DesignTokens.cs`)
- **Paleta própria** (não a azul do Teleport): background `#0A0B10`, **Primary indigo-violeta `#6D5DFB`**, **Accent âmbar `#FFB23E`**, neutros generosos, semânticas (success/warning/danger).
- **Glass**: translucido + highlight (`Glass/GlassStrong/GlassBorder`). *(Blur real = pós-processado URP; o atual aproxima com cor translúcida.)*
- **Tipografia**: Display 34 / Title 26 / Heading 19 / Body 16 / Caption 13 / Micro 11.
- **Espaçamento**: 4/8/16/24/40/64. **Raios**: 10/16/24/32 + pill.
- **Movimento**: fast 0.14 / normal 0.26 / slow 0.42 (ease cubic-out). Ver `UITween.Pop`.
- **Primitivos em código** (`UIFactory`): `Card` (arredondado + elevação), `Glass`, botões pílula, texto com fit/wrap. Cantos arredondados gerados por **SDF → sprite 9-slice em runtime** (sem assets).

## 3. Fluxo do usuário (IA premium)
```
Login (Google/Apple/email) → Dashboard/Lista de imóveis → + Novo imóvel
  → Dados do imóvel (endereço, tipo, preço, dorms, banhos, vagas, área)
  → Captura guiada dos ambientes (1 ambiente por vez, guiado)
  → Processamento automático (nuvem, assíncrono)
  → Tour gerado (3D + 360° + miniaturas + descrição IA)
  → Compartilhar (link / QR / embed / WhatsApp / Agendar visita)
  → Dashboard de métricas (visualizações, leads, cômodos mais vistos)
```

## 4. Captura guiada (coração do produto)
- **HUD de captura**: barra de progresso, **indicador de estabilidade**, **nível eletrônico**, **feedback por vibração** (haptics), e **IA de qualidade em tempo real** (avisos de pouca luz / tremor / cobertura).
- **Ambientes padrão** (selecionáveis): Sala, Cozinha, Banheiro, Quarto, Suíte, Lavanderia, Garagem, Área Gourmet, Quintal, Piscina, Fachada, Outros.
- **Meta: < 1 min por ambiente**; o app guia **exatamente onde apontar**. Reaproveitar `ARCaptureSession` + `FrameSelector` (nitidez/pose) + `CoverageTracker`.

## 5. Recursos premium (diferenciais)
- **IA automática**: descrição do imóvel, pontos fortes, **remoção de objetos pessoais** (roupas/fios/brinquedos), **home staging virtual** (móveis em vazio), HDR, denoise, sharpen, correção de perspectiva.
- **Planta baixa automática + medições** (LiDAR no iPhone Pro / fallback visão computacional): paredes, portas, áreas (m²).
- **Tour 3D (Gaussian Splatting) + Tour 360°** no mesmo imóvel; hotspots clicáveis, vídeo e objetos 3D embutidos.
- **Exportações**: link, **QR Code**, **iframe** p/ sites, **PDF com QR**.

## 6. Página pública do imóvel (conversão)
Tour em tela cheia + Fotos + Vídeo + **Planta** + Mapa + **botão WhatsApp** + **Agendar visita** + infos completas + Compartilhar + QR.

## 7. Dashboard (analytics p/ imobiliária)
Qtd. imóveis, qtd. tours, **visualizações**, **tempo médio de visita**, **origem dos acessos**, imóveis mais visitados, compartilhamentos, **leads gerados**, e (diferencial) **atenção por cômodo** + taxa de conclusão do tour.

## 8. Arquitetura (escala SaaS)
- **Clean Architecture modular** (já há 8 asmdef: Core/Performance/Capture/Network/Rendering/UI/Editor/Tests).
- **Multi-tenant**: cada imobiliária = tenant; usuários com papéis (admin/corretor).
- **Backend desacoplado** (API REST) + **auth segura** (OAuth/JWT, refresh) + **storage em nuvem** (S3/R2 p/ splats e mídia) + **fila de processamento assíncrono** (captura → reconstrução → tour).
- **Reconstrução** via provedor (Luma/worldlabs/meshy) isolada por interface (`IReconstructionProvider`) — troca sem tocar o app.
- **Performance mobile**: `DeviceProfiler` (tier/thermal) + `FramePacer` + budget de splats por dispositivo → 60 fps.

## 9. Roadmap em fases (do MVP à escala)
| Fase | Escopo | Reaproveitar do atual |
|---|---|---|
| **MVP (2–4 sem)** | Login + Lista/Criar imóvel + Captura guiada (1 ambiente) + Reconstrução + Viewer 3D + Link/QR + Página pública simples | ARCaptureSession, ReconstructionClient, SplatViewer, AppFlow, DesignTokens/UIFactory |
| **Growth (4–8 sem)** | IA (descrição/HDR/remoção), planta/medidas (LiDAR), dashboard analytics, WhatsApp/Agendar, embed, hotspots/vídeo | — |
| **Scale (8+ sem)** | Multi-tenant + papéis, CRM, Google Business/Street View, feed social, home staging IA, billing/planos | — |

> **Recomendação:** construir o **MVP primeiro** e validar com 1–2 imobiliárias reais antes do Growth/Scale. O atual projeto (capture→splat→viewer) já cobre o núcleo técnico do MVP.

## 10. Compliance & segurança
- LGPD/GDPR: consentimento de captura, dados de ambientes (podem conter pessoas/objetos pessoais) → retenção/exclusão.
- Não versionar chaves (já em `config.json`/PlayerPrefs; em prod via endpoint de auth).
- HTTPS público obrigatório p/ o backend acessível do device.

---
*Este doc é o brief canônico do produto. Detalhes de build/publicação: `BUILD.md` (iOS/Android) e `IPHONE.md` (instalação no iPhone).*
