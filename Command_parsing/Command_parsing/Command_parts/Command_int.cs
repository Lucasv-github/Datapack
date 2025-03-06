using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_int : Command_part
    {
        //Model

        //Set
        public int Value;

        public Command_int(bool optional) 
        {
            Optional = optional;
        }

        public Command_int()
        {

        }

        public override Command_part Validate(Command command, out bool done)
        {
            Command_int return_int = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_excpetion("Expected an int, got nothing");
            }

            if (!int.TryParse(text, out int result))
            {
                throw new Command_parse_excpetion("Expected an int, got: " + text);
            }

            done = false;
            return_int.Value = result;
            return return_int;
        }
    }
}
