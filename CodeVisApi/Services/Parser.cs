using System.Collections.Generic;
using CodeVisApi.Models;

namespace CodeVisApi.Services
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _position;
        private readonly List<string> _errors;

        public Parser(List<Token> tokens, List<string> errors)
        {
            _tokens = tokens;
            _position = 0;
            _errors = errors;
        }

        private Token CurrentToken => _position < _tokens.Count ? _tokens[_position] : _tokens[_tokens.Count - 1];

        private void Advance()
        {
            if (_position < _tokens.Count) _position++;
        }

        private bool Match(TokenType type, string value = null)
        {
            if (CurrentToken.Type == type && (value == null || CurrentToken.Value == value))
            {
                Advance();
                return true;
            }
            return false;
        }

        private void Expect(TokenType type, string value = null)
        {
            if (CurrentToken.Type == type && (value == null || CurrentToken.Value == value))
            {
                Advance();
            }
            else
            {
                string expected = value != null ? $"'{value}'" : type.ToString();
                _errors.Add($"Syntax Error at line {CurrentToken.Line}, col {CurrentToken.Column}: Expected {expected}, but found '{CurrentToken.Value}'");
            }
        }

        public AstNode ParseProgram()
        {
            var programNode = new AstNode("Program");

            while (CurrentToken.Type != TokenType.EOF)
            {
                try
                {
                    var stmt = ParseStatement();
                    if (stmt != null)
                    {
                        programNode.AddChild(stmt);
                    }
                    else
                    {
                        // To prevent infinite loops on error, advance
                        Advance();
                    }
                }
                catch
                {
                    // Basic panic mode recovery: skip to next semicolon
                    while (CurrentToken.Type != TokenType.EOF && CurrentToken.Value != ";")
                    {
                        Advance();
                    }
                    if (CurrentToken.Value == ";") Advance();
                }
            }

            return programNode;
        }

        private AstNode ParseStatement()
        {
            if (CurrentToken.Type == TokenType.Keyword && (CurrentToken.Value == "int" || CurrentToken.Value == "string" || CurrentToken.Value == "String"))
            {
                return ParseVarDeclaration();
            }
            else if (CurrentToken.Type == TokenType.Keyword && CurrentToken.Value == "printf")
            {
                return ParsePrintStatement();
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                return ParseAssignment();
            }
            else
            {
                _errors.Add($"Syntax Error at line {CurrentToken.Line}: Unexpected token '{CurrentToken.Value}'");
                throw new System.Exception("Syntax error");
            }
        }

        private AstNode ParseVarDeclaration()
        {
            var node = new AstNode("VarDeclaration");
            string type = CurrentToken.Value;
            if (type == "int" || type == "string" || type == "String")
            {
                node.Attributes["type"] = type;
                Advance();
            }
            else
            {
                _errors.Add($"Syntax Error at line {CurrentToken.Line}: Expected type, but found '{CurrentToken.Value}'");
                throw new System.Exception("Syntax error");
            }

            var assignNode = new AstNode("Assignment");

            if (CurrentToken.Type == TokenType.Identifier)
            {
                var idNode = new AstNode("Identifier", CurrentToken.Value);
                assignNode.AddChild(idNode);
                Advance();
            }
            else
            {
                _errors.Add($"Syntax Error at line {CurrentToken.Line}: Expected identifier after '{type}'");
                throw new System.Exception("Syntax error");
            }

            // Check if there is an initialization assignment '='
            if (CurrentToken.Type == TokenType.Operator && CurrentToken.Value == "=")
            {
                Advance(); // skip '='
                assignNode.AddChild(ParseExpression());
                Expect(TokenType.Punctuation, ";");
            }
            else
            {
                // Uninitialized: assign default value to maintain AST compatibility
                string defaultValue = (type == "int") ? "0" : "\"\"";
                var defaultLiteralNode = new AstNode("Literal", defaultValue);
                assignNode.AddChild(defaultLiteralNode);
                Expect(TokenType.Punctuation, ";");
            }

            node.AddChild(assignNode);
            return node;
        }

        private AstNode ParseAssignment()
        {
            var node = new AstNode("Assignment");
            
            var idNode = new AstNode("Identifier", CurrentToken.Value);
            node.AddChild(idNode);
            Advance();

            Expect(TokenType.Operator, "=");
            node.AddChild(ParseExpression());
            Expect(TokenType.Punctuation, ";");

            return node;
        }

        private AstNode ParsePrintStatement()
        {
            var node = new AstNode("PrintStatement");
            Expect(TokenType.Keyword, "printf");
            Expect(TokenType.Punctuation, "(");
            node.AddChild(ParseExpression());
            Expect(TokenType.Punctuation, ")");
            Expect(TokenType.Punctuation, ";");
            return node;
        }

        private AstNode ParseExpression()
        {
            var left = ParseTerm();

            while (CurrentToken.Value == "+" || CurrentToken.Value == "-")
            {
                string op = CurrentToken.Value;
                Advance();
                var right = ParseTerm();
                
                var opNode = new AstNode("BinaryOp", op);
                opNode.AddChild(left);
                opNode.AddChild(right);
                left = opNode;
            }

            return left;
        }

        private AstNode ParseTerm()
        {
            var left = ParseFactor();

            while (CurrentToken.Value == "*" || CurrentToken.Value == "/")
            {
                string op = CurrentToken.Value;
                Advance();
                var right = ParseFactor();

                var opNode = new AstNode("BinaryOp", op);
                opNode.AddChild(left);
                opNode.AddChild(right);
                left = opNode;
            }

            return left;
        }

        private AstNode ParseFactor()
        {
            if (CurrentToken.Type == TokenType.Literal)
            {
                var node = new AstNode("Literal", CurrentToken.Value);
                Advance();
                return node;
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                var node = new AstNode("Identifier", CurrentToken.Value);
                Advance();
                return node;
            }
            else if (CurrentToken.Value == "(")
            {
                Advance();
                var node = ParseExpression();
                Expect(TokenType.Punctuation, ")");
                return node;
            }
            else
            {
                _errors.Add($"Syntax Error at line {CurrentToken.Line}: Expected expression, found '{CurrentToken.Value}'");
                throw new System.Exception("Syntax error");
            }
        }
    }
}
