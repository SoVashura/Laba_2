using Lab1_compile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace  Lab1_compile
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _currentTokenIndex;
        private readonly List<ParseError> _errors;
        private ParseTreeNode _parseTreeRoot;
        private bool _parsingFailed;

        public ParseTreeNode ParseTree => _errors.Count == 0 ? _parseTreeRoot : null;
        public IReadOnlyList<ParseError> Errors => _errors.AsReadOnly();
        public bool HasErrors => _errors.Count > 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _errors = new List<ParseError>();
        }

        public bool Parse()
        {
            _currentTokenIndex = 0;
            _errors.Clear();
            _parsingFailed = false;
            _parseTreeRoot = new ParseTreeNode("Программа");

            ParseConditionalStatement(_parseTreeRoot);

            // Убрали проверку на конец ввода, чтобы дерево выводилось в любом случае
            return !_parsingFailed && _errors.Count == 0;
        }

        private Token CurrentToken => _parsingFailed || _currentTokenIndex >= _tokens.Count
            ? null
            : _tokens[_currentTokenIndex];

        private void AddError(string message, int position)
        {
            _errors.Add(new ParseError(message, position));
            _parsingFailed = true;
        }

        private bool ParseConditionalStatement(ParseTreeNode parentNode)
        {
            var ifNode = new ParseTreeNode("Условный оператор");
            parentNode.Children.Add(ifNode);

            // IF
            if (CurrentToken?.Type != TokenType.IfKeyword)
            {
                AddError("Ожидалось ключевое слово IF", CurrentToken?.Position ?? 0);
                return false;
            }
            ifNode.Children.Add(new ParseTreeNode("Ключевое слово", "IF", CurrentToken.Position));
            _currentTokenIndex++;

            // Условие
            if (!ParseCondition(ifNode)) return false; // Передаем ifNode вместо создания нового узла

            // THEN
            if (CurrentToken?.Type != TokenType.ThenKeyword)
            {
                AddError("Ожидалось ключевое слово THEN", CurrentToken?.Position ?? 0);
                return false;
            }
            ifNode.Children.Add(new ParseTreeNode("Ключевое слово", "THEN", CurrentToken.Position));
            _currentTokenIndex++;

            // Оператор
            if (CurrentToken?.Type == TokenType.IfKeyword)
            {
                return ParseConditionalStatement(ifNode);
            }
            return ParseExpression(ifNode, true);
        }

        public ParseError FirstError => _errors.FirstOrDefault();

        private bool ParseCondition(ParseTreeNode parentNode)
        {
            if (_parsingFailed) return false;

            var conditionNode = new ParseTreeNode("Условие");
            parentNode.Children.Add(conditionNode);

            // Левый операнд
            if (!ParseExpression(conditionNode, false)) return false;

            // Проверка оператора сравнения
            if (CurrentToken == null || !IsComparisonOperator(CurrentToken.Type))
            {
                AddError($"Ожидался оператор сравнения (==, !=, <, <=, >, >=), но получено: {CurrentToken?.Value ?? "конец ввода"}",
                       CurrentToken?.Position ?? 0);
                return false;
            }

            conditionNode.Children.Add(new ParseTreeNode("Оператор сравнения", CurrentToken.Value, CurrentToken.Position));
            _currentTokenIndex++;

            // Правый операнд
            return ParseExpression(conditionNode, false);
        }

        private bool ParseExpression(ParseTreeNode parentNode, bool strictMode)
        {
            var exprNode = new ParseTreeNode("Выражение");
            parentNode.Children.Add(exprNode);

            if (!ParseTerm(exprNode, strictMode)) return false;

            while (CurrentToken != null && (CurrentToken.Type == TokenType.Plus))
            {
                exprNode.Children.Add(new ParseTreeNode("Оператор", CurrentToken.Value, CurrentToken.Position));
                _currentTokenIndex++;
                if (!ParseTerm(exprNode, strictMode))
                {
                    AddError($"Ожидался терм после оператора {CurrentToken?.Value}", CurrentToken?.Position ?? 0);
                    return false;
                }
            }

            if (strictMode && CurrentToken != null &&
               (CurrentToken.Type == TokenType.Identifier || CurrentToken.Type == TokenType.Number) &&
               exprNode.Children.LastOrDefault()?.NodeType == "Терм")
            {
                AddError($"Ожидался оператор между операндами", CurrentToken.Position);
                return false;
            }

            return true;
        }

        private bool ParseTerm(ParseTreeNode parentNode, bool strictMode)
        {
            var termNode = new ParseTreeNode("Терм");
            parentNode.Children.Add(termNode);

            if (!ParseFactor(termNode)) return false;

            while (CurrentToken?.Type == TokenType.Multiply)
            {
                termNode.Children.Add(new ParseTreeNode("Оператор", "*", CurrentToken.Position));
                _currentTokenIndex++;
                if (!ParseFactor(termNode)) return false;
            }

            return true;
        }

        private bool ParseFactor(ParseTreeNode parentNode)
        {
            if (_parsingFailed) return false;

            if (CurrentToken == null)
            {
                AddError("Ожидался идентификатор или число", 0);
                return false;
            }

            switch (CurrentToken.Type)
            {
                case TokenType.Identifier:
                    // Проверка на русские буквы
                    if (ContainsCyrillic(CurrentToken.Value))
                    {
                        AddError($"Идентификатор содержит русские буквы: {CurrentToken.Value}", CurrentToken.Position);
                        return false;
                    }
                    parentNode.Children.Add(new ParseTreeNode("Идентификатор", CurrentToken.Value, CurrentToken.Position));
                    _currentTokenIndex++;
                    return true;

                case TokenType.Number:
                    parentNode.Children.Add(new ParseTreeNode("Число", CurrentToken.Value, CurrentToken.Position));
                    _currentTokenIndex++;
                    return true;

                case TokenType.LeftParenthesis:
                    var parenNode = new ParseTreeNode("Скобки");
                    parentNode.Children.Add(parenNode);
                    _currentTokenIndex++;

                    if (!ParseExpression(parenNode, false)) return false;

                    if (CurrentToken?.Type != TokenType.RightParenthesis)
                    {
                        AddError("Ожидалась закрывающая скобка", CurrentToken?.Position ?? 0);
                        return false;
                    }
                    _currentTokenIndex++;
                    return true;

                default:
                    AddError($"Недопустимый токен: {CurrentToken.Value}", CurrentToken.Position);
                    return false;
            }
        }

        private bool ContainsCyrillic(string value)
        {
            return value.Any(c => c >= 'А' && c <= 'я') || value.Contains('Ё') || value.Contains('ё');
        }

        private static bool IsComparisonOperator(TokenType type)
        {
            return type == TokenType.Equals || type == TokenType.NotEquals ||
                   type == TokenType.LessThan || type == TokenType.LessOrEqual ||
                   type == TokenType.GreaterThan || type == TokenType.GreaterOrEqual;
        }

        public ParseTreeNode GetParseTree() => _errors.Count == 0 ? _parseTreeRoot : null;
    }
}