using Stenguage.Ast.Expressions;
using Stenguage.Errors;

namespace Stenguage.Ast
{
    public class Parser
    {
        public string SourceCode { get; set; }

        private List<Token> Tokens;

        public Parser(string sourceCode)
        {
            SourceCode = sourceCode;
            Tokens = new List<Token>(); 
        }

        private Token At()
        {
            return Tokens[0];
        }

        private Token Eat()
        {
            Token token = At();
            Tokens.RemoveAt(0);
            return token;
        }

        private ParseResult Expect(TokenType type)
        {
            Token token = At();
            if (token.Type != type)
            {
                Console.WriteLine(token);
                return new ParseResult().Failure(new Error($"Expected '{type}', got '{token.Type}'.", SourceCode, token.Start, token.End));
            }
            Tokens.RemoveAt(0);
            return new ParseResult();
        }

        public ParseResult ProduceAST()
        {
            Lexer lexer = new Lexer(SourceCode);
            Tokens = lexer.Tokenize();

            //foreach (Token token in Tokens)
            //{
            //    Console.WriteLine(token);
            //}

            ParseResult res = new ParseResult();

            if (Tokens.Count <= 1)
                return res.Success(new Program(new List<Expr>(), new Position(0, 0, 0), new Position(0, 0, 0)));
            Program program = new Program(new List<Expr>(), Tokens[0].Start.Copy(), Tokens[Tokens.Count - 2].End.Copy());

            while (At().Type != TokenType.EOF)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                program.Body.Add(expr);
            }

            return res.Success(program);
        }

        private ParseResult ParseStmt()
        {
            ParseResult res = new ParseResult();
            Expr expr;
            switch (At().Type)
            {
                case TokenType.Let:
                case TokenType.Const:
                    expr = res.Register(ParseVarDeclaration());
                    break;
                case TokenType.Fn:
                    expr = res.Register(ParseFnDeclaration());
                    break;
                case TokenType.If:
                    expr = res.Register(ParseIfStatement());
                    break;
                case TokenType.For:
                    expr = res.Register(ParseForLoopExpr());
                    break;
                case TokenType.Break:
                    expr = res.Register(ParseBreakExpr());
                    break;
                case TokenType.Continue:
                    expr = res.Register(ParseContinueExpr());
                    break;
                case TokenType.Return:
                    expr = res.Register(ParseReturnExpr());
                    break;
                case TokenType.Skip:
                    expr = res.Register(ParseSkipExpr());
                    break;
                case TokenType.While:
                    expr = res.Register(ParseWhileExpr());
                    break;
                case TokenType.Import:
                    expr = res.Register(ParseImportExpr());
                    break;
                case TokenType.From:
                    expr = res.Register(ParseFromExpr());
                    break;
                case TokenType.Class:
                    expr = res.Register(ParseClassExpr());
                    break;
                default:
                    expr = res.Register(ParseExpr());
                    break;
            }
            if (res.ShouldReturn()) return res;
            return res.Success(expr);
        }

