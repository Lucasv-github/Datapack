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

        public override string Get_nice_name()
        {
            return "Integer";
        }

        public override Command_part Validate(Command command, out string error)
        {
            Command_int return_int = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }

                error = "Expected an int, got nothing";
                return null;
            }

            if (int.TryParse(text, out int result))
            {

            }
            else
            {
                error = "Expected an int, got: " + text;
                return null;
            }

            if (result > Max)
            {
                error = "Max value here is: " + Max + " got: " + result;
                return null;
            }

            return_int.Value = result;
            error = "";
            return return_int;
        }

        public static void Validate_range(string text, out int min, out int max, out string error)
        {
            min = int.MinValue;
            max = int.MaxValue;

            if (text.StartsWith(".."))  //x..
            {
                if (!int.TryParse(text.AsSpan(2), out max))
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

                if (!int.TryParse(text.AsSpan(0, text.Length - 2), out min))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }
            else if (text.Contains("..")) //x..y
            {
                string[] parts = text.Split("..");

                if (!int.TryParse(parts[0], out min) || !int.TryParse(parts[1], out max))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }
            else  //x
            {
                if (!int.TryParse(text, out min))
                {
                    error = "Cannot parse: " + text + " as a range";
                    return;
                }
            }

            error = "";
        }
    }
}
