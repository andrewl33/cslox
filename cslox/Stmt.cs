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
			R VisitBlockStmt(Block stmt);
			R VisitExpressionStmt(Expression stmt);
			R VisitPrintStmt(Print stmt);
			R VisitVarStmt(Var stmt);
		}
		public class Block : Stmt
		{
			public readonly List<Stmt> statements;
			public Block(List<Stmt> statements)
			{
				this.statements = statements;
			}
			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitBlockStmt(this);
			}
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

		public class Var : Stmt
		{
			public readonly Token name;
			public readonly Expr initializer;
			public Var(Token name, Expr initializer)
			{
				this.name = name;
				this.initializer = initializer;
			}
			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitVarStmt(this);
			}
		}

	}
}
