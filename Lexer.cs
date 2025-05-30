using Lab1_compile;
using System;
using System.Collections.Generic;

namespace Lab1_compile
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private readonly List<ParseError> _errors = new List<ParseError>();

        public Lexer(string input)
        {
            _input = input ?? string.Empty;
            _position = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();
            _errors.Clear();

            while (_position < _input.Length)
            {
                char current = _input[_position];

                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                if (TryMatchKeyword("IF", TokenType.IfKeyword, ref tokens) ||
                    TryMatchKeyword("THEN", TokenType.ThenKeyword, ref tokens))
                {
                    continue;
                }

                if (TryMatchOperator("==", TokenType.Equals, ref tokens) ||
                    TryMatchOperator("!=", TokenType.NotEquals, ref tokens) ||
                    TryMatchOperator("<=", TokenType.LessOrEqual, ref tokens) ||
                    TryMatchOperator(">=", TokenType.GreaterOrEqual, ref tokens) ||
                    TryMatchOperator("<", TokenType.LessThan, ref tokens) ||
                    TryMatchOperator(">", TokenType.GreaterThan, ref tokens) ||
                    TryMatchOperator("+", TokenType.Plus, ref tokens) ||
                    TryMatchOperator("*", TokenType.Multiply, ref tokens) ||
                    TryMatchOperator("(", TokenType.LeftParenthesis, ref tokens) ||
                    TryMatchOperator(")", TokenType.RightParenthesis, ref tokens))
                {
                    continue;
                }

                if (char.IsLetter(current))
                {
                    tokens.Add(ReadIdentifier());
                    continue;
                }

                if (char.IsDigit(current))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                _errors.Add(new ParseError($"Неизвестный символ: '{current}'", _position));
                _position++;
            }

            tokens.Add(new Token(TokenType.EndOfInput, "", _position));
            return tokens;
        }

        public List<ParseError> GetErrors() => _errors;

        private bool TryMatchKeyword(string keyword, TokenType type, ref List<Token> tokens)
        {
            if (_position + keyword.Length > _input.Length)
                return false;

            string substring = _input.Substring(_position, keyword.Length);
            if (substring.Equals(keyword, StringComparison.OrdinalIgnoreCase))
            {
                tokens.Add(new Token(type, substring, _position));
                _position += keyword.Length;
                return true;
            }

            return false;
        }

        private bool TryMatchOperator(string op, TokenType type, ref List<Token> tokens)
        {
            if (_position + op.Length > _input.Length)
                return false;

            string substring = _input.Substring(_position, op.Length);
            if (substring == op)
            {
                tokens.Add(new Token(type, substring, _position));
                _position += op.Length;
                return true;
            }

            return false;
        }

        private Token ReadIdentifier()
        {
            int start = _position;
            while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position])))
            {
                _position++;
            }

            string value = _input.Substring(start, _position - start);
            return new Token(TokenType.Identifier, value, start);
        }

        private Token ReadNumber()
        {
            int start = _position;
            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }

            string value = _input.Substring(start, _position - start);
            return new Token(TokenType.Number, value, start);
        }
    }
}