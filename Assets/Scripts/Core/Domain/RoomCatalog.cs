namespace TeleportNative.Core
{
    /// <summary>Tipos de comodo do mercado imobiliario brasileiro (catalogo da captura guiada).</summary>
    public enum RoomType
    {
        Sala, Cozinha, Banheiro, Quarto, Suite, Lavanderia, Garagem,
        AreaGourmet, Quintal, Piscina, Fachada, Outros
    }

    /// <summary>
    /// Catalogo de comodos: etiqueta amigavel (PT-BR) e meta minima de fotos para a
    /// reconstrucao 3D de qualidade. Logica pura, testavel em EditMode.
    /// </summary>
    public static class RoomCatalog
    {
        public readonly struct Info
        {
            public readonly RoomType Type;
            public readonly string Label;
            public readonly int MinPhotos;
            public Info(RoomType type, string label, int minPhotos)
            { Type = type; Label = label; MinPhotos = minPhotos; }
        }

        public static readonly Info[] All =
        {
            New(RoomType.Sala,        "Sala",         10),
            New(RoomType.Cozinha,     "Cozinha",       8),
            New(RoomType.Banheiro,    "Banheiro",      6),
            New(RoomType.Quarto,      "Quarto",        8),
            New(RoomType.Suite,       "Suíte",         8),
            New(RoomType.Lavanderia,  "Lavanderia",    5),
            New(RoomType.Garagem,     "Garagem",       8),
            New(RoomType.AreaGourmet, "Área gourmet", 10),
            New(RoomType.Quintal,     "Quintal",       8),
            New(RoomType.Piscina,     "Piscina",      10),
            New(RoomType.Fachada,     "Fachada",       8),
            New(RoomType.Outros,      "Outros",        6),
        };

        /// <summary>Etiqueta curta do comodo (ex.: RoomType.Suite -> "Suíte").</summary>
        public static Info Of(RoomType type)
        {
            foreach (var i in All) if (i.Type == type) return i;
            return New(RoomType.Outros, "Outros", 6);
        }

        private static Info New(RoomType type, string label, int minPhotos) =>
            new Info(type, label, minPhotos);
    }
}
