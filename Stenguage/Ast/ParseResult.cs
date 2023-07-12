using Stenguage.Ast.Expressions;
using Stenguage.Errors;

namespace Stenguage.Ast
{
    public class ParseResult
    {
        public Expr Expr { get; set; }
        public Error Error { get; set; }

        public ParseResult()
        {
            Reset();
        }

        public void Reset()
        {
            Expr = null;
            Error = null;
        }

        public Expr Register(ParseResult res)
        {
            if (res.ShouldReturn()) Error = res.Error;
            Expr = res.Expr;
            return res.Expr;
        }

        public bool ShouldReturn()
        {
            return Error != null;
        }

        public ParseResult Success(Expr expr)
        {
            Expr = expr;
            return this;
        }

        public ParseResult Failure(Error error)
        {
            Reset();
            Error = error;
            return this;
        }

    }
}
