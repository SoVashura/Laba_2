using System;
using System.Collections.Generic;

namespace Lab1_compile
{
    internal class Lexer
    {
        private string _input;
        private int _position;
        private List<ParseError> _errors;
        private bool _hasCriticalError = false;
        private const int STATE_START = 0;
        private const int STATE_AFTER_STRUCT = 1;
        private const int STATE_AFTER_NAME = 2;
        private const int STATE_IN_STRUCT_BODY = 3;
        private const int STATE_EXPECT_NAME = 4;
        private const int STATE_EXPECT_SEMICOLON = 5;
        private const int STATE_EXPECT_STRUCT_END = 6;

        private int _currentState = STATE_START;
        private int _lastSpacePosition = -1;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            _errors = new List<ParseError>();
        }

        private bool IsRussianLetter(char c)
        {
            return (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') || c == 'ё' || c == 'Ё';
        }

        private void AddError(string message, string symbol, int position)
        {
            _errors.Add(new ParseError(message,
                new Token(-1, "Ошибка", symbol, position, position + symbol.Length - 1)));
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (_position < _input.Length && !_hasCriticalError)
            {
                char current = _input[_position];

                // Обработка пробелов
                if (char.IsWhiteSpace(current))
                {
                    if (_position > 0 && IsLetterOrDigitOrUnderscore(_input[_position - 1]) &&
                        _position < _input.Length - 1 && IsLetterOrDigitOrUnderscore(_input[_position + 1]))
                    {
                        tokens.Add(new Token(12, "Пробел", " ", _position, _position));
                    }
                    _position++;
                    continue;
                }

                // Проверка на недопустимые символы
                if (!IsAllowedCharacter(current))
                {
                    AddErrorAndStop("Недопустимый символ", current.ToString(), _position);
                    break;
                }

                // Начальное состояние
                if (_currentState == STATE_START)
                {
                    if (current == 's' && LookAhead("struct"))
                    {
                        tokens.Add(ProcessKeyword("struct", 7));
                        _currentState = STATE_AFTER_STRUCT;
                        continue;
                    }
                    AddErrorAndStop("Ожидается ключевое слово 'struct'", current.ToString(), _position);
                    break;
                }

                // После struct
                if (_currentState == STATE_AFTER_STRUCT)
                {
                    if (char.IsLetter(current))
                    {
                        tokens.Add(ProcessName());
                        _currentState = STATE_AFTER_NAME;
                        continue;
                    }
                    AddErrorAndStop("Ожидается имя структуры", current.ToString(), _position);
                    break;
                }

                // После имени структуры
                if (_currentState == STATE_AFTER_NAME)
                {
                    if (current == '{')
                    {
                        tokens.Add(new Token(9, "Открывающая скобка", "{", _position, _position));
                        _position++;
                        _currentState = STATE_IN_STRUCT_BODY;
                        continue;
                    }
                    AddErrorAndStop("Ожидается '{' после имени структуры", current.ToString(), _position);
                    break;
                }

                // Внутри тела структуры
                if (_currentState == STATE_IN_STRUCT_BODY)
                {
                    if (IsTypeKeyword(current))
                    {
                        tokens.Add(ProcessTypeKeyword());
                        _currentState = STATE_EXPECT_NAME;
                        continue;
                    }

                    if (current == '}')
                    {
                        tokens.Add(new Token(10, "Закрывающая скобка", "}", _position, _position));
                        _position++;
                        _currentState = STATE_EXPECT_STRUCT_END;
                        continue;
                    }

                    AddErrorAndStop("Ожидается тип поля или '}'", current.ToString(), _position);
                    break;
                }

                // Ожидается имя поля
                if (_currentState == STATE_EXPECT_NAME)
                {
                    if (char.IsLetter(current))
                    {
                        tokens.Add(ProcessName());
                        _currentState = STATE_EXPECT_SEMICOLON;
                        continue;
                    }
                    AddErrorAndStop("Ожидается имя поля", current.ToString(), _position);
                    break;
                }

                // Ожидается точка с запятой
                if (_currentState == STATE_EXPECT_SEMICOLON)
                {
                    if (current == ';')
                    {
                        tokens.Add(new Token(11, "Точка с запятой", ";", _position, _position));
                        _position++;
                        _currentState = STATE_IN_STRUCT_BODY;
                        continue;
                    }
                    AddErrorAndStop("Ожидается ';' после имени поля", current.ToString(), _position);
                    break;
                }

                // Ожидается завершение структуры
                if (_currentState == STATE_EXPECT_STRUCT_END)
                {
                    if (current == ';')
                    {
                        tokens.Add(new Token(11, "Точка с запятой", ";", _position, _position));
                        _position++;
                        return tokens;
                    }
                    AddErrorAndStop("Ожидается ';' после закрывающей скобки", current.ToString(), _position);
                    break;
                }
            }

            // Финальные проверки
            if (!_hasCriticalError)
            {
                if (_currentState == STATE_IN_STRUCT_BODY)
                {
                    AddErrorAndStop("Отсутствует '}' в конце структуры", "", _position);
                }
                else if (_currentState != STATE_EXPECT_STRUCT_END)
                {
                    AddErrorAndStop("Незавершенное определение структуры", "", _position);
                }
            }

            return tokens;
        }

        private bool IsAllowedCharacter(char c)
        {
            return c <= 127 || c == '_';
        }

        private bool LookAhead(string expected)
        {
            if (_position + expected.Length > _input.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (_input[_position + i] != expected[i])
                    return false;
            }
            return true;
        }

        private Token ProcessKeyword(string keyword, int code)
        {
            var token = new Token(code, "Ключевое слово", keyword, _position, _position + keyword.Length - 1);
            _position += keyword.Length;
            return token;
        }

        private Token ProcessName()
        {
            int start = _position;
            while (_position < _input.Length && IsLetterOrDigitOrUnderscore(_input[_position]))
            {
                _position++;
            }
            return new Token(1, "Идентификатор", _input.Substring(start, _position - start), start, _position - 1);
        }

        private Token ProcessTypeKeyword()
        {
            int start = _position;
            while (_position < _input.Length && char.IsLetter(_input[_position]))
            {
                _position++;
            }
            string type = _input.Substring(start, _position - start);

            int code = -1;
            if (type == "int") code = 2;
            else if (type == "float") code = 3;
            else if (type == "string") code = 4;
            else if (type == "bool") code = 5;
            else if (type == "char") code = 6;

            if (code == -1)
            {
                AddErrorAndStop($"Неизвестный тип '{type}'", type, start);
                return new Token(-1, "Ошибка", type, start, _position - 1);
            }

            return new Token(code, "Ключевое слово", type, start, _position - 1);
        }

        private bool IsTypeKeyword(char c)
        {
            return c == 'i' || c == 'f' || c == 's' || c == 'b' || c == 'c';
        }

        private bool IsLetterOrDigitOrUnderscore(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private void AddErrorAndStop(string message, string symbol, int position)
        {
            _errors.Add(new ParseError(message,
                new Token(-1, "Ошибка", symbol, position, position + symbol.Length - 1)));
            _hasCriticalError = true;
        }

        public List<ParseError> GetErrors()
        {
            return _errors;
        }
    }
}