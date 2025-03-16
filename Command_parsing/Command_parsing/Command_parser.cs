using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;

namespace Command_parsing
{
    public class Command_parser
    {
        public readonly string Version;
        public readonly Action<Command_entity,Command_entity> Selector_validator;
        public readonly List<Command_model> Models;
        public readonly Dictionary<string, string> Aliases;
        public readonly Dictionary<string, List<string>> Collections;

        private readonly Dictionary<string, Parse_result> work;

        public Command_parser(string version, Action<Command_entity, Command_entity> selector_validator)
        {
            Version = version;
            Selector_validator = selector_validator;
            Models = new();
            Aliases = new();
            work = new();
            Collections = new();
        }

        public static List<string> Split_ignore(string input, char delimiter)
        {
            List<string> result = new();
            bool in_square_bracket = false;
            bool in_bracket = false;
            bool in_quote = false;

            string build_string = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                char current_char = input[i];

                if (current_char == '[')
                {
                    in_square_bracket = true;
                    build_string += current_char;
                }
                else if (current_char == ']')
                {
                    in_square_bracket = false;
                    build_string += current_char;
                }
                else if (current_char == '{')
                {
                    in_bracket = true;
                    build_string += current_char;
                }
                else if (current_char == '}')
                {
                    in_bracket = false;
                    build_string += current_char;
                }
                else if (current_char == '"')
                {
                    in_quote = !in_quote;
                    build_string += current_char;
                }
                else if (current_char == delimiter && !in_square_bracket && !in_quote && !in_bracket)
                {
                    result.Add(build_string.Trim());
                    build_string = string.Empty;
                }
                else
                {
                    build_string += current_char;
                }
            }

            if (!string.IsNullOrWhiteSpace(build_string))
            {
                result.Add(build_string.Trim());
            }

            return result;
        }

        public void Add_alias(string alias, string real)
        {
            Aliases.Add(alias, real);
        }

        public void Add_rule(Command_model model)
        {
            Models.Add(model);
        }

        public void Add_collection(string name, List<string> collection)
        {
            Collections.Add(name,collection);
        }

        public List<string> Get_collection(string name)
        {
            return Collections[name];
        }
        public bool Parse(string name, string[] entire_file, out string message)
        {
            bool success = true;
            message = "";
            Parse_result result = new();

            for(int i = 0; i < entire_file.Length; i++)
            {
                string line = entire_file[i];

                //Comments
                if (line.StartsWith('#'))
                {
                    continue;
                }

                if(line == "")
                {
                    continue;
                }

                result.Commands.Add(new Command(this,line, i + 1));
            }

            foreach(Command command in result.Commands)
            {
                try
                {
                    command.Parse(this);
                }
                catch(Command_parse_exception ex)
                {
                    success = false;
                    message += "Error parsing line: " + command.Line_num +"\n" + "Entire line: " + command.Entire_line + "\n" + ex.Message + "\n";
                }
            }

            work.Add(name, result);
            //return "Parsed: " + result.Commands.Count;

            if(success)
            {
                return true;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.ResetColor();

            return false;
        }

        public void Clean()
        {
            work.Clear();
        }

        public Parse_result Get_result(string name)
        {
            return work[name];
        }

        public void Clean(string name)
        {
            work.Remove(name);
        }

        public void Verify_collection(string collection, string value)
        {
            if (!Collections.ContainsKey(collection))
            {
                throw new Exception("Collection: " + collection + " is not yet added");
            }

            if (!Collections[collection].Contains(value))
            {
                throw new Command_parse_exception("Collection: " + collection + " does not contain: " + value);
            }
        }

        public Command_model Get_command_model(string name)
        {
            return Models.Find(m  => ((Command_name)m.Parts[0]).Name == name);
        }
    }
}
