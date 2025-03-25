using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1_compile
{
    internal class Lexer
    {
        private string _input;
        private int _position;
        private List<ParseError> _errors;
        private bool isKeyword;

        public Lexer(string input)
        {
            isKeyword = false;
            _input = input;
            _position = 0;
            _errors = new List<ParseError>();
        }
        private bool IsLetter(char c)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'a')) return true;
            else return false;
        }
        private bool IsLetterOrDigit(char c)
        {
            if (char.IsDigit(c) || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'a')) return true;
            else return false;
        }

        public List<Token> Tokenize()
        {
            
            List<Token> tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char currentChar = _input[_position];

                if (char.IsWhiteSpace(currentChar))
                {
                    tokens.Add(new Token(8, "Пробел", " ", _position, _position));
                    _position++;
                    continue;
                }

                if (IsLetter(currentChar) || currentChar == '_')
                {
                    Token temp = ExtractIdentifier();
                    if (temp.Code == -1) return tokens;
                    tokens.Add(temp);
                    continue;
                }


                if (currentChar == '{')
                {
                    tokens.Add(new Token(9, "Скобка", "{", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '}')
                {
                    tokens.Add(new Token(10, "Скобка", "}", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == ';')
                {
                    tokens.Add(new Token(11, "Оператор", ";", _position, _position));
                    _position++;
                    continue;
                }

                _errors.Add(new ParseError($"Ошибка: недопустимый символ '{currentChar}' в позиции {_position}",
                    new Token(-1, "Ошибка", currentChar.ToString(), _position, _position)));
                break;

                _position++;
            }

            return tokens;
        }

        private Token ExtractIdentifier()
        {
            int start = _position;
            while (_position < _input.Length && (IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            {
                _position++;
            }

            string value = _input.Substring(start, _position - start);

            if (value == "struct")
            {
                isKeyword = true;
                return new Token(1, "Ключевое слово", value, start, _position - 1);
            }
            else if (value == "int")
            {
                return new Token(2, "Ключевое слово", value, start, _position - 1);
            }
            else if (value == "float")
            {
                return new Token(3, "Ключевое слово", value, start, _position - 1);
            }
            else if (value == "string")
            {
                return new Token(4, "Ключевое слово", value, start, _position - 1);
            }
            else if (value == "bool")
            {
                return new Token(5, "Ключевое слово", value, start, _position - 1);
            }
            else if (value == "char")
            {
                return new Token(6, "Ключевое слово", value, start, _position - 1);
            }
            else if (value.StartsWith("st") && value.Contains("ct"))
            {
                if (!isKeyword)
                {
                _errors.Add(new ParseError($"Ошибка: неверное написание ключевого слова '{value}', позиция {start}",
                    new Token(-1, "Ошибка", value, start, _position - 1)));
                    isKeyword = true;
                    return new Token(-1, "Ошибка", value, start, _position - 1);
                }
            }

            return new Token(7, "Идентификатор", value, start, _position - 1);
        }

        

        public List<ParseError> GetErrors()
        {
            return _errors;
        }
    }
}
