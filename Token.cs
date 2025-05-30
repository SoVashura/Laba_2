using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
    public enum TokenType
    {
        IfKeyword,
        ThenKeyword,
        Identifier,
        Number,
        Plus,
        Multiply,
        Equals,
        NotEquals,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual,
        LeftParenthesis,
        RightParenthesis,
        EndOfInput,
        Unknown
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Position { get; }

        public Token(TokenType type, string value, int position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        public override string ToString() => $"{Type}: '{Value}' at {Position}";
    }
}