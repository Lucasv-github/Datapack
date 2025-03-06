using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing
{
    public class Parse_result
    {
        public List<Command> Commands;

        public Parse_result() 
        {
            Commands = new();
        }
    }
}
