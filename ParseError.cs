namespace Lab1_compile
{
    public class ParseError
    {
        public string Message { get; }
        public int Position { get; }

        public ParseError(string message, int position)
        {
            Message = message;
            Position = position;
        }
    }
}