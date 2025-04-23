using System.Diagnostics.Contracts;
using Command_parsing;
using Minecraft_common;
using Newtonsoft.Json;

namespace Datapack
{
    public class Version_identifier
    {
        //private static Dictionary<Version_range, Command_parser> parsers;

        public Version_range Specified_compatibility;
        private int specified_min_own;
        private int specified_max_own;

        public Version_range Scanned_compatibility;

        //TODO add final, combined that takes into account overlays, scanned and specified

        //TODO could be pretty bad with multiple overlays overlaying, need warning for that
        private List<Overlay> overlays;

        private List<Function_call> functions;

        private Pack_mcmeta_json pack_mcmeta;
        private string extracted_location;
        
        private readonly Version_range scan_directive;

        private readonly External_registers datapack_registers;

        private readonly List<Tuple<string, ConsoleColor>> messages;

        private readonly bool silent;

        private readonly Dictionary<string, Dictionary<int, Parse_result>> parsed_results;

        //TODO prevent path traversals, minecraft seems to block with just missing
        public Version_identifier(string location, out bool parse_success, Version_range scan_directive = null, bool silent = false)
        {
            messages = new();

            parse_success = true;
            this.scan_directive = scan_directive;
            this.silent = silent;

            parsed_results = new();

            Specified_compatibility = new();

            Write_line("----------------------------------------");

            if (!Pre_handle(location))
            {
                Write_line("----------------------------------------");
                Write_line("");
                parse_success = false;
                return;
            }

            if (!Parse_mcmeta())
            {
                Write_line("----------------------------------------");
                Write_line("");
                parse_success = false;
                return;
            }

            Write_line("");

            if (scan_directive == null)
            {
                Write_line("----------------------------------------");
                Write_line("");
                return;
            }

            //TODO this could probably fail as well
            datapack_registers = new External_registers(extracted_location, overlays);

            if (!Scan_datapack())
            {
                Write_line("----------------------------------------");
                Write_line("");
                parse_success = false;
                return;
            }

            Write_line("----------------------------------------");
            Write_line("");
        }

