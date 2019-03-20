using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public abstract class LoxCallable
    {
        public abstract int Arity();
        public abstract object Call(Interpreter interpreter, List<object> arguments);
    }
}
