using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1_compile
{
        public enum TokenType
        {
            IfKeyword,      // IF
            ThenKeyword,    // THEN
            Identifier,     // Идентификаторы (a, b, x, y)
            Number,         // Числа (42, 3.14)
            Plus,           // +
            Multiply,       // *
            Equals,         // ==
            NotEquals,      // !=
            LessThan,       // <
            LessOrEqual,    // <=
            GreaterThan,    // >
            GreaterOrEqual, // >=
            LeftParenthesis, // (
            RightParenthesis, // )
            EndOfInput,     // Конец ввода
            Unknown         // Неизвестный токен
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
        }
}