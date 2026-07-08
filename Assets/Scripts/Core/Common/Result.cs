namespace TeleportNative.Core
{
    /// <summary>Resultado de operacoes que podem falhar (rede/IO), sem lancar excecao no caminho feliz.</summary>
    public readonly struct Result<T>
    {
        public readonly T Value;
        public readonly string Error;
        public bool IsSuccess => Error == null;

        public Result(T value) { Value = value; Error = null; }
        public Result(string error) { Value = default; Error = error; }

        public static Result<T> Ok(T value) => new(value);
        public static Result<T> Fail(string error) => new(error);
    }
}
