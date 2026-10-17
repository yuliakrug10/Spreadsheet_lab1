namespace Core
{
    public enum TokenTypes
    {
        Number,
        Operator,
        Reference,
        Function,    
        Parenthesis,
        Comma,
        End
    }

    public class Token
    {
        public TokenTypes Type { get; }
        public string Value { get; }

        public Token(TokenTypes type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type}: {Value}";
    }
}