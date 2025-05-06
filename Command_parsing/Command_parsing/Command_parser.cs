using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Command_parsing.Command_parts;
using Command_parsing.Validators;
using Minecraft_common.Resources;

namespace Command_parsing
{
    public class Command_parser
    {
        public readonly string Version;
        public readonly int Permission_level;
        public readonly bool Multilined;
        public readonly bool Macros;

        public readonly List<Command_model> Models;
        public readonly Dictionary<string, string> Aliases;
        public readonly Dictionary<string, Validator> Validators;
        private Func<string, string, string, string> collection_verifier;

        private string[] all_lines;
        private int line_index = 0;

        private List<Tuple<string, ConsoleColor>> messages;

        private Command current_validate_line;
        public Parse_result Result;



        public Command_parser(string version, int permission_level)
        {
            Version = version;
            Permission_level = permission_level;
            Models = new();
            Aliases = new();
            Validators = new();

            foreach(string key in Resource_handler.Get_resource_names(version))
            {
                Validators.Add(key,new Collection_validator(key));
            }
        }

        /// <summary>
        /// Will generate a new with an own work, everything else will be instanced to the original
        /// </summary>
        /// <param name="original"></param>
        public Command_parser(Command_parser original)
        {
            Version = original.Version;
            Permission_level = original.Permission_level;
            Models = original.Models;
            Aliases = original.Aliases;
            Validators = original.Validators;
            Multilined = original.Multilined;
            Macros = original.Macros;
        }

        /// <summary>
        /// Will generae a new one with everything new, no instances to the old one
        /// </summary>
        /// <param name="versio"></param>
        /// <param name="original"></param>

        public Command_parser(string version, Command_parser original, bool? macros = null, bool? multilined = null)
        {
            Version = version;
            Permission_level = original.Permission_level;

            Macros = macros ?? original.Macros;
            Multilined = multilined ?? original.Multilined;

            Models = new();
            
            //This doesn't deep clone far enough 
            //Should be fine as long as command changes are overwritten

            foreach (Command_model model in original.Models)
            {
                Models.Add((Command_model)model.Clone());
            }

            Aliases = new Dictionary<string, string>(original.Aliases);


            Validators = new(original.Validators);

            foreach (string key in Resource_handler.Get_resource_names(Version))
            {
                if (Validators.ContainsKey(key))
                {
                    Problem_severity severify = Validators[key].Severity;
                    Validators[key] = new Collection_validator(key)
                    {
                        Severity = severify
                    };
                }
                else
                {
                    Validators.Add(key, new Collection_validator(key));
                }
            }
        }

        public static List<string> Split_ignore(string input, char delimiter)
        {
            List<string> result = new();

            int in_bracket = 0;
            int in_square_bracket = 0;

            bool in_quote = false;

            string build_string = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                char current_char = input[i];

                if (current_char == '[')
                {
                    in_square_bracket++;
                    build_string += current_char;
                }
                else if (current_char == ']')
                {
                    in_square_bracket--;
                    build_string += current_char;
                }
                else if (current_char == '{')
                {
                    in_bracket++;
                    build_string += current_char;
                }
                else if (current_char == '}')
                {
                    in_bracket--;
                    build_string += current_char;
                }
                else if (current_char == '"')
                {
                    in_quote = !in_quote;
                    build_string += current_char;
                }
                else if (current_char == delimiter && in_square_bracket == 0 && !in_quote && in_bracket == 0)
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
                throw new Exception("Models already contain: " + name + " use: " + nameof(Replace_command) + " instead");
            }

            Models.Add(model);
        }

        public void Replace_command(Command_model model)
        {
            string name = ((Command_name)model.Parts[0]).Name;

            if (Models.RemoveAll(m => ((Command_name)m.Parts[0]).Name == name) == 0)
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
            if (Models.RemoveAll(c => ((Command_name)c.Parts[0]).Name == name) == 0)
            {
                throw new ArgumentException(name + " is not present in the command list");
            }
        }

