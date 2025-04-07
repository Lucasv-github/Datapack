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
        private readonly Version_range scan_directive;

        private readonly External_registers datapack_registers;

        public Version_identifier(string location, Action<string, ConsoleColor> output, out bool parse_success, Version_range scan_directive = null)
        {
            parse_success = true;
            this.output = output;
            this.scan_directive = scan_directive;

            Mcmeta_version = new();

            //if (parsers == null)
            //{
            //    Initialize();
            //}

            Write_line("----------------------------------------");

            if (!Pre_handle(location))
            {
                Write_line("----------------------------------------");
                Write_line("");
                parse_success = false;
                return;
            }

            if (!Parse_mcmeta(extracted_location))
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
            datapack_registers = new External_registers(extracted_location);

            if (!Scan_datapack(extracted_location))
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
                    Console.ForegroundColor = ConsoleColor.Red;
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
                    Console.ForegroundColor = ConsoleColor.Red;
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
                Console.ForegroundColor = ConsoleColor.Red;
                Write_line("Neither file nor folder path provided: " + location);
                Console.ResetColor();
                return false;
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
                Console.ResetColor();
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


            if (min_ver_num == -1 && max_ver_num == -1)
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
                    int supported_version = JsonConvert.DeserializeObject<int>(input.ToString());

                    if (pack_mcmeta.pack.pack_format != supported_version)
                    {
                        throw new Exception("Unparseable supported not equal to pack version");
                    }

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
        }

        public bool Scan_datapack(string path)
        {
            //bool[] supported = new bool[versions.Max_own+1];

            List<Function_call> load_run = new();
            List<Function_call> tick_run = new();

            string[] namespaces = Directory.GetDirectories(path + "/data");

            foreach (string namespace_ in namespaces)
            {
                try
                {
                    Add_function_namespace(namespace_);
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Write_line("Cannot parse function tags from: " + namespace_);
                    Console.ResetColor();
                    return false;
                }
            }

            //Overlays can ovverride the /data on specified versions

            if (pack_mcmeta.overlays != null && pack_mcmeta.overlays.entries != null)
            {
                foreach (Entry entry in pack_mcmeta.overlays.entries)
                {
                    string current_overlay_path = path + "/" + entry.directory + "/data/";

                    if (!Directory.Exists(current_overlay_path))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Overlay doesn't exist: " + current_overlay_path);
                        Console.ResetColor();
                        continue;
                    }


                    namespaces = Directory.GetDirectories(current_overlay_path);

                    foreach (string namespace_ in namespaces)
                    {
                        try
                        {
                            Add_function_namespace(namespace_, entry.directory);
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Write_line("Cannot parse function tags from overlay: " + namespace_);
                            Console.ResetColor();
                            return false;
                        }
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
                for (int j = 0; j <= Versions.Max_own; j++)
                {
                    if (functions[i].Compatibility.Is_set(j))
                    {
                        entire_pack.Add(j);
                    }
                }
            }


            int max = entire_pack.Get_max();
            //Debug levels
            //Write_line("");
            //for (int i = 0; i <= Versions.Max_own; i++)
            //{
            //    Write_line(Versions.Get_own_version(i) + ": " + entire_pack.Get_level(i));
            //}

            //TODO might want to list total function count as well (only really important when we don't have lots of validators)
            //Can't cont those the normal way

            float leniancy = 1;  //Up for higher

            Write_line("");
            Write_line("");
            Write_line("Version range scores: ");
            entire_pack.Write_scores(output);

            Write_line("");
            Write_line("");
            Write_line("Entire pack: ");
            entire_pack.Write((int)Math.Round(max / leniancy), output);
            Write_line("");

            void Add_function_namespace(string namespace_, string override_directory = "")
            {
                if (Directory.Exists(namespace_ + "/tags") && Path.GetFileName(namespace_) == "minecraft")
                {
                    if (Directory.Exists(namespace_ + "/tags/functions"))
                    {
                        Handle_function_tag("/tags/functions/load.json",true, load_run);
                        Handle_function_tag("/tags/functions/tick.json",true, tick_run);
                    }

                    if (Directory.Exists(namespace_ + "/tags/function"))
                    {
                        Handle_function_tag("/tags/function/load.json",false, load_run);
                        Handle_function_tag("/tags/function/tick.json",false, tick_run);
                    }
                }

                void Handle_function_tag(string path, bool legacy, List<Function_call> result_list)
                {
                    if (File.Exists(namespace_ + path))
                    {
                        Tag root = JsonConvert.DeserializeObject<Tag>(File.ReadAllText(namespace_ + path));

                        List<string> values = new();

                        try //Is it as an object
                        {
                            List<Tag_value> tag_values = new();

                            foreach(object unserialized in root.values)
                            {
                                tag_values.Add(JsonConvert.DeserializeObject<Tag_value>(unserialized.ToString()));
                            }

                            foreach(Tag_value tag_value in tag_values)
                            {
                                values.Add(tag_value.id);
                            }
                        }
                        catch  //Is it as a string
                        {
                            foreach(object unserialized in root.values)
                            {
                                values.Add(unserialized.ToString());
                            }
                        }

                        //List<string> tag_values = .tag_values;

                        //TODO will need to handle function tags in tag

                        foreach (string value in values)
                        {
                            result_list.Add(new Function_call(legacy, value, override_directory));
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
                    //It wasn't an overriden function that failed
                    if (function.Overide_directory == "")
                    {
                        function.Compatibility.Unset();
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        if (function.Overide_directory != "") { Console.Write("(" + function.Overide_directory + ")"); }
                        Write_line(function.Function + " does not exist yet is called");
                        Console.ResetColor();
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
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        if (function.Overide_directory != "") { Console.Write("(" + function.Overide_directory + ")"); }
                        Write_line(function.Function + " does not exist yet is called");
                        Console.ResetColor();
                        return;
                    }
                }

                for (int i = 0; i <= Versions.Max_own; i++)
                {
                    if (function.Compatibility.Is_set(i) && (scan_directive == null || scan_directive.Is_set(i)))
                    {
                        string minecraft_version = Versions.Get_own_version(i);

                        if (Parser_creator.Get_parser(minecraft_version, out Command_parser parser))
                        {
                            if (!parser.Parse(File.ReadAllLines(path), out List<Tuple<string, ConsoleColor>> output, datapack_registers))
                            {
                                Console.ResetColor();

                                function.Compatibility.Unset(i);
                            }

                            foreach (Tuple<string, ConsoleColor> message in output)
                            {
                                Console.ForegroundColor = message.Item2;
                                Write(message.Item1);
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
                    else
                    {
                        //Just unsetting compatibility if we weren't given this to scan (reduces output confusion)
                        function.Compatibility.Unset(i);
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

        public void Write(string text)
        {
            output.Invoke(text, Console.ForegroundColor);
        }

        public void Write_line(string text)
        {
            output.Invoke(text + "\n", Console.ForegroundColor);
        }
    }

    public class Pack_mcmeta
    {
        public Pack pack;
        public Overlays overlays;
    }

    public class Pack
    {
        public int pack_format;
        public object description;
        public object supported_formats;
    }

    public class Overlays
    {
        public List<Entry> entries;
    }
    public class Entry
    {
        public object formats;
        public string directory;
    }

    public class Description
    {
        public string text;
    }

    public class Supported_formats
    {
        public int min_inclusive;
        public int max_inclusive;
    }

    public class Tag
    {
        public bool replace;
        public List<object> values;
    }

    public class Tag_value
    {
        public string id;
        public bool required;
    }
}