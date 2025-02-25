using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace Datapack
{
    public class Detector
    {
        private static List<Change> changes;
        private static void Initialize()
        {
            string item_change_directory = AppDomain.CurrentDomain.BaseDirectory + "/Changes/";

            changes = new List<Change>
            {
                new ("\"execute if data\"", 0, Versions.Get_own_version("1.14") - 1, Change_types.block, Execute_if_data),
                new ("\"forceload\"", 0, Versions.Get_own_version("1.14.4") - 1, Change_types.block, Forceload),
                new ("\"execute if predicate\"", 0, Versions.Get_own_version("1.15") - 1, Change_types.block, Execute_if_predicate),
                new ("\"locatebiome\"", Versions.Get_own_version("1.16"), Versions.Get_own_version("1.18.2"), Change_types.block_other, Locatebiome),
                new ("\"locate\" old syntax", Versions.Get_own_version("1.18.2") + 1, Versions.Max, Change_types.block, Locate_old),
                new ("\"locate\" new syntax", 0, Versions.Get_own_version("1.19") - 1, Change_types.block, Locate_new),
                new ("\"replaceitem\"", Versions.Get_own_version("1.16.5")+1, Versions.Max , Change_types.block, Replaceitem),
                new ("\"item replace\"", 0, Versions.Get_own_version("1.17") - 1, Change_types.block, Item_replace),
                new ("\"item modify\"", 0, Versions.Get_own_version("1.17") - 1, Change_types.block, Item_modify),
                new ("No scoreboard length limit", 0, Versions.Get_own_version("1.18") - 1, Change_types.block, No_scoreboard_length_limits),
                new ("\"execute if biome\"", 0, Versions.Get_own_version("1.19.3") - 1, Change_types.block, Execute_if_biome),
                new ("\"execute on\"", 0, Versions.Get_own_version("1.19.4") - 1, Change_types.block, Execute_on),
                new ("\"execute if dimension\"", 0, Versions.Get_own_version("1.19.4") - 1, Change_types.block, Execute_if_dimension),
                new ("\"execute if loaded\"", 0, Versions.Get_own_version("1.19.4") - 1, Change_types.block, Execute_if_loaded),
                new ("\"execute positioned over\"", 0, Versions.Get_own_version("1.19.4") - 1, Change_types.block, Execute_positioned_over),
                new ("Macros", 0, Versions.Get_own_version("1.20.2") - 1, Change_types.block, Macros),
                new ("BelowName", Versions.Get_own_version("1.20.1") + 1, Versions.Max, Change_types.block, BelowName),
                new ("below_name", 0, Versions.Get_own_version("1.20.2")-1, Change_types.block, Below_name),
                new ("\"scoreboard players display\"", 0, Versions.Get_own_version("1.20.3")-1, Change_types.block, Scoreboard_players_display),
                new ("custom_data", 0, Versions.Get_own_version("1.20.5")-1, Change_types.block, Custom_data),
                new ("components", 0, Versions.Get_own_version("1.20.5")-1, Change_types.block, Components),
                new ("attribute \".generic\"", Versions.Get_own_version("1.21.1")+1, Versions.Max, Change_types.block, Attribute_genetic),
                new ("\"TNTFuse\"", Versions.Get_own_version("1.21.3")+1, Versions.Max, Change_types.block, TNTFuse),
                new ("\"custom_model_data\" number", Versions.Get_own_version("1.21.3")+1, Versions.Max, Change_types.block, Custom_model_data_old),
                new ("\"fuse\"", 0,Versions.Get_own_version("1.21.4")-1, Change_types.block, Fuse),
                new ("\"custom_model_data\" new", 0,Versions.Get_own_version("1.21.4")-1, Change_types.block, Custom_model_data_new),
                new ("\"item_model\"", 0,Versions.Get_own_version("1.21.4")-1, Change_types.block, Item_model),
            };

            if(Directory.Exists(item_change_directory) && false)
            {
                string[] directories = Directory.GetDirectories(item_change_directory);

                Dictionary<string, List<string>> below_present = new ();
                Dictionary<string, List<string>> above_present = new();

                foreach (string directory in directories)
                {
                    string disallow_above = Path.GetFileName(directory).Split('-')[0];
                    string disallow_below = Path.GetFileName(directory).Split('-')[1];

                    using StreamReader reader = new(directory + "/items.txt");
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("+"))
                        {
                            if(!above_present.ContainsKey(disallow_below))
                            {
                                above_present.Add(disallow_below, new List<string>());
                                changes.Add(new Change("Items not existing before: " + disallow_below, 0,Versions.Get_own_version(disallow_below)-1,Change_types.block, Disallow_below));
                            }

                            above_present[disallow_below].Add(line.Substring(1));
                        }
                        else if(line.StartsWith("-"))
                        {
                            if (!below_present.ContainsKey(disallow_above))
                            {
                                below_present.Add(disallow_above, new List<string>());
                                changes.Add(new Change("Items not existing after: " + disallow_above, Versions.Get_own_version(disallow_above)+1, Versions.Max, Change_types.block, Disallow_above));
                            }

                            below_present[disallow_above].Add(line.Substring(1));
                        }
                        else
                        {
                            if (!above_present.ContainsKey(disallow_below))
                            {
                                above_present.Add(disallow_below, new List<string>());
                                changes.Add(new Change("Items not existing before: " + disallow_below, 0, Versions.Get_own_version(disallow_below)-1, Change_types.block, Disallow_below));
                            }

                            if (!below_present.ContainsKey(disallow_above))
                            {
                                below_present.Add(disallow_above, new List<string>());
                                changes.Add(new Change("Items not existing after: " + disallow_above, Versions.Get_own_version(disallow_above)+1, Versions.Max, Change_types.block, Disallow_above));
                            }

                            string[] parts = line.Split('>');
                            below_present[disallow_above].Add(parts[0]);
                            above_present[disallow_below].Add(parts[1]);
                        }
                    }
                }

                //TODO char before + better tables

                bool Disallow_below(string line, Change change)
                {
                    List<string> disalloweds = above_present[Versions.Get_own_version(change.Max_inc_version)];

                    foreach (string disallowed in disalloweds)
                    {
                        if(String_utils.Contains_not_middle(line, disallowed))
                        {
                            Console.WriteLine(disallowed);
                            return true;
                        }
                    }

                    return false;
                }

                bool Disallow_above(string line, Change change)
                {
                    List<string> disalloweds = below_present[Versions.Get_own_version(change.Min_inc_version-2)];

                    foreach (string disallowed in disalloweds)
                    {
                        if (String_utils.Contains_not_middle(line, disallowed))
                        {
                            Console.WriteLine(disallowed);
                            return true;
                        }
                    }

                    return false;
                }
            }

            bool Execute_if_data(string line, Change _) { return line.Contains(" if data ") || line.Contains(" unless data "); }
            bool Forceload(string line, Change _) { return line.Contains("forceload "); }
            bool Execute_if_predicate(string line, Change _) { return line.Contains(" if predicate ") || line.Contains(" unless predicate "); }
            bool Locatebiome(string line, Change _) { return line.Contains("locatebiome "); }

            bool Locate_old(string line, Change _)
            {
                int start_index = line.IndexOf("locate ");

                if (start_index != -1)
                {
                    start_index += "locate ".Length;
                    int end_index = line.IndexOf(' ', start_index);

                    string type_or_selector = line.Substring(start_index, Math.Max(end_index - start_index, line.Length - start_index));

                    if (!(type_or_selector == "poi" || type_or_selector == "biome" || type_or_selector == "structure"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Locate_new(string line, Change _)
            {
                int start_index = line.IndexOf("locate ");

                if (start_index != -1)
                {
                    start_index += "locate ".Length;
                    int end_index = line.IndexOf(' ', start_index);

                    string type_or_selector = line.Substring(start_index, Math.Max(end_index - start_index, line.Length - start_index));

                    if (type_or_selector == "poi" || type_or_selector == "biome" || type_or_selector == "structure")
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Replaceitem(string line, Change _) { return line.Contains("replaceitem "); }
            bool Item_replace(string line, Change _) { return line.Contains("item replace "); }
            bool Item_modify(string line, Change _) { return line.Contains("item modify "); }

            bool No_scoreboard_length_limits(string line, Change _)
            {
                int start_index = line.IndexOf("scoreboard objectives add ");

                if (start_index != -1)
                {
                    start_index += "scoreboard objectives add ".Length;

                    int end_index = line.IndexOf(' ', start_index);
                    string scoreboard = line.Substring(start_index, end_index - start_index);

                    if (scoreboard.Length > 16)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Execute_if_biome(string line, Change _) { return line.Contains(" if biome ") || line.Contains(" unless biome "); }
            bool Execute_on(string line, Change _) { return line.Contains(" on "); }
            bool Execute_if_dimension(string line, Change _) { return line.Contains(" if dimension ") || line.Contains(" unless dimension "); }
            bool Execute_if_loaded(string line, Change _) { return line.Contains(" if loaded ") || line.Contains(" unless loaded "); }
            bool Execute_positioned_over(string line, Change _) { return line.Contains(" positioned over "); }

            bool Macros(string line, Change _) { return line.Contains('$'); }
            bool BelowName(string line, Change _) { return line.Contains("scoreboard objectives setdisplay belowName"); }
            bool Below_name(string line, Change _) { return line.Contains("scoreboard objectives setdisplay below_name"); }
            bool Scoreboard_players_display(string line, Change _) { return line.Contains("scoreboard players display"); }
            bool Custom_data(string line, Change _) { return line.Contains("\"minecraft:custom_data\""); }
            bool Components(string line, Change _) { return line.Contains("components"); }
            bool Attribute_genetic(string line, Change _) { return line.Contains("generic."); }
            bool TNTFuse(string line, Change _) { return line.Contains("TNTFuse"); }

            bool Custom_model_data_old(string line, Change _)
            {
                int start_index = line.IndexOf("custom_model_data");

                if (start_index != -1)
                {
                    start_index += "custom_model_data".Length;

                    //Old style either custom_model_data:1234 or custom_model_data=1234

                    if (char.IsNumber(line[start_index + 1]))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Fuse(string line, Change _) { return line.Contains("fuse"); }

            bool Custom_model_data_new(string line, Change _)
            {
                int start_index = line.IndexOf("custom_model_data");

                if (start_index != -1)
                {
                    start_index += "custom_model_data".Length;

                    //Old style either custom_model_data:1234 or custom_model_data=1234

                    if (!char.IsNumber(line[start_index + 1]))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Item_model(string line, Change _) { return line.Contains("minecraft:item_model"); }
        }

        private string extracted_location;

        private Pack_mcmeta pack_mcmeta;
        private Version_range entire_pack;

        public string output;

        public Detector(string location)
        {
            output = "";

            if (changes == null)
            {
                Initialize();
            }

            Write_line("----------------------------------------");

            Pre_handle(location);

            if (!Parse_mcmeta(extracted_location))
            {
                Write_line("----------------------------------------");
                Write_line("");
            }

            Write_line("");

            Probe_version(extracted_location);

            Write_line("----------------------------------------");
            Write_line("");
        }

        public void Delete_extracted()
        {
            Directory.Delete(extracted_location,true);
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

                Copy(location, temp_folder + "/" + name);
                extracted_location = temp_folder + "/" + name;
            }
        }

        public void Write(string text)
        {
            output += text;
            Console.Write(text);
        }

        public void Write_line(string text)
        {
            output += text + "\n";
            Console.WriteLine(text);
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
                Write_line("Unparseable mcmeta");
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

            Write_line("Assumed version range: " + min_mcmeta_version + "-" + max_mcmeta_version);
            //return mcmeta_version;
            return true;

            Supported_formats Parse_supported_formats(object input)
            {
                if(input == null)
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

        public void Probe_version(string path)
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

            foreach(Function_call function in load_run)
            {
                Scan_function(path, function);
            }

            foreach (Function_call function in tick_run)
            {
                Scan_function(path, function);
            }

            Write_line("");

            Write_line("Load functions: ");


            for(int i = 0; i < load_run.Count; i++)
            {
                if (load_run[i].Overide_directory != "") { Console.Write("(" + load_run[i].Overide_directory + ")"); }
                Console.Write(load_run[i].Function);
                Console.Write(": ");
                load_run[i].Compatibility.Write(this);
                Write_line("");
            }

            Write_line("");
            Write_line("Ticked functions: ");

            for (int i = 0; i < tick_run.Count; i++)
            {
                if (tick_run[i].Overide_directory != "") { Console.Write("(" + tick_run[i].Overide_directory + ")"); }

                Console.Write(tick_run[i].Function);
                Console.Write(": ");
                tick_run[i].Compatibility.Write(this);
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

            for(int i = 0; i < functions.Count; i++)
            {
                for(int j = 0; j <= Versions.Max; j++)
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

            Write_line("");
            Write_line("");
            Write_line("Entire pack: ");
            entire_pack.Write((int)Math.Round(max / 1.5f), this);
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
                                load_run.Add(new Function_call(true, value,override_directory));
                            }
                        }

                        if (File.Exists(namespace_ + "/tags/functions/tick.json"))
                        {
                            List<string> values = JsonConvert.DeserializeObject<Tags_root>(File.ReadAllText(namespace_ + "/tags/functions/tick.json")).values;

                            foreach (string value in values)
                            {
                                tick_run.Add(new Function_call(true, value,override_directory));
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
                                load_run.Add(new Function_call(false, value,override_directory));
                            }
                        }

                        if (File.Exists(namespace_ + "/tags/function/tick.json"))
                        {
                            List<string> values = JsonConvert.DeserializeObject<Tags_root>(File.ReadAllText(namespace_ + "/tags/function/tick.json")).values;

                            foreach (string value in values)
                            {
                                tick_run.Add(new Function_call(false, value,override_directory));
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

                using StreamReader reader = new(path);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        for (int i = 0; i < changes.Count; i++)
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            changes[i].Check(line, function.Compatibility,false, this);
                            Console.ResetColor();
                        }

                        if (line.Contains("function "))
                        {
                            int start_index = line.IndexOf("function ");

                            start_index += "function ".Length;
                            int end_index = line.IndexOf(' ', start_index);

                            string function_name = line.Substring(start_index, Min_greater_zero(end_index - start_index, line.Length - start_index));

                            //Filtering out some crap (this needs to be done better)
                            if(function_name.Contains(':'))
                            {
                                if (!functions.Any(f => f.Function == function_name && f.Legacy == function.Legacy && f.Overide_directory == function.Overide_directory))
                                {
                                    functions.Add(new Function_call(function.Legacy, function_name, function.Overide_directory));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static int Min_greater_zero(int a, int b)
        {
            if(a < 0)
            {
                return b;
            }

            if(b < 0)
            {
                return a;
            }

            return Math.Min(a, b);
        }

        private static void Copy(string source_directory, string target_directory)
        {
            DirectoryInfo source = new DirectoryInfo(source_directory);
            DirectoryInfo target = new DirectoryInfo(target_directory);

            Copy_all(source, target);
        }

        private static void Copy_all(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                Copy_all(diSourceSubDir, nextTargetSubDir);
            }
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

    public class Tags_root
    {
        public List<string> values;
    }
}
