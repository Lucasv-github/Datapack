using System.IO.Compression;
using Command_parsing;
using Datapack;
using Minecraft_common;
namespace Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("[Identifier] [Parser]");
                string result = Console.ReadLine().ToLower();

                if (result == "identifier")
                {
                    Version_identifier last;

                    while (true)
                    {
                        Console.Write("Input version: ");
                        Version_range scan_range = Get_version_range();

                        Console.Write("Please input the datapack for processing: ");
                        string location = Console.ReadLine();

                        //Windows puts "" around path with spaces
                        location = location.Trim('"');

                        //Linux puts '' around path
                        location = location.Trim('\'');

                        //Was a directory that isn't a datapack provided?
                        if (!File.Exists(location) && Directory.Exists(location) && !File.Exists(location + "/pack.mcmeta"))
                        {
                            Console.WriteLine("Folder with no pack.mcmeta, assuming every pack in folder");

                            string[] files = Directory.GetFiles(location);

                            foreach (string file in files)
                            {
                                last = new(file, out _, scan_range);
                                //Print_messages(last.Get_messages());
                            }

                            return;
                        }
                        last = new Version_identifier(location, out _, scan_range);

                        Console.Write("Do anything more: ");

                        string action;

                        while(true)
                        {
                            action = Console.ReadLine().ToLower();

                            if(action == "no" || action == "n")
                            {
                                break;
                            }

                            if(action == "print")
                            {
                                Console.Write("Input SINGLE print version: ");
                                string print_version = Console.ReadLine();

                                Console.Write("Input parent directory: ");
                                string parent_directory = Console.ReadLine();

                                last.Print(parent_directory,Versions.Get_own_version(print_version),true,true);
                            }
                        }

                        //Print_messages(last.Get_messages());
                    }
                }
                else if (result == "parser")
                {
                    Console.Write("Input version: ");
                    Version_range scan_range = Get_version_range();

                    while (true)
                    {
                        while (true)
                        {
                            Console.Write("Please input command file/command for processing: ");
                            string input = Console.ReadLine();

                            //Windows puts "" around path with spaces
                            if (input.StartsWith('"') && input.EndsWith('"'))
                            {
                                input = input[1..^1];
                            }

                            //string[] lines;

                            if (Directory.Exists(input))
                            {
                                string[] function = Directory.GetFiles(input, "*.mcfunction", SearchOption.AllDirectories);

                                foreach (string file in function)
                                {
                                    Console.WriteLine(file);
                                    Parse_lines(File.ReadAllLines(file));
                                }
                            }
                            else if (File.Exists(input))
                            {
                                if (Path.GetExtension(input).ToLower() == ".zip")
                                {
                                    using var file = File.OpenRead(input);
                                    using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                                    foreach (ZipArchiveEntry entry in zip.Entries)
                                    {
                                        if (Path.GetExtension(entry.FullName).ToLower() != ".mcfunction")
                                        {
                                            continue;
                                        }

                                        using StreamReader reader = new(entry.Open());
                                        string[] lines = reader.ReadToEnd().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                                        Console.WriteLine(entry.FullName);
                                        Parse_lines(lines);
                                    }
                                }
                                else
                                {
                                    Parse_lines(File.ReadAllLines(input));
                                }
                            }
                            else
                            {
                                if (input.StartsWith('/'))
                                {
                                    input = input[1..];
                                }

                                Parse_lines(new string[] { input });
                            }
                        }
                    }

                    void Parse_lines(string[] all_lines)
                    {
                        for (int i = 0; i <= Versions.Max_own; i++)
                        {
                            if(!scan_range.Is_set(i))
                            {
                                continue;
                            }

                            string minecraft_version = Versions.Get_own_version(i);

                            if (Parser_creator.Get_parser(minecraft_version, out Command_parser parser))
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine(minecraft_version + ":");
                                Console.ResetColor();

                                bool success = parser.Parse(all_lines, out List<Tuple<string, ConsoleColor>> output, stop_at_error: false);

                                foreach (Tuple<string, ConsoleColor> message in output)
                                {
                                    Console.ForegroundColor = message.Item2;
                                    Console.Write(message.Item1);
                                }
                                Console.ResetColor();

                                if (success)
                                {
                                    Console.WriteLine("Number of commands: " + parser.Result.Commands.Count);
                                }
                            }
                        }
                    }
                }
            }

            static Version_range Get_version_range()
            {
                Version_range range;
                while (true)
                {
                    string console_input = Console.ReadLine();
                    
                    if(Version_range.Try_parse(console_input, out range))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Could not parse: " + console_input + " as version range, version, \"all\", \"none\" or list of versions");
                        Console.Write("Input version: ");
                    }
                }

                return range;
            }

            //static void Print_messages(List<Tuple<string, ConsoleColor>> messages)
            //{
            //    foreach(Tuple<string, ConsoleColor> message in messages)
            //    {
            //        Console.ForegroundColor = message.Item2;
            //        Console.Write(message.Item1);
            //    }

            //    Console.ResetColor();
            //}
        }
    }
}
