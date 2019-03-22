using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{

    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private sealed class Clock : LoxCallable
        {
            public override int Arity()
            {
                return 0;
            }

            public override object Call(Interpreter interpreter, List<object> arguments)
            {
                return (double)System.Environment.TickCount;
            }

            public override string ToString()
            {
                return "<native fn>";
            }
        }

        public readonly Environment globals;
        private Environment environment;
        private readonly Dictionary<Expr, int> locals;
        public Interpreter()
        {
            globals = new Environment();
            environment = globals;
            locals = new Dictionary<Expr, int>();
            // define clock
            globals.Define("clock", new Clock());
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
            
        }

        object Expr.IVisitor<object>.VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        object Expr.IVisitor<object>.VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.op.type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            } else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        object Expr.IVisitor<object>.VisitSetExpr(Expr.Set expr)
        {
            object obj = Evaluate(expr.obj);

            if (!(obj is LoxInstance))
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)obj).Set(expr.name, value);
            return value;
        }

        object Expr.IVisitor<object>.VisitSuperExpr(Expr.Super expr)
        {
            int distance = locals[expr];
            LoxClass superclass = (LoxClass)environment.GetAt(distance, "super");

            LoxInstance obj = (LoxInstance)environment.GetAt(distance - 1, "this");

            LoxFunction method = superclass.FindMethod(obj, expr.method.lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.method, "Undefined property '" + expr.method.lexeme + "'.");
            }

            return method;
        }

        object Expr.IVisitor<object>.VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        object Expr.IVisitor<object>.VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        object Expr.IVisitor<object>.VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch(expr.op.type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    return -(double)right;
                
            }

            return null;
        }

        object Expr.IVisitor<object>.VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }


        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return environment.GetAt(distance, name.lexeme);
            }
            else
            {
                return globals.Get(name);
            }
        }

        object Expr.IVisitor<object>.VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);


            switch(expr.op.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.op, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.op, "Operands must be two numbers of two strings.");

                case TokenType.SLASH:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left * (double)right;
            }

            return null;
        }

        object Expr.IVisitor<object>.VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is LoxCallable))
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            LoxCallable function = (LoxCallable)callee;

            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, "Expected " +
                    function.Arity() + " arguments but got " +
                    arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }

        object Expr.IVisitor<object>.VisitGetExpr(Expr.Get expr)
        {
            object obj = Evaluate(expr.obj);
            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            } finally
            {
                this.environment = previous;
            }
        }

        object Stmt.IVisitor<object>.VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        object Stmt.IVisitor<object>.VisitClassStmt(Stmt.Class stmt)
        {
            object superclass = null;
            if (stmt.superclass != null)
            {
                superclass = Evaluate(stmt.superclass);
                if (!(superclass is LoxClass))
                {
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
                }
            }

            environment.Define(stmt.name.lexeme, null);

            if (stmt.superclass  != null)
            {
                environment = new Environment(environment);
                environment.Define("super", superclass);
            }

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            foreach(Stmt.Function method in stmt.methods)
            {
                LoxFunction function = new LoxFunction(method, environment, method.name.lexeme.Equals("init"));
                methods[method.name.lexeme] = function;
            }


            LoxClass klass = new LoxClass(stmt.name.lexeme, (LoxClass)superclass, methods);

            if (superclass != null)
            {
                environment = environment.enclosing;
            }

            environment.Assign(stmt.name, klass);
            return null;
        }

        object Stmt.IVisitor<object>.VisitExpressionStmt(Stmt.Expression stmt)
        {
            object value = Evaluate(stmt.expression);
            // Console.WriteLine(Stringify(value));
            return null;
        }

        object Stmt.IVisitor<object>.VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        object Stmt.IVisitor<object>.VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        object Stmt.IVisitor<object>.VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        object Stmt.IVisitor<object>.VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        object Stmt.IVisitor<object>.VisitWhileStmt(Stmt.While stmt)
        {
            while(IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }

            return null;
        }

        object Expr.IVisitor<object>.VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            environment.Assign(expr.name, value);
            return value;
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool value) return value;
            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperand(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
            {
                string text = obj.ToString();
                if(text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return obj.ToString();
        }
    }
}
