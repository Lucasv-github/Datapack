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

                        Detector current;

                        //Was a directory that isn't a datapack provided?
                        if (!File.Exists(location) && Directory.Exists(location) && !File.Exists(location + "/pack.mcmeta"))
                        {
                            Console.WriteLine("Folder with no pack.mcmeta, assuming every pack in folder");

                            string[] files = Directory.GetFiles(location);

                            foreach (string file in files)
                            {
                                current = new(file);
                            }
                            return;
                        }

                        _ = new Detector(location);
                    }
                }
                else if (result == "parser")
                {
                    Command_parser parser = Parser_creator.Create_1_13();

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
                                    if (parser.Parse(file, File.ReadAllLines(file), out _))
                                    {
                                        Console.WriteLine("Number of commands: " + parser.Get_result(file).Commands.Count);
                                    }
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

                                        if (parser.Parse(entry.FullName, lines, out _))
                                        {
                                            Console.WriteLine("Number of commands: " + parser.Get_result(entry.FullName).Commands.Count);
                                        }
                                    }
                                }
                                else if (parser.Parse(input, File.ReadAllLines(input), out _))
                                {
                                    Console.WriteLine("Number of commands: " + parser.Get_result(input).Commands.Count);
                                }
                            }
                            else
                            {
                                if (parser.Parse("command_input", new string[] { input }, out _))
                                {
                                    Console.WriteLine("Number of commands: " + parser.Get_result("command_input").Commands.Count);
                                }
                            }

                            //if(parser.Parse("file_input", new string[] { Console.ReadLine()}, out _))
                            //{
                            //    Console.WriteLine("Number of commands: " + parser.Get_result("file_input").Commands.Count);
                            //}

                            parser.Clean();
                        }
                    }
                }
            }
        }
    }
}
