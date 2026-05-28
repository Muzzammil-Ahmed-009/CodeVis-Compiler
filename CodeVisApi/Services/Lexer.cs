using System.Collections.Generic;
using CodeVisApi.Models;

namespace CodeVisApi.Services
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;
        private readonly List<string> _errors;

        public Lexer(string input, List<string> errors)
        {
            _input = input;
            _position = 0;
            _line = 1;
            _column = 1;
            _errors = errors;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_position < _input.Length)
            {
                char currentChar = _input[_position];

                if (char.IsWhiteSpace(currentChar))
                {
                    if (currentChar == '\n')
                    {
                        _line++;
                        _column = 0;
                    }
                    Advance();
                    continue;
                }

                if (char.IsLetter(currentChar))
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                if (char.IsDigit(currentChar))
                {
                    tokens.Add(ReadNumber());
                    continue;
                }

                if (currentChar == '"')
                {
                    tokens.Add(ReadString());
                    continue;
                }

                switch (currentChar)
                {
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '=':
                        tokens.Add(new Token(TokenType.Operator, currentChar.ToString(), _line, _column));
                        Advance();
                        break;
                    case ';':
                    case '(':
                    case ')':
                        tokens.Add(new Token(TokenType.Punctuation, currentChar.ToString(), _line, _column));
                        Advance();
                        break;
                    default:
                        _errors.Add($"Lexical Error: Unexpected character '{currentChar}' at line {_line}, col {_column}");
                        Advance(); // Skip invalid char to continue parsing
                        break;
                }
            }

            tokens.Add(new Token(TokenType.EOF, "", _line, _column));
            return tokens;
        }

        private Token ReadIdentifierOrKeyword()
        {
            int startColumn = _column;
            string value = "";
            while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
            {
                value += _input[_position];
                Advance();
            }

            if (value == "int" || value == "printf" || value == "string" || value == "String")
            {
                return new Token(TokenType.Keyword, value, _line, startColumn);
            }

            return new Token(TokenType.Identifier, value, _line, startColumn);
        }

        private Token ReadNumber()
        {
            int startColumn = _column;
            string value = "";
            bool hasDecimal = false;

            while (_position < _input.Length && (char.IsDigit(_input[_position]) || _input[_position] == '.'))
            {
                if (_input[_position] == '.')
                {
                    if (hasDecimal) break; // Only allow one decimal point
                    hasDecimal = true;
                }
                value += _input[_position];
                Advance();
            }

            return new Token(TokenType.Literal, value, _line, startColumn);
        }

        private Token ReadString()
        {
            int startColumn = _column;
            Advance(); // skip opening quote
            string value = "";
            while (_position < _input.Length && _input[_position] != '"')
            {
                value += _input[_position];
                Advance();
            }
            if (_position < _input.Length && _input[_position] == '"')
            {
                Advance(); // skip closing quote
            }
            else
            {
                _errors.Add($"Lexical Error: Unterminated string literal at line {_line}");
            }
            return new Token(TokenType.Literal, $"\"{value}\"", _line, startColumn);
        }

        private void Advance()
        {
            _position++;
            _column++;
        }
    }
}
