namespace Stenguage.Ast
{
    public enum TokenType
    {
        // Literal types
        Number,
        String,
        Identifier,

        // Keywords
        Let, Const,      // let, const
        Fn, Return,      // fn, return
        If, Else,        // if, else
        For, While,      // for, while
        Break, Continue, // break, continue
        Skip,            // skip
        Import, From,    // import, from
        Class,           // class

        // Operations and Groupings
        OpenParen, CloseParen, // ()
        Comma,                 // ,
        Colon,                 // :
        Period,                // .
        OpenBrace, CloseBrace, // {}
        OpenBrack, CloseBrack, // []
        BinaryOperator,        // +, -, *, /
        Equals,                // =
        Not,                   // !
        EE,                    // ==
        NE,                    // !=
        LT,                    // <
        LTE,                   // <=
        GT,                    // >
        GTE,                   // >=
        And,                   // &&
        Or,                    // ||
        EOF,
    }

    public class Token
    {
        public static string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890_";

        public static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            ["let"] = TokenType.Let,
            ["const"] = TokenType.Const,
            ["fn"] = TokenType.Fn,
            ["return"] = TokenType.Return,
            ["if"] = TokenType.If,
            ["else"] = TokenType.Else,
            ["for"] = TokenType.For,
            ["while"] = TokenType.While,
            ["break"] = TokenType.Break,
            ["continue"] = TokenType.Continue,
            ["import"] = TokenType.Import,
            ["from"] = TokenType.From,
            ["skip"] = TokenType.Skip,
            ["class"] = TokenType.Class
        };

        public static Dictionary<char, TokenType> Symbols = new Dictionary<char, TokenType>
        {
            ['('] = TokenType.OpenParen,
            [')'] = TokenType.CloseParen,
            ['{'] = TokenType.OpenBrace,
            ['}'] = TokenType.CloseBrace,
            ['['] = TokenType.OpenBrack,
            [']'] = TokenType.CloseBrack,
            [','] = TokenType.Comma,
            [':'] = TokenType.Colon,
        };

        public string Value { get; set; }
        public TokenType Type { get; set; }
        public Position Start { get; set; }
        public Position End { get; set; }

        public Token(string value, TokenType type, Position start, Position end)
        {
            Value = value;
            Type = type;
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            if (Start != null && End != null)
            {
                return $"[\"{Value}\", {Type}, Line: {Start.Line}, Column: {Start.Column}]";
            }
            else
            {
                return $"[\"{Value}\", {Type}]";
            }
        }

    }
}
