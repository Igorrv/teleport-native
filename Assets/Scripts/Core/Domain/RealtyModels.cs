using System;
using System.Collections.Generic;

namespace TeleportNative.Core
{
    /// <summary>Um comodo de um imovel, com estado de captura.</summary>
    public sealed class RoomEntry
    {
        public RoomType Type;
        public bool Done;
        public string SpaceId;       // vinculo ao Space reconstruido (Library)
        public DateTime? CapturedAt;
    }

    /// <summary>
    /// Imovel "em construcao": titulo + comodos selecionados, avancados um a um conforme o
    /// corretor captura cada ambiente. Sessao (nao persistido por enquanto; ver Ctx.RealtyDraft).
    /// Logica de avanco pura, testavel em EditMode.
    /// </summary>
    public sealed class RealtyDraft
    {
        public string Title;
        public string Address;
        public readonly List<RoomType> Rooms; // ordem de captura sugerida
        public int Index;                     // proximo comodo a capturar (0-based)

        public RealtyDraft(string title, string address, List<RoomType> rooms)
        {
            Title = title ?? "Imóvel";
            Address = address ?? string.Empty;
            Rooms = rooms ?? new List<RoomType>();
            Index = 0;
        }

        public int Total => Rooms.Count;
        public int Done => Math.Min(Index, Rooms.Count);
        public bool HasNext => Index < Rooms.Count;
        public RoomType NextType => HasNext ? Rooms[Index] : RoomType.Outros;

        /// <summary>"Titulo — Sala" (usado como nome do Space durante a captura).</summary>
        public string NextLabel() =>
            HasNext ? $"{Title} — {RoomCatalog.Of(NextType).Label}" : Title;

        /// <summary>Confirma o comodo atual e avanca para o proximo. No-op se ja concluido.</summary>
        public void Advance()
        {
            if (HasNext) Index++;
        }
    }
}
