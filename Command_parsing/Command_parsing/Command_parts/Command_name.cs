using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_name : Command_part
    {
        //Both
        public string Name;
        public int Permission_level;

        public Command_name(string name, int permission_level = 2)
        {
            Name = name;
            Permission_level = permission_level;
        }

        public override string ToString() 
        {
            return Name;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            //Have already found it by name, so no reason to validate that

            string name = command.Read_next();

            if (Permission_level > command.Parser.Permission_level)
            {
                //For some reason someone thought it would be a good idea to support only "forceload query"
                if (name == "forceload" && (command.Parser.Version == "1.14" || command.Parser.Version == "1.14.1" || command.Parser.Version == "1.14.2" || command.Parser.Version == "1.14.3"))
                {
                    string next = command.Read_next();
                    command.Read_index--;

                    if(next == "query")
                    {
                        done = false;
                        return new Command_text(name);
                    }
                }


                throw new Command_parse_exception("Command: " + name + " expected a permission level of: " + Permission_level + " but current is: " + command.Parser.Permission_level);
            }

            done = false;
            return new Command_text(name);
        }
    }
}
