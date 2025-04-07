namespace Command_parsing.Command_parts
{
    public class Command_text : Command_part
    {
        //This text will read to the next space

        //Model
        private readonly string validator_name;
        private readonly bool to_end;

        //Set
        public string Value;
        public Command_text()
        {

        }

        public Command_text(bool optional, bool to_end = false)
        {
            Optional = optional;
            this.to_end = to_end;
        }

        public override string ToString()
        {
            return Value;
        }

        public override string Get_nice_name()
        {
            return "Text";
        }
        public Command_text(string validator_name, bool optional = false, bool to_end = false)
        {
            if (validator_name.EndsWith('S'))
            {
                throw new ArgumentException(nameof(validator_name) + " should probably not be plural");
            }

            Optional = optional;
            this.validator_name = validator_name;
            this.to_end = to_end;
        }

        public override Command_part Validate(Command command, out string error)
        {
            Command_text return_text = new();

            string value = command.Read_next();

            if (value == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }
                error = "Expected a text, got nothing";
                return null;
            }

            if (to_end)
            {
                string next = command.Read_next();

                while (next != null)
                {
                    value += " " + next;
                    next = command.Read_next();
                }
            }

            return_text.Value = value;

            if (validator_name == null)
            {
                error = "";
                return return_text;
            }
            else
            {
                command.Parser.Get_validator(validator_name).Validate(command, value, out error);

                if (error != "")
                {
                    return null;
                }

                error = "";
                return return_text;
            }
        }
    }
}
