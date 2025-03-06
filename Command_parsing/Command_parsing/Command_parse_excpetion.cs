using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing
{
    public class Command_parse_excpetion : Exception
    {
        public Command_parse_excpetion()
        {
        }

        public Command_parse_excpetion(string message): base(message)
        {
        }

        public Command_parse_excpetion(string message, Exception inner): base(message, inner)
        {
        }
    }
}
