using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_bool : Command_part
    {
        //Model

        //Set
        public bool Value;

        public Command_bool(bool optional = false)
        {
            Optional = optional;
        }

        public Command_bool()
        {

        }

        public override Command_part Validate(Command command, out bool done)
        {
            Command_bool return_int = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_excpetion("Expected a bool, got nothing");
            }

            if (!bool.TryParse(text, out bool result))
            {
                throw new Command_parse_excpetion("Expected a bool, got: " + text);
            }

            done = false;
            return_int.Value = result;
            return return_int;
        }
    }
}
