namespace cslox
{
    public enum TokenType
    {
        // Single character tokens
        PAREN_LEFT, PAREN_RIGHT, BRACE_LEFT, BRACE_RIGHT,
        COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

        // One or two character tokens
        BANG, BANG_EQUAL, EQUAL, EQUAL_EQUAL,
        GT, GTE, LT, LTE,

        // Literal
        IDENTIFIER, STRING, NUMBER,

        // Keywords
        AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
        PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

        //
        EOF
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public object Literal { get; set; }
        public int Line;

        private Token() { }

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Type, Lexeme, Literal);
        }

    }
}