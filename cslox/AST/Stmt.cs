using System;
using System.Collections.Generic;

namespace cslox.AST
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmtStmt(BlockStmt stmt);
            R VisitExprStmtStmt(ExprStmt stmt);
            R VisitPrintStmtStmt(PrintStmt stmt);
            R VisitVarStmtStmt(VarStmt stmt);
        }
        public class BlockStmt : Stmt
        {
            public BlockStmt(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitBlockStmtStmt(this);
            }

            public List<Stmt> Statements { get; }
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
        public class VarStmt : Stmt
        {
            public VarStmt(Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitVarStmtStmt(this);
            }

            public Token Name { get; }
            public Expr Initializer { get; }
        }

    public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
