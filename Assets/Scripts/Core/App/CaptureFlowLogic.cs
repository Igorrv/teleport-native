namespace TeleportNative.Core
{
    /// <summary>Textos e regras de UX da captura AR (logica pura, testavel).</summary>
    public static class CaptureFlowLogic
    {
        public const int MinKeyframes = 5;
        public const float MinCoverage = 0.35f;

        /// <summary>Meta de fotos: catalogo do comodo (realty) ou minimo global.</summary>
        public static int EffectiveMinKeyframes(RoomType? room) =>
            room.HasValue ? RoomCatalog.Of(room.Value).MinPhotos : MinKeyframes;

        public static string Hint(bool hasAr, bool armed, int keyframes, float coverage01, RoomType? room = null)
        {
            int min = EffectiveMinKeyframes(room);
            string roomHint = room.HasValue ? $" ({RoomCatalog.Of(room.Value).Label})" : "";
            if (!hasAr)
                return "Instale no iPhone para usar a camera AR (ARKit).";
            if (!armed)
                return room.HasValue
                    ? $"Aponte para {RoomCatalog.Of(room.Value).Label} e toque Iniciar captura."
                    : "Aponte para o ambiente e toque Iniciar captura.";
            if (keyframes < min)
                return $"Gire devagar — meta: {min}+ fotos{roomHint} (nitidas, boa luz).";
            if (coverage01 < MinCoverage)
                return "Continue girando para cobrir 360 graus do espaco.";
            if (coverage01 >= 0.85f)
                return "Cobertura excelente! Toque Finalizar quando estiver pronto.";
            return "Quase la — complete a volta ao redor do ambiente.";
        }

        public static bool CanFinish(int keyframes, float coverage01, RoomType? room = null) =>
            keyframes >= EffectiveMinKeyframes(room) && coverage01 >= MinCoverage;

        public static string FinishBlockReason(int keyframes, float coverage01, RoomType? room = null)
        {
            int min = EffectiveMinKeyframes(room);
            if (keyframes == 0) return "Nenhuma foto capturada. Gire devagar e tente de novo.";
            if (keyframes < min)
                return $"Minimo {min} fotos (voce tem {keyframes}). Continue girando.";
            if (coverage01 < MinCoverage)
                return "Cobertura baixa — gire mais ao redor do ambiente.";
            return null;
        }
    }
}
