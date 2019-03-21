using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class LoxFunction: LoxCallable
    {
        private readonly Environment closure;
        private readonly Stmt.Function declaration;
        private readonly bool isInitializer;
  
        public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.isInitializer = isInitializer;
            this.closure = closure;
            this.declaration = declaration;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public override object Call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.parameters.Count; i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            } catch(Return returnValue)
            {
                if (isInitializer) return closure.GetAt(0, "this");
                return returnValue.value;
            }

            if (isInitializer) return closure.GetAt(0, "this");
            
            return null;
        }

        public override int Arity()
        {
            return declaration.parameters.Count;
        }

        public override string ToString()
        {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
