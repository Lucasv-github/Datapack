namespace Command_parsing.Command_parts
{
    public class Command_bool : Command_part
    {
        //Model

        //Set

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
            Command_bool return_bool = new();

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

            if (!bool.TryParse(text, out bool _))
            {
                error = "Expected a bool, got: " + text;
                return null;
            }

            error = "";
            return_bool.Value = text;
            return return_bool;
        }
    }
}
