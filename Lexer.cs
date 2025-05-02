using System;
using System.Collections.Generic;

namespace Lab1_compile
{
    public class Lexer
    {
        private string _input;
        private int _position;
        private List<ParseError> _errors;
        private Stack<int> _bracketStack;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            _errors = new List<ParseError>();
            _bracketStack = new Stack<int>();
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char currentChar = _input[_position];

                if (char.IsWhiteSpace(currentChar))
                {
                    _position++;
                    continue;
                }

                if (char.IsDigit(currentChar) || (currentChar == '-' && IsUnaryMinus(tokens)))
                {
                    var numberToken = ExtractNumber();
                    if (numberToken != null)
                        tokens.Add(numberToken);
                    continue;
                }

                if (currentChar == '+')
                {
                    tokens.Add(new Token(4, "Оператор", "+", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '-')
                {
                    tokens.Add(new Token(4, "Оператор", "-", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '*')
                {
                    tokens.Add(new Token(4, "Оператор", "*", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '/')
                {
                    tokens.Add(new Token(4, "Оператор", "/", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == '(')
                {
                    _bracketStack.Push(_position);
                    tokens.Add(new Token(5, "Скобка", "(", _position, _position));
                    _position++;
                    continue;
                }

                if (currentChar == ')')
                {
                    if (_bracketStack.Count == 0)
                    {
                        _errors.Add(new ParseError(
                            "Закрывающая скобка без соответствующей открывающей",
                            new Token(-1, "Ошибка", ")", _position, _position))
                        );
                    }
                    else
                    {
                        _bracketStack.Pop();
                    }
                    tokens.Add(new Token(6, "Скобка", ")", _position, _position));
                    _position++;
                    continue;
                }

                _errors.Add(new ParseError(
                    $"Недопустимый символ '{currentChar}' в позиции {_position}",
                    new Token(-1, "Ошибка", currentChar.ToString(), _position, _position))
                );
                _position++;
            }

            // Проверка оставшихся незакрытых скобок
            while (_bracketStack.Count > 0)
            {
                int pos = _bracketStack.Pop();
                _errors.Add(new ParseError(
                    "Отсутствует закрывающая скобка",
                    new Token(-1, "Ошибка", "(", pos, pos))
                );
            }

            CheckForMissingOperators(tokens);
            return tokens;
        }

        private Token ExtractNumber()
        {
            int start = _position;
            bool hasDecimal = false;
            bool isNegative = false;

            if (_input[_position] == '-')
            {
                isNegative = true;
                _position++;

                if (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
                {
                    _errors.Add(new ParseError(
                        "Пробел после унарного минуса",
                        new Token(-1, "Ошибка", "-", start, _position))
                    );
                    return null;
                }
            }

            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }

            if (_position < _input.Length && _input[_position] == '.')
            {
                if (hasDecimal)
                {
                    _errors.Add(new ParseError(
                        "Несколько точек в числе",
                        new Token(-1, "Ошибка", _input.Substring(start, _position - start), start, _position - 1)
                    ));
                    return null;
                }

                hasDecimal = true;
                _position++;

                if (_position >= _input.Length || !char.IsDigit(_input[_position]))
                {
                    _errors.Add(new ParseError(
                        "Отсутствует дробная часть после точки",
                        new Token(-1, "Ошибка", _input.Substring(start, _position - start), start, _position - 1)
                    ));
                    return null;
                }

                while (_position < _input.Length && char.IsDigit(_input[_position]))
                {
                    _position++;
                }
            }

            if (_position < _input.Length && !IsValidNumberEnd(_input[_position]))
            {
                _errors.Add(new ParseError(
                    "Некорректный символ в числе",
                    new Token(-1, "Ошибка", _input.Substring(start, _position - start + 1), start, _position)
                ));
                return null;
            }

            string value = _input.Substring(start, _position - start);
            return new Token(
                hasDecimal ? 9 : 8,
                hasDecimal ? "Вещественное число" : "Целое число",
                value,
                start,
                _position - 1
            );
        }

        private void CheckForMissingOperators(List<Token> tokens)
        {
            for (int i = 1; i < tokens.Count; i++)
            {
                if ((tokens[i - 1].Type == 8 || tokens[i - 1].Type == 9) &&
                    (tokens[i].Type == 8 || tokens[i].Type == 9))
                {
                    _errors.Add(new ParseError(
                        "Отсутствует оператор между числами",
                        tokens[i]
                    ));
                }
            }
        }

        private bool IsUnaryMinus(List<Token> tokens)
        {
            return tokens.Count == 0 ||
                   tokens[tokens.Count - 1].Type == 5 ||
                   tokens[tokens.Count - 1].Type == 4;
        }

        private bool IsValidNumberEnd(char c)
        {
            return char.IsWhiteSpace(c) ||
                   c == '+' || c == '-' || c == '*' || c == '/' ||
                   c == '(' || c == ')';
        }

        public List<ParseError> GetErrors()
        {
            return _errors;
        }
    }
}