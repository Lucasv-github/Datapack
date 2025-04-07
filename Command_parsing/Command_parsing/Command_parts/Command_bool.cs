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

        public override string ToString()
        {
            return Value.ToString();
        }

        public override string Get_nice_name()
        {
            return "Boolean";
        }

        public override Command_part Validate(Command command, out string error)
        {
            Command_bool return_int = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }

                error = "Expected a bool, got nothing";
                return null;
            }

            if (!bool.TryParse(text, out bool result))
            {
                error = "Expected a bool, got: " + text;
                return null;
            }

            error = "";
            return_int.Value = result;
            return return_int;
        }
    }
}
