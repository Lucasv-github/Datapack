using Datapack;
using Command_parsing;
using Command_parsing.Command_parts;
using System.Diagnostics.Metrics;
using System.Numerics;
using System;
using System.IO.Compression;
using System.IO;
namespace Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                Console.Write("[Identifier] [Parser]");
                string result = Console.ReadLine().ToLower();

                if (result == "identifier")
                {
                    while (true)
                    {
                        Console.Write("Please input the datapack for processing: ");

                        string location = Console.ReadLine();

                        //Windows puts "" around path with spaces
                        location = location.Replace("\"", "");

                        Version_identifier current;

                        //Was a directory that isn't a datapack provided?
                        if (!File.Exists(location) && Directory.Exists(location) && !File.Exists(location + "/pack.mcmeta"))
                        {
                            Console.WriteLine("Folder with no pack.mcmeta, assuming every pack in folder");

                            string[] files = Directory.GetFiles(location);

                            foreach (string file in files)
                            {
                                current = new(file, Output, out _);
                            }
                            return;
                        }

                        _ = new Version_identifier(location, Output, out _);
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
                            if(input.StartsWith('"') && input.EndsWith('"'))
                            {
                                input = input[1..^1];
                            }

                            //string[] lines;

                            if(Directory.Exists(input))
                            {
                                string[] function = Directory.GetFiles(input, "*.mcfunction", SearchOption.AllDirectories);

                                foreach (string file in function)
                                {
                                    Console.WriteLine(file);
                                    Parse_lines(File.ReadAllLines(file));
                                }
                            }
                            else if(File.Exists(input))
                            {
                                if(Path.GetExtension(input).ToLower() == ".zip")
                                {
                                    using var file = File.OpenRead(input);
                                    using var zip = new ZipArchive(file, ZipArchiveMode.Read);
                                    foreach (ZipArchiveEntry entry in zip.Entries)
                                    {
                                        if(Path.GetExtension(entry.FullName).ToLower() != ".mcfunction")
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
                                if(input.StartsWith('/'))
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
                            foreach (var parser in Parser_creator.Parser_collection)
                            {
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.WriteLine(parser.Key + ":");
                                Console.ResetColor();

                                if (parser.Value.Parse(all_lines, out _))
                                {
                                    Console.WriteLine("Number of commands: " + parser.Value.Result.Commands.Count);
                                }
                            }
                        }
                        else
                        {
                            Command_parser parser = Parser_creator.Get_parser(parser_version);

                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.ResetColor();

                            if (parser.Parse(all_lines, out _))
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
