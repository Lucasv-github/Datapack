using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;

namespace Command_parsing
{
    public class Command
    {
        public string Entire_line;
        public string[] Line_parts;

        public List<Command_part> Parts;
        public int Line_num;

        public int Read_index;

        public readonly Command_parser Parser;

        public Command(Command_parser parser,string entire_line, int line_num)
        {
            Parser = parser;

            Entire_line = entire_line;
            Line_parts = Command_parser.Split_ignore(entire_line,' ').ToArray();

            Parts = new();

            Line_num = line_num;
        }

        public string Read_next()
        {
            if(Read_index >= Line_parts.Length)
            {
                return null;
            }

            return Line_parts[Read_index++];
        }

        public void Parse(Command_parser parser)
        {
            Parse_sub(parser);

            if(Read_index < Line_parts.Length)
            {
                string trailing = "";

                for (int i = Read_index; i < Line_parts.Length; i++)
                {
                    trailing += Line_parts[i] + " ";
                }

                //Remove last space
                trailing = trailing.Remove(trailing.Length - 1);

                throw new Command_parse_exception("Found trailing data: " + trailing);
            }
        }

        public void Parse_sub(Command_parser parser)
        {
            string command_name = Read_next() ?? throw new Command_parse_exception("Expected command, got nothing");
            if (parser.Aliases.ContainsKey(command_name))
            {
                command_name = parser.Aliases[command_name];
            }

            //Will no matter what have the command name first

            Parts.Add(new Command_name(command_name));

            int model_index = parser.Models.FindIndex(m => ((Command_name)m.Parts[0]).Name == command_name);

            if(model_index == -1)
            {
                throw new Command_parse_exception("Command: " + command_name + " is not a recognized command");
            }

            //Skipping name
            for(int model_part_index = 1; model_part_index < parser.Models[model_index].Parts.Length; model_part_index++)
            {
                Command_part next = parser.Models[model_index].Parts[model_part_index].Validate(this, out bool done);

                if(next != null) Parts.Add(next);

                //Some things (like text) will gulp everything up
                if (done)
                {
                    break;
                }

                if(next is Command_choice choice)
                {
                    if (choice.Choices[choice.Choice_index].Parts.Length != 0)
                    {
                        Choice_branch(choice);
                        return;
                    }
                    //This means the choice doesn't have its own branch 
                }

                if (next is Command_execute_stop stop_)
                {
                    ;

                    return;
                }

                //TODO warning if here if above triggered
            }

            void Choice_branch(Command_choice choice)
            {
                for (int i = 0; i < choice.Choices[choice.Choice_index].Parts.Length; i++)
                {
                    Command_part next = choice.Choices[choice.Choice_index].Parts[i].Validate(this, out bool _);

                    if (next != null) Parts.Add(next);

                    if (next is Command_choice choice_)
                    {
                        if (choice_.Choices[choice_.Choice_index].Parts.Length != 0)
                        {
                            Choice_branch(choice_);
                            return;
                        }
                        //This means the choice doesn't have its own branch
                    }

                    if(next is Command_execute_stop stop_)
                    {
                        if(stop_.Value == "run")
                        {
                            Parse_sub(parser);
                            return;
                        }
                        else
                        {
                            Read_index--;

                            Command_part execute_type = Parser.Get_command_model("execute").Parts[1].Validate(this, out bool _);
                            
                            Choice_branch((Command_choice)execute_type);
                            return;
                        }
                    }

                    //TODO warning if here if above triggered
                }
            }
        }
    }
}
