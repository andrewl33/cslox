using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    class LoxFunction: LoxCallable
    {
        private readonly Environment closure;
        private readonly Stmt.Function declaration;
  
        public LoxFunction(Stmt.Function declaration, Environment closure)
        {
            this.closure = closure;
            this.declaration = declaration;
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
                return returnValue.value;
            }
            
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