        public void Set_validator_severity(string name, Problem_severity severity)
        {
            Validators[name].Severity = severity;
        }
        /// <summary>
        /// Validator severity is carried over
        /// </summary>
        /// <param name="name"></param>
        /// <param name="namespaced"></param>
        /// <param name="collection"></param>
        //public void Add_replace_collection(string name, bool namespaced, List<string> collection)
        //{
        //    Problem_severity severity = Problem_severity.Error;

        //    if (Validators.ContainsKey(name))
        //    {
        //        severity = Validators[name].Severity;
        //        Validators[name] = new Collection_validator(name)
        //        {
        //            Severity = severity
        //        };
        //    }
        //    else
        //    {
        //        Validators.Add(name, new Collection_validator(name));
        //        Validators[name].Severity = severity;
        //    }

        //    if (Collections.ContainsKey(name))
        //    {
        //        Collections[name] = new Tuple<bool, List<string>>(namespaced, collection);
        //    }
        //    else
        //    {
        //        Collections.Add(name, new Tuple<bool, List<string>>(namespaced, collection));
        //    }
        //}

        //public List<string> Get_collection(string name)
        //{
        //    return Collections[name].Item2;
        //}

        public bool Parse(string[] all_lines, out List<Tuple<string, ConsoleColor>> messages, Func<string, string, string, string> collection_verifier = null, bool stop_at_error = true)
        {
            bool success = true;
            this.all_lines = all_lines;
            this.messages = new();
            messages = this.messages;
            this.collection_verifier = collection_verifier;

            if(collection_verifier == null)
            {
                throw new NotImplementedException();
            }

            Result = new();

            while (true)
            {
                string next_line = Read_line();

                if (next_line == null)
                {
                    break;
                }

                Result.Commands.Add(new Command(this, next_line, line_index, out string error));

                if (error != "")
                {
                    messages.Add(new Tuple<string, ConsoleColor>(error, ConsoleColor.Red));

                    if (stop_at_error)
                    {
                        return false;
                    }
                }
            }

            foreach (Command command in Result.Commands)
            {
                current_validate_line = command;

                command.Parse(out string error);

                if (error != "")
                {
                    success = false;
                    messages.Add(new("Error parsing line: " + command.Line_num + "\n" + " Offending lines: \"" + current_validate_line.ToString() + "\"\n    " + error + "\n" + "\n", ConsoleColor.Red));

                    if (stop_at_error)
                    {
                        return false;
                    }
                }
            }

            //return "Parsed: " + Result.Commands.Count;

            if (success)
            {
                return true;
            }

            return false;
        }

        public string Read_line()
        {
            if (line_index >= all_lines.Length)
            {
                return null;
            }

            return all_lines[line_index++];
        }

        public void Verify_collection(string collection, string value, out string error)
        {
            error = collection_verifier.Invoke(Version, collection, value);
            return;
        }
        public void Add_validator(string validator_name, Validator validator)
        {
            if (Validators.ContainsKey(validator_name))
            {
                throw new Exception("Validators already contain: " + validator + " use: " + nameof(Replace_validator) + " instead");
            }

            Validators.Add(validator_name, validator);
        }

        public void Replace_validator(string validator_name, Validator validator)
        {
            if (Validators.ContainsKey(validator_name))
            {
                Validators[validator_name] = validator;
                return;
            }

            throw new Exception("Validators dot not contain: " + validator + " use: " + nameof(Add_validator) + " instead");
        }

        public Validator Get_validator(string validator_name)
        {
            if (!Validators.ContainsKey(validator_name))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("No existing validator: " + validator_name);
                Console.ResetColor();
                return new Unknown_validator(validator_name);
            }

            return Validators[validator_name];
        }
        public Command_model Get_command_model(string name)
        {
            return Models.Find(m => ((Command_name)m.Parts[0]).Name == name);
        }

        public void Write_warning(Command command, string warning)
        {
            messages.Add(new("Warning parsing line: " + command.Line_num + "\n" + "  Entire line: \"" + current_validate_line.All_lines[0] + "\"\n    " + warning + "\n" + "\n", ConsoleColor.Yellow));
        }
    }

    public enum Problem_severity
    {
        Message = 1,
        Warning = 2,  //This will give a warning but still continue as if the parsing went without a problem
        Error = 3  //This will create an warning
    }
}
