using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Command_parsing;
using Datapack.Content_serializers;
using Microsoft.Win32;
using Minecraft_common;
using Minecraft_common.Resources;
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
        /// The comaptibility created from specified_min_own and specified_max_own
        /// </summary>
        public Version_range Specified_compatibility;

        /// <summary>
        /// The key is the minecraft_type, be it function or advancement, all in lowercase even though some might truly be uppercase
        /// </summary>
        private Dictionary<string, List<Datapack_file>> datapack_files;

        /// <summary>
        /// Serialized minecraft files types if a serializer for that type exits
        /// </summary>
        private Dictionary<string, Content_serializer> datapack_serialized_files;

        /// <summary>
        /// The compatibility created by taking the features in the datapack into account as well as the specified compatibility
        /// </summary>
        public Version_range Identified_compatibility;

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
            if (datapack_files == null)
            {
                return false;
            }

            Write_line("----------------------------------------");
            datapack_serialized_files = new();

            foreach (KeyValuePair<string, List<Datapack_file>> file_type in datapack_files)
            {
                //TODO want to at least warn in function if tag non existing or function non existing (right now we error for non existing tags everytime, even functions)
                //TODO this at the very least could use them real minecraft names

                if(file_type.Key == "function")
                {
                    datapack_serialized_files.Add(file_type.Key, new Function_serializer(this, serialization_directives, file_type.Value));
                }
                else if (file_type.Key == "dimension_type")
                {
                    datapack_serialized_files.Add(file_type.Key, new Dimension_type_serializer(this, serialization_directives, file_type.Value));
                }
                else if (file_type.Key == "dimension")
                {
                    datapack_serialized_files.Add(file_type.Key, new Dimension_serializer(this, serialization_directives, file_type.Value));
                }
                else if(file_type.Key == "enchantment")
                {
                    datapack_serialized_files.Add(file_type.Key, new Enchantment_serializer(this, serialization_directives, file_type.Value));
                }
                else if(file_type.Key == "predicate")
                {
                    datapack_serialized_files.Add(file_type.Key, new Predicate_serializer(this, serialization_directives, file_type.Value));
                }
                else if(file_type.Key == "loot_table")
                {
                    datapack_serialized_files.Add(file_type.Key, new Loot_table_serializer(this, serialization_directives, file_type.Value));
                }

                else if(file_type.Key == "tags/block")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value,"BLOCK_TAG"));
                }
                else if (file_type.Key == "tags/item")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "ITEM_TAG"));
                }
                else if (file_type.Key == "tags/function")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value));
                }
                else if (file_type.Key == "tags/fluid")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "FLUID_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/entity_type")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "ENTITY_TAG"));
                }
                else if (file_type.Key == "tags/game_event")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "GAME_EVENT_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/cat_variant")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "CAT_VARIANT_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/point_of_interest_type")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "POINT_OF_INTEREST_TYPE_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/painting_variant")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "PAINTING_VARIANT_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/banner_pattern")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "BANNER_PATTERN_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/instrument")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "INSTRUMENT_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/damage_type")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "DAMAGE_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/enchantment")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "ENCHANTMENT_TAG"));  //TODO miss
                }
                else if (file_type.Key == "tags/worldgen/biome")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "BIOME_TAG"));
                }
                else if (file_type.Key == "tags/worldgen/flat_level_generator_preset")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "FLAT_LEVEL_GENERATOR_PRESET_TAG")); //TODO miss
                }
                else if (file_type.Key == "tags/worldgen/world_preset")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "WORLD_PRESET")); //TODO miss
                }
                else if (file_type.Key == "tags/worldgen/structure")
                {
                    datapack_serialized_files.Add(file_type.Key, new Tag_serializer(this, serialization_directives, file_type.Value, "STRUCTURE_TAG"));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Write_line("No serializer currently exists for: " + file_type.Key);
                    Console.ResetColor();
                }
            }

            Write_line("----------------------------------------");
            return true;
        }

        /// <summary>
        /// Will attempt to decipher a version range that the datapack is compatible with
        /// </summary>
        public bool Identify_compatibility()
        {
            //Datapack load or serialization failed
            if(datapack_files == null || datapack_serialized_files == null)
            {
                return false;
            }

            Write_line("----------------------------------------");
            Console.ForegroundColor = ConsoleColor.Blue;
            Write_line("Datapack assumed compatibility: ");
            Write_line(Specified_compatibility.ToString(false));
            Console.ResetColor();

            Identified_compatibility = new();

            //To handle when minecrafts version isn't granular enoug
            int specified_min_own_pedantic = Versions.Get_pedantic_min(specified_min_own);
            int specified_max_own_pedantic = Versions.Get_pedantic_max(specified_max_own);

            Identified_compatibility.Set(specified_min_own_pedantic, specified_max_own_pedantic);

            //We continue outside the ranges specified if the function compatibility on those are greater or equal to the compatibility in the limits provided

            Version_range all_scanned_compatibility = new();

            foreach(KeyValuePair<string, Content_serializer> file in datapack_serialized_files)
            {
                all_scanned_compatibility.Add(file.Value.Serialization_success);
            }

            int supported_on_min = all_scanned_compatibility.Get_level(specified_min_own_pedantic);

            for (int i = specified_min_own_pedantic; i >= 0; i--)
            {
                if (all_scanned_compatibility.Get_level(i) >= supported_on_min)
                {
                    Identified_compatibility.Set(i);
                }
            }

            int supported_on_max = all_scanned_compatibility.Get_level(specified_max_own_pedantic);

            for (int i = specified_max_own_pedantic; i <= Versions.Max_own; i++)
            {
                if (all_scanned_compatibility.Get_level(i) >= supported_on_max)
                {
                    Identified_compatibility.Set(i);
                }
            }

            Write_line("");
            Console.ForegroundColor = ConsoleColor.Blue;
            Write_line("Final version range: " + Identified_compatibility.ToString(false));
            Console.ResetColor();
            Write_line("----------------------------------------");

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

            Write_line("\nMcmeta number: " + pack_mcmeta.pack.pack_format + " Version: " + mcmeta_minecraft_version);

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

            Specified_compatibility = new Version_range(specified_min_own, specified_max_own);

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

                        if (min_overlay_version < specified_min_own)
                        {
                            throw new Exception("Overlay min: " + min_overlay_version + " should not be less than min format: " + specified_min_own);
                        }

                        if (max_overlay_version > specified_max_own)
                        {
                            throw new Exception("Overlay max: " + max_overlay_version + " should not be greater than max format: " + specified_max_own);
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
//function_files         -> function X
//item_modifiers    -> item_modifier X
//loot_tables       -> loot_table X
//predicates        -> predicate X
//recipes           -> recipe X
//structures        -> structure X
//tags/blocks       -> tags/block X
//tags/entity_types -> tags/entity_type
//tags/fluids       -> tags/fluid X
//tags/function_files    -> tags/function X
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
            namespace_directories.Remove("tags/worldgen");

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
            //Add_files(namespace_path, "tags/cat_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json",true);
            Add_files(namespace_path, "tags/point_of_interest_type", oldschool_namespaces, ".json", true);
            //Add_files(namespace_path, "tags/painting_variant", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json",true);
            Add_files(namespace_path, "tags/banner_pattern", oldschool_namespaces, ".json", true);
            //Add_files(namespace_path, "tags/instrument", oldschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json",true);
            Add_files(namespace_path, "tags/damage_type", oldschool_namespaces.Get_common(new Version_range("1.19.4", Versions.Max_minecraft)), ".json", true);
            //Add_files(namespace_path, "tags/enchantment", oldschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json",true);

            Add_files(namespace_path, "tags/worldgen/biome", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/worldgen/flat_level_generator_preset", oldschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/worldgen/world_preset", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/worldgen/structures", oldschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);


            Add_files(namespace_path, "advancements", oldschool_namespaces, ".json");
            Add_files(namespace_path, "banner_pattern", oldschool_namespaces, ".json");
            //Add_files(namespace_path, "cat_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "chat_types", oldschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "cow_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            //Add_files(namespace_path, "chicken_variant", oldschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
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
            Add_files(namespace_path, "worldgen/flat_level_generator_presets", oldschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
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
            Add_files(namespace_path, "tags/cat_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/point_of_interest_type", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/painting_variant", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/banner_pattern", newschool_namespaces, ".json", true);
            Add_files(namespace_path, "tags/instrument", newschool_namespaces.Get_common(new Version_range("1.21.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/damage_type", newschool_namespaces.Get_common(new Version_range("1.19.4", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/enchantment", newschool_namespaces.Get_common(new Version_range("1.21", Versions.Max_minecraft)), ".json", true);

            Add_files(namespace_path, "tags/worldgen/biome", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/worldgen/flat_level_generator_preset", newschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/worldgen/world_preset", newschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json", true);
            Add_files(namespace_path, "tags/worldgen/structure", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json", true);


            Add_files(namespace_path, "advancement", newschool_namespaces, ".json");
            Add_files(namespace_path, "banner_pattern", newschool_namespaces, ".json");
            Add_files(namespace_path, "cat_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "chat_type", newschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "cow_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "chicken_variant", newschool_namespaces.Get_common(new Version_range("1.21.5", Versions.Max_minecraft)), ".json");
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
            Add_files(namespace_path, "worldgen/world_preset", newschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/flat_level_generator_presets", newschool_namespaces.Get_common(new Version_range("1.19", Versions.Max_minecraft)), ".json");
            Add_files(namespace_path, "worldgen/multi_noise_biome_source_parameter_list", newschool_namespaces.Get_common(new Version_range("1.16.2", Versions.Max_minecraft)), ".json");

            foreach(string check in namespace_directories)
            {
                if(!added_directories.Any(d => check.StartsWith(d)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Datapack folder: " + Path.GetRelativePath(working_directory, namespace_path + "/" + check) + " is unnecessary");
                    Console.ResetColor();
                }
            }

            void Add_files(string namespace_path, string minecraft_name, Version_range compatibility, string file_type, bool tag = false)
            {
                if (Directory.Exists(namespace_path + "/" + minecraft_name))
                {
                    if(compatibility.Is_entire(false))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Write_line("Datapack folder: " + Path.GetRelativePath(working_directory, namespace_path + "/" + minecraft_name) + " is unreachable in any version");
                        Console.ResetColor();
                    }

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

        //TODO this should perhaps be reworked with the ideas from Resource_handling
        //The point being to eliminate the resources as far as possible

        /// <summary>
        /// The colletions itself knows if it is namespaced or not, this should thus be used everywhere
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="Command_parse_exception"></exception>
        public string Verify_collection(string version, string collection, string value)
        {
            Tuple<bool, List<string>> collection_data = Resource_handler.Get_resource(version, collection);
            //Is it namespaced

            string namespace_;
            string item;

            string[] parts = value.Split(':');

            if (parts.Length == 2)
            {
                namespace_ = parts[0];
                item = parts[1];
            }
            else
            {
                namespace_ = "minecraft";
                item = value;
            }

            bool namespaced = namespace_[0] == '#';

            if (namespaced)
            {
                item = "#" + item;
                namespace_ = namespace_[1..];
            }

            if (!Regex.IsMatch(namespace_, @"^[0-9a-z_\-\.]+$"))  //Mainly to prevent item{test:34} from being slit up into tem{test 34} and then giving invalid item validator ITEM which is confusing
            {
                return "Namespace: " + namespace_ + " has illegal characters";
            }

            if (collection_data.Item1)
            {
                if (namespace_ == "minecraft")
                {
                    if (!collection_data.Item2.Contains(item))
                    {
                        return "Minecraft collection: " + collection + " does not contain: " + item;
                    }
                }
                else
                {
                    return Register_verifyer(version, collection, namespace_, item);

                }
            }
            else
            {
                if (!collection_data.Item2.Contains(value))
                {
                    return "Collection: " + collection + " does not contain: " + value;
                }
            }
            return "";
        }

        /// <summary>
        /// Called from the command parser to verify external register_own
        /// </summary>
        /// <param name="minecraft_version"></param>
        /// <param name="register_own">The own name of the register</param>
        /// <param name="namespace_"></param>
        /// <param name="name"></param>
        /// <param name="error"></param>
        private string Register_verifyer(string minecraft_version, string register_own, string namespace_, string name)
        {
            if (register_own == "SOUND")
            {
                return "Sounds stored in resourcepack, which this currently doesn't support";
            }

            string minecraft_name = To_minecraft_register(register_own);

            if (minecraft_name == null)
            {
                //TODO observe this can fire if something impossible is used like give @s magic:test as it is impossible to define custom items with a datapack need flag for this to make yellow instead of purple error

                Console.ForegroundColor = ConsoleColor.Magenta;
                Write_line("Could not convert internal register name: " + register_own + " to minecraft");
                Console.ResetColor();
                return "";
            }

            List<Datapack_file> collection_filtered = Get_datapack_files(minecraft_name);

            //Null would be if folder doesn't exist, zero if folder empty
            if (collection_filtered == null || collection_filtered.Count == 0)
            {
                return "No external collection by the name: " + register_own + " in namespace: " + namespace_ + " provided for version: " + minecraft_version;
            }

            List<Datapack_file> version_filtered = collection_filtered.FindAll(r => r.Context_compatibility.Is_set(Versions.Get_own_version(minecraft_version)));

            ;

            if (version_filtered.Count == 0)
            {
                return "No external register for version: " + minecraft_version + " provided";
            }

            List<Datapack_file> namespace_filtered = version_filtered.FindAll(r => r.Namespace == namespace_);

            ;

            if (namespace_filtered.Count == 0)
            {
                return "No external namespace by the name: " + namespace_ + " provided for version: " + minecraft_version;
            }

            foreach (Datapack_file register in namespace_filtered)
            {
                if (register.Name == name)
                {
                    return "";
                }
            }

            return "External collection: " + register_own + " does not contain: " + name;
        }

        /// <summary>
        /// Translates the own internal register names used in the validators in Command_parser to minecrafts real names
        /// </summary>
        /// <returns></returns>
        private static string To_minecraft_register(string own_name)
        {
            return own_name switch
            {
                "DIMENSION" => "dimension",
                "BLOCK_TAG" => "tags/block",
                "ITEM_TAG" => "tags/item",
                "ENTITY_TAG" => "tags/entity_type",
                "BIOME_TAG" => "tags/worldgen/biome",
                "LOOT_TABLE" => "loot_table",
                "ADVANCEMENT" => "advancement",
                "DAMAGE" => "damage_type",
                "RECIPE" => "recipe",
                _ => null,
            };
        }

        /// <summary>
        /// Will return all unserialized datapack files provided the minecraft name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Datapack_file> Get_datapack_files(string name)
        {
            if(datapack_files.TryGetValue(name, out List<Datapack_file> list))
            {
                return list;
            }
            
            return null;
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
        /// <summary>
        /// Will convert the message list into a single string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string value = "";

            foreach (Tuple<string, ConsoleColor> message in messages)
            {
                value += message.Item1;
            }

            return value;
        }

        public List<Tuple<string, ConsoleColor>> Get_messages()
        {
            return messages;
        }
    }
}
