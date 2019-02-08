using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
	public abstract class Expr
	{
	}
	public class Binary : Expr
	{
		public readonly Expr left;
		public readonly Token op;
		public readonly Expr right;
		public Binary(Expr left, Token op, Expr right)
		{
			this.left = left;
			this.op = op;
			this.right = right;
		}
	}

	public class Grouping : Expr
	{
		public readonly Expr expression;
		public Grouping(Expr expression)
		{
			this.expression = expression;
		}
	}

	public class Literal : Expr
	{
		public readonly Object value;
		public Literal(Object value)
		{
			this.value = value;
		}
	}

	public class Unary : Expr
	{
		public readonly Token op;
		public readonly Expr right;
		public Unary(Token op, Expr right)
		{
			this.op = op;
			this.right = right;
		}
	}

}
