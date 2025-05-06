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
        /// Serialized functions of the datapack
        /// </summary>
        private readonly List<Serialized_function> serialized_functions;

        public Function_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files) : base (loader, serialization_directive, files)
        {
            serialized_functions = new();

            foreach (Datapack_file file_function in files)
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

            Write_line("\nTotal serialized function count: " + files.Count);
            Write_line("Function serialization success ranges: \n" + Serialization_success.ToString());
        }

        public override void Print(string root_directory, int version, bool strip_empty, bool strip_comments)
        {
            foreach(Datapack_file function_file in files)
            {
                List<Serialized_function> correct_function = serialized_functions.FindAll(f => f.Short_path == function_file.Short_path);

                if (correct_function.Count == 0)
                {
                    //And no we do not output the unserialized here, the places we do that are when we haven't had time for implement a serializer, to get it to work temporarily.
                    //If serialization fails when it shouldn't thats a bug, not a missing feature

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Function: " + function_file.Short_path + " hasn't been serialized at all");
                    Console.ResetColor();
                }

                correct_function = correct_function.FindAll(f => f.Version == version);

                if (correct_function.Count > 1)
                {
                    throw new Exception("Something is wrong, " + correct_function.Count + " serializations for version: " + version + " of the function: " + function_file.Short_path);
                }

                if (correct_function.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Function: " + function_file.Short_path + " hasn't been serialized at all in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                if (!correct_function[0].Success)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Function: " + function_file.Short_path + " hasn't been serialized succesfully in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                string current_result_path = root_directory + function_file.Short_path;

                Directory.CreateDirectory(Directory.GetParent(current_result_path).ToString());
                File.WriteAllLines(current_result_path, correct_function[0].Serialization_result.Print(strip_empty, strip_comments));
            }
        }

        /// <summary>
        /// Scans the provided files function and generates a serialized_function even in case of parse failure but not in case of no compatible parser against context compatibility
        /// </summary>
        /// <param name="function_file"></param>
        private void Scan_function(Datapack_file function_file)
        {
            Write_line("Scanning function: " + function_file.Short_path);
            string[] all_lines = function_file.Data.Split(new string[] { "\r\n", "\r", "\n" },StringSplitOptions.None);

            for (int i = 0; i <= Versions.Max_own; i++)
            {
                if (function_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    string minecraft_version = Versions.Get_own_version(i);

                    if (Parser_creator.Get_parser(minecraft_version, out Command_parser parser))  //If no parser available the functions doesn't get added
                    {
                        bool success = parser.Parse(all_lines, out List<Tuple<string, ConsoleColor>> output, loader.Verify_collection);
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

                        serialized_functions.Add(new Serialized_function(function_file, i, success, parser.Result));
                    }
                }
            }
        }
    }

    public class Serialized_function : Serialized_datapack_file
    {
        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Parse_result Serialization_result;

        public Serialized_function(Datapack_file file,  int version, bool success, Parse_result serialization_result) : base(file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }
}
