using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cslox
{
    class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD,
            INITIALIZER
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        object Stmt.IVisitor<object>.VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        object Stmt.IVisitor<object>.VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);

            if (stmt.superclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                BeginScope();
                scopes.Peek()["super"] = true;
                Resolve(stmt.superclass);
            }

            Define(stmt.name);

            BeginScope();
            scopes.Peek()["this"] = true;

            foreach(Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;

                if (method.name.lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();
            if (stmt.superclass != null) EndScope();
            currentClass = enclosingClass;
            return null;
        }

        object Stmt.IVisitor<object>.VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }


        object Stmt.IVisitor<object>.VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        object Stmt.IVisitor<object>.VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        object Stmt.IVisitor<object>.VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Lox.Error(stmt.keyword, "Cannot return a value from an initializer.");
                }

                Resolve(stmt.value);
            }

            return null;
        }

        object Stmt.IVisitor<object>.VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        object Stmt.IVisitor<object>.VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }


        object Expr.IVisitor<object>.VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        object Expr.IVisitor<object>.VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        object Expr.IVisitor<object>.VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach(Expr argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        object Expr.IVisitor<object>.VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);
            return null;
        }

        object Expr.IVisitor<object>.VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        object Expr.IVisitor<object>.VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        object Expr.IVisitor<object>.VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        object Expr.IVisitor<object>.VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        object Expr.IVisitor<object>.VisitSuperExpr(Expr.Super expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Cannot use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                Lox.Error(expr.keyword, "Cannot user 'super' in a class with no superclass");
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        object Expr.IVisitor<object>.VisitThisExpr(Expr.This expr)
        {
            ResolveLocal(expr, expr.keyword);
            return null;
        }

        object Expr.IVisitor<object>.VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        object Expr.IVisitor<object>.VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count > 0 && scopes.Peek().TryGetValue(expr.name.lexeme, out bool value))
            {
                if (!value)
                {
                    Lox.Error(expr.name, "Cannot read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach(Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach(Token p in function.parameters)
            {
                Declare(p);
                Define(p);
            }
            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }
            
        private void Declare(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }
            scopes.Peek()[name.lexeme] = false;
        }
        
        private void Define(Token name)
        {
            if (scopes.Count == 0)
            {
                return;
            }

            if (scopes.Peek().ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Variable with this name already declared in this scope.");
            }

            scopes.Peek()[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            Dictionary<string, bool>[] scopesArray = scopes.ToArray();
            for(int i = scopesArray.Length - 1; i >= 0; i--)
            {
                if (scopesArray[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopesArray.Length - 1 - i);
                    return;
                }
            }
        }
    }
}
