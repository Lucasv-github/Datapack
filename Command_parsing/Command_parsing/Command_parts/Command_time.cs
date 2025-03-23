using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_time : Command_part
    {
        //Model
        public float Max;

        //Set
        public float Value;
        public Time_unit Unit;

        public Command_time(bool optional = false)
        {
            Max = float.MaxValue;
            Optional = optional;
        }

        public Command_time(float max, bool optional = false)
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
            Command_time return_time = new();

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

            if(text.EndsWith('d'))
            {
                text = text.Remove(text.Length - 1);
                return_time.Unit = Time_unit.Day;
            }
            else if(text.EndsWith('s'))
            {
                text = text.Remove(text.Length - 1);
                return_time.Unit = Time_unit.Second;
            }
            else if(text.EndsWith('t'))
            {
                text = text.Remove(text.Length - 1);
                return_time.Unit = Time_unit.Tick;
            }
            else if(!char.IsNumber(text[^1]))
            {
                throw new Command_parse_exception("Unknown time unit: \"" + text[^1] + "\"");
            }

            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                throw new Command_parse_exception("Expected an float, got: " + text);
            }

            if (result > Max)
            {
                throw new Command_parse_exception("Max value here is: " + Max + " got: " + result);
            }

            done = false;
            return_time.Value = result;
            return return_time;
        }
    }

    public enum Time_unit
    {
        Tick = 0,
        Second = 1,
        Day = 2
    }
}