        private bool Pre_handle(string location)
        {
            string temp_folder = AppDomain.CurrentDomain.BaseDirectory + "/Temp/";

            if (!Directory.Exists(temp_folder))
            {
                Directory.CreateDirectory(temp_folder);
            }

            if (File.Exists(location))  //If is a file
            {
                if (Path.GetExtension(location).ToLower() != ".zip")  //Might want to mime check as well but the try catch around the extract should take care of those errors
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Write_line("Provided file not a zip file");
                    Console.ResetColor();
                    return false;
                }

                Write_line("Zip provided, extracting");
                Write_line("");

                string name = Path.GetFileNameWithoutExtension(location);

                //Want to remove everything old in folder
                if (Directory.Exists(temp_folder + "/" + name))
                {
                    Directory.Delete(temp_folder + "/" + name, true);
                }

                try
                {
                    Utils.Extract_zip(location, temp_folder + "/" + name);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Write_line("Error extracting: " + ex);
                    Console.ResetColor();
                    return false;
                }

                extracted_location = temp_folder + "/" + name;
                return true;
            }
            else if (Directory.Exists(location))
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
                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Write_line("Neither file nor folder path provided: " + location);
                Console.ResetColor();
                return false;
            }
        }

        //TODO option to continue even if this contains garbage
        private bool Parse_mcmeta()
        {
            if (!File.Exists(extracted_location + "/pack.mcmeta") || !Directory.Exists(extracted_location + "/data"))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Write_line("Not a datapack");
                Console.ResetColor();
                return false;
            }

            try
            {
                pack_mcmeta = JsonConvert.DeserializeObject<Pack_mcmeta_json>(File.ReadAllText(extracted_location + "/pack.mcmeta"));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Write_line("Unparseable mcmeta: ");
                Console.ResetColor();

                Write_line(File.ReadAllText(extracted_location + "/pack.mcmeta"));

                return false;
            }

            Write("Description: ");

            if (pack_mcmeta.pack.description is string text)
            {
                Write_line(text);
            }
            else if (pack_mcmeta.pack.description is IList<Description_json> texts)
            {
                foreach (Description_json line in texts)
                {
                    Write_line(line.text);
                }
            }
            string mcmeta_minecraft_version = Versions.Get_minecraft_version(pack_mcmeta.pack.pack_format, out _);

            Write_line("Mcmeta number: " + pack_mcmeta.pack.pack_format + " Version: " + mcmeta_minecraft_version);

            Supported_formats_json pack_supported;

            try
            {
                pack_supported = Parse_supported_formats(pack_mcmeta.pack.supported_formats);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Write_line("Error reading supported versions: " + ex);
                Console.ResetColor();
                return false;
            }

            //These are from the "Pack format" (don't make my mistake of thinking that can't have a min and max as well)
            int min_pack_own = Versions.Get_own_version(Versions.Get_min_minecraft_version(pack_mcmeta.pack.pack_format, true));
            int max_pack_own = Versions.Get_own_version(Versions.Get_max_minecraft_version(pack_mcmeta.pack.pack_format, true));

            if(min_pack_own == -1 || max_pack_own == -1)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                //Using minecraft's in prints
                Write_line("Pack format: " + pack_mcmeta.pack.pack_format + " is not a recognized version");
                Console.ResetColor();
                return false;
            }

            //Null if no supported, parse failure would have returned out
            if (pack_supported != null)
            {
                string min_supported_minecraft_version = Versions.Get_min_minecraft_version(pack_supported.min_inclusive, true);
                string max_supported_minecraft_version = Versions.Get_max_minecraft_version(pack_supported.max_inclusive, true);

                int min_supported_own = Versions.Get_own_version(min_supported_minecraft_version);
                int max_supported_own = Versions.Get_own_version(max_supported_minecraft_version);

                Write_line("Min inclusive: " + pack_supported.min_inclusive + " Version: " + Versions.Get_minecraft_version(pack_supported.min_inclusive, out _));
                Write_line("Max inclusive " + pack_supported.max_inclusive + " Version: " + Versions.Get_minecraft_version(pack_supported.max_inclusive, out _));

                if(min_supported_own == -1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    //Using minecraft's in prints
                    Write_line("Supported format: " + pack_supported.min_inclusive + " is not a recognized version");
                    Console.ResetColor();
                    return false;
                }

                if (max_supported_own == -1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    //Using minecraft's in prints
                    Write_line("Supported format: " + pack_supported.max_inclusive + " is not a recognized version");
                    Console.ResetColor();
                    return false;
                }


                //This check being here makes it only run if we have specified supported formats
                if (min_pack_own < min_supported_own)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    //Using minecraft's in prints
                    Write_line("Pack format: " + pack_mcmeta.pack.pack_format + " should not be less than min format: " + pack_supported.min_inclusive);
                    Console.ResetColor();
                    return false;
                }

                if (max_pack_own > max_supported_own)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    //Using minecraft's in prints
                    Write_line("Pack format: " + pack_mcmeta.pack.pack_format + " should not be greater than max format: " + pack_supported.max_inclusive);
                    Console.ResetColor();
                    return false;
                }


                //Can then accept this
                //Using what has been confirmed to be the broadest range
                specified_min_own = min_supported_own;
                specified_max_own = max_supported_own;
            }
            else
            {
                specified_min_own = min_pack_own;
                specified_max_own = max_pack_own;
            }

            Specified_compatibility.Set(specified_min_own, specified_max_own, true);

            Write_line("");
            Console.ForegroundColor = ConsoleColor.Blue;
            Write_line("Assumed version range: " + Specified_compatibility.ToString(false));
            Console.ResetColor();

            overlays = new();

            if(pack_mcmeta.overlays != null && pack_mcmeta.overlays.entries != null)
            {
                for (int i = 0; i < pack_mcmeta.overlays.entries.Count; i++)
                {
                    try
                    {
                        Supported_formats_json supported = Parse_supported_formats(pack_mcmeta.overlays.entries[i].formats);

                        int min_overlay_version = Versions.Get_own_version(Versions.Get_min_minecraft_version(supported.min_inclusive, true));
                        int max_overlay_version = Versions.Get_own_version(Versions.Get_max_minecraft_version(supported.max_inclusive, true));

                        if (pack_mcmeta.pack.pack_format < pack_supported.min_inclusive)
                        {
                            throw new Exception("Overlay min: " + min_overlay_version + " should not be less than min format: " + pack_supported.min_inclusive);
                        }

                        if (pack_mcmeta.pack.pack_format > pack_supported.max_inclusive)
                        {
                            throw new Exception("Overlay max: " + max_overlay_version + " should not be greater than max format: " + pack_supported.max_inclusive);
                        }

                        overlays.Add(new Overlay(new Version_range(min_overlay_version, max_overlay_version), pack_mcmeta.overlays.entries[i].directory));
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Write_line("Error parsing overlay supported versions: " + ex);
                        Console.ResetColor();
                        return false;
                    }
                }
            }

            return true;

            static Supported_formats_json Parse_supported_formats(object input)
            {
                if (input == null)
                {
                    return null;
                }

                Supported_formats_json supported_formats;

                try
                {
                    int supported_version = JsonConvert.DeserializeObject<int>(input.ToString());

                    supported_formats = new()
                    {
                        min_inclusive = supported_version,
                        max_inclusive = supported_version
                    };

                    return supported_formats;
                }
                catch (JsonException)
                {
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

                        if (supported_formats.min_inclusive > supported_formats.max_inclusive)
                        {
                            throw new Exception("Min: " + supported_formats.min_inclusive + " should be less than: " + supported_formats.max_inclusive);
                        }

                        return supported_formats;

                    }
                    catch (JsonException)
                    {
                        // If above fails this should suceed, else something is wrong
                        try
                        {
                            supported_formats = JsonConvert.DeserializeObject<Supported_formats_json>(input.ToString());

                            if(supported_formats.min_inclusive > supported_formats.max_inclusive)
                            {
                                throw new Exception("Min: " + supported_formats.min_inclusive + " should be less than: " + supported_formats.max_inclusive);
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
        }

        public bool Scan_datapack()
        {
            //bool[] supported = new bool[versions.Max_own+1];

            functions = new List<Function_call>();

            List<Function_call> load_run = new();
            List<Function_call> tick_run = new();

            for (int i = 0; i <= Versions.Max_own; i++)
            {
                if (scan_directive.Is_set(i))
                {
                    List<string>  loaded_functions = datapack_registers.Get_tag_value(Versions.Get_own_version(i), "FUNCTION_TAG", "minecraft", "#load", out string error);

                    if(error == "")
                    {
                        foreach (string function in loaded_functions)
                        {
                            Add_function(function, new Version_range(i, i), load_run);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error reading tag minecraft:load: " + error);
                        Console.ResetColor();
                    }

                    List<string> ticked_functions = datapack_registers.Get_tag_value(Versions.Get_own_version(i), "FUNCTION_TAG", "minecraft", "#tick", out error);

                    if(error == "")
                    {
                        foreach (string function in ticked_functions)
                        {
                            Add_function(function, new Version_range(i, i), tick_run);
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error reading tag minecraft:tick: " + error);
                        Console.ResetColor();
                    }
                }
            }


            if (load_run.Count != 0)
            {
                Write_line("");
                Write_line("Load functions: ");

                for (int i = 0; i < load_run.Count; i++)
                {
                    Write(load_run[i].String);
                    Write(": ");
                    Write(load_run[i].Compatibility.ToString());
                    Write(": " + load_run[i].Short_path);
                    Write_line("");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Write_line("\nNo load functions\n");
                Console.ResetColor();
            }

            if (tick_run.Count != 0)
            {
                Write_line("");
                Write_line("Tick functions: ");

                for (int i = 0; i < tick_run.Count; i++)
                {
                    Write(tick_run[i].String);
                    Write(": ");
                    Write(tick_run[i].Compatibility.ToString());
                    Write(": " + tick_run[i].Short_path);
                    Write_line("");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Write_line("\nNo tick functions\n");
                Console.ResetColor();
            }

            foreach (Function_call function in load_run)
            {
                Scan_function(function);
            }

            foreach (Function_call function in tick_run)
            {
                Scan_function(function);
            }

            for (int i = 0; i < functions.Count; i++)
            {
                Scan_function(functions[i]);
            }

            //Also add already scanned load + tick
            functions.AddRange(load_run);
            functions.AddRange(tick_run);

            //Now count how many times a given versions is Scanned_compatibility

            Scanned_compatibility = new();

            for (int i = 0; i < functions.Count; i++)
            {
                for (int j = 0; j <= Versions.Max_own; j++)
                {
                    if (functions[i].Compatibility.Is_set(j))
                    {
                        Scanned_compatibility.Add(j);
                    }
                }
            }


            int max = Scanned_compatibility.Get_max();

            if(max == 0)  //If no function at all scanned useless to show that info
            {
                return true;
            }

            //Debug levels
            //Write_line("");
            //for (int i = 0; i <= Versions.Max_own; i++)
            //{
            //    Write_line(Versions.Get_own_version(i) + ": " + Scanned_compatibility.Get_level(i));
            //}

            //TODO might want to list total function count as well (only really important when we don't have lots of validators)
            //Can't cont those the normal way

            //float leniancy = 1;  //Up for higher

            Write_line("");
            Write_line("");
            Write_line("Version range scores: ");

            //Doing it this way to get the colors
            List<Tuple<string, int>> ranges = Scanned_compatibility.Get_ranges();

            foreach (Tuple<string, int> range in ranges)
            {
                Console.ForegroundColor = Scanned_compatibility.Get_range_color(range.Item2);
                Write_line(range.Item1 + ": " + range.Item2);
            }
            Console.ResetColor();

            //Write_line("");
            //Write_line("");
            //Write_line("Entire pack: ");
            //Scanned_compatibility.Write((int)Math.Round(max / leniancy), output);
            //Write_line("");

            void Scan_function(Function_call function)
            {
                Write_line("Scanning function: " + function.Path);

                //File must exist, wouldn't have been added if not
                string[] all_lines = File.ReadAllLines(function.Path);

                for (int i = 0; i <= Versions.Max_own; i++)
                {
                    List<int> versions = new();
                    List<Parse_result> results = new();

                    if (function.Compatibility.Is_set(i) && (scan_directive == null || scan_directive.Is_set(i)))
                    {
                        string minecraft_version = Versions.Get_own_version(i);

                        if (Parser_creator.Get_parser(minecraft_version, out Command_parser parser))
                        {
                            if (!parser.Parse(all_lines, out List<Tuple<string, ConsoleColor>> output, datapack_registers))
                            {
                                function.Compatibility.Unset(i);
                            }

                            if(output.Count != 0)
                            {
                                Write_line(minecraft_version + ": ");
                            }

                            foreach (Tuple<string, ConsoleColor> message in output)
                            {
                                Console.ForegroundColor = message.Item2;
                                Write(message.Item1);
                            }

                            Console.ResetColor();

                            versions.Add(i);
                            results.Add(parser.Result);
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
                    else
                    {
                        //Just unsetting compatibility if we weren't given this to scan (reduces output confusion)
                        function.Compatibility.Unset(i);
                    }

                    //Need to do this last when the compatibility of the current function has been narrowed down
                    for (int j = 0; j <  results.Count; j++)
                    {
                        foreach (string result_function in results[j].Called_functions)
                        {
                            Add_function(result_function, function.Compatibility, functions);
                        }

                        if (parsed_results.ContainsKey(function.Path))
                        {
                            parsed_results[function.Path].Add(versions[j], results[j]);
                        }
                        else
                        {
                            parsed_results[function.Path] = new()
                            {
                                { versions[j], results[j] }
                            };
                        }
                    }
                }
            }

            return true;
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

        //    return Math.Min_own(a, b);
        //}

        //This thing might add a lot of functions as a common load might call lots of other functions depending on how overlays are configured
        public void Add_function(string function_string, Version_range compatibility, List<Function_call> result_list)
        {
            List<string> paths = new();
            Dictionary<string, Version_range> possibilities = new();

            Function_call parsed = new(function_string);

            string namespace_ = parsed.Namespace;


            if(parsed.Tag)
            {
                for (int i = 0; i <= Versions.Max_own; i++)
                {
                    if (compatibility.Is_set(i))
                    {
                        string minecraft_version = Versions.Get_own_version(i);
                        List<string> tag_data = datapack_registers.Get_tag_value(minecraft_version, "FUNCTION_TAG", namespace_, parsed.Name, out string error);

                        if(error != "")
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Write_line("Function tag: " + parsed.String + " is called yet doesn't exist, Version: " + minecraft_version);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Write_line("  Tag error: " + error);
                            Console.ResetColor();
                            continue;
                        }

                        foreach(string function_in_tag in tag_data)
                        {
                            Function_call parsed_in_tag = new(function_in_tag);

                            string function_path = Get_function_path(i, parsed_in_tag.Namespace, parsed_in_tag.Name);

                            if (function_path == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Write_line("Function in tag: " + function_in_tag + " is called yet doesn't exist");
                                Console.ResetColor();
                                continue;
                            }

                            if (possibilities.ContainsKey(function_path))
                            {
                                possibilities[function_path].Set(i);
                            }
                            else
                            {
                                Version_range new_set = new();
                                new_set.Set(i);
                                possibilities.Add(function_path, new_set);
                                paths.Add(function_path);
                            }
                        }
                    } 
                }
            }
            else
            {
                for (int i = 0; i <= Versions.Max_own; i++)
                {
                    if (compatibility.Is_set(i))
                    {
                        string function_path = Get_function_path(i, namespace_, parsed.Name);

                        if (function_path == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Write_line("Function: " + parsed.String + " is called yet doesn't exist");
                            Console.ResetColor();
                            continue;
                        }

                        if (possibilities.ContainsKey(function_path))
                        {
                            possibilities[function_path].Set(i);
                        }
                        else
                        {
                            Version_range new_set = new();
                            new_set.Set(i);
                            possibilities.Add(function_path, new_set);
                            paths.Add(function_path);
                        }
                    }
                }
            }

            
            foreach(KeyValuePair<string, Version_range> possibility in possibilities)
            {
                int existing_index = result_list.FindIndex(f => f.Path == possibility.Key);

                if (existing_index == -1)
                {
                    result_list.Add(new Function_call(parsed, possibility.Value, possibility.Key, Path.GetRelativePath(extracted_location, possibility.Key)));
                }
                else
                {
                    result_list[existing_index].Compatibility.Set(possibility.Value);
                }
            }
        }

        //This will return the path for the function is it was run in the provided version
        //Will return null if the function doesn't exis in both overlay or default
        public string Get_function_path(int own_version, string function_namespace, string function_name)
        {
            string path;

            //First start by chcking for matching overlay

            foreach (Overlay overlay in overlays)
            {
                if (overlay.Compatibility.Is_set(own_version))
                {
                    path = extracted_location + "/" + overlay.Path + "/data/";

                    if (own_version < Versions.Get_own_version("1.21"))  //Legacy
                    {
                        path += function_namespace + "/functions/" + function_name + ".mcfunction";
                    }
                    else
                    {
                        path += function_namespace + "/function/" + function_name + ".mcfunction";
                    }

                    //Question here whether to abort, think more correct is continue checking the overlays (if they are on top of each other)
                    if (File.Exists(path))
                    {
                        //Normalizing the returned stuff to allow use in Dictionary
                        path = Path.GetFullPath(path);
                        return path;
                    }
                }
            }

            path = extracted_location + "/data/";

            if (own_version < Versions.Get_own_version("1.21"))  //Legacy
            {
                path += function_namespace + "/functions/" + function_name + ".mcfunction";
            }
            else
            {
                path += function_namespace + "/function/" + function_name + ".mcfunction";
            }

            if (File.Exists(path))
            {
                //Normalizing the returned stuff to allow use in Dictionary
                path = Path.GetFullPath(path);
                return path;
            }

            return null;
        }

        public void Write(string text)
        {
            if(!silent)
            {
                Console.Write(text);
            }

            messages.Add(new Tuple<string, ConsoleColor>(text, Console.ForegroundColor));
        }

        public void Write_line(string text)
        {
            if (!silent)
            {
                Console.WriteLine(text);
            }

            messages.Add(new Tuple<string, ConsoleColor>(text + "\n", Console.ForegroundColor));
        }
        public List<Tuple<string, ConsoleColor>> Get_messages()
        {
            return messages;
        }

        public override string ToString()
        {
            string value = "";

            foreach(Tuple<string, ConsoleColor> message in messages)
            {
                value += message.Item1;
            }

            return value;
        }

        public void Print(string parent_directory, int version, bool strip_empty, bool strip_comments)
        {
            //TOOD proper error out

            foreach(KeyValuePair<string, Dictionary<int, Parse_result>> result in parsed_results)
            {
                if(!result.Value.ContainsKey(version))
                {
                    Console.WriteLine("File: " + result.Key + " has not been parsed succesfully in version: " + Versions.Get_own_version(version));
                    continue;
                }

                string current_result_path = parent_directory + "/TEST/" + Path.GetRelativePath(extracted_location, result.Key);

                Directory.CreateDirectory(Directory.GetParent(current_result_path).ToString());
                File.WriteAllLines(current_result_path, result.Value[version].Print(strip_empty, strip_comments));
            }

        }
    }
}