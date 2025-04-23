using System;
using System.Globalization;

namespace Command_parsing.Command_parts
{
    public class Command_float : Command_part
    {
        //Model
        public float Max;
        public bool Range;

        public Command_float(bool optional = false, bool range = false)
        {
            Max = float.MaxValue;
            Optional = optional;
            Range = range;
        }

        public Command_float(float max, bool optional = false, bool range = false)
        {
            Max = max;
            Optional = optional;
            Range = range;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override string Get_nice_name()
        {
            return "Float";
        }

        public override Command_part Validate(Command command, out string error)
        {
            Command_float return_float = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }

                error = "Expected an float, got nothing";
                return null;
            }

            if (Range)
            {
                Validate_range(text, out float _, out float _, out error);
                return_float.Value = text;
                return return_float;
            }
            else
            {
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {

                }
                else
                {
                    error = "Expected an float, got: " + text;
                    return null;
                }

                if (result > Max)
                {
                    error = "Max value here is: " + Max + " got: " + result;
                    return null;
                }

                return_float.Value = text;
                error = "";
                return return_float;
            }
        }

        public static void Validate_range(string text, out float min, out float max, out string error)
        {
            min = float.MinValue;
            max = float.MaxValue;

            if (text.StartsWith(".."))  //x..
            {
                if (!float.TryParse(text.AsSpan(2), NumberStyles.Float, CultureInfo.InvariantCulture, out max))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }
            else if (text.EndsWith(".."))  //..x
            {
                if (text.EndsWith("..."))  //Can apparently start with that but not end with that
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }

                if (!float.TryParse(text.AsSpan(0, text.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out min))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }
            else if (text.Contains("..")) //x..y
            {
                string[] parts = text.Split("..");

                if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out min) || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out max))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }
            else  //x
            {
                if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out min))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }

            error = "";
        }
    }
}
