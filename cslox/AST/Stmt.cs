using System;
using System.Collections.Generic;

namespace cslox.AST
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitBlockStmt(Block stmt);
            R VisitExpressionStmt(Expression stmt);
            R VisitIfStmt(If stmt);
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
            R VisitWhileStmt(While stmt);
        }
        public class Block : Stmt
        {
            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitBlockStmt(this);
            }

            public List<Stmt> Statements { get; }
        }
        public class Expression : Stmt
        {
            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitExpressionStmt(this);
            }

            public Expr Expr { get; }
        }
        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitIfStmt(this);
            }

            public Expr Condition { get; }
            public Stmt ThenBranch { get; }
            public Stmt ElseBranch { get; }
        }
        public class Print : Stmt
        {
            public Print(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitPrintStmt(this);
            }

            public Expr Expr { get; }
        }
        public class Var : Stmt
        {
            public Var(Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitVarStmt(this);
            }

            public Token Name { get; }
            public Expr Initializer { get; }
        }
        public class While : Stmt
        {
            public While(Expr condition, Stmt body)
            {
                Condition = condition;
                Body = body;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitWhileStmt(this);
            }

            public Expr Condition { get; }
            public Stmt Body { get; }
        }

    public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
