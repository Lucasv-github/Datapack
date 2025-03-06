using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_float : Command_part
    {
        //Model

        //Set
        public float Value;

        public Command_float(bool optional = false)
        {
            Optional = optional;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            Command_float return_float = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_excpetion("Expected an float, got nothing");
            }

            if (!float.TryParse(text, out float result))
            {
                throw new Command_parse_excpetion("Expected an float, got: " + text);
            }

            done = false;
            return_float.Value = result;
            return return_float;
        }
    }
}
