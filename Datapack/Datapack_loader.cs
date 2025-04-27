using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Command_parsing;
using Datapack.Content_serializers;
using Microsoft.Win32;
using Minecraft_common;
using Newtonsoft.Json;

namespace Datapack
{
    public class Datapack_loader
    {
        /// <summary>
        /// Copy of all messages from this class
        /// </summary>
        private readonly List<Tuple<string, ConsoleColor>> messages;
        /// <summary>
        /// Should datapack be loaded even without a valid mcmeta
        /// </summary>
        private readonly bool bypass_mcmeta;
        /// <summary>
        /// Should a directory still be copied to a teporary directory
        /// </summary>
        private readonly bool force_move;
        /// <summary>
        /// Should errors be logged in console from this class
        /// </summary>
        private readonly bool silent;

        /// <summary>
        /// The location of the datapack, be it the provided or a temporary
        /// </summary>
        private string working_directory;

        /// <summary>
        /// The serialized pack.mcmeta from the datapack
        /// </summary>
        private Pack_mcmeta_json pack_mcmeta;
        /// <summary>
        /// The overlays defined in the pack.mcmeta of the datapack
        /// </summary>
        private List<Overlay> overlays;

        /// <summary>
        /// The minimum version that the datapack indicates it supports, in own version
        /// </summary>
        private int specified_min_own;
        /// <summary>
        /// The maximum version that the datapack indicates it supports, in own version
        /// </summary>
        private int specified_max_own;

        /// <summary>
        /// The key is the minecraft_type, be it function or advancement, all in lowercase even though some might truly be uppercase
        /// </summary>
        private Dictionary<string, List<Datapack_file>> datapack_files;
        private Dictionary<string, Content_serializer> datapack_serialized_files;

        /// <summary>
        /// Will load all the data from the datapack into memory 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="success"></param>
        /// <param name="bypass_mcmeta"></param>
        /// <param name="force_move"></param>
        /// <param name="silent"></param>
        public Datapack_loader(string location, out bool success, bool bypass_mcmeta = false, bool force_move = false, bool silent = false)
        {
            messages = new();
            this.bypass_mcmeta = bypass_mcmeta;
            this.force_move = force_move;
            this.silent = silent;

            Write_line("----------------------------------------");

            if (!Pre_handle(location))
            {
                Write_line("----------------------------------------");
                Write_line("");
                success = false;
                return;
            }

            if (!Parse_mcmeta())
            {
                Write_line("----------------------------------------");
                Write_line("");
                success = false;
                return;
            }

            if(!Load_files())
            {
                Write_line("----------------------------------------");
                Write_line("");
                success = false;
                return;
            }

            Write_line("----------------------------------------");
            success = true;
        }

        /// <summary>
        /// Will serailize the already loaded data in all possible versions
        /// </summary>
        public bool Serialize_datapack(Version_range serialization_directives)
        {
            //Datapack load failed
            if(datapack_files == null)
            {
                return false;
            }

            datapack_serialized_files = new();

            foreach (KeyValuePair<string, List<Datapack_file>> file_type in datapack_files)
            {
                if(file_type.Key == "function")
                {
                    datapack_serialized_files.Add(file_type.Key, new Function_serializer(this, serialization_directives, file_type.Value));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Write_line("No serializer currently exists for: " + file_type.Key);
                    Console.ResetColor();
                }
            }

            return true;
        }

        /// <summary>
        /// Will print the serialized data if exists or else just the raw data to a new datapack
        /// </summary>
        public void Print(string parent_directory, int version, bool strip_empty, bool strip_comments)
        {
            if(datapack_serialized_files == null)
            {
                throw new Exception(nameof(Serialize_datapack) + " needs to be called before " + nameof(Print));
            }

            string print_root = parent_directory + "/TEST/";

            Pack_json pack = new()
            {
                description = "Printed by lprogrammer's Datapack handler on: " + DateTime.UtcNow,
                pack_format = Versions.Get_minecraft_version(Versions.Get_own_version(version))
            };

            Pack_mcmeta_json pack_mcmeta = new()
            {
                pack = pack
            };

            Directory.CreateDirectory(print_root);
            File.WriteAllText(print_root + "/pack.mcmeta", JsonConvert.SerializeObject(pack_mcmeta, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));


            foreach(KeyValuePair<string, List<Datapack_file>> datapack_files in datapack_files)
            {
                if(!datapack_serialized_files.ContainsKey(datapack_files.Key))
                {
                    //We do not have serialized content that can be printed

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Write_line("No serializer currently exists for: " + datapack_files.Key + " using raw passthrough during print");
                    Console.ResetColor();

                    //Fallback consist of just writing the raw text back

                    foreach(Datapack_file datapack_file in datapack_files.Value)
                    {
                        datapack_file.Raw_print(print_root, version);
                    }

                    continue;
                }

                //We have serialized content that can be printed

                datapack_serialized_files[datapack_files.Key].Print(print_root, version, strip_empty, strip_comments);
            }
        }

