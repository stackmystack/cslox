namespace cslox.AST
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitExprStmtStmt(ExprStmt stmt);
            R VisitPrintStmtStmt(PrintStmt stmt);
        }
        public class ExprStmt : Stmt
        {
            public ExprStmt(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitExprStmtStmt(this);
            }

            public Expr Expression { get; }
        }
        public class PrintStmt : Stmt
        {
            public PrintStmt(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitPrintStmtStmt(this);
            }

            public Expr Expression { get; }
        }

    public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
