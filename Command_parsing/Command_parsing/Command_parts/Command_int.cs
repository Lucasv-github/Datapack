using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_int : Command_part
    {
        //Model
        public int Max;

        //Set
        public int Value;

        public Command_int(bool optional = false) 
        {
            Max = int.MaxValue;
            Optional = optional;
        }

        public Command_int(int max, bool optional = false)
        {
            Max = max;
            Optional = optional;
        }

        public override string ToString()
        {
            return Value.ToString();
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

                throw new Command_parse_exception("Expected an int, got nothing");
            }

            if (!int.TryParse(text, out int result))
            {
                throw new Command_parse_exception("Expected an int, got: " + text);
            }

            if(result > Max)
            {
                throw new Command_parse_exception("Max value here is: " + Max + " got: " + result);
            }

            done = false;
            return_int.Value = result;
            return return_int;
        }

        public static void Validate_range(string text, out int min, out int max)
        {
            min = int.MinValue;
            max = int.MaxValue;

            if (text.StartsWith(".."))  //x..
            {
                if (!int.TryParse(text.AsSpan(2), out max))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
            else if (text.EndsWith(".."))  //..x
            {
                if (text.EndsWith("..."))  //Can apparently start with that but not end with that
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }

                if (!int.TryParse(text.AsSpan(0, text.Length - 2), out min))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
            else if (text.Contains("..")) //x..y
            {
                string[] parts = text.Split("..");

                if (!int.TryParse(parts[0], out min) || !int.TryParse(parts[1], out max))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
            else  //x
            {
                if (!int.TryParse(text, out min))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
        }
    }
}
