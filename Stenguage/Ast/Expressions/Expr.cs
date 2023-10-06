using Stenguage.Runtime;

namespace Stenguage.Ast.Expressions
{
    public enum NodeType
    {
        None,

        // Statements
        Program,
        VarDeclaration,
        FunctionDeclaration,
        IfStatement,
        ForLoop,
        Break,
        Continue,
        Return,
        Skip,
        WhileLoop,
        Import,
        ClassDeclaration,

        // Expressions
        AssignmentExpr,
        BinaryExpr,
        MemberExpr,
        CallExpr,
        UnaryExpr,

        // Literals
        Property,
        ObjectLiteral,
        ListLiteral,
        NumericLiteral,
        StringLiteral,
        Identifier,
    }

    public abstract class Expr
    {
        public NodeType Kind { get; set; }
        public Position Start { get; set; }
        public Position End { get; set; }

        public Expr(NodeType kind, Position start, Position end)
        {
            Kind = kind;
            Start = start;
            End = end;
        }

        public abstract RuntimeResult Evaluate(Runtime.Environment env);

    }

}
