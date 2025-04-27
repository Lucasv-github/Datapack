using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing;
using Microsoft.Win32;
using Minecraft_common;

namespace Datapack.Content_serializers
{
    public class Function_serializer : Content_serializer
    {
        /// <summary>
        /// Data read straight from the datapack
        /// </summary>
        private readonly List<Datapack_file> file_functions;

        /// <summary>
        /// Serialized functions of the datapack
        /// </summary>
        private readonly List<Serialized_function> serialized_functions;

        public Function_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> file) : base (loader, serialization_directive)
        {
            file_functions = file;
            serialized_functions = new();

            foreach (Datapack_file file_function in file_functions)
            {
                Scan_function(file_function);
            }

            for (int i = 0; i < serialized_functions.Count; i++)
            {
                if (serialized_functions[i].Success)
                {
                    Serialization_success.Add(serialized_functions[i].Version);
                }
            }

            Write_line("\nTotal function count: " + file_functions.Count);
            Write_line("Function serialization success ranges: \n" + Serialization_success.ToString());
        }

        public override void Print(string root_directory, int version, bool strip_empty, bool strip_comments)
        {
            foreach(Datapack_file file_function in file_functions)
            {
                List<Serialized_function> correct_function = serialized_functions.FindAll(f => f.Short_path == file_function.Short_path);

                if (correct_function.Count == 0)
                {
                    //And no we do not output the unserialized here, the places we do that are when we haven't had time for implement a serializer, to get it to work temporarily.
                    //If serialization fails when it shouldn't thats a bug, not a missing feature

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Function: " + file_function.Short_path + " hasn't been serialized at all");
                    Console.ResetColor();
                }

                correct_function = correct_function.FindAll(f => f.Version == version);

                if (correct_function.Count > 1)
                {
                    throw new Exception("Something is wrong, " + correct_function.Count + " serializations for version: " + version + " of the function: " + file_function.Short_path);
                }

                if (correct_function.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Function: " + file_function.Short_path + " hasn't been serialized at all in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                if (!correct_function[0].Success)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Function: " + file_function.Short_path + " hasn't been serialized succesfully in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                string current_result_path = root_directory + file_function.Short_path;

                Directory.CreateDirectory(Directory.GetParent(current_result_path).ToString());
                File.WriteAllLines(current_result_path, correct_function[0].Serialization_result.Print(strip_empty, strip_comments));
            }
        }

        /// <summary>
        /// Scans the provided file function and generates a serialized_function even in case of parse failure but not in case of no compatible parser against context compatibility
        /// </summary>
        /// <param name="function_file"></param>
        private void Scan_function(Datapack_file function_file)
        {
            Write_line("Scanning function: " + function_file.Short_path);

            //File must exist, wouldn't have been added if not
            string[] all_lines = File.ReadAllLines(function_file.Path);

            for (int i = 0; i <= Versions.Max_own; i++)
            {
                if (function_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    string minecraft_version = Versions.Get_own_version(i);

                    if (Parser_creator.Get_parser(minecraft_version, out Command_parser parser))  //If no parser available the functions doesn't get added
                    {
                        bool success = parser.Parse(all_lines, out List<Tuple<string, ConsoleColor>> output, Register_verifyer);
                        if (output.Count != 0)
                        {
                            Write_line(minecraft_version + ": ");
                        }

                        foreach (Tuple<string, ConsoleColor> message in output)
                        {
                            Console.ForegroundColor = message.Item2;
                            Write(message.Item1);
                        }

                        Console.ResetColor();

                        serialized_functions.Add(new Serialized_function(function_file.Short_path, i, success, parser.Result));
                    }
                }
            }
        }

        //TODO in the future this needs to take into account if that item was serialized succesfully. That also means function serialization will be the last thing to be called to make sure we have everything before this

        /// <summary>
        /// Called from the command parser to verify external register_own
        /// </summary>
        /// <param name="minecraft_version"></param>
        /// <param name="register_own">The own name of the register</param>
        /// <param name="namespace_"></param>
        /// <param name="name"></param>
        /// <param name="error"></param>
        private string Register_verifyer(string minecraft_version, string register_own, string namespace_, string name)
        {
            if(register_own == "SOUND")
            {
                return "Sounds stored in resourcepack, which this currently doesn't support";
            }

            string minecraft_name = To_minecraft_register(register_own);

            if (minecraft_name == null)
            {
                //TODO observe this can fire if something impossible is used like give @s magic:test as it isn't impossible to define custom items with a datapack need flag for this to make yellow instead of purple error

                Console.ForegroundColor = ConsoleColor.Magenta;
                Write_line("Could not convert internal register name: " + register_own + " to minecraft");
                Console.ResetColor();
                return "";
            }

            List<Datapack_file> collection_filtered = loader.Get_datapack_files(minecraft_name);

            ;

            if (collection_filtered.Count == 0)
            {
                return "No external collection by the name: " + register_own + " in namespace: " + namespace_ + " provided for version: " + minecraft_version;
            }

            List<Datapack_file> version_filtered = collection_filtered.FindAll(r => r.Context_compatibility.Is_set(Versions.Get_own_version(minecraft_version)));

            ;

            if (version_filtered.Count == 0)
            {
                return "No external register for version: " + minecraft_version + " provided";
            }

            List<Datapack_file> namespace_filtered = version_filtered.FindAll(r => r.Namespace == namespace_);

            ;

            if (namespace_filtered.Count == 0)
            {
                return "No external namespace by the name: " + namespace_ + " provided for version: " + minecraft_version;
            }

            foreach (Datapack_file register in namespace_filtered)
            {
                if (register.Name == name)
                {
                    return "";
                }
            }

            return "External collection: " + register_own + " does not contain: " + name;
        }

        /// <summary>
        /// Translates the own internal register names used in the validators in Command_parser to minecrafts real names
        /// </summary>
        /// <returns></returns>
        private static string To_minecraft_register(string own_name)
        {
            return own_name switch
            {
                "DIMENSION" => "dimension_type",
                "BLOCK_TAG" => "tags/block",
                "ITEM_TAG" => "tags/item",
                "ENTITY_TAG" => "tags/entity_type",
                "LOOT_TABLE" => "loot_table",
                "ADVANCEMENT" => "advancement",
                "DAMAGE" => "damage_type",
                _ => null,
            };
        }
    }

    public class Serialized_function
    {
        /// <summary>
        /// The short path of the function starting with /data
        /// </summary>
        public string Short_path;

        /// <summary>
        /// The version that this serialization happned in, in ownversion is set even in case of failure
        /// </summary>
        public int Version;

        /// <summary>
        /// Whether or not the serialization was successfull
        /// </summary>
        public bool Success;

        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Parse_result Serialization_result;

        public Serialized_function(string short_path,  int version, bool success, Parse_result serialization_result) 
        {
            Short_path = short_path;
            Version = version;
            Success = success;
            Serialization_result = serialization_result;
        }
    }
}
