using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    internal class ParseError
    {
        public string Message { get; }
        public string TokenValue { get; }
        public int Position { get; }

        public ParseError(string message, Token token)
        {
            Message = message;
            TokenValue = token?.Value ?? "EOF";
            Position = token?.Start ?? -1;
        }
    }
}
