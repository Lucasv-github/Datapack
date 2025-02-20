using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace Datapack
{
    public class Detector
    {
        private static List<Change> changes;

        public static void Initialize()
        {
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
                new ("attribute \".genetir\"", Versions.Get_own_version("1.21.1")+1, Versions.Max, Change_types.block, Attribute_genetic),
                new ("\"TNTFuse\"", Versions.Get_own_version("1.21.3")+1, Versions.Max, Change_types.block, TNTFuse),
                new ("\"custom_model_data\" number", Versions.Get_own_version("1.21.3")+1, Versions.Max, Change_types.block, Custom_model_data_old),
                new ("\"fuse\"", 0,Versions.Get_own_version("1.21.4")-1, Change_types.block, Fuse),
                new ("\"custom_model_data\" new", 0,Versions.Get_own_version("1.21.4")-1, Change_types.block, custom_model_data_new),
                new ("\"item_model\"", 0,Versions.Get_own_version("1.21.4")-1, Change_types.block, Item_model),
            };

            bool Execute_if_data(string line) { return line.Contains(" if data ") || line.Contains(" unless data "); }
            bool Forceload(string line) { return line.Contains("forceload "); }
            bool Execute_if_predicate(string line) { return line.Contains(" if predicate ") || line.Contains(" unless predicate "); }
            bool Locatebiome(string line) { return line.Contains("locatebiome "); }

            bool Locate_old(string line)
            {
                int start_index = line.IndexOf("locate ");

                if (start_index != -1)
                {
                    start_index += "locate ".Length;


                    int end_index = line.IndexOf(' ', start_index);

                    string type_or_selector = line.Substring(start_index, end_index - start_index);

                    if (!(type_or_selector == "poi" || type_or_selector == "biome" || type_or_selector == "structure"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Locate_new(string line)
            {
                int start_index = line.IndexOf("locate ");

                if (start_index != -1)
                {
                    start_index += "locate ".Length;


                    int end_index = line.IndexOf(' ', start_index);

                    string type_or_selector = line.Substring(start_index, end_index - start_index);

                    if (type_or_selector == "poi" || type_or_selector == "biome" || type_or_selector == "structure")
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Replaceitem(string line) { return line.Contains("replaceitem "); }
            bool Item_replace(string line) { return line.Contains("item replace "); }
            bool Item_modify(string line) { return line.Contains("item modify "); }

            bool No_scoreboard_length_limits(string line)
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

            bool Execute_if_biome(string line) { return line.Contains(" if biome ") || line.Contains(" unless biome "); }
            bool Execute_on(string line) { return line.Contains(" on "); }
            bool Execute_if_dimension(string line) { return line.Contains(" if dimension ") || line.Contains(" unless dimension "); }
            bool Execute_if_loaded(string line) { return line.Contains(" if loaded ") || line.Contains(" unless loaded "); }
            bool Execute_positioned_over(string line) { return line.Contains(" positioned over "); }

            bool Macros(string line) { return line.Contains('$'); }
            bool BelowName(string line) { return line.Contains("scoreboard objectives setdisplay belowName"); }
            bool Below_name(string line) { return line.Contains("scoreboard objectives setdisplay below_name"); }
            bool Scoreboard_players_display(string line) { return line.Contains("scoreboard players display"); }
            bool Custom_data(string line) { return line.Contains("\"minecraft:custom_data\""); }
            bool Components(string line) { return line.Contains("components"); }
            bool Attribute_genetic(string line) { return line.Contains("generic."); }
            bool TNTFuse(string line) { return line.Contains("TNTFuse"); }

            bool Custom_model_data_old(string line)
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

            bool Fuse(string line) { return line.Contains("fuse"); }

            bool custom_model_data_new(string line)
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

            bool Item_model(string line) { return line.Contains("minecraft:item_model"); }


        }

        private static void Purge_detected()
        {
            for(int i = 0; i < changes.Count; i++)
            {
                changes[i].Purge();
            }
        }

        public static string Everything(string location, out List<string> assumed_versions, out List<string> probed_versions)
        {
            Purge_detected();

            Console.WriteLine("----------------------------------------");

            string extracted_location = Pre_handle(location);

            if (!Parse_mcmeta(extracted_location, out assumed_versions))
            {
                probed_versions = new List<string>();
                return extracted_location;
            }

            Console.WriteLine();

            Probe_version(extracted_location, out probed_versions);

            Console.WriteLine("----------------------------------------");
            Console.WriteLine();

            return extracted_location;
        }

        public static string Pre_handle(string location)
        {
            string temp_folder = AppDomain.CurrentDomain.BaseDirectory + "/Temp/";

            if (!Directory.Exists(temp_folder))
            {
                Directory.CreateDirectory(temp_folder);
            }


            if (Path.GetExtension(location).ToLower() == ".zip")
            {
                Console.WriteLine("Zip provided, extracting");
                Console.WriteLine();

                string name = Path.GetFileNameWithoutExtension(location);

                //Want to remove everything old in folder
                if (Directory.Exists(temp_folder + "/" + name))
                {
                    Directory.Delete(temp_folder + "/" + name, true);
                }

                System.IO.Compression.ZipFile.ExtractToDirectory(location, temp_folder + "/" + name);
                return temp_folder + "/" + name;
            }
            else
            {
                Console.WriteLine("Directory provided");
                Console.WriteLine();

                string name = Path.GetFileNameWithoutExtension(location);

                //Want to remove everything old in folder
                if (Directory.Exists(temp_folder + "/" + name))
                {
                    Directory.Delete(temp_folder + "/" + name, true);
                }

                Copy(location, temp_folder + "/" + name);
                return temp_folder + "/" + name;
            }
        }

        public static bool Parse_mcmeta(string extracted_location, out List<string> assumed_version)
        {
            //TODO in own format
            assumed_version = null;

            if (!File.Exists(extracted_location + "/pack.mcmeta") || !Directory.Exists(extracted_location + "/data"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not a datapack");
                Console.ResetColor();
                //return "Not datapack";
                return false;
            }

            Pack_mcmeta pack;

            try
            {
                pack = JsonConvert.DeserializeObject<Pack_mcmeta>(File.ReadAllText(extracted_location + "/pack.mcmeta"));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unparseable mcmeta");
                Console.ResetColor();

                Console.WriteLine(File.ReadAllText(extracted_location + "/pack.mcmeta"));

                //return "Unparsable";
                return false;
            }

            Console.Write("Description: ");

            if (pack.pack.description is string text)
            {
                Console.WriteLine(text);
            }
            else if (pack.pack.description is IList<Description> texts)
            {
                foreach (Description line in texts)
                {
                    Console.WriteLine(line.text);
                }
            }

            //if(pack.pack.supported_formats != null)
            //{
            //    Console.WriteLine("Mcmeta min: " + pack.pack.supported_formats.min_inclusive);
            //    Console.WriteLine("Mcmeta max: " + pack.pack.supported_formats.max_inclusive);
            //}

            string mcmeta_version = Versions.Get_minecraft_version(pack.pack.pack_format, out _);

            Console.WriteLine("Mcmeta number: " + pack.pack.pack_format + " Version: " + mcmeta_version);
            Supported_formats supported_formats = null;

            if (pack.pack.supported_formats != null)
            {
                try
                {
                    List<int> supported_versions = JsonConvert.DeserializeObject<List<int>>(pack.pack.supported_formats.ToString());

                    //Assuming this right now (0 is min, 1 is max)

                    if (supported_versions.Count != 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unparseable min/max");
                        Console.ResetColor();

                        //return "Unparsable";
                        return false;
                    }

                    supported_formats = new()
                    {
                        min_inclusive = supported_versions[0],
                        max_inclusive = supported_versions[1]
                    };

                }
                catch (JsonException)
                {
                    // If above fails this should suceed, else something is wrong
                    try
                    {
                        supported_formats = JsonConvert.DeserializeObject<Supported_formats>(pack.pack.supported_formats.ToString());
                    }
                    catch (JsonException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unparseable supported list");
                        Console.ResetColor();
                        //return "Unparsable";
                        return false;
                    }
                }
            }

            string min_mcmeta_version;
            string max_mcmeta_version;

            if (supported_formats != null)
            {
                if (pack.pack.pack_format > supported_formats.max_inclusive || pack.pack.pack_format < supported_formats.min_inclusive)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unparseable outside min/max");
                    Console.ResetColor();

                    //return "Unparsable";
                    return false;
                }

                min_mcmeta_version = Versions.Get_min_minecraft_version(supported_formats.min_inclusive);
                max_mcmeta_version = Versions.Get_max_minecraft_version(supported_formats.max_inclusive);

                Console.WriteLine("Min inclusive: " + supported_formats.min_inclusive + " Version: " + Versions.Get_minecraft_version(supported_formats.min_inclusive, out _));
                Console.WriteLine("Max inclusive " + supported_formats.max_inclusive + " Version: " + Versions.Get_minecraft_version(supported_formats.max_inclusive, out _));
            }
            else
            {
                min_mcmeta_version = Versions.Get_min_minecraft_version(pack.pack.pack_format);
                max_mcmeta_version = Versions.Get_max_minecraft_version(pack.pack.pack_format);
            }

            Console.WriteLine("Assumed version range: " + min_mcmeta_version + "-" + max_mcmeta_version);
            //return mcmeta_version;
            return true;
        }

        public static void Probe_version(string path, out List<string> probed_versions)
        {
            bool[] supported = new bool[Versions.Max+1];

            //TODO possibly 3 or more levels (neutral, blocked, posive)

            string[] namespaces = Directory.GetDirectories(path + "/data");

            //Function (..1.20.6)
            bool functions = false;

            //Predicates (1.15..)
            bool predicates = false;

            //1.17
            bool item_modifiers = false;


            //Functions (1.21..)
            bool function = false;

            //Predicates (1.21..)
            bool predicate = false;

            //1.21..
            bool item_modifier = false;

            //1.21..
            bool enchantment = false;

            foreach (string ns in namespaces)
            {
                if (Directory.Exists(ns + "/functions"))
                {
                    Scan_functions(ns + "/functions");
                    functions = true;
                }

                if (Directory.Exists(ns + "/item_modifiers"))
                {
                    item_modifiers = true;
                }

                if (Directory.Exists(ns + "/predicates"))
                {
                    predicates = true;
                }
            }


            foreach (string ns in namespaces)
            {
                if (Directory.Exists(ns + "/function"))
                {
                    Scan_functions(ns + "/function");
                    function = true;
                }

                if (Directory.Exists(ns + "/item_modifier"))
                {
                    item_modifier = true;
                }

                if (Directory.Exists(ns + "/predicate"))
                {
                    predicate = true;
                }

                if (Directory.Exists(ns + "/predicate"))
                {
                    enchantment = true;
                }
            }

            //The big allowers need to be first

            Console.ForegroundColor = ConsoleColor.Blue;

            if (functions)
            {
                Console.WriteLine("\"/functions/\" points to <=1.20.6");
                Set_below_inc(Versions.Get_own_version("1.20.6"), true);
            }

            if (function)
            {
                Console.WriteLine("\"/function/\" points to >=1.21");
                Set_above_inc(Versions.Get_own_version("1.21"), true);
            }

            //No item modifiers at all can exist below 1.17
            //Question here if still allow it
            //But then everything could really be allowed (some functions work in some versions)

            if (item_modifiers)
            {
                Console.WriteLine("\"/item_modifiers/\" points to >=1.17");
                Set_below_inc(Versions.Get_own_version("1.17") - 1, false);
            }

            if (item_modifier)
            {
                Console.WriteLine("\"/item_modifier/\" points to >=1.21");
                Set_below_inc(Versions.Get_own_version("1.21") - 1, false);
            }

            //No predicates at all can exist below 1.15
            //Question here if still allow it
            //But then everything could really be allowed (some functions work in some versions)

            if (predicates)
            {
                Console.WriteLine("\"/predicates/\" points to >=1.15 <1.21");
                Set_below_inc(Versions.Get_own_version("1.15") - 1, false);
                //Set_above_inc_max(Versions.Get_own_version("1.15"), true, Versions.Get_own_version("1.21"));
            }

            if (predicate)
            {
                Console.WriteLine("\"/predicate/\" points to >=1.21");
                Set_below_inc(Versions.Get_own_version("1.21") - 1, false);
            }

            if (enchantment)
            {
                Console.WriteLine("\"/enchantment/\" points to >=1.21");
                Set_below_inc(Versions.Get_own_version("1.21") - 1, false);
            }

            for(int i = 0; i < changes.Count; i++)
            {
                changes[i].Apply(ref supported);
            }

            Console.ResetColor();

            Console.WriteLine();
            Console.WriteLine("Probed compatibility: ");

            Console.ForegroundColor = ConsoleColor.Green;

            probed_versions = new List<string>();

            for (int i = 0; i < supported.Length; i++)
            {
                if (supported[i])
                {
                    Console.WriteLine(Versions.Get_own_version(i));
                    probed_versions.Add(Versions.Get_own_version(i));
                }
            }

            Console.ResetColor();

            void Set_below_inc(int index, bool value)
            {
                for (int i = index; i >= 0; i--)
                {
                    supported[i] = value;
                }
            }

            void Set_above_inc(int index, bool value)
            {
                for (int i = index; i < supported.Length; i++)
                {
                    supported[i] = value;
                }
            }

            //Lover is included but not max
            void Set_above_inc_max(int index, bool value, int max)
            {
                for (int i = index; i < Math.Min(max, supported.Length); i++)
                {
                    supported[i] = value;
                }
            }

            void Scan_functions(string root)
            {
                //TODO perhaps indexof all?

                //TODO reqursive
                string[] paths = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);

                //https://minecraft.fandom.com/wiki/Commands/execute

                foreach (string path in paths)
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!line.StartsWith("#"))
                            {
                                for(int i = 0; i < changes.Count; i++)
                                {
                                    changes[i].Check(line);
                                }
                            }
                        }
                    }
                }
            }

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

    class Pack_mcmeta
    {
        public Pack pack;
    }

    class Pack
    {
        public int pack_format;
        public object description;
        public object supported_formats;
    }

    class Description
    {
        public string text;
    }

    class Supported_formats
    {
        public int min_inclusive;
        public int max_inclusive;
    }
}
