using Lab1_compile;
using System;
using System.Collections.Generic;
using System.Linq;

namespace  Lab1_compile
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _currentIndex;
        private readonly List<ParseError> _errors = new List<ParseError>();

        public ParseTreeNode ParseTree { get; private set; }
        public bool HasErrors => _errors.Count > 0;
        public IReadOnlyList<ParseError> Errors => _errors;
        public ParseError FirstError => _errors.FirstOrDefault();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens ?? new List<Token>();
            _currentIndex = 0;
        }

        public void Parse()
        {
            _errors.Clear();
            ParseTree = ParseConditionalStatement();
        }

        private Token CurrentToken => _currentIndex < _tokens.Count ? _tokens[_currentIndex] : null;
        private Token NextToken => _currentIndex + 1 < _tokens.Count ? _tokens[_currentIndex + 1] : null;

        private Token Consume(TokenType expectedType, string errorMessage)
        {
            if (CurrentToken?.Type == expectedType)
            {
                var token = CurrentToken;
                _currentIndex++;
                return token;
            }

            _errors.Add(new ParseError(errorMessage, CurrentToken?.Position ?? 0));
            return null;
        }

        private bool Match(TokenType type) => CurrentToken?.Type == type;

        private ParseTreeNode ParseConditionalStatement()
        {
            var node = new ParseTreeNode("ConditionalStatement");
            int startPos = CurrentToken?.Position ?? 0;

            if (Match(TokenType.IfKeyword))
            {
                node.AddChild(new ParseTreeNode("Keyword", CurrentToken.Value, CurrentToken.Position));
                Consume(TokenType.IfKeyword, "Ожидается ключевое слово IF");

                var condition = ParseCondition();
                if (condition != null)
                {
                    node.AddChild(condition);

                    if (Match(TokenType.ThenKeyword))
                    {
                        node.AddChild(new ParseTreeNode("Keyword", CurrentToken.Value, CurrentToken.Position));
                        Consume(TokenType.ThenKeyword, "Ожидается ключевое слово THEN");

                        var statement = ParseStatement();
                        if (statement != null)
                        {
                            node.AddChild(statement);
                        }
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ожидается THEN после условия", CurrentToken?.Position ?? startPos));
                    }
                }
            }
            else
            {
                _errors.Add(new ParseError("Ожидается IF в начале условного оператора", startPos));
            }

            return node;
        }

        private ParseTreeNode ParseCondition()
        {
            var node = new ParseTreeNode("Condition");
            int startPos = CurrentToken?.Position ?? 0;

            var leftExpr = ParseExpression();
            if (leftExpr != null)
            {
                node.AddChild(leftExpr);

                if (IsRelationalOperator(CurrentToken?.Type))
                {
                    var opNode = new ParseTreeNode("Operator", CurrentToken.Value, CurrentToken.Position);
                    node.AddChild(opNode);
                    _currentIndex++;

                    var rightExpr = ParseExpression();
                    if (rightExpr != null)
                    {
                        node.AddChild(rightExpr);
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ожидается выражение после оператора отношения", CurrentToken?.Position ?? startPos));
                    }
                }
                else
                {
                    _errors.Add(new ParseError("Ожидается оператор отношения", CurrentToken?.Position ?? startPos));
                }
            }

            return node;
        }

        private bool IsRelationalOperator(TokenType? type)
        {
            return type == TokenType.Equals ||
                   type == TokenType.NotEquals ||
                   type == TokenType.LessThan ||
                   type == TokenType.LessOrEqual ||
                   type == TokenType.GreaterThan ||
                   type == TokenType.GreaterOrEqual;
        }

        private ParseTreeNode ParseExpression()
        {
            var node = new ParseTreeNode("Expression");
            int startPos = CurrentToken?.Position ?? 0;

            var term = ParseTerm();
            if (term != null)
            {
                node.AddChild(term);

                while (Match(TokenType.Plus))
                {
                    var opNode = new ParseTreeNode("Operator", CurrentToken.Value, CurrentToken.Position);
                    node.AddChild(opNode);
                    _currentIndex++;

                    var nextTerm = ParseTerm();
                    if (nextTerm != null)
                    {
                        node.AddChild(nextTerm);
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ожидается терм после оператора +", CurrentToken?.Position ?? startPos));
                    }
                }
            }

            return node;
        }

        private ParseTreeNode ParseTerm()
        {
            var node = new ParseTreeNode("Term");
            int startPos = CurrentToken?.Position ?? 0;

            var factor = ParseFactor();
            if (factor != null)
            {
                node.AddChild(factor);

                while (Match(TokenType.Multiply))
                {
                    var opNode = new ParseTreeNode("Operator", CurrentToken.Value, CurrentToken.Position);
                    node.AddChild(opNode);
                    _currentIndex++;

                    var nextFactor = ParseFactor();
                    if (nextFactor != null)
                    {
                        node.AddChild(nextFactor);
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ожидается множитель после оператора *", CurrentToken?.Position ?? startPos));
                    }
                }
            }

            return node;
        }

        private ParseTreeNode ParseFactor()
        {
            if (Match(TokenType.Identifier))
            {
                var node = new ParseTreeNode("Identifier", CurrentToken.Value, CurrentToken.Position);
                _currentIndex++;
                return node;
            }

            if (Match(TokenType.Number))
            {
                var node = new ParseTreeNode("Number", CurrentToken.Value, CurrentToken.Position);
                _currentIndex++;
                return node;
            }

            if (Match(TokenType.LeftParenthesis))
            {
                var node = new ParseTreeNode("Parentheses");
                node.AddChild(new ParseTreeNode("Symbol", "(", CurrentToken.Position));
                _currentIndex++;

                var expr = ParseExpression();
                if (expr != null)
                {
                    node.AddChild(expr);

                    if (Match(TokenType.RightParenthesis))
                    {
                        node.AddChild(new ParseTreeNode("Symbol", ")", CurrentToken.Position));
                        _currentIndex++;
                        return node;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ожидается закрывающая скобка", CurrentToken?.Position ?? 0));
                    }
                }
            }

            _errors.Add(new ParseError("Ожидается идентификатор, число или выражение в скобках", CurrentToken?.Position ?? 0));
            return null;
        }

        private ParseTreeNode ParseStatement()
        {
            if (Match(TokenType.IfKeyword))
            {
                return ParseConditionalStatement();
            }
            else
            {
                return ParseExpression();
            }
        }
    }
}