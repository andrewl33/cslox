using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class LoxClass: LoxCallable
    {
        public readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            this.name = name;
            this.methods = methods;
        }

        public override string ToString()
        {
            return name;
        }

        public override object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = methods["init"];
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public override int Arity()
        {
            LoxFunction initializer = methods["init"];
            if (initializer == null) return 0;
            return initializer.Arity();
        }

        public LoxFunction FindMethod(LoxInstance instance, string name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name].Bind(instance);
            }

            return null;
        }
    }
}
