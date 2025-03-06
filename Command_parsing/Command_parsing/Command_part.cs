using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing
{
    public abstract class Command_part
    {
        public bool Optional;
        //Uses self as a model and returns new
        public virtual Command_part Validate(Command command, out bool done)
        {
            throw new NotSupportedException();
        }

        //Sets self
        public virtual bool Set_validate(string part)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            throw new NotSupportedException();
        }
    }
}
