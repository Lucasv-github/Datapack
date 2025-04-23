namespace Command_parsing.Command_parts
{
    public class Command_execute_setting : Command_choice
    {
        //Model
        //public Command_execute_setting_part[] Settings;

        public Command_execute_setting(params Command_choice_part[] choices)
        {
            Choices = new Command_choice_part[choices.Length];

            for (int i = 0; i < choices.Length; i++)
            {
                Choices[i] = (Command_choice_part)choices.GetValue(i);
            }
        }
    }

    //public class Command_execute_setting_part
    //{
    //    public string Type;
    //    public Command_part[] Parts;

    //    public Command_execute_setting_part(string choice, params Command_part[] parts)
    //    {
    //        Type = choice;

    //        Parts = new Command_part[parts.Length];

    //        for (int i = 0; i < parts.Length; i++)
    //        {
    //            Parts[i] = (Command_part)parts.GetValue(i);
    //        }
    //    }

    //This is used as a stop, skipping back to the execute setting validator_name, thus handling both nested and the run word
    //
    public class Command_execute_stop : Command_part
    {
        private readonly bool only_run;

        public override string Get_nice_name()
        {
            if(only_run)
            {
                return "run";
            }

            return "Execute part";
        }

        public override Command_part Validate(Command command, out string error)
        {
            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }

                error = "Expected execute part, got nothing";
                return null;
            }

            if (only_run && text != "run")
            {
                error = "Expected execute part: \"run\", got nothing";
                return null;
            }

            error = "";

            Command_execute_stop execute_stop = new()
            {
                Value = text
            };

            return execute_stop;
        }

        public Command_execute_stop(bool optinal = false, bool only_run = false)
        {
            Optional = optinal;
            this.only_run = only_run;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}