        private ParseResult ParseClassExpr()
        {
            ParseResult res = new ParseResult();

            res.Register(Expect(TokenType.Class));
            if (res.ShouldReturn()) return res;
            Position start = At().Start.Copy();

            Token identifier = At();
            res.Register(Expect(TokenType.Identifier));
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.OpenBrace));
            if (res.ShouldReturn()) return res;

            ClassDeclaration classDecl = new ClassDeclaration(identifier.Value, new List<Expr>(), start, null);
            while (At().Type != TokenType.CloseBrace && At().Type != TokenType.EOF)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                classDecl.Body.Add(expr);

            }

            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;
            classDecl.End = At().End.Copy();

            return res.Success(classDecl);
        }

        private ParseResult ParseImportExpr()
        {
            ParseResult res = new ParseResult();

            res.Register(Expect(TokenType.Import));
            if (res.ShouldReturn()) return res;
            Position start = At().Start.Copy();
            Position end = At().End.Copy();

            string path = "";
            while (At().Type != TokenType.EOF)
            {
                Token token = Eat();
                end = token.End.Copy();
                path = Path.Combine(path, token.Value);
                if (At().Type != TokenType.Period)
                {
                    break;
                }
                if (At().Value == "*" && At().Type == TokenType.BinaryOperator)
                {
                    end = Eat().End.Copy();
                    path += "*";
                    break;
                }
                Eat();
            }

            return res.Success(new ImportExpr(path, false, new List<string>(), start, end));
        }

        private ParseResult ParseFromExpr()
        {
            ParseResult res = new ParseResult();
            // from <module path> import <names>

            // from keyword
            res.Register(Expect(TokenType.From));
            if (res.ShouldReturn()) return res;
            Position start = At().Start.Copy();
            Position end = At().End.Copy();

            // module path
            string path = "";
            while (At().Type != TokenType.EOF)
            {
                Token token = Eat();
                end = token.End.Copy();
                path = Path.Combine(path, token.Value);
                if (At().Type != TokenType.Period)
                {
                    break;
                }
                if (At().Value == "*" && At().Type == TokenType.BinaryOperator)
                {
                    end = Eat().End.Copy();
                    path += "*";
                    break;
                }
                Eat();
            }

            // import keywords
            res.Register(Expect(TokenType.Import));
            if (res.ShouldReturn()) return res;

            bool isStaticImport = false;
            List<string> names = new List<string>();
            while (At().Type != TokenType.EOF)
            {
                Token token = Eat();
                end = token.End.Copy();
                if (token.Value == "*" && token.Type == TokenType.BinaryOperator)
                {
                    end = token.End.Copy();
                    isStaticImport = true;
                    break;
                }

                names.Add(token.Value);
                if (At().Type != TokenType.Comma)
                    break;
                Eat();
            }

            return res.Success(new ImportExpr(path, isStaticImport, names, start, end));
        }

        private ParseResult ParseWhileExpr()
        {
            ParseResult res = new ParseResult();

            Position start = At().Start.Copy();
            res.Register(Expect(TokenType.While));
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.OpenParen));
            if (res.ShouldReturn()) return res;

            Expr condition = res.Register(ParseExpr());
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.CloseParen));
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.OpenBrace));
            if (res.ShouldReturn()) return res;

            List<Expr> body = new List<Expr>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                body.Add(expr);
            }

            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;

            return res.Success(new WhileLoopExpr(condition, body, start, end));
        }

        private ParseResult ParseBreakExpr()
        {
            ParseResult res = new ParseResult();

            Token token = At();
            res.Register(Expect(TokenType.Break));
            if (res.ShouldReturn()) return res;

            return res.Success(new BreakExpr(token.Start.Copy(), token.End.Copy()));
        }

        private ParseResult ParseContinueExpr()
        {
            ParseResult res = new ParseResult();
            Token token = At();
            res.Register(Expect(TokenType.Continue));
            if (res.ShouldReturn()) return res;

            return res.Success(new ContinueExpr(token.Start.Copy(), token.End.Copy()));
        }

        private ParseResult ParseReturnExpr()
        {
            ParseResult res = new ParseResult();

            Position start = At().Start.Copy();
            res.Register(Expect(TokenType.Return));
            if (res.ShouldReturn()) return res;

            Expr expr = res.Register(ParseExpr());
            if (res.ShouldReturn()) return res;

            return res.Success(new ReturnExpr(expr, start, expr.End.Copy()));
        }

        private ParseResult ParseSkipExpr()
        {
            ParseResult res = new ParseResult();

            Position start = At().Start.Copy();
            res.Register(Expect(TokenType.Skip));
            if (res.ShouldReturn()) return res;

            Expr expr = res.Register(ParseExpr());
            if (res.ShouldReturn()) return res;

            return res.Success(new SkipExpr(expr, start, expr.End.Copy()));
        }

        private ParseResult ParseForLoopExpr()
        {
            ParseResult res = new ParseResult();

            Position start = At().Start.Copy();
            res.Register(Expect(TokenType.For));
            if (res.ShouldReturn()) return res;
            res.Register(Expect(TokenType.OpenParen));
            if (res.ShouldReturn()) return res;

            Expr identifier = res.Register(ParsePrimaryExpr());
            if (res.ShouldReturn()) return res;
            if (identifier.Kind != NodeType.Identifier) return res.Failure(new Error($"Expected identifier, got '{identifier.Kind}'", SourceCode, identifier.Start, identifier.End));
            string name = ((Identifier)identifier).Symbol;

            res.Register(Expect(TokenType.Colon));
            if (res.ShouldReturn()) return res;

            Expr listExpr = res.Register(ParseExpr());
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.CloseParen));
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.OpenBrace));
            if (res.ShouldReturn()) return res;

            List<Expr> body = new List<Expr>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                body.Add(expr);
            }

            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;

            return res.Success(new ForLoopExpr(name, listExpr, body, start, end));
        }

        private ParseResult ParseIfStatement()
        {
            ParseResult res = new ParseResult();
            Position start = At().Start.Copy();

            res.Register(Expect(TokenType.If));
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.OpenParen));
            if (res.ShouldReturn()) return res;

            Expr condition = res.Register(ParseExpr());
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.CloseParen));
            if (res.ShouldReturn()) return res;

            res.Register(Expect(TokenType.OpenBrace));
            if (res.ShouldReturn()) return res;

            List<Expr> body = new List<Expr>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                body.Add(expr);
            }

            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;

            if (At().Type != TokenType.Else)
                return res.Success(new IfStatement(condition, body, new List<Expr>(), start, end));
            Eat();

            // Else if statements
            if (At().Type == TokenType.If)
            {
                Expr expr = res.Register(ParseIfStatement());
                if (res.ShouldReturn()) return res;
                return res.Success(new IfStatement(condition, body, new List<Expr> { expr }, start, end));
            }

            res.Register(Expect(TokenType.OpenBrace));
            if (res.ShouldReturn()) return res;

            List<Expr> elseBody = new List<Expr>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                elseBody.Add(expr);
            }

            end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;

            return res.Success(new IfStatement(condition, body, elseBody, start, end));
        }

        private ParseResult ParseFnDeclaration()
        {
            ParseResult res = new ParseResult();
            Position start = At().Start.Copy();

            res.Register(Expect(TokenType.Fn));
            if (res.ShouldReturn()) return res;

            Token token = At();
            res.Register(Expect(TokenType.Identifier));
            if (res.ShouldReturn()) return res;
            string name = token.Value;

            (ParseResult result, List<Expr> args, Position e) = ParseArgs();
            res.Register(result);
            if (res.ShouldReturn()) return res;

            List<string> parameters = new List<string>();
            foreach (Expr arg in args)
            {
                if (arg.Kind != NodeType.Identifier)
                    return res.Failure(new Error($"Expected an identifier inside function parameters, got '{arg.Kind}'.", SourceCode, arg.Start, arg.End));

                parameters.Add(((Identifier)arg).Symbol);
            }

            res.Register(Expect(TokenType.OpenBrace));
            if (res.ShouldReturn()) return res;

            List<Expr> body = new List<Expr>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
            {
                Expr expr = res.Register(ParseStmt());
                if (res.ShouldReturn()) return res;
                body.Add(expr);
            }
            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;

            return res.Success(new FunctionDeclaration(parameters, name, body, start, end));
        }

        private ParseResult ParseVarDeclaration()
        {
            ParseResult res = new ParseResult();
            Position start = At().Start.Copy();
            bool isConstant = Eat().Type == TokenType.Const;

            Token token = At();
            res.Register(Expect(TokenType.Identifier));
            if (res.ShouldReturn()) return res;
            string identifier = token.Value;

            if (At().Type == TokenType.Equals)
            {
                Eat();
                Expr expr = res.Register(ParseExpr());
                if (res.ShouldReturn()) return res;
                return res.Success(new VarDeclaration(identifier, expr, isConstant, start, expr.End.Copy()));
            }
            else if (isConstant)
            {
                return res.Failure(new Error("Must assign a value to a constant.", SourceCode, start, At().End));
            }
            else
            {
                return res.Success(new VarDeclaration(identifier, null, isConstant, start, token.End.Copy()));
            }
        }

        private ParseResult ParseAssignmentExpr()
        {
            ParseResult res = new ParseResult();

            Expr left = res.Register(ParseObjectExpr());
            if (res.ShouldReturn()) return res;

            if (At().Type == TokenType.Equals)
            {
                Eat();
                Expr value = res.Register(ParseAssignmentExpr());
                if (res.ShouldReturn()) return res;
                return res.Success(new AssignmentExpr(left, value, left.Start.Copy(), value.End.Copy()));
            }
            else if ((At().Value == "+=" || At().Value == "-=" || At().Value == "*=" || At().Value == "/=" || At().Value == "%=") && At().Type == TokenType.BinaryOperator)
            {
                string binop = At().Value[0].ToString();
                Eat();
                Expr value = res.Register(ParseAssignmentExpr());
                if (res.ShouldReturn()) return res;
                return res.Success(new AssignmentExpr(left, new BinaryExpr(left, value, binop, left.Start.Copy(), value.End.Copy()), left.Start.Copy(), value.End.Copy()));
            }

            return res.Success(left);
        }

        private ParseResult ParseExpr()
        {
            ParseResult res = new ParseResult();
            Expr left = res.Register(ParseAssignmentExpr());
            if (res.ShouldReturn()) return res;

            while (At().Type == TokenType.And || At().Type == TokenType.Or) // && and ||
            {
                string op = Eat().Value;
                Expr right = res.Register(ParseAssignmentExpr());
                if (res.ShouldReturn()) return res;
                left = new BinaryExpr(left, right, op, left.Start.Copy(), right.End.Copy());
            }
            return res.Success(left);
        }

        private (ParseResult, List<Expr>, Position) ParseArgs()
        {
            ParseResult res = new ParseResult();
            res.Register(Expect(TokenType.OpenParen));
            if (res.ShouldReturn()) return (res, new List<Expr>(), At().End.Copy());

            (ParseResult result, List<Expr> args) = At().Type == TokenType.CloseParen ? (res, new List<Expr>()) : ParseArgsList();
            res.Register(result);
            if (res.ShouldReturn()) return (res, new List<Expr>(), At().End.Copy());

            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseParen));
            if (res.ShouldReturn()) return (res, new List<Expr>(), end);
            return (res, args, end);
        }

        private (ParseResult, List<Expr>) ParseArgsList()
        {
            ParseResult res = new ParseResult();
            Expr arg = res.Register(ParseExpr());
            if (res.ShouldReturn()) return (res, new List<Expr>());

            List<Expr> args = new List<Expr> { arg };
            while (At().Type == TokenType.Comma && Eat() != null)
            {
                Expr expr = res.Register(ParseAssignmentExpr());
                if (res.ShouldReturn()) return (res, new List<Expr>());
                args.Add(expr);
            }
            return (res, args);
        }

        private ParseResult ParseListExpr()
        {
            ParseResult res = new ParseResult();
            Position start = At().Start.Copy();
            if (At().Type != TokenType.OpenBrack)
            {
                return ParseCompExpr();
            }

            Eat();
            List<Expr> items = new List<Expr>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrack)
            {
                Expr item = res.Register(ParseExpr());
                if (res.ShouldReturn()) return res;
                items.Add(item);
                if (At().Type == TokenType.CloseBrack)
                    break;
                res.Register(Expect(TokenType.Comma));
                if (res.ShouldReturn()) return res;
            }

            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrack));
            if (res.ShouldReturn()) return res;
            return res.Success(new ListLiteral(items, start, end));
        }

        private ParseResult ParseObjectExpr()
        {
            ParseResult res = new ParseResult();
            Position start = At().Start.Copy();
            if (At().Type != TokenType.OpenBrace)
            {
                return ParseListExpr();
            }

            Eat();
            List<Property> properties = new List<Property>();
            while (At().Type != TokenType.EOF && At().Type != TokenType.CloseBrace)
            {
                Token token = At();
                res.Register(Expect(TokenType.Identifier));
                if (res.ShouldReturn()) return res;
                string key = token.Value;

                if (At().Type == TokenType.Comma)
                {
                    Eat();
                    properties.Add(new Property(key, null));
                    continue;
                }

                res.Register(Expect(TokenType.Colon));
                if (res.ShouldReturn()) return res;

                Expr value = res.Register(ParseExpr());
                if (res.ShouldReturn()) return res;

                properties.Add(new Property(key, value));
                if (At().Type != TokenType.CloseBrace)
                {
                    res.Register(Expect(TokenType.Comma));
                    if (res.ShouldReturn()) return res;
                }
            }
            Position end = At().End.Copy();
            res.Register(Expect(TokenType.CloseBrace));
            if (res.ShouldReturn()) return res;

            return res.Success(new ObjectLiteral(properties, start, end));
        }

        private ParseResult ParseCompExpr()
        {
            ParseResult res = new ParseResult();
            Expr left = res.Register(ParseAdditiveExpr());
            if (res.ShouldReturn()) return res;

            while (At().Type == TokenType.EE || At().Type == TokenType.NE ||  // == and !=
                   At().Type == TokenType.LT || At().Type == TokenType.LTE || // <  and <=
                   At().Type == TokenType.GT || At().Type == TokenType.GTE) // >  and >=
            {
                string op = Eat().Value;
                Expr right = res.Register(ParseAdditiveExpr());
                if (res.ShouldReturn()) return res;
                left = new BinaryExpr(left, right, op, left.Start.Copy(), right.End.Copy());
            }
            return res.Success(left);
        }

        private ParseResult ParseAdditiveExpr()
        {
            ParseResult res = new ParseResult();
            Expr left = res.Register(ParseMultiplicative());
            if (res.ShouldReturn()) return res;

            while ((At().Value == "+" || At().Value == "-") && At().Type == TokenType.BinaryOperator)
            {
                string op = Eat().Value;
                Expr right = res.Register(ParseMultiplicative());
                if (res.ShouldReturn()) return res;
                left = new BinaryExpr(left, right, op, left.Start.Copy(), right.End.Copy());
            }
            return res.Success(left);
        }

        private ParseResult ParseMultiplicative()
        {
            ParseResult res = new ParseResult();
            Expr left = res.Register(ParseUnaryExpr());
            if (res.ShouldReturn()) return res;

            while ((At().Value == "*" || At().Value == "/" || At().Value == "%") && At().Type == TokenType.BinaryOperator)
            {
                string op = Eat().Value;
                Expr right = res.Register(ParseUnaryExpr());
                if (res.ShouldReturn()) return res;
                left = new BinaryExpr(left, right, op, left.Start.Copy(), right.End.Copy());
            }
            return res.Success(left);
        }

        private ParseResult ParseUnaryExpr()
        {
            ParseResult res = new ParseResult();
            if (!(At().Value == "-" && At().Type == TokenType.BinaryOperator) && At().Type != TokenType.Not)
            {
                return ParseQuadraticExpr();
            }
            Position start = At().Start.Copy();
            string op = Eat().Value;
            Expr expr = res.Register(ParseQuadraticExpr());
            if (res.ShouldReturn()) return res;
            return res.Success(new UnaryExpr(expr, op, start, expr.End.Copy()));
        }

        private ParseResult ParseQuadraticExpr()
        {
            ParseResult res = new ParseResult();
            Expr left = res.Register(ParseCallMemberExpr());
            if (res.ShouldReturn()) return res;

            while (At().Value == "^" && At().Type == TokenType.BinaryOperator)
            {
                string op = Eat().Value;
                Expr right = res.Register(ParseCallMemberExpr());
                if (res.ShouldReturn()) return res;
                left = new BinaryExpr(left, right, op, left.Start.Copy(), right.End.Copy());
            }
            return res.Success(left);
        }

        private ParseResult ParseCallMemberExpr()
        {
            ParseResult res = new ParseResult();
            Expr member = res.Register(ParseMemberExpr());
            if (res.ShouldReturn()) return res;

            if (At().Type == TokenType.OpenParen)
            {
                Expr expr = res.Register(ParseCallExpr(member));
                if (res.ShouldReturn()) return res;
                if (At().Type == TokenType.Period)
                {
                    Eat();
                    Expr property = res.Register(ParsePrimaryExpr());
                    if (res.ShouldReturn()) return res;

                    if (property.Kind != NodeType.Identifier)
                    {
                        return res.Failure(new Error("Cannot use '.' without an identifier.", SourceCode, expr.Start, property.End));
                    }
                    return res.Success(new MemberExpr(expr, property, false, expr.Start.Copy(), property.End.Copy()));
                }
                else if (At().Type == TokenType.OpenBrack)
                {
                    Eat();
                    Expr property = res.Register(ParseExpr());
                    if (res.ShouldReturn()) return res;
                    res.Register(Expect(TokenType.CloseBrack));
                    if (res.ShouldReturn()) return res;
                    return res.Success(new MemberExpr(expr, property, true, expr.Start.Copy(), property.End.Copy()));
                }
                else
                {
                    return res.Success(expr);
                }
            }

            return res.Success(member);
        }

        private ParseResult ParseCallExpr(Expr caller)
        {
            ParseResult res = new ParseResult();
            (ParseResult result, List<Expr> args, Position end) = ParseArgs();
            res.Register(result);
            if (res.ShouldReturn()) return res;

            Expr expr = new CallExpr(args, caller, caller.Start, end);
            if (At().Type == TokenType.OpenParen)
            {
                expr = res.Register(ParseCallExpr(expr));
                if (res.ShouldReturn()) return res;
            }

            return res.Success(expr);
        }

        private ParseResult ParseMemberExpr()
        {
            ParseResult res = new ParseResult();
            Expr obj = res.Register(ParsePrimaryExpr());
            if (res.ShouldReturn()) return res;

            while (At().Type == TokenType.Period || At().Type == TokenType.OpenBrack)
            {
                Token op = Eat();
                Expr property;
                Position end;
                bool computed = op.Type != TokenType.Period;
                if (computed)
                {
                    property = res.Register(ParseExpr());
                    if (res.ShouldReturn()) return res;
                    end = At().End.Copy();
                    res.Register(Expect(TokenType.CloseBrack));
                    if (res.ShouldReturn()) return res;
                }
                else
                {
                    property = res.Register(ParsePrimaryExpr());
                    if (res.ShouldReturn()) return res;
                    if (property.Kind != NodeType.Identifier)
                    {
                        return res.Failure(new Error("Cannot use '.' without an identifier.", SourceCode, obj.Start, property.End));
                    }
                    end = property.End.Copy();
                }

                obj = new MemberExpr(obj, property, computed, obj.Start.Copy(), end);
            }

            return res.Success(obj);
        }

        private ParseResult ParsePrimaryExpr()
        {
            ParseResult res = new ParseResult();
            TokenType tk = At().Type;
            switch (tk)
            {
                case TokenType.Identifier:
                    return res.Success(new Identifier(At().Value, At().Start.Copy(), Eat().End.Copy()));
                case TokenType.Number:
                    return res.Success(new NumericLiteral(double.Parse(At().Value), At().Start.Copy(), Eat().End.Copy()));
                case TokenType.String:
                    return res.Success(new StringLiteral(At().Value, At().Start.Copy(), Eat().End.Copy()));
                case TokenType.OpenParen:
                    Eat();
                    Expr value = res.Register(ParseExpr());
                    if (res.ShouldReturn()) return res;
                    res.Register(Expect(TokenType.CloseParen));
                    if (res.ShouldReturn()) return res;
                    return res.Success(value);
                default:
                    return res.Failure(new Error($"Unexpected token found during parsing, '{At().Value}'.", SourceCode, At().Start, At().End));
            }
        }

    }
}