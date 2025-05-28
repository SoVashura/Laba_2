using Lab1_compile;
using System;
using System.Collections.Generic;

namespace Lab1_compile
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private List<ParseError> _errors;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            _errors = new List<ParseError>();
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char current = _input[_position];

                // Пропускаем пробелы
                if (char.IsWhiteSpace(current))
                {
                    _position++;
                    continue;
                }

                // Ключевые слова
                if (char.IsLetter(current))
                {
                    string word = ReadWord();
                    TokenType type = TokenType.Identifier;

                    if (word.Equals("IF", StringComparison.OrdinalIgnoreCase))
                        type = TokenType.IfKeyword;
                    else if (word.Equals("THEN", StringComparison.OrdinalIgnoreCase))
                        type = TokenType.ThenKeyword;

                    tokens.Add(new Token(type, word, _position - word.Length));
                    continue;
                }

                // Числа
                if (char.IsDigit(current))
                {
                    string num = ReadNumber();
                    tokens.Add(new Token(TokenType.Number, num, _position - num.Length));
                    continue;
                }

                // Операторы
                switch (current)
                {
                    case '+':
                        tokens.Add(new Token(TokenType.Plus, "+", _position));
                        _position++;
                        continue;
                    case '*':
                        tokens.Add(new Token(TokenType.Multiply, "*", _position));
                        _position++;
                        continue;
                    case '(':
                        tokens.Add(new Token(TokenType.LeftParenthesis, "(", _position));
                        _position++;
                        continue;
                    case ')':
                        tokens.Add(new Token(TokenType.RightParenthesis, ")", _position));
                        _position++;
                        continue;
                }

                // Операции сравнения (двухсимвольные)
                if (_position + 1 < _input.Length)
                {
                    string twoCharOp = _input.Substring(_position, 2);
                    switch (twoCharOp)
                    {
                        case "==":
                            tokens.Add(new Token(TokenType.Equals, "==", _position));
                            _position += 2;
                            continue;
                        case "!=":
                            tokens.Add(new Token(TokenType.NotEquals, "!=", _position));
                            _position += 2;
                            continue;
                        case "<=":
                            tokens.Add(new Token(TokenType.LessOrEqual, "<=", _position));
                            _position += 2;
                            continue;
                        case ">=":
                            tokens.Add(new Token(TokenType.GreaterOrEqual, ">=", _position));
                            _position += 2;
                            continue;
                    }
                }

                // Односимвольные операции сравнения
                switch (current)
                {
                    case '<':
                        tokens.Add(new Token(TokenType.LessThan, "<", _position));
                        _position++;
                        continue;
                    case '>':
                        tokens.Add(new Token(TokenType.GreaterThan, ">", _position));
                        _position++;
                        continue;
                }

                // Неизвестный символ
                _errors.Add(new ParseError($"Неизвестный символ: '{current}'", _position));
                _position++;
            }

            tokens.Add(new Token(TokenType.EndOfInput, "", _position));
            return tokens;
        }

        private string ReadWord()
        {
            int start = _position;
            while (_position < _input.Length && char.IsLetterOrDigit(_input[_position]))
                _position++;
            return _input.Substring(start, _position - start);
        }

        private string ReadNumber()
        {
            int start = _position;
            while (_position < _input.Length && char.IsDigit(_input[_position]))
                _position++;
            return _input.Substring(start, _position - start);
        }

        public List<ParseError> GetErrors() => _errors;
    }
}