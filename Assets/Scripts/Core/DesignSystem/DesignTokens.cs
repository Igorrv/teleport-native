using UnityEngine;

namespace TeleportNative.Core
{
    /// <summary>
    /// Design tokens premium (dark-first), identidade PROPRIA (nao Teleport). Inspirado em
    /// Apple/Linear/Arc/Notion/Airbnb: neutros generosos, acentos restritos, hierarquia forte.
    /// Edite aqui para trocar o tema em todo o app. UI em codigo le isto (ver UIFactory).
    /// </summary>
    public static class DesignTokens
    {
        // ---- Paleta (dark-first) ----
        public static readonly Color Background      = Hex("#0A0B10"); // quase-preto, tom frio sutil
        public static readonly Color BackgroundGrad  = Hex("#11131B"); // stop de gradiente ambiente
        public static readonly Color Surface         = Hex("#14161F");
        public static readonly Color SurfaceRaised   = Hex("#1C1F2B");
        public static readonly Color SurfaceHover    = Hex("#232735");
        public static readonly Color Primary         = Hex("#6D5DFB"); // indigo-violeta (identidade)
        public static readonly Color PrimarySoft     = new(0.43f, 0.36f, 0.98f, 0.16f);
        public static readonly Color Accent          = Hex("#FFB23E"); // ambre (destaques/CTA premium)
        public static readonly Color Text            = Hex("#F4F6FB");
        public static readonly Color TextSecondary   = Hex("#C3C9D6");
        public static readonly Color TextMuted       = Hex("#8A91A3");
        public static readonly Color Success         = Hex("#34D399");
        public static readonly Color Warning         = Hex("#FBBF24");
        public static readonly Color Danger          = Hex("#FB7185");
        public static readonly Color Overlay         = new(0.03f, 0.04f, 0.07f, 0.62f);
        public static readonly Color Divider         = new(1, 1, 1, 0.07f);

        // ---- Glassmorphism (discreto; blur real exige pos-process, aqui translucido + highlight) ----
        public static readonly Color Glass           = new(1, 1, 1, 0.06f);
        public static readonly Color GlassStrong     = new(1, 1, 1, 0.10f);
        public static readonly Color GlassBorder     = new(1, 1, 1, 0.12f);
        public static readonly Color GlassHighlight  = new(1, 1, 1, 0.18f);

        private static bool IsPhone =>
            Application.isMobilePlatform || (Screen.width < Screen.height && Screen.width <= 520);

        // ---- Tipografia (maior no celular) ----
        public static float DisplaySize  => IsPhone ? 38f : 34f;
        public static float TitleSize    => IsPhone ? 30f : 26f;
        public static float HeadingSize  => IsPhone ? 22f : 19f;
        public static float BodySize     => IsPhone ? 18f : 16f;
        public static float CaptionSize  => IsPhone ? 15f : 13f;
        public static float MicroSize    => IsPhone ? 12f : 11f;

        // ---- Espacamento ----
        public const float SpaceXS  = 4f;
        public const float SpaceS   = 8f;
        public const float SpaceM   = 16f;
        public const float SpaceL   = 24f;
        public const float SpaceXL  = 40f;
        public const float SpaceXXL = 64f;

        // ---- Raios ----
        public const float RadiusS   = 10f;
        public const float RadiusM   = 16f;
        public const float RadiusL   = 24f;
        public const float RadiusXL  = 32f;
        public const float RadiusPill = 999f;

        // ---- Componentes ----
        public static float ButtonHeight => IsPhone ? 60f : 54f;
        public static float TouchTarget  => IsPhone ? 52f : 48f;
        public const float CardHeight   = 100f;
        public const float HeaderHeight = 100f;

        // ---- Movimento (segundos) ----
        public const float MotionFast   = 0.14f;
        public const float MotionNormal = 0.26f;
        public const float MotionSlow   = 0.42f;

        public static Color Hex(string h) =>
            ColorUtility.TryParseHtmlString(h, out var c) ? c : Color.magenta;
    }
}
