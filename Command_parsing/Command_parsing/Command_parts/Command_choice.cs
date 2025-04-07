namespace Command_parsing.Command_parts
{
    public class Command_choice : Command_part
    {
        //Model
        public Command_choice_part[] Choices;

        //Set 
        public int Choice_index;
        public string Value;
        //public int Choice_index;

        //As this has a separate branch it will not touch the mainline again
        public Command_choice(params Command_choice_part[] choices)
        {
            Choices = new Command_choice_part[choices.Length];

            for (int i = 0; i < choices.Length; i++)
            {
                Choices[i] = (Command_choice_part)choices.GetValue(i);
            }
        }

        public Command_choice(bool optional, params Command_choice_part[] choices)
        {
            Optional = optional;

            Choices = new Command_choice_part[choices.Length];

            for (int i = 0; i < choices.Length; i++)
            {
                Choices[i] = (Command_choice_part)choices.GetValue(i);
            }
        }

        //No special action after this
        //Thus this should continue with the main line
        public Command_choice(string[] choices, bool optional = false)
        {
            Optional = optional;

            Choices = new Command_choice_part[choices.Length];

            for (int i = 0; i < choices.Length; i++)
            {
                Choices[i] = new Command_choice_part((string)choices.GetValue(i));
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public override string Get_nice_name()
        {
            string expected = "";

            for (int i = 0; i < Choices.Length; i++)
            {
                if (Choices[i].Choice == null)
                {
                    for (int j = 0; j < Choices[i].Parts.Length; j++)
                    {
                        expected += Choices[i].Parts[j].Get_nice_name() + ", ";
                    }
                }
                else
                {
                    for (int j = 0; j < Choices[i].Choice.Length; j++)
                    {
                        expected += Choices[i].Choice[j] + ", ";
                    }
                }
            }

            //Remove last ", "
            return expected[..^2];
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

                error = "Expected: " + Get_nice_name() + ", got nothing";
                return null;
            }

            int choice_index = command.Read_index - 1;  //Points to the choice
            int next_read_index = command.Read_index;

            string no_choice_fail = "";

            for (int i = 0; i < Choices.Length; i++)
            {
                if (Choices[i].Choice == null)  //If choice is null we need to try parse as both
                {
                    command.Read_index = choice_index;


                    //Only checking first validity right now
                    Choices[i].Parts[0].Validate(command, out string local_error);

                    if (local_error == "")
                    {
                        Command_choice choice = new()
                        {
                            Choice_index = i,
                            Value = text,
                            Choices = Choices
                        };

                        //Reset for return with index for choice (what just got validated will be rechecked)
                        command.Read_index = choice_index;

                        error = "";
                        return choice;
                    }
                    else
                    {
                        no_choice_fail += Choices[i].Parts[0].GetType().Name + ": " + local_error + "\n";
                    }
                }
                else
                {
                    if (Choices[i].Choice.Contains(text))
                    {
                        Command_choice choice = new()
                        {
                            Choice_index = i,
                            Value = text,
                            Choices = Choices
                        };

                        //Reset for return with index for next
                        command.Read_index = next_read_index;
                        error = "";
                        return choice;
                    }
                }
            }
            //throw new Command_parse_exception(no_choice_fail+ "Expected: " + expected + ", got: " + text);
            error = "Expected: " + Get_nice_name() + ", got: " + text;
            return null;
        }
    }

    public class Command_choice_part
    {
        public string[] Choice;
        public Command_part[] Parts;

        public Command_choice_part(string choice, params Command_part[] parts)
        {
            Choice = new string[] { choice };

            Parts = new Command_part[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                Parts[i] = (Command_part)parts.GetValue(i);
            }
        }


        public Command_choice_part(string[] choice, params Command_part[] parts)
        {
            Choice = choice;

            Parts = new Command_part[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                Parts[i] = (Command_part)parts.GetValue(i);
            }
        }

        //A choice without an option would mean both might work
        public Command_choice_part(params Command_part[] parts)
        {
            Choice = null;

            Parts = new Command_part[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                Parts[i] = (Command_part)parts.GetValue(i);
            }
        }
    }
}
