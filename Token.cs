namespace Lab1_compile
{
    public class Token
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public Token(int code, string type, string value, int start, int end)
        {
            Code = code;
            Type = type;
            Value = value;
            Start = start;
            End = end;
        }
    }
}