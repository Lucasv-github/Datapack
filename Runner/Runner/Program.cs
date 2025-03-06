using Datapack;
using Command_parsing;
using Command_parsing.Command_parts;
using System.Diagnostics.Metrics;
using System.Numerics;
using System;
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
                    Command_parser parser = new();
                    Parser_creator.Create_1_13(parser);

                    while (true)
                    {
                        while (true)
                        {
                            Console.Write("Please input command file/command for processing: ");
                            string input = Console.ReadLine();

                            //Windows puts "" around path with spaces
                            input = input.Replace("\"", "");

                            string[] lines;

                            if(File.Exists(input))
                            {
                                lines = File.ReadAllLines(input);
                            }
                            else
                            {
                                lines = new string[] { input };
                            }

                            if (parser.Parse("file_input", lines, out _))
                            {
                                Console.WriteLine("Number of commands: " + parser.Get_result("file_input").Commands.Count);
                            }

                            //if(parser.Parse("file_input", new string[] { Console.ReadLine()}, out _))
                            //{
                            //    Console.WriteLine("Number of commands: " + parser.Get_result("file_input").Commands.Count);
                            //}

                            parser.Clean("file_input");
                        }
                    }
                }
            }

            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
