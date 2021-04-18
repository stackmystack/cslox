using System;
using cslox.AST;

namespace cslox
{
    public class Interpreter : Expr.IVisitor<object>
    {

        public void Interpret(Expr expr)
        {
            try
            {
                var value = Eval(expr);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError e)
            {
                Lox.RuntimeError(e);
            }
        }

        private string Stringify(object value)
        {
            if (value == null) return "nil";

            if (value is double)
            {
                var text = value.ToString();

                if (text.EndsWith(".0"))
                {
                    text = text[0..(text.Length - 2)];
                }

                return text;
            }

            return value.ToString();
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Eval(expr.Left);
            var right = Eval(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.GT:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case TokenType.GTE:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LT:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case TokenType.LTE:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (left is double l && right is double r)
                        return l + r;
                    if (left is string ls && right is string rs)
                        return ls + rs;
                    throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
                case TokenType.BANG_EQUAL: return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: return !IsEqual(left, right);
                default:
                    return null;
            }
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Eval(expr.Expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            var right = Eval(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.BANG: return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
                default: return null;
            }
        }

        private void CheckNumberOperand(Token op, object right)
        {
            if (right is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (right is double && right is double) return;
            throw new RuntimeError(op, "Operands must be a number.");
        }

        private object Eval(Expr expression)
        {
            return expression.Accept(this);
        }

        private bool IsTruthy(object o)
        {
            if (o == null) return false;
            if (o is bool boolean) return boolean;

            return true;
        }

        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;

            return left.Equals(right);
        }

    }
}