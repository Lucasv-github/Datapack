using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_float : Command_part
    {
        //Model
        public float Max;

        //Set
        public float Value;

        public Command_float(bool optional = false)
        {
            Max = float.MaxValue;
            Optional = optional;
        }

        public Command_float(float max, bool optional = false)
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
            Command_float return_float = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_exception("Expected an float, got nothing");
            }

            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture,out float result))
            {
                throw new Command_parse_exception("Expected an float, got: " + text);
            }

            if(result > Max)
            {
                throw new Command_parse_exception("Max value here is: " + Max + " got: " + result);
            }

            done = false;
            return_float.Value = result;
            return return_float;
        }

        public static void Validate_range(string text, out float min, out float max)
        {
            min = float.MinValue;
            max = float.MaxValue;

            if (text.StartsWith(".."))  //x..
            {
                if (!float.TryParse(text.AsSpan(2), NumberStyles.Float, CultureInfo.InvariantCulture, out max))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
            else if(text.EndsWith(".."))  //..x
            {
                if(text.EndsWith("..."))  //Can apparently start with that but not end with that
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }

                if (!float.TryParse(text.AsSpan(0,text.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out min))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
            else if(text.Contains("..")) //x..y
            {
                string[] parts = text.Split("..");

                if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out min) || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out max))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
            else  //x
            {
                if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out min))
                {
                    throw new Command_parse_exception("Cannot parse: " + text + " as a range");
                }
            }
        }
    }
}
