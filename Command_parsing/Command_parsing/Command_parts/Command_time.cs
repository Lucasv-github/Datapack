using System.Globalization;

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

        public override string Get_nice_name()
        {
            return "Time";
        }
        public override Command_part Validate(Command command, out string error)
        {
            Command_time return_time = new();

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

            if (text.EndsWith('d'))
            {
                text = text[..^1];
                return_time.Unit = Time_unit.Day;
            }
            else if (text.EndsWith('s'))
            {
                text = text[..^1];
                return_time.Unit = Time_unit.Second;
            }
            else if (text.EndsWith('t'))
            {
                text = text[..^1];
                return_time.Unit = Time_unit.Tick;
            }
            else if (!char.IsNumber(text[^1]))
            {
                error = "Unknown time unit: \"" + text[^1] + "\"";
                return null;
            }

            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                error = "Expected an float, got: " + text;
                return null;
            }

            if (result > Max)
            {
                error = "Max value here is: " + Max + " got: " + result;
                return null;
            }

            error = "";
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