        /// <summary>
        /// Deals with extraction or copying the file_paths
        /// </summary>
        /// <param name="location"></param>
        /// <returns>false if extraction fails or file doesn't exist, else true </returns>
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

                working_directory = temp_folder + "/" + name;
                return true;
            }
            else if (Directory.Exists(location))
            {
                Write_line("Directory provided");
                Write_line("");

                string name = Path.GetFileNameWithoutExtension(location);

                if(!force_move)
                {
                    working_directory = location;
                    return true;
                }

                //Want to remove everything old in folder
                if (Directory.Exists(temp_folder + "/" + name))
                {
                    Directory.Delete(temp_folder + "/" + name, true);
                }

                Utils.Copy(location, temp_folder + "/" + name);
                working_directory = temp_folder + "/" + name;
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

        /// <summary>
        /// Serializes the pack.mcmeta and sets overlays, specified_min_own and specified_max_own
        /// </summary>
        /// <returns>false if not a datapack, fails to parse pack.mcmeta or if its data is nonsensical, else true</returns>
        /// <exception cref="Exception"></exception>
        private bool Parse_mcmeta()
        {
            if (!File.Exists(working_directory + "/pack.mcmeta") || !Directory.Exists(working_directory + "/data"))
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Write_line("Not a datapack");
                Console.ResetColor();
                return false;
            }

            try
            {
                pack_mcmeta = JsonConvert.DeserializeObject<Pack_mcmeta_json>(File.ReadAllText(working_directory + "/pack.mcmeta"));
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Write_line("Unparseable mcmeta: ");
                Console.ResetColor();

                Write_line(File.ReadAllText(working_directory + "/pack.mcmeta"));

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

            if (min_pack_own == -1 || max_pack_own == -1)
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

                if (min_supported_own == -1)
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

            overlays = new();

            if (pack_mcmeta.overlays != null && pack_mcmeta.overlays.entries != null)
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

                            if (supported_formats.min_inclusive > supported_formats.max_inclusive)
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

        /// <summary>
        /// Populatest the datapack_files Dictionary with every file in a datapack 
        /// </summary>
        /// <returns></returns>
        private bool Load_files() 
        {
            datapack_files = new();

            //Start by handling all file_paths in the regular /data

            string[] namespaces = Directory.GetDirectories(working_directory + "/data");
            Handle_namespaces(namespaces, Version_range.All());

            //Then tackle the overlays

            if (overlays != null)
            {
                foreach (Overlay overlay in overlays)
                {
                    string current_overlay_path = working_directory + "/" + overlay.Path + "/data/";

                    if (!Directory.Exists(current_overlay_path))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Overlay doesn't exist: " + current_overlay_path);
                        Console.ResetColor();
                        continue;
                    }


                    namespaces = Directory.GetDirectories(current_overlay_path);

                    Handle_namespaces(namespaces, overlay.Compatibility);
                }
            }

            Write_line("\nFile types present in the datapack: ");

            foreach(KeyValuePair<string, List<Datapack_file>> file in datapack_files)
            {
                Write_line(file.Key);
            }

            Write_line("");

            return true;
        }

        /// <summary>
        /// Reads in all the files in all the provided namespaces
        /// </summary>
        /// <param name="namespace_paths"></param>
        /// <param name="external_compatibility"></param>
        private void Handle_namespaces(string[] namespace_paths, Version_range external_compatibility)
        {
            Version_range oldschool_compatibility = new(Versions.Get_own_version("1.13"), Versions.Get_own_version("1.20.6"));
            Version_range newschool_compatibility = new(Versions.Get_own_version("1.21"), Versions.Max_own);

            //Very possible that one of these is entirely unset
            oldschool_compatibility = external_compatibility.Get_common(oldschool_compatibility);
            newschool_compatibility = external_compatibility.Get_common(newschool_compatibility);

            foreach (string namespace_path in namespace_paths)
            {
                Handle_namespace(namespace_path, oldschool_compatibility, newschool_compatibility);
            }
        }

//advancements      -> advancement
//chat_types        -> chat_type
//file_functions         -> function X
//item_modifiers    -> item_modifier X
//loot_tables       -> loot_table X
//predicates        -> predicate X
//recipes           -> recipe X
//structures        -> structure X
//tags/blocks       -> tags/block X
//tags/entity_types -> tags/entity_type
//tags/fluids       -> tags/fluid X
//tags/file_functions    -> tags/function X
//tags/game_events  -> tags/game_event
//tags/items        -> tags/item X

        /// <summary>
        /// Reads in all the files in the provided namespace
        /// </summary>
        /// <param name="namespace_path"></param>
        /// <param name="oldschool_namespaces"></param>
        /// <param name="newschool_namespaces"></param>
        private void Handle_namespace(string namespace_path, Version_range oldschool_namespaces, Version_range newschool_namespaces)
        {
            //This allows warning the user if we find folders that doesn't correspond to a something minecraft handles (and allows us to find bugs in this code)
            List<string> namespace_directories = new (Directory.GetDirectories(namespace_path, "*", SearchOption.AllDirectories));

            for(int i = 0; i < namespace_directories.Count; i++)
            {
                namespace_directories[i] = Path.GetRelativePath(namespace_path, namespace_directories[i]).Replace("\\","/");
            }

            namespace_directories.Remove("tags");
            namespace_directories.Remove("worldgen");

            List<string> added_directories = new();

            //https://minecraft.wiki/w/Data_pack

            //These are the oldschool
            //Some have been dissabled as they are unreacable below 1.21
            Add_files(namespace_path, "functions", oldschool_namespaces, ".mcfunction");
            Add_files(namespace_path, "structures", oldschool_namespaces, ".nbt");

            Add_files(namespace_path, "tags/blocks", oldschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/items", oldschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/functions", oldschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/fluids", oldschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/entity_types", oldschool_namespaces.Get_common(new Version_range("1.14", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/game_event", oldschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/biome", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/flat_level_generator_preset", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/world_preset", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/structures", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            //Add_files(namespace_path, "tags/cat_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json",true);
            Add_files(namespace_path, "tags/point_of_interest_type", oldschool_namespaces, ".json", true);
            //Add_files(namespace_path, "tags/painting_variant", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json",true);
            Add_files(namespace_path, "tags/banner_pattern", oldschool_namespaces, ".json", true);
            //Add_files(namespace_path, "tags/instrument", oldschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json",true);
            Add_files(namespace_path, "tags/damage_type", oldschool_namespaces.Get_common(new Version_range("1.19.4", Versions.Max_minecraft)), ".json", true);
            //Add_files(namespace_path, "tags/enchantment", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json",true);

            Add_files(namespace_path, "advancements", oldschool_namespaces, ".json");
            Add_files(namespace_path, "banner_pattern", oldschool_namespaces, ".json");
            //Add_files(namespace_path, "cat_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "chat_types", oldschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "cow_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "damage_type", oldschool_namespaces.Get_common(new Version_range("1.19.4", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "dimension", oldschool_namespaces.Get_common(new Version_range("1.16", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "dimension_type", oldschool_namespaces.Get_common(new Version_range("1.16", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "enchantment", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "enchantment_provider", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "frog_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "instrument", oldschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "item_modifiers", oldschool_namespaces.Get_common(new Version_range("1.17", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "jukebox_song", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "loot_tables", oldschool_namespaces, ".json");
            //Add_files(namespace_path, "painting_variant", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "pig_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "predicates", oldschool_namespaces.Get_common(new Version_range("1.15", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "recipes", oldschool_namespaces, ".json");
            //Add_files(namespace_path, "test_environment", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "test_instance", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "trial_spawner", oldschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "trim_material", oldschool_namespaces.Get_common(new Version_range("1.20", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "trim_pattern", oldschool_namespaces.Get_common(new Version_range("1.20", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "wolf_variant", oldschool_namespaces.Get_common(new Version_range("1.20.5", Versions.Max_minecraft)), ".json");

            //FIXME the likelyhood that all these are correct is slim
            Add_files(namespace_path, "worldgen/biome", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/configured_carver", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/configured_feature", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/density_function", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/noise", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/noise_settings", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/placed_feature", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/processor_list", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/structure", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/structure_set", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/template_pool", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/world_preset", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/flat_level_generator_presets", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/multi_noise_biome_source_parameter_list", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");



            //And these are the newschool
            Add_files(namespace_path, "function", newschool_namespaces, ".mcfunction");
            Add_files(namespace_path, "structure", newschool_namespaces, ".nbt");

            Add_files(namespace_path, "tags/block", newschool_namespaces, ".json",true);
            Add_files(namespace_path, "tags/item", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/function", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/fluid", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/entity_type", newschool_namespaces.Get_common(new Version_range("1.14", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/game_event", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/biome", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/flat_level_generator_preset", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/world_preset", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/structure", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/cat_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/point_of_interest_type", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/painting_variant", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/banner_pattern", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/instrument", newschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/damage_type", newschool_namespaces.Get_common(new Version_range("1.19.4", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/enchantment", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json", true);

            Add_files(namespace_path, "advancement", newschool_namespaces, ".json");
            Add_files(namespace_path, "banner_pattern", newschool_namespaces, ".json");
            Add_files(namespace_path, "cat_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "chat_type", newschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "cow_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "damage_type", newschool_namespaces.Get_common(new Version_range("1.19.4", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "dimension", newschool_namespaces.Get_common(new Version_range("1.16", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "dimension_type", newschool_namespaces.Get_common(new Version_range("1.16", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "enchantment", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "enchantment_provider", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "frog_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "instrument", newschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "item_modifier", newschool_namespaces.Get_common(new Version_range("1.17", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "jukebox_song", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "loot_table", newschool_namespaces, ".json");
            Add_files(namespace_path, "painting_variant", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "pig_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "predicate", newschool_namespaces.Get_common(new Version_range("1.15", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "recipe", newschool_namespaces, ".json");
            Add_files(namespace_path, "test_environment", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "test_instance", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "trial_spawner", newschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "trim_material", newschool_namespaces.Get_common(new Version_range("1.20", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "trim_pattern", newschool_namespaces.Get_common(new Version_range("1.20", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "wolf_variant", newschool_namespaces.Get_common(new Version_range("1.20.5", Versions.Max_minecraft)), ".json");

            //FIXME the likelyhood that all these are correct is slim
            Add_files(namespace_path, "worldgen/biome", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/configured_carver", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/configured_feature", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/density_function", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/noise", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/noise_settings", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/placed_feature", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/processor_list", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/structure", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/structure_set", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/template_pool", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/world_preset", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/flat_level_generator_presets", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/multi_noise_biome_source_parameter_list", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");

            foreach(string check in namespace_directories)
            {
                if(!added_directories.Any(d => check.StartsWith(d)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Datapack file: " + Path.GetRelativePath(working_directory, namespace_path + "/" + check) + " is unnecessary");
                    Console.ResetColor();
                }
            }

            void Add_files(string namespace_path, string minecraft_name, Version_range compatibility, string file_type, bool tag = false)
            {
                //TODO make sure compatibility is set, warn otherwise but still add to mute checking

                if (Directory.Exists(namespace_path + "/" + minecraft_name))
                {
                    added_directories.Add(minecraft_name);

                    string minecraft_rectified;

                    if (minecraft_name.EndsWith('s'))
                    {
                        minecraft_rectified = minecraft_name[..^1];
                    }
                    else
                    {
                        minecraft_rectified = minecraft_name;
                    }

                    if (!datapack_files.ContainsKey(minecraft_rectified))
                    {
                        datapack_files.Add(minecraft_rectified, new List<Datapack_file>());
                    }

                    string namespace_name = Path.GetFileName(namespace_path);

                    string[] file_paths = Directory.GetFiles(namespace_path + "/" + minecraft_name, "*" + file_type, SearchOption.AllDirectories);

                    foreach (string path in file_paths)
                    {
                        string full_subpath = Path.GetRelativePath(namespace_path + "/" + minecraft_name, path);
                        string subpath = Path.GetDirectoryName(full_subpath) + "/" + Path.GetFileNameWithoutExtension(full_subpath);

                        subpath = subpath.Replace('\\', '/');

                        if (subpath[0] == '/')
                        {
                            subpath = subpath[1..];
                        }

                        string cleaned_name;

                        //If it is a tag e.g. #magic:test we store it as #test
                        if (tag)
                        {
                            cleaned_name = "#" + subpath;

                        }
                        else
                        {
                            cleaned_name = subpath;
                        }

                        datapack_files[minecraft_rectified].Add(new Datapack_file(path, Path.GetRelativePath(working_directory, path), File.ReadAllText(path), namespace_name, cleaned_name, compatibility));
                    }
                }
            }
        }

        /// <summary>
        /// Will return all unserialized datapack files provided the minecraft name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Datapack_file> Get_datapack_files(string name)
        {
            return datapack_files[name];
        }

        /// <summary>
        /// Write function that saves the text in a per datapack_loader buffer
        /// </summary>
        /// <param name="text"></param>
        public void Write(string text)
        {
            if (!silent)
            {
                Console.Write(text);
            }

            messages.Add(new Tuple<string, ConsoleColor>(text, Console.ForegroundColor));
        }

        /// <summary>
        /// Write function that saves the text in a per datapack_loader buffer
        /// </summary>
        /// <param name="text"></param>
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
    }
}
