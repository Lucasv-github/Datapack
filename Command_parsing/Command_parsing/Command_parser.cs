using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Command_parsing.Command_parts;
using Command_parsing.Validators;

namespace Command_parsing
{
    public class Command_parser
    {
        public readonly string Version;
        public readonly int Permission_level;
        public readonly Selector_validator Selector_validator;
        public readonly List<Command_model> Models;
        public readonly Dictionary<string, string> Aliases;
        public readonly Dictionary<string, List<string>> Collections;

        public Parse_result Result;

        public Command_parser(string version, int permission_level, Selector_validator selector_validator)
        {
            Version = version;
            Permission_level = permission_level;
            Selector_validator = selector_validator;
            Models = new();
            Aliases = new();
            Collections = new();
        }

        /// <summary>
        /// Will generate a new with an own work, everything else will be instanced to the original
        /// </summary>
        /// <param name="original"></param>
        public Command_parser(Command_parser original)
        {
            Version = original.Version;
            Permission_level = original.Permission_level;
            Selector_validator = original.Selector_validator;
            Models = original.Models;
            Aliases = original.Aliases;
            Collections = original.Collections;
        }

        /// <summary>
        /// Will generae a new one with everything new, no instances to the old one
        /// </summary>
        /// <param name="versio"></param>
        /// <param name="original"></param>

        public Command_parser(string version, Command_parser original, Selector_validator selector_validator = null)
        {
            Version = version;
            Permission_level = original.Permission_level;

            if(selector_validator == null)
            {
                Selector_validator = original.Selector_validator;
            }
            else
            {
                this.Selector_validator = selector_validator;
            }

            Models = new();

            //TODO think this doesn't clone deep enough yet

            foreach(Command_model model in original.Models)
            {
                Models.Add((Command_model)model.Clone());
            }

            Aliases = new Dictionary<string, string>(original.Aliases);
            Collections = new(original.Collections);
            
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

        public void Add_command(Command_model model)
        {
            string name = ((Command_name)model.Parts[0]).Name;

            if (Models.Any(m => ((Command_name)m.Parts[0]).Name == name))
            {
                throw new Exception("Models already contain: " + name + " use: " + nameof(Add_replace_command) + " instead");
            }

            Models.Add(model);
        }

        public void Add_replace_command(Command_model model)
        {
            string name = ((Command_name)model.Parts[0]).Name;

            if(Models.RemoveAll(m => ((Command_name)m.Parts[0]).Name == name) == 0)
            {
                throw new ArgumentException(name + " is not present in the command list");
            }

            Models.Add(model);
        }

        /// <summary>
        /// This should not be used unless the instances are made clear, safest now is just to remove and re-add anything
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public void Change_command()
        {
            throw new NotSupportedException();
        }

        public void Remove_command(string name)
        {
            Models.RemoveAll(c => ((Command_name)c.Parts[0]).Name == name);
        }

        public void Add_replace_collection(string name, List<string> collection)
        {
            if(Collections.ContainsKey(name))
            {
                Collections[name] = collection;
                return;
            }

            Collections.Add(name, collection);
        }

        public List<string> Get_collection(string name)
        {
            return Collections[name];
        }
        public bool Parse(string[] entire_file, out string message)
        {
            bool success = true;
            message = "";
            Result = new();

            for(int i = 0; i < entire_file.Length; i++)
            {
                string line = entire_file[i];

                //Comments
                if (line.StartsWith('#') || line.StartsWith('$'))
                {
                    continue;
                }

                if(line == "")
                {
                    continue;
                }

                Result.Commands.Add(new Command(this,line, i + 1));
            }

            foreach(Command command in Result.Commands)
            {
                try
                {
                    command.Parse(this);
                }
                catch(Command_parse_exception ex)
                {
                    success = false;
                    message += "Error parsing line: " + command.Line_num +"\n" + "  Entire line: \"" + command.Entire_line + "\"\n    " + ex.Message + "\n" + "\n";
                }
            }

            //return "Parsed: " + Result.Commands.Count;

            if(success)
            {
                return true;
            }

            //TODO remove this
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.ResetColor();

            return false;
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
