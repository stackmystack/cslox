using System;
using System.Collections.Generic;

namespace cslox.AST
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitUnaryExpr(Unary expr);
            R VisitVariableExpr(Variable expr);
        }
        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitAssignExpr(this);
            }

            public Token Name { get; }
            public Expr Value { get; }
        }
        public class Binary : Expr
        {
            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Op = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitBinaryExpr(this);
            }

            public Expr Left { get; }
            public Token Op { get; }
            public Expr Right { get; }
        }
        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitGroupingExpr(this);
            }

            public Expr Expression { get; }
        }
        public class Literal : Expr
        {
            public Literal(object value)
            {
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitLiteralExpr(this);
            }

            public object Value { get; }
        }
        public class Unary : Expr
        {
            public Unary(Token op, Expr right)
            {
                Op = op;
                Right = right;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitUnaryExpr(this);
            }

            public Token Op { get; }
            public Expr Right { get; }
        }
        public class Variable : Expr
        {
            public Variable(Token name)
            {
                Name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                 return visitor.VisitVariableExpr(this);
            }

            public Token Name { get; }
        }

    public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
