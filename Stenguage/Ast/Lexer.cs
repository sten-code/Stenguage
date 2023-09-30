using static Stenguage.Ast.Token;

namespace Stenguage.Ast
{
    public class Lexer
    {
        private char CurrentChar;

        public string SourceCode;
        public Position Position;

        public Lexer(string sourceCode)
        {
            Position = new Position(0, 0, 0);
            SourceCode = sourceCode;
            if (SourceCode.Length > 0)
            {
                Position.Index = 0;
                Position.Column = 0;
                CurrentChar = sourceCode[0];
            }
        }

        public void NextChar()
        {
            if (SourceCode.Length > Position.Index + 1)
            {
                Position.Index++;
                CurrentChar = SourceCode[Position.Index];
                if (CurrentChar == '\n')
                {
                    Position.Column = -1; // It will increase it to 0 the next iteration
                    Position.Line++;
                    NextChar();
                }
                else
                {
                    Position.Column++;
                }
            }
            else
            {
                CurrentChar = '\0';
            }
        }

        public List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();

            while (CurrentChar != '\0')
            {
                if ("\n\t\r ".Contains(CurrentChar))
                {
                    NextChar();
                    continue;
                }

                if ("(){}[],:".Contains(CurrentChar))
                    tokens.Add(new Token(CurrentChar.ToString(), Symbols[CurrentChar], Position.Copy(), Position.Copy()));

                else if (CurrentChar == '/')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar == '/')
                    {
                        while (CurrentChar != '\n' && CurrentChar != '\r')
                        {
                            NextChar();
                        }
                    }
                    else if (CurrentChar == '*')
                    {
                        while (CurrentChar != '\0')
                        {
                            NextChar();
                            if (CurrentChar == '*')
                            {
                                NextChar();
                                if (CurrentChar == '/')
                                    break;
                            }
                        }
                    }
                    else if (CurrentChar == '=')
                    {
                        tokens.Add(new Token("/=", TokenType.BinaryOperator, start, Position.Copy()));
                    }
                    else
                    {
                        tokens.Add(new Token("/", TokenType.BinaryOperator, start, start.Copy()));
                        continue;
                    }
                }

                else if (CurrentChar == '+' || CurrentChar == '-' || CurrentChar == '*' || CurrentChar == '%' || CurrentChar == '^')
                {
                    Position start = Position.Copy();
                    string op = CurrentChar.ToString();
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        tokens.Add(new Token(op + "=", TokenType.BinaryOperator, start, Position.Copy()));
                    }
                    else
                    {
                        tokens.Add(new Token(op, TokenType.BinaryOperator, start, start.Copy()));
                        continue;
                    }
                }

                else if (CurrentChar == '&')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar != '&')
                    {
                        Console.WriteLine("Error: '&' doesn't exist, did you mean '&&'?");
                        return new List<Token>();
                    }

                    tokens.Add(new Token("&&", TokenType.And, start, Position.Copy()));
                }
                else if (CurrentChar == '|')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar != '|')
                    {
                        Console.WriteLine("Error: '|' doesn't exist, did you mean '||'?");
                        return new List<Token>();
                    }

                    tokens.Add(new Token("||", TokenType.Or, start, Position.Copy()));
                }
                else if (CurrentChar == '=')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        tokens.Add(new Token("==", TokenType.EE, start, Position.Copy()));
                    }
                    else
                    {
                        tokens.Add(new Token("=", TokenType.Equals, start, start.Copy()));
                        continue;
                    }
                }
                else if (CurrentChar == '!')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        tokens.Add(new Token("!=", TokenType.NE, start, Position.Copy()));
                    }
                    else
                    {
                        tokens.Add(new Token("!", TokenType.Not, start, start.Copy()));
                        continue;
                    }
                }
                else if (CurrentChar == '<')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        tokens.Add(new Token("<=", TokenType.LTE, start, Position.Copy()));
                    }
                    else
                    {
                        tokens.Add(new Token("<", TokenType.LT, start, start.Copy()));
                        continue;
                    }
                }
                else if (CurrentChar == '>')
                {
                    Position start = Position.Copy();
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        tokens.Add(new Token(">=", TokenType.GTE, start, Position.Copy()));
                    }
                    else
                    {
                        tokens.Add(new Token(">", TokenType.GT, start, start.Copy()));
                        continue;
                    }
                }

                else if (CurrentChar == '\"')
                    tokens.Add(GetString());
                else if (".0123456789".Contains(CurrentChar))
                {
                    tokens.Add(GetNumberOrPeriod());
                    continue;
                }
                else if (IdentifierChars.Contains(CurrentChar))
                {
                    tokens.Add(GetIdentifierOrKeyword());
                    continue;
                }
                else
                {
                    Console.WriteLine($"Error: Unrecognized character found in the source code: {CurrentChar}, {(int)CurrentChar}");
                    return new List<Token>();
                }

                NextChar();
            }

            tokens.Add(new Token("EOF", TokenType.EOF, new Position(0, 0, 0), new Position(0, 0, 0)));
            return tokens;
        }

        private Token GetString()
        {
            Position start = Position.Copy();
            string str = string.Empty;
            bool escape = false;
            NextChar();
            while (!"\0".Contains(CurrentChar))
            {
                if (escape)
                {
                    escape = false;
                    if (CurrentChar == 'u')
                    {
                        string num = "";
                        for (int i = 1; i < 5; i++)
                        {
                            NextChar();
                            num += CurrentChar;
                        }
                        str += (char)Convert.ToInt16(num, 16);
                    }
                    else if ("\"\\/bfnrt".Contains(CurrentChar)) str += "\"\\/\b\f\n\r\t"["\"\\/bfnrt".IndexOf(CurrentChar)];
                    else str += CurrentChar;
                }
                else
                {
                    if (CurrentChar == '\"') break;
                    if (CurrentChar == '\\') escape = true;
                    else if (CurrentChar == '\r') str += "\n";
                    else str += CurrentChar;
                }
                NextChar();
            }
            return new Token(str, TokenType.String, start, Position.Copy());
        }

        private Token GetNumberOrPeriod()
        {
            Position start = Position.Copy();
            string num = string.Empty;
            int dotCount = 0;

            if (CurrentChar == '.')
            {
                num = "0.";
                dotCount = 1;
                NextChar();
                if (!"0123456789".Contains(CurrentChar))
                {
                    return new Token(".", TokenType.Period, start, Position.Copy());
                }
            }

            Position end = Position.Copy();

            while (".0123456789".Contains(CurrentChar))
            {
                num += CurrentChar;
                if (CurrentChar == '.')
                {
                    if (dotCount == 1)
                    {
                        Console.WriteLine("Error: A number can only contain 1 dot.");
                    }
                    else
                    {
                        dotCount++;
                    }
                }
                NextChar();
                if (".0123456789".Contains(CurrentChar))
                {
                    end = Position.Copy();
                }
            }

            return new Token(num, TokenType.Number, start, end);
        }

        private Token GetIdentifierOrKeyword()
        {
            Position start = Position.Copy();
            string str = string.Empty;
            Position end = Position.Copy();
            while (IdentifierChars.Contains(CurrentChar))
            {
                str += CurrentChar;
                NextChar();
                if (IdentifierChars.Contains(CurrentChar))
                {
                    end = Position.Copy();
                }
            }
            return new Token(str, Keywords.ContainsKey(str) ? Keywords[str] : TokenType.Identifier, start, end);
        }

    }
}
