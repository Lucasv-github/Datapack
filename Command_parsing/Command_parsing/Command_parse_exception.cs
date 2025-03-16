using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing
{
    public class Command_parse_exception : Exception
    {
        public Command_parse_exception()
        {
        }

        public Command_parse_exception(string message): base(message)
        {
        }

        public Command_parse_exception(string message, Exception inner): base(message, inner)
        {
        }
    }
}
