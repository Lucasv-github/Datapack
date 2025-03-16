using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Command_choice(bool optional,params Command_choice_part[] choices)
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
        public override Command_part Validate(Command command, out bool done)
        {
            string expected;

            string text = command.Read_next();

            if (text == null)
            {
                if(Optional)
                {
                    done = false;
                    return null;
                }

                Generate_expected();
                throw new Command_parse_exception("Expected: " + expected + ", got nothing");
            }
            int preserved_read_index = command.Read_index - 1;

            string no_choice_fail = "";

            for (int i = 0; i < Choices.Length; i++)
            {
                //Passed it

                if (Choices[i].Choice == null)  //If choice is null we need to try parse as both
                {
                    command.Read_index = preserved_read_index;

                    try
                    {
                        //Only checking first validity right now
                        Command_part next = Choices[i].Parts[0].Validate(command, out bool _);

                        Command_choice choice = new()
                        {
                            Choice_index = i,
                            Value = text,
                            Choices = Choices
                        };

                        command.Read_index = preserved_read_index;
                        done = false;
                        return choice;
                    }
                    catch (Command_parse_exception ex)
                    {
                        no_choice_fail += Choices[i].Parts[0].GetType().Name + ": " + ex.Message + "\n";
                        command.Read_index = preserved_read_index + 1;
                    }
                }
                else
                {
                    if(Choices[i].Choice.Contains(text))
                    {
                        Command_choice choice = new()
                        {
                            Choice_index = i,
                            Value = text,
                            Choices = Choices
                        };

                        done = false;
                        return choice;
                    }
                }
            }
            Generate_expected();
            throw new Command_parse_exception(no_choice_fail+ "Expected: " + expected + ", got: " + text);

            void Generate_expected()
            {
                expected = "";

                for (int i = 0; i < Choices.Length; i++)
                {
                    if(Choices[i].Choice == null)
                    {
                        for (int j = 0; j < Choices[i].Parts.Length; j++)
                        {
                            expected += Choices[i].Parts[j].GetType().Name + ", ";
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
                expected = expected.Remove(expected.Length - 2, 2);
            }
        }
    }

    public class Command_choice_part
    {
        public string[] Choice;
        public Command_part[] Parts;

        public Command_choice_part(string choice, params Command_part[] parts)
        {
            Choice = new string[] {choice};

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
