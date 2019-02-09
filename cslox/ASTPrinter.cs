using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    class ASTPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        string Expr.IVisitor<string>.VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        string Expr.IVisitor<string>.VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        string Expr.IVisitor<string>.VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        string Expr.IVisitor<string>.VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
        // Tests to make sure ASTPrinter works
        //public static void Main(String[] args)
        //{
        //    Expr expression = new Expr.Binary(
        //        new Expr.Unary(
        //            new Token(TokenType.MINUS, "-", null, 1),
        //            new Expr.Literal(123)),
        //        new Token(TokenType.STAR, "*", null, 1),
        //        new Expr.Grouping(
        //            new Expr.Literal(45.67)));

        //    Console.WriteLine(new ASTPrinter().Print(expression));
        //    Console.ReadLine();
        //}
    }
}
