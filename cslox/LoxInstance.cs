using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class LoxInstance
    {
        private readonly LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public override string ToString()
        {
            return klass.name + " instance";
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            LoxFunction method = klass.FindMethod(this, name.lexeme);
            if (method != null) return method;

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        public void Set(Token name, object value)
        {
            fields[name.lexeme] = value;
        }
    }
}
