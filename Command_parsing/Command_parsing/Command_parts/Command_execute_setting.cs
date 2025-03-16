using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //TODO need this to support different versions

        //TODO need to pass out continueation index
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

        //This is used as a stop, skipping back to the execute setting validator, thus handling both nested and the run word
        //
    public class Command_execute_stop : Command_part
    {
        public string Value;

        public override Command_part Validate(Command command, out bool done)
        {
            string text = command.Read_next() ?? throw new Command_parse_exception("Expexted execute part, got nothing");
            done = false;
            return new Command_execute_stop(text);
        }

        public Command_execute_stop()
        {

        }

        public Command_execute_stop(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}

