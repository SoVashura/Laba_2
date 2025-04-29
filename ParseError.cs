namespace Lab1_compile
{
    public class ParseError
    {
        public string Message { get; }
        public Token Token { get; }

        public int Position => Token?.Start ?? -1;
        public int Length => (Token?.Code == -1 && Token?.Value.Length == 1)
            ? 1
            : Token?.Value?.Length ?? 1;

        public ParseError(string message, Token token)
        {
            Message = message;
            Token = token;
        }

        public override string ToString()
        {
            return Position >= 0
                ? $"{Message} (позиция: {Position})"
                : Message;
        }
    }
}