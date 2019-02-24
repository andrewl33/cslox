using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class RuntimeError: SystemException
    {
        public readonly Token token;

        public RuntimeError(Token token, string message)
            :base(message)
        {
            this.token = token;
        }
    }
}
