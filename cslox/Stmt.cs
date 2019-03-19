using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
	public abstract class Stmt
	{
		public abstract R Accept<R>(IVisitor<R> visitor);
		public interface IVisitor<R>
		{
			R VisitExpressionStmt(Expression stmt);
			R VisitPrintStmt(Print stmt);
		}
		public class Expression : Stmt
		{
			public readonly Expr expression;
			public Expression(Expr expression)
			{
				this.expression = expression;
			}
			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitExpressionStmt(this);
			}
		}

		public class Print : Stmt
		{
			public readonly Expr expression;
			public Print(Expr expression)
			{
				this.expression = expression;
			}
			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitPrintStmt(this);
			}
		}

	}
}
