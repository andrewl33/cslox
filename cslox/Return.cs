using System;
using System.Collections.Generic;
using System.Text;

namespace cslox
{
    public class Return: SystemException
    {
         public readonly object value;

        public Return(object value)
            :base(null, null)
        {
            this.value = value;
        }
    }
}
