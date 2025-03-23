using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Command_parsing;
using Newtonsoft.Json;

namespace Datapack
{
    public class Version_identifier
    {
        //private static Dictionary<Version_range, Command_parser> parsers;

        public Version_range Mcmeta_version;

        private string extracted_location;
        private Pack_mcmeta pack_mcmeta;
        private Version_range entire_pack;
        private readonly Action<string, ConsoleColor> output;

        public Version_identifier(string location, Action<string, ConsoleColor> output, out bool parse_success)
        {
            parse_success = true;
            this.output = output;

            Mcmeta_version = new();

            //if (parsers == null)
            //{
            //    Initialize();
            //}

            Write_line("----------------------------------------");

            Pre_handle(location);

            if (!Parse_mcmeta(extracted_location))
            {
                Write_line("----------------------------------------");
                Write_line("");
                parse_success = false;
                return;
            }

            Write_line("");

            Check_version_compatibility(extracted_location);

            Write_line("----------------------------------------");
            Write_line("");
        }
        private void Pre_handle(string location)
        {
            string temp_folder = AppDomain.CurrentDomain.BaseDirectory + "/Temp/";

            if (!Directory.Exists(temp_folder))
            {
                Directory.CreateDirectory(temp_folder);
            }

            if (Path.GetExtension(location).ToLower() == ".zip")
            {
                Write_line("Zip provided, extracting");
                Write_line("");

                string name = Path.GetFileNameWithoutExtension(location);

                //Want to remove everything old in folder
                if (Directory.Exists(temp_folder + "/" + name))
                {
                    Directory.Delete(temp_folder + "/" + name, true);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(location, temp_folder + "/" + name);
                extracted_location = temp_folder + "/" + name;
            }
            else
            {
                Write_line("Directory provided");
                Write_line("");

                string name = Path.GetFileNameWithoutExtension(location);

                //Want to remove everything old in folder
                if (Directory.Exists(temp_folder + "/" + name))
                {
                    Directory.Delete(temp_folder + "/" + name, true);
                }

                Utils.Copy(location, temp_folder + "/" + name);
                extracted_location = temp_folder + "/" + name;
            }
        }

        private bool Parse_mcmeta(string extracted_location)
        {
            if (!File.Exists(extracted_location + "/pack.mcmeta") || !Directory.Exists(extracted_location + "/data"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Write_line("Not a datapack");
                Console.ResetColor();
                return false;
            }

            try
            {
                pack_mcmeta = JsonConvert.DeserializeObject<Pack_mcmeta>(File.ReadAllText(extracted_location + "/pack.mcmeta"));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Write_line("Unparseable mcmeta: ");
                Console.ResetColor();

                Write_line(File.ReadAllText(extracted_location + "/pack.mcmeta"));

                return false;
            }

            Console.Write("Description: ");

            if (pack_mcmeta.pack.description is string text)
            {
                Write_line(text);
            }
            else if (pack_mcmeta.pack.description is IList<Description> texts)
            {
                foreach (Description line in texts)
                {
                    Write_line(line.text);
                }
            }
            string mcmeta_version = Versions.Get_minecraft_version(pack_mcmeta.pack.pack_format, out _);

            Write_line("Mcmeta number: " + pack_mcmeta.pack.pack_format + " Version: " + mcmeta_version);

            Supported_formats pack_supported;

            try
            {
                pack_supported = Parse_supported_formats(pack_mcmeta.pack.supported_formats);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Write_line("Error reading supported versions: " + ex);
                return false;
            }

            string min_mcmeta_version;
            string max_mcmeta_version;

            if (pack_supported != null)
            {
                min_mcmeta_version = Versions.Get_min_minecraft_version(pack_supported.min_inclusive);
                max_mcmeta_version = Versions.Get_max_minecraft_version(pack_supported.max_inclusive);

                Write_line("Min inclusive: " + pack_supported.min_inclusive + " Version: " + Versions.Get_minecraft_version(pack_supported.min_inclusive, out _));
                Write_line("Max inclusive " + pack_supported.max_inclusive + " Version: " + Versions.Get_minecraft_version(pack_supported.max_inclusive, out _));
            }
            else
            {
                min_mcmeta_version = Versions.Get_min_minecraft_version(pack_mcmeta.pack.pack_format);
                max_mcmeta_version = Versions.Get_max_minecraft_version(pack_mcmeta.pack.pack_format);
            }

            int min_ver_num = Versions.Get_own_version(min_mcmeta_version);
            int max_ver_num = Versions.Get_own_version(max_mcmeta_version);


            if(min_ver_num == -1 && max_ver_num == -1)
            {
                
            }
            else if (min_ver_num == -1)
            {
                Mcmeta_version.Set(max_ver_num);
            }
            else if (max_ver_num == -1)
            {
                Mcmeta_version.Set(min_ver_num);
            }
            else
            {
                Mcmeta_version.Set(min_ver_num, max_ver_num, true);
            }

            Write_line("Assumed version range: " + min_mcmeta_version + "-" + max_mcmeta_version);

            return true;

            Supported_formats Parse_supported_formats(object input)
            {
                if (input == null)
                {
                    return null;
                }

                Supported_formats supported_formats;

                try
                {
                    List<int> supported_versions = JsonConvert.DeserializeObject<List<int>>(input.ToString());

                    //Assuming this right now (0 is min, 1 is max)

                    if (supported_versions.Count != 2)
                    {
                        throw new Exception("Unparseable min/max");
                    }

                    supported_formats = new()
                    {
                        min_inclusive = supported_versions[0],
                        max_inclusive = supported_versions[1]
                    };

                    if (pack_mcmeta.pack.pack_format > supported_formats.max_inclusive || pack_mcmeta.pack.pack_format < supported_formats.min_inclusive)
                    {
                        throw new Exception("Unparseable outside min/max");
                    }

                    return supported_formats;

                }
                catch (JsonException)
                {
                    // If above fails this should suceed, else something is wrong
                    try
                    {
                        supported_formats = JsonConvert.DeserializeObject<Supported_formats>(input.ToString());

                        if (pack_mcmeta.pack.pack_format > supported_formats.max_inclusive || pack_mcmeta.pack.pack_format < supported_formats.min_inclusive)
                        {
                            throw new Exception("Unparseable outside min/max");
                        }

                        return supported_formats;
                    }
                    catch (JsonException)
                    {
                        throw new Exception("Unparseable supported list");
                    }
                }
            }
        }

        public void Check_version_compatibility(string path)
        {
            //bool[] supported = new bool[versions.Max+1];

            List<Function_call> load_run = new();
            List<Function_call> tick_run = new();

            string[] namespaces = Directory.GetDirectories(path + "/data");

            ////Function (..1.20.6)
            //bool functions = false;

            ////Predicates (1.15..)
            //bool predicates = false;

            ////1.17
            //bool item_modifiers = false;


            ////Functions (1.21..)
            //bool function = false;

            ////Predicates (1.21..)
            //bool predicate = false;

            ////1.21..
            //bool item_modifier = false;

            ////1.21..
            //bool enchantment = false;

            foreach (string namespace_ in namespaces)
            {
                Add_function_namespace(namespace_);
            }

            //Overlays can ovverride the /data on specified versions

            if (pack_mcmeta.overlays != null && pack_mcmeta.overlays.entries != null)
            {
                foreach (Entry entry in pack_mcmeta.overlays.entries)
                {
                    namespaces = Directory.GetDirectories(path + "/" + entry.directory + "/data/");

                    foreach (string namespace_ in namespaces)
                    {
                        Add_function_namespace(namespace_, entry.directory);
                    }
                }
            }


            List<Function_call> functions = new();

            foreach (Function_call function in load_run)
            {
                Scan_function(path, function);
            }

            foreach (Function_call function in tick_run)
            {
                Scan_function(path, function);
            }

            Write_line("");

            Write_line("Load functions: ");


            for (int i = 0; i < load_run.Count; i++)
            {
                if (load_run[i].Overide_directory != "") { Console.Write("(" + load_run[i].Overide_directory + ")"); }
                Console.Write(load_run[i].Function);
                Console.Write(": ");
                load_run[i].Compatibility.Write(output);
                Write_line("");
            }

            Write_line("");
            Write_line("Ticked functions: ");

            for (int i = 0; i < tick_run.Count; i++)
            {
                if (tick_run[i].Overide_directory != "") { Console.Write("(" + tick_run[i].Overide_directory + ")"); }

                Console.Write(tick_run[i].Function);
                Console.Write(": ");
                tick_run[i].Compatibility.Write(output);
                Write_line("");
            }

            for (int i = 0; i < functions.Count; i++)
            {
                Scan_function(path, functions[i]);
            }

            //Also add already scanned load + tick
            functions.AddRange(load_run);
            functions.AddRange(tick_run);

            //Now count how many times a given versions is entire_pack

            entire_pack = new();

            for (int i = 0; i < functions.Count; i++)
            {
                for (int j = 0; j <= Versions.Max; j++)
                {
                    //TODO wrap into function
                    if (functions[i].Compatibility.Is_set(j))
                    {
                        entire_pack.Add(j);
                    }
                }
            }

            int max = entire_pack.Get_max();
            //Debug levels
            //Write_line("");
            //for (int i = 0; i <= Versions.Max; i++)
            //{
            //    Write_line(Versions.Get_own_version(i) + ": " + entire_pack.Get_level(i));
            //}

            //TODO might want to list total function count as well (only really important when we don't have lots of validators)
            //Can't cont those the normal way

            Write_line("");
            Write_line("");
            Write_line("Version range scores: ");
            entire_pack.Write_scores(output);

            Write_line("");
            Write_line("");
            Write_line("Entire pack: ");
            entire_pack.Write((int)Math.Round(max / 1.5f), output);
            Write_line("");

            //TODO allow allowed versions argument
            void Add_function_namespace(string namespace_, string override_directory = "")
            {
                if (Directory.Exists(namespace_ + "/tags") && Path.GetFileName(namespace_) == "minecraft")
                {
                    string[] tags = Directory.GetDirectories(namespace_ + "/tags");

                    if (Directory.Exists(namespace_ + "/tags/functions"))
                    {
                        if (File.Exists(namespace_ + "/tags/functions/load.json"))
                        {
                            List<string> values = JsonConvert.DeserializeObject<Tags_root>(File.ReadAllText(namespace_ + "/tags/functions/load.json")).values;

                            foreach (string value in values)
                            {
                                load_run.Add(new Function_call(true, value, override_directory));
                            }
                        }

                        if (File.Exists(namespace_ + "/tags/functions/tick.json"))
                        {
                            List<string> values = JsonConvert.DeserializeObject<Tags_root>(File.ReadAllText(namespace_ + "/tags/functions/tick.json")).values;

                            foreach (string value in values)
                            {
                                tick_run.Add(new Function_call(true, value, override_directory));
                            }
                        }
                    }

                    if (Directory.Exists(namespace_ + "/tags/function"))
                    {
                        if (File.Exists(namespace_ + "/tags/function/load.json"))
                        {
                            List<string> values = JsonConvert.DeserializeObject<Tags_root>(File.ReadAllText(namespace_ + "/tags/function/load.json")).values;

                            foreach (string value in values)
                            {
                                load_run.Add(new Function_call(false, value, override_directory));
                            }
                        }

                        if (File.Exists(namespace_ + "/tags/function/tick.json"))
                        {
                            List<string> values = JsonConvert.DeserializeObject<Tags_root>(File.ReadAllText(namespace_ + "/tags/function/tick.json")).values;

                            foreach (string value in values)
                            {
                                tick_run.Add(new Function_call(false, value, override_directory));
                            }
                        }
                    }
                }
            }

            void Scan_function(string root, Function_call function)
            {
                string path;

                if (function.Overide_directory == "")
                {
                    path = root + "/data/";
                }
                else
                {
                    path = root + "/" + function.Overide_directory + "/data/";
                }

                if (function.Legacy)
                {
                    path += function.Namespace + "/functions/" + function.Name + ".mcfunction";
                }
                else
                {
                    path += function.Namespace + "/function/" + function.Name + ".mcfunction";
                }

                Console.WriteLine("Scanning function: " + path);

                if (!File.Exists(path))
                {
                    //Not just overrided that wasn't found
                    if (function.Overide_directory == "")
                    {
                        return;
                    }

                    //And if a override function does not exist it still falls back on the non overriden, even though called from overriden

                    path = root + "/data/";

                    if (function.Legacy)
                    {
                        path += function.Namespace + "/functions/" + function.Name + ".mcfunction";
                    }
                    else
                    {
                        path += function.Namespace + "/function/" + function.Name + ".mcfunction";
                    }

                    //And if that fails we actually have an unknown file
                    if (!File.Exists(path))
                    {
                        function.Compatibility.Unset();
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (function.Overide_directory != "") { Console.Write("(" + function.Overide_directory + ")"); }
                        Write_line(function.Function + " does not exist yet is called");
                        Console.ResetColor();
                        return;
                    }
                }

                for (int i = 0; i <= Versions.Max; i++)
                {
                    if(function.Compatibility.Is_set(i))
                    {
                        string minecraft_version = Versions.Get_own_version(i);

                        if (Parser_creator.Get_parser(minecraft_version, out Command_parser parser))
                        {
                            if(!parser.Parse(File.ReadAllLines(path), out string error))
                            {
                                function.Compatibility.Unset(i);
                            }

                            foreach (string function_name in parser.Result.Called_functions)
                            {
                                if (!functions.Any(f => f.Function == function_name && f.Legacy == function.Legacy && f.Overide_directory == function.Overide_directory))
                                {
                                    functions.Add(new Function_call(function.Legacy, function_name, function.Overide_directory));
                                }
                            }
                        }
                        else  
                        {
                            //Just unsetting compatibility for this version if no validator exists
                            function.Compatibility.Unset(i);

                            //Want the code to scream about missing validators?
                            //Console.ForegroundColor = ConsoleColor.Red;
                            //Write_line(minecraft_version + " does not have a validator yet");
                            //Console.ResetColor();
                        }
                    }
                }

                //using StreamReader reader = new(path);
                //string line;
                //while ((line = reader.ReadLine()) != null)
                //{
                //    if (!line.StartsWith("#"))
                //    {
                //        for (int i = 0; i < changes.Count; i++)
                //        {
                //            Console.ForegroundColor = ConsoleColor.Blue;
                //            changes[i].Check(line, function.Compatibility, false, this);
                //            Console.ResetColor();
                //        }

                //        if (line.Contains("function "))
                //        {
                //            int start_index = line.IndexOf("function ");

                //            start_index += "function ".Length;
                //            int end_index = line.IndexOf(' ', start_index);

                //            string function_name = line.Substring(start_index, Min_greater_zero(end_index - start_index, line.Length - start_index));

                //            //Filtering out some crap (this needs to be done better)
                //            if (function_name.Contains(':'))
                //            {
                //                if (!functions.Any(f => f.Function == function_name && f.Legacy == function.Legacy && f.Overide_directory == function.Overide_directory))
                //                {
                //                    functions.Add(new Function_call(function.Legacy, function_name, function.Overide_directory));
                //                }
                //            }
                //        }
                //    }
                //}
            }
        }

        //private static int Min_greater_zero(int a, int b)
        //{
        //    if (a < 0)
        //    {
        //        return b;
        //    }

        //    if (b < 0)
        //    {
        //        return a;
        //    }

        //    return Math.Min(a, b);
        //}

        public void Write(string text)
        {
            output.Invoke(text, Console.ForegroundColor);
            Console.Write(text);
        }

        public void Write_line(string text)
        {
            output.Invoke(text + "\n", Console.ForegroundColor);
        }
    }
}
