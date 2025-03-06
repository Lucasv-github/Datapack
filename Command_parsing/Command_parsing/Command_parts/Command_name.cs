using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_name : Command_part
    {
        public string Name;
        public Command_name(string name)
        {
            Name = name;
        }

        public override string ToString() 
        {
            return Name;
        }
    }
}
