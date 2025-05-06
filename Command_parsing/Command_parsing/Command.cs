using Command_parsing.Command_parts;

namespace Command_parsing
{
    public class Command
    {
        public Command_type Lines_type;

        public int Line_num;
        private string all_lines_unformatted;

        public List<string> All_lines;
        public string[] Parts_string;
        public int Read_index;

        public List<Command_part> Parts;

        public readonly Command_parser Parser;
        public readonly bool Macro_line;

        public readonly bool Errored;

        //TODO can we preserve the format exactly? spaces, tabs and newlines

        public Command(Command_parser parser, string first_line, int line_num, out string error)
        {
            first_line = first_line.Trim(' ', '\t'); //Minecraft seems to remove spaces and tabs at the beginning (and end, at least in 1.21.5)
            all_lines_unformatted = first_line;

            Parser = parser;

            if (first_line.StartsWith('#'))
            {
                Lines_type = Command_type.Comment;
            }
            else
            {
                Lines_type = Command_type.Command;

                if (first_line.StartsWith('$'))
                {
                    Macro_line = true;
                }

                if (first_line.EndsWith('\\'))  //This is a multilined command
                {
                    All_lines = new List<string> { first_line[..^1] };  //Removing the last \

                    while (true)
                    {
                        string next_part = parser.Read_line();
                        
                        if (next_part == null)
                        {
                            error = "Error parsing line: " + Line_num + "\n" + "Expected a new line, but reached end of file\n";
                            Errored = true;
                            return;
                        }

                        if (next_part.StartsWith('#'))
                        {
                            error = "Error parsing line: " + Line_num + "\n" + "Expected a new line, but got a comment\n";
                            Errored = true;
                            return;
                        }

                        all_lines_unformatted += "\n" + next_part;

                        next_part = next_part.Trim(' ', '\t');  //Again removing the first tabs/spaces Yes minecraft does this (at least in 1.21.5)

                        if (!next_part.EndsWith('\\'))
                        {
                            All_lines.Add(next_part);  //Keeping the last char
                            break;
                        }
                        else
                        {
                            All_lines.Add(next_part[..^1]);  //Removing the last \
                        }
                    }
                }
                else
                {
                    All_lines = new List<string> { first_line };
                }

                string single_line = "";

                //Cannot split correclty without combining into single line
                foreach (string line in All_lines)
                {
                    single_line += line;
                }

                Parts_string = Command_parser.Split_ignore(single_line, ' ').ToArray();
                
                Parts = new();
            }

            error = "";
            Line_num = line_num;
        }
        /// <summary>
        /// Will return all lines in the command, with the \ at the end
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return all_lines_unformatted;
        }

        public string Read_next()
        {
            if (Read_index >= Parts_string.Length)
            {
                return null;
            }

            return Parts_string[Read_index++];
        }

        public void Parse(out string error)
        {
            //Already had an error during earlier phase, don't try parsing it
            if(Errored)
            {
                error = "";
                return;
            }

            error = "";

            if (Lines_type == Command_type.Comment)
            {
                return;
            }

            if (Parts_string.Length == 0)
            {
                return;
            }

            //TODO handle correctly
            if (Macro_line)
            {
                return;
            }

            Parse_sub(Parser, out error);

            if (error != "")
            {
                return;
            }

            if (Read_index < Parts_string.Length)
            {
                string trailing = "";

                for (int i = Read_index; i < Parts_string.Length; i++)
                {
                    trailing += Parts_string[i] + " ";
                }

                //Remove last space
                trailing = trailing[..^1];

                error = "Found trailing data: " + trailing;
                return;
            }
        }

        public void Parse_sub(Command_parser parser, out string error)
        {
            error = "";

            string command_name = Read_next();

            if (command_name == null)
            {
                error = "Expected command, got nothing";
                return;
            }

            //Get back to name
            Read_index--;

            if (parser.Aliases.ContainsKey(command_name))
            {
                command_name = parser.Aliases[command_name];
            }

            int model_index = parser.Models.FindIndex(m => ((Command_name)m.Parts[0]).Name == command_name);

            if (model_index == -1)
            {
                error = "Command: " + command_name + " is not a recognized command";
                return;
            }

            for (int model_part_index = 0; model_part_index < parser.Models[model_index].Parts.Length; model_part_index++)
            {
                Command_part next = parser.Models[model_index].Parts[model_part_index].Validate(this, out error);

                if (error != "")
                {
                    return;
                }

                if (next != null) Parts.Add(next);

                if (next is Command_choice choice)
                {
                    if (choice.Choices[choice.Choice_index].Parts.Length != 0)
                    {
                        Choice_branch(choice, out error);
                        //return;
                    }
                    //This means the choice doesn't have its own branch
                }
            }

            void Choice_branch(Command_choice choice, out string error)
            {
                error = "";

                //This right now handles "execute run"
                //if(choice.Value == "run")
                //{
                //    Parse_sub(parser);
                //    return;
                //}

                for (int i = 0; i < choice.Choices[choice.Choice_index].Parts.Length; i++)
                {
                    Command_part next = choice.Choices[choice.Choice_index].Parts[i].Validate(this, out error);

                    if (error != "")
                    {
                        return;
                    }

                    if (next != null) Parts.Add(next);

                    if (next is Command_choice choice_)
                    {
                        if (choice_.Choices[choice_.Choice_index].Parts.Length != 0)
                        {
                            Choice_branch(choice_, out error);
                            //return;
                        }
                        //This means the choice doesn't have its own branch
                    }

                    if (next is Command_execute_stop stop_)
                    {
                        if (stop_.Value == "run")
                        {
                            Parse_sub(parser, out error);
                            return;
                        }
                        else
                        {
                            Read_index--;

                            Command_part execute_type = Parser.Get_command_model("execute").Parts[1].Validate(this, out error);

                            if (error != "")
                            {
                                return;
                            }

                            Choice_branch((Command_choice)execute_type, out error);
                            return;
                        }
                    }
                }
            }
        }

        public string Print(bool strip_comments)
        {
            if(Lines_type == Command_type.Command)
            {
                if(Macro_line)
                {
                    //Need support if multilined

                    if (All_lines.Count != 1)
                    {
                        throw new NotImplementedException();
                    }

                    return All_lines[0];
                }
                else
                {
                    if (Parts == null || Parts.Count == 0)
                    {
                        return "";
                    }

                    string result = "";

                    for (int i = 0; i < Parts.Count; i++)
                    {
                        if (Parts[i] is Command_choice choice && choice.Print_include)
                        {
                            continue;
                        }

                        result += Parts[i].ToString() + " ";
                    }

                    //Remove last space
                    result = result[..^1];

                    return result;
                }
            }
            else if(Lines_type == Command_type.Comment)
            {
                if(strip_comments)
                {
                    return null;
                }

                if(All_lines.Count != 1)
                {
                    throw new Exception("Comments should be a single line long");
                }

                return All_lines[0];
            }

            throw new ArgumentException(nameof(Lines_type));
        }
    }

    public enum Command_type
    {
        Comment = 1,
        Command = 2,
    }
}
