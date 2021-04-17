using System;
using System.Text;

namespace cslox.AST
{
    class ASTPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            if (expr == null)
                return "nil";
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Paren(expr.Op.Lexeme, new Expr[] { expr.Left, expr.Right });
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Paren("group", new Expr[] { expr.Expression });
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value.ToString();
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Paren(expr.Op.Lexeme, new Expr[] { expr.Right });
        }

        private string Paren(string name, Expr[] exprs)
        {
            var sb = new StringBuilder();

            sb.Append('(').Append(name);
            foreach (var expr in exprs)
            {
                sb.Append(' ');
                sb.Append(expr.Accept(this));
            }
            sb.Append(')');

            return sb.ToString();
        }
    }
}