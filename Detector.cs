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
        public static string Everything(string location, out List<string> assumed_versions, out List<string> probed_versions)
        {
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
            bool[] supported = new bool[40];

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


            //1.14..
            bool execute_if_data = false;

            //1.14.4..
            bool forceload = false;

            //1.15..
            bool execute_if_predicate = false;

            //1.16..1.18.2
            bool locate_biome = false;

            //..1.18.2
            bool locate_old = false;

            //1.19..
            bool locate_new = false;

            //..1.16.5
            bool replace_item = false;

            //1.17..
            bool item_replace = false;
            bool item_modify = false;


            //18..
            bool no_length_limit = false;

            //1.19.3..
            bool execute_if_biome = false;

            //1.19.4..
            bool execute_on = false;
            bool execute_if_dimension = false;
            bool execute_if_loaded = false;
            bool execute_positioned_over = false;


            //1.20.2
            bool macros = false;

            //..1.20.1
            bool scoreboard_below_name_old = false;

            //1.20.1..
            bool scoreboard_below_name_new = false;

            //1.20.3..
            bool scoreboard_display = false;

            //1.20.5..
            bool custom_data = false;

            //1.20.5..
            bool components = false;

            //1.21..
            bool enchantment = false;

            //..1.21.1
            bool attribute_generic = false;

            //..1.21.3
            bool tnt_old = false;
            bool custom_model_data_old = false;

            //1.21.4..
            bool tnt_new = false;
            bool custom_model_data_new = false;
            bool item_model = false;


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


            //After that it is time for the disallowers

            if (execute_if_data)
            {
                Console.WriteLine("\"execute if data\" points to >=1.14");
                //Set_above(Versions.Get_own_version("1.14"), true);
                Set_below_inc(Versions.Get_own_version("1.14") - 1, false);
            }

            if (forceload)
            {
                Console.WriteLine("\"forceload\" points to >=1.14.4");
                //Set_above(Versions.Get_own_version("1.14.4"), true);
                Set_below_inc(Versions.Get_own_version("1.14.4") - 1, false);
            }

            if (locate_biome)
            {
                Console.WriteLine("\"locatebiome\" points to >=1.16 <= 1.18.2");
                Set_below_inc(Versions.Get_own_version("1.16") - 1, false);
                Set_above_inc(Versions.Get_own_version("1.18.2") + 1, false);
            }

            if (locate_old)
            {
                Console.WriteLine("Locate syntax points to <= 1.18.1");
                Set_above_inc(Versions.Get_own_version("1.18.1") + 1, false);
            }

            if (locate_new)
            {
                Console.WriteLine("Locate syntax points to >= 1.18.2");
                Set_below_inc(Versions.Get_own_version("1.18.2") - 1, false);
            }

            if (replace_item)
            {
                Console.WriteLine("\"replaceitem\" points to <=1.16.5");
                Set_above_inc(Versions.Get_own_version("1.65.5") + 1, false);
            }

            if (item_replace)
            {
                Console.WriteLine("\"item replace\" points to >=1.17");
                Set_below_inc(Versions.Get_own_version("1.17") - 1, false);
            }

            if (item_modify)
            {
                Console.WriteLine("\"item modify\" points to >=1.17");
                Set_below_inc(Versions.Get_own_version("1.17") - 1, false);
            }

            if (execute_if_predicate)
            {
                Console.WriteLine("\"execute if predicate\" points to >=1.15");
                //Set_above(Versions.Get_own_version("1.15"), true);
                Set_below_inc(Versions.Get_own_version("1.15") - 1, false);
            }

            if (execute_if_biome)
            {
                Console.WriteLine("\"execute if biome\" points to >=1.19.3");
                //Set_above(Versions.Get_own_version("1.19.3"), true);
                Set_below_inc(Versions.Get_own_version("1.19.3") - 1, false);
            }

            if (no_length_limit)
            {
                Console.WriteLine("No scoreboard name limits points to >=1.18");
                //Set_above(Versions.Get_own_version("1.19.3"), true);
                Set_below_inc(Versions.Get_own_version("1.18") - 1, false);
            }

            if (scoreboard_below_name_old)
            {
                Console.WriteLine("Scoreboard \"BelowName\" points to <=1.20.1");
                Set_above_inc(Versions.Get_own_version("1.20.1") + 1, false);
            }

            if (scoreboard_below_name_new)
            {
                Console.WriteLine("Scoreboard \"below_name\" points to >=1.20.2");
                Set_below_inc(Versions.Get_own_version("1.20.2") - 1, false);
            }

            if (scoreboard_display)
            {
                Console.WriteLine("Scoreboard display settings points to >=1.20.3");
                Set_below_inc(Versions.Get_own_version("1.20.3") - 1, false);
            }

            if (execute_on)
            {
                Console.WriteLine("\"execute on\" points to >=1.19.4");
                //Set_above(Versions.Get_own_version("1.19.4"), true);
                Set_below_inc(Versions.Get_own_version("1.19.4") - 1, false);
            }

            if (execute_if_dimension)
            {
                Console.WriteLine("\"execute if dimension\" points to >=1.19.4");
                //Set_above(Versions.Get_own_version("1.19.4"), true);
                Set_below_inc(Versions.Get_own_version("1.19.4") - 1, false);
            }

            if (execute_if_loaded)
            {
                Console.WriteLine("\"execute if loaded\" points to >=1.19.4");
                //Set_above(Versions.Get_own_version("1.19.4"), true);
                Set_below_inc(Versions.Get_own_version("1.19.4") - 1, false);
            }

            if (execute_positioned_over)
            {
                Console.WriteLine("\"execute positioned over\" points to >=1.19.4");
                //Set_above(Versions.Get_own_version("1.19.4"), true);
                Set_below_inc(Versions.Get_own_version("1.19.4") - 1, false);
            }

            if (macros)
            {
                Console.WriteLine("Macros points to >=1.20.2");
                Set_below_inc(Versions.Get_own_version("1.20.2") - 1, false);
            }

            if (custom_data)
            {
                Console.WriteLine("\"custom_data\" points to >=1.20.5");
                Set_below_inc(Versions.Get_own_version("1.20.5") - 1, false);
            }

            if (components)
            {
                Console.WriteLine("\"components\" points to >=1.20.5");
                Set_below_inc(Versions.Get_own_version("1.20.5") - 1, false);
            }

            if (enchantment)
            {
                Console.WriteLine("\"/enchentment/\" points to >=1.21");
                Set_below_inc(Versions.Get_own_version("1.21") - 1, false);
            }

            if (attribute_generic)
            {
                Console.WriteLine("\"/generic./\" points to <=1.21.1");
                Set_above_inc(Versions.Get_own_version("1.21.1") + 1, false);
            }

            if (tnt_old)
            {
                Console.WriteLine("\"TNTFuse\" points to <=1.21.3");
                Set_above_inc(Versions.Get_own_version("1.21.3") + 1, false);
            }

            if (tnt_new)
            {
                Console.WriteLine("\"fuse\" points to >=1.21.4");
                Set_below_inc(Versions.Get_own_version("1.21.4") - 1, false);
            }

            if (custom_model_data_old)
            {
                Console.WriteLine("\"custom_model_data\" points to <=1.21.3");
                Set_above_inc(Versions.Get_own_version("1.21.3") + 1, false);
            }

            if (custom_model_data_new)
            {
                Console.WriteLine("\"custom_model_data\" points to >=1.21.4");
                Set_below_inc(Versions.Get_own_version("1.21.4") - 1, false);
            }

            if (item_model)
            {
                Console.WriteLine("\"item_model\" points to >=1.21.4");
                Set_below_inc(Versions.Get_own_version("1.21.4") - 1, false);
            }

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

                                if (!execute_if_data && (line.Contains(" if data ") || line.Contains(" unless data ")))
                                {
                                    execute_if_data = true;
                                }

                                if (!forceload && line.Contains("forceload "))
                                {
                                    forceload = true;
                                }

                                if (!execute_if_predicate && (line.Contains(" if predicate ") || line.Contains(" unless predicate ")))
                                {
                                    execute_if_predicate = true;
                                }

                                if (!locate_biome && line.Contains("locatebiome "))
                                {
                                    locate_biome = true;
                                }

                                if (!locate_old || !locate_new)
                                {
                                    int start_index = line.IndexOf("locate ");

                                    if (start_index != -1)
                                    {
                                        start_index += "locate ".Length;


                                        int end_index = line.IndexOf(' ', start_index);

                                        string type_or_selector = line.Substring(start_index, end_index - start_index);

                                        if (type_or_selector == "poi" || type_or_selector == "biome" || type_or_selector == "structure")
                                        {
                                            locate_new = true;
                                        }
                                        else
                                        {
                                            locate_old = false;
                                        }
                                    }
                                }

                                if (!replace_item && line.Contains("replaceitem "))
                                {
                                    replace_item = true;
                                }

                                if (!item_replace && line.Contains("item replace "))
                                {
                                    item_replace = true;
                                }

                                if (!item_modify && line.Contains("item modify "))
                                {
                                    item_modify = true;
                                }

                                if (!execute_if_biome && (line.Contains(" if biome ") || line.Contains(" unless biome ")))
                                {
                                    execute_if_biome = true;
                                }


                                if (!execute_on && line.Contains(" on "))
                                {
                                    execute_on = true;
                                }

                                if (!execute_if_dimension && (line.Contains(" if dimension ") || line.Contains(" unless dimension ")))
                                {
                                    execute_if_dimension = true;
                                }

                                if (!execute_if_loaded && (line.Contains(" if loaded ") || line.Contains(" unless loaded ")))
                                {
                                    execute_if_loaded = true;
                                }

                                if (!execute_positioned_over && line.Contains(" positioned over "))
                                {
                                    execute_positioned_over = true;
                                }

                                //TOOD temas as well

                                if (!no_length_limit)
                                {
                                    int start_index = line.IndexOf("scoreboard objectives add ");

                                    if (start_index != -1)
                                    {
                                        start_index += "scoreboard objectives add ".Length;

                                        int end_index = line.IndexOf(' ', start_index);
                                        string scoreboard = line.Substring(start_index, end_index - start_index);

                                        if (scoreboard.Length > 16)
                                        {
                                            no_length_limit = true;
                                        }
                                    }
                                }

                                if (!scoreboard_below_name_old && line.Contains("scoreboard objectives setdisplay belowName"))
                                {
                                    scoreboard_below_name_old = true;
                                }

                                if (!scoreboard_below_name_new && line.Contains("scoreboard objectives setdisplay below_name"))
                                {
                                    scoreboard_below_name_new = true;
                                }

                                if (!scoreboard_display && line.Contains("scoreboard players display"))
                                {
                                    scoreboard_display = true;
                                }

                                if (!macros && line.Contains('$'))
                                {
                                    macros = true;
                                }

                                //TODO definitely need stricter checks

                                if (!custom_data && line.Contains("\"minecraft:custom_data\""))
                                {
                                    custom_data = true;
                                }

                                if (!components && line.Contains("components"))
                                {
                                    components = true;
                                }

                                if (!attribute_generic && line.Contains("generic."))
                                {
                                    attribute_generic = true;
                                }

                                if (!tnt_old && line.Contains("TNTFuse"))
                                {
                                    tnt_old = true;
                                }

                                if (!tnt_new && line.Contains("fuse"))
                                {
                                    tnt_new = true;
                                }

                                if (!custom_model_data_old || !custom_model_data_new)
                                {
                                    int start_index = line.IndexOf("custom_model_data");

                                    if (start_index != -1)
                                    {
                                        start_index += "custom_model_data".Length;

                                        //Old style either custom_model_data:1234 or custom_model_data=1234

                                        if (char.IsNumber(line[start_index + 1]))
                                        {
                                            custom_model_data_old = true;
                                        }
                                        else if (line.Contains("custom_model_data"))
                                        {
                                            custom_model_data_new = true;
                                        }
                                    }
                                }

                                //if (!custom_model_data_old || !custom_model_data_new)
                                //{
                                //    if()
                                //        {

                                //    //int start_index = line.IndexOf("custom_model_data=");

                                //    //if(start_index != -1)
                                //    //{
                                //    //    if (char.IsNumber(line[start_index + 1]))
                                //    //    {
                                //    //        custom_model_data_old = true;
                                //    //    }

                                //    //    if (line[start_index + 1] == '{')
                                //    //    {
                                //    //        custom_model_data_new = true;
                                //    //    }

                                //    //    ;
                                //    //}
                                //}

                                if (!item_model && line.Contains("minecraft:item_model"))
                                {
                                    item_model = true;
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
