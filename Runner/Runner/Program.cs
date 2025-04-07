using System.IO.Compression;
using Command_parsing;
using Datapack;
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
                    while (true)
                    {
                        Console.Write("Input version/all: ");
                        string parser_version = Console.ReadLine().ToLower();

                        Version_range range;
                        if (parser_version == "all")
                        {
                            range = Version_range.All();
                        }
                        else
                        {
                            range = new();
                            range.Set(Versions.Get_own_version(parser_version));
                        }


                        Console.Write("Please input the datapack for processing: ");
                        string location = Console.ReadLine();

                        //Windows puts "" around path with spaces
                        location = location.Trim('"');

                        //Linux puts '' around path
                        location = location.Trim('\'');


                        Version_identifier current;

                        //Was a directory that isn't a datapack provided?
                        if (!File.Exists(location) && Directory.Exists(location) && !File.Exists(location + "/pack.mcmeta"))
                        {
                            Console.WriteLine("Folder with no pack.mcmeta, assuming every pack in folder");

                            string[] files = Directory.GetFiles(location);

                            foreach (string file in files)
                            {
                                current = new(file, Output, out _, range);
                            }
                            return;
                        }

                        _ = new Version_identifier(location, Output, out _, range);
                    }
                }
                else if (result == "parser")
                {
                    Console.Write("Minecraft version/all: ");
                    string parser_version = Console.ReadLine().ToLower();

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
                        Parser_creator.Create_all();

                        if (parser_version == "all")
                        {
                            for(int i = 0; i <= Versions.Max_own; i++)
                            {
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
                        else
                        {
                            Command_parser parser = Parser_creator.Get_parser(parser_version);

                            Console.ForegroundColor = ConsoleColor.Blue;
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

            static void Output(string text, ConsoleColor color)
            {
                Console.ForegroundColor = color;

                Console.Write(text);

                Console.ResetColor();
            }
        }
    }
}
