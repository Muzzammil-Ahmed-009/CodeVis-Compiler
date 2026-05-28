namespace CodeVisApi.Models
{
    public enum TokenType
    {
        Keyword,     // int, print
        Identifier,  // x, y
        Operator,    // +, -, *, /, =
        Literal,     // 10, 5
        Punctuation, // ;, (, )
        EOF
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Token(TokenType type, string value, int line, int column)
        {
            Type = type;
            Value = value;
            Line = line;
            Column = column;
        }

        public override string ToString() => $"[{Type}: '{Value}']";
    }
}
