using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1_compile
{
    internal class Parser
    {
        private readonly List<Token> _tokens;
        private int _currentIndex;
        public List<ParseError> Errors { get; } = new List<ParseError>();

        // Синхронизирующие множества для метода Айронса
        private readonly string[] _structSyncTokens = { "}", ";" };
        private readonly string[] _fieldSyncTokens = { ";", "}", "int", "char", "float", "double", "bool", "string" };
        private readonly string[] _globalSyncTokens = { "struct" };

        public Parser(List<Token> tokens)
        {
            _tokens = tokens.Where(t => t.Code != 8 /* whitespace */).ToList();
            _currentIndex = 0;
        }

        public bool Parse()
        {
            try
            {
                while (!IsAtEnd())
                {
                    ParseStruct();
                }
                return !Errors.Any();
            }
            catch (Exception ex)
            {
                Errors.Add(new ParseError($"Critical parsing error: {ex.Message}", GetCurrentToken()));
                return false;
            }
        }

        private void ParseStruct()
        {
            // 1. Ожидаем ключевое слово 'struct'
            if (!MatchKeyword("struct"))
            {
                AddError("Expected 'struct' keyword");
                Synchronize(_globalSyncTokens);
                if (IsAtEnd()) return;

                if (!IsNextKeyword("struct")) return;
            }

            // 2. Ожидаем имя структуры
            if (!MatchIdentifier())
            {
                AddError("Expected struct name identifier");
                Synchronize(_structSyncTokens);
                if (IsAtEnd()) return;
            }

            // 3. Ожидаем открывающую скобку '{'
            if (!MatchSymbol("{"))
            {
                AddError("Expected opening brace '{'");
                Synchronize(_structSyncTokens);
                if (IsAtEnd()) return;
            }

            // 4. Обрабатываем поля (0 или более)
            while (!IsAtEnd() && !IsNextSymbol("}"))
            {
                ParseField();

                // 5. Точка с запятой после каждого поля обязательна
                if (!IsNextSymbol("}") && !MatchSymbol(";"))
                {
                    AddError("Expected semicolon ';' after field declaration");
                    Synchronize(_fieldSyncTokens);
                }
            }

            // 6. Ожидаем закрывающую скобку '}'
            if (!MatchSymbol("}"))
            {
                AddError("Expected closing brace '}'");
                Synchronize(new[] { ";" });
                if (IsAtEnd()) return;
            }

            // 7. Точка с запятой после структуры обязательна
            if (!MatchSymbol(";"))
            {
                AddError("Expected semicolon ';' after struct definition");
                Synchronize(_globalSyncTokens);
            }
        }

        private void ParseField()
        {
            // 1. Ожидаем тип поля
            if (!MatchType())
            {
                AddError("Expected field type (int, char, float, double, bool, string)");
                Synchronize(_fieldSyncTokens);
                return;
            }

            // 2. Ожидаем имя поля
            if (!MatchIdentifier())
            {
                AddError("Expected field name identifier");
                Synchronize(_fieldSyncTokens);
            }

            // 3. Обработка массива (опционально)
            if (MatchSymbol("["))
            {
                if (!MatchSymbol("]"))
                {
                    AddError("Expected closing bracket ']' for array declaration");
                    Synchronize(_fieldSyncTokens);
                }
            }
        }

        /// <summary>
        /// Реализация метода Айронса для синхронизации после ошибок
        /// </summary>
        private void Synchronize(string[] syncTokens)
        {
            int startIndex = _currentIndex;

            while (!IsAtEnd())
            {
                var current = GetCurrentToken();

                // Проверяем все возможные токены для синхронизации
                if (syncTokens.Contains(current.Value) ||
                    current.Value == ";" || current.Value == "}")
                {
                    return;
                }

                _currentIndex++;
            }

            // Если ничего не нашли, логируем сколько пропустили
            if (startIndex != _currentIndex)
            {
                AddError($"Skipped {_currentIndex - startIndex} tokens during synchronization");
            }
        }

        #region Вспомогательные методы для работы с токенами

        private bool MatchKeyword(string keyword)
        {
            if (IsAtEnd()) return false;
            var token = GetCurrentToken();
            if (token.Value == keyword && (token.Code == 1 || token.Code >= 2 && token.Code <= 6))
            {
                _currentIndex++;
                return true;
            }
            return false;
        }

        private bool MatchIdentifier()
        {
            if (IsAtEnd()) return false;
            if (_tokens[_currentIndex].Code == 7) // Код идентификатора
            {
                _currentIndex++;
                return true;
            }
            return false;
        }

        private bool MatchSymbol(string symbol)
        {
            if (IsAtEnd()) return false;
            var token = GetCurrentToken();
            if (token.Value == symbol && (token.Code == 9 || token.Code == 10 || token.Code == 11))
            {
                _currentIndex++;
                return true;
            }
            return false;
        }

        private bool MatchType()
        {
            if (IsAtEnd()) return false;
            var token = GetCurrentToken();
            if (token.Code >= 2 && token.Code <= 6) // Коды типов данных
            {
                _currentIndex++;
                return true;
            }
            return false;
        }

        private bool IsNextSymbol(string symbol)
        {
            if (IsAtEnd()) return false;
            return _tokens[_currentIndex].Value == symbol;
        }

        private bool IsNextKeyword(string keyword)
        {
            if (IsAtEnd()) return false;
            return _tokens[_currentIndex].Value == keyword;
        }

        private bool IsAtEnd()
        {
            return _currentIndex >= _tokens.Count;
        }

        private Token GetCurrentToken()
        {
            return IsAtEnd() ? new Token(-1, "EOF", "EOF", -1, -1) : _tokens[_currentIndex];
        }

        private void AddError(string message)
        {
            Errors.Add(new ParseError(message, GetCurrentToken()));
        }

        #endregion
    }
}