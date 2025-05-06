using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datapack.Content_serializers;
using Datapack;
using Minecraft_common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Datapack.Content_serializers
{
    public class Loot_table_serializer : Content_serializer
    {
        private readonly List<Serialized_loot_table> serialized_loot_tables;

        public Loot_table_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files) : base(loader, serialization_directive, files)
        {
            serialized_loot_tables = new();

            foreach (Datapack_file file_function in files)
            {
                Scan_loot_table(file_function);
            }

            for (int i = 0; i < serialized_loot_tables.Count; i++)
            {
                if (serialized_loot_tables[i].Success)
                {
                    Serialization_success.Add(serialized_loot_tables[i].Version);
                }
            }

            Write_line("\nTotal serialized loot table count: " + serialized_loot_tables.Count);
            Write_line("Loot table serialization success ranges: \n" + Serialization_success.ToString());
        }

        private void Scan_loot_table(Datapack_file loot_table_file)
        {
            Write_line("Scanning loot table: " + loot_table_file.Short_path);

            for (int i = 0; i <= Versions.Max_own; i++)
            {
                if (loot_table_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    Loot_table loot_table;
                    bool success = true;

                    string version = Versions.Get_own_version(i);

                    loot_table = Deserialize_loot_table<Loot_table_15>(loot_table_file.Data, out string error);

                    if (error != "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                        Console.ResetColor();
                        success = false;
                    }

                    serialized_loot_tables.Add(new Serialized_loot_table(loot_table_file, i, success, loot_table));
                }
            }
        }

        private static T Deserialize_loot_table<T>(string input, out string error) where T : Loot_table
        {
            try
            {
                T deserialized_loot_table = JsonConvert.DeserializeObject<T>(
                    input,
                    new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error }
                );

                error = "";
                return deserialized_loot_table;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public override void Print(string root_directory, int version, bool strip_empty, bool strip_comments)
        {
            throw new NotImplementedException();
        }

    }

    public class Serialized_loot_table : Serialized_datapack_file
    {
        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Loot_table Serialization_result;

        public Serialized_loot_table(Datapack_file file, int version, bool success, Loot_table serialization_result) : base(file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }

    public abstract class Loot_table
    {

    }

    public class Loot_table_15 : Loot_table
    {
        [JsonProperty(Required = Required.Always)]
        public string type;  //TODO verify type from a few

        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_pool> pools;

        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_function> functions;
    }

    public class Loot_table_pool
    {
        [JsonProperty(Required = Required.Always)]
        public object rolls; //Either int or more complex object

        public object bonus_rolls;  //Unset, float or float with min and max

        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_entry> entries;

        public List<Loot_table_function> functions;

        public List<Predicate_15> conditions;
    }

    [JsonConverter(typeof(Loot_table_entry_converter))]
    public class Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public string type;
    }

    public class Loot_table_entry_converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string condition = token["type"].Value<string>();

            string[] parts = condition.Split(':');

            if (parts.Length == 2 && parts[0] == "minecraft")
            {
                condition = parts[1];
            }

            Loot_table_entry target = condition switch
            {
                "alternatives" => new Loot_table_entry_alternatives(),
                "dynamic" => new Loot_table_entry_dynamic(),
                "empty" => new Loot_table_entry_empty(),
                "group" => new Loot_table_entry_group(),
                "item" => new Loot_table_entry_item(),
                "loot_table" => new Loot_table_entry_loot_table(),
                "sequence" => new Loot_table_entry_sequence(),
                "tag" => new Loot_table_entry_tag(),
                _ => throw new JsonSerializationException("Unknown loot table pool entry type: " + condition),
            };

            serializer.Populate(token.CreateReader(), target);
            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Loot_table_entry).IsAssignableFrom(objectType);
        }
    }

    public class Loot_table_entry_alternatives : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_entry> children;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_dynamic : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        public int? weight;

        public int? quality;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_empty : Loot_table_entry
    {
        public int? weight;

        public int? quality;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_group : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_entry> children;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_item : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        public int? weight;

        public int? quality;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_loot_table : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        public int weight;

        public int quality;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_sequence : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_entry> children;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    public class Loot_table_entry_tag : Loot_table_entry
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        [JsonProperty(Required = Required.Always)]
        public bool expand;

        public int? weight;

        public int? quality;

        public List<Loot_table_function> functions;
        public List<Predicate_15> conditions;
    }

    [JsonConverter(typeof(Loot_table_function_converter))]
    public class Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public string function;
    }

    public class Loot_table_function_converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string condition = token["function"].Value<string>();

            string[] parts = condition.Split(':');

            if (parts.Length == 2 && parts[0] == "minecraft")
            {
                condition = parts[1];
            }

            Loot_table_function target = condition switch
            {
                "apply_bonus" => new Loot_table_function_apply_bonus(),
                "copy_name" => new Loot_table_function_apply_copy_name(),
                "copy_nbt" => new Loot_table_function_apply_bonus_copy_nbt(),
                "copy_state" => new Loot_table_function_copy_state(),
                "enchant_randomly" => new Loot_table_function_enchant_randomly(),
                "enchant_with_levels" => new Loot_table_function_enchant_with_levels(),
                "explosion_decay" => new Loot_table_function_explosion_decay(),
                "exploration_map" => new Loot_table_function_exploration_map(),
                "fill_player_head" => new Loot_table_function_fill_player_head(),
                "furnace_smelt" => new Loot_table_function_furnace_smelt(),
                "limit_count" => new Loot_table_function_limit_count(),
                "looting_enchant" => new Loot_table_function_looting_enchant(),
                "set_attributes" => new Loot_table_function_set_attributes(),
                "set_contents" => new Loot_table_function_set_contents(),
                "set_count" => new Loot_table_function_set_count(),
                "set_damage" => new Loot_table_function_set_damage(),
                "set_loot_table" => new Loot_table_function_set_loot_table(),
                "set_lore" => new Loot_table_function_set_lore(),
                "set_name" => new Loot_table_function_set_name(),
                "set_nbt" => new Loot_table_function_set_nbt(),
                "set_stew_effect" => new Loot_table_function_set_stew_effect(),
                _ => throw new JsonSerializationException("Unknown loot table function type: " + condition),
            };

            serializer.Populate(token.CreateReader(), target);
            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Loot_table_function).IsAssignableFrom(objectType);
        }
    }

    public class Loot_table_function_apply_bonus : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public string enchantment;

        //TODO missing some things here
        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_apply_copy_name : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public string source;  //TODO validate against list

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_apply_bonus_copy_nbt : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public object source;  //Enum or object

        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_function_apply_bonus_copy_nbt_op> ops;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_apply_bonus_copy_nbt_op
    {
        [JsonProperty(Required = Required.Always)]
        public string source;

        [JsonProperty(Required = Required.Always)]
        public string target;

        [JsonProperty(Required = Required.Always)]
        public string op;
    }

    public class Loot_table_function_copy_state : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public string block;

        [JsonProperty(Required = Required.Always)]
        public List<string> properties;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_enchant_randomly : Loot_table_function
    {
        public List<string> enchantments;
        public object options; //String or list
        public bool? only_compatible;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_enchant_with_levels : Loot_table_function
    {
        public object levels; //Int or object
        public bool? treasure;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_explosion_decay : Loot_table_function
    {
        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_exploration_map : Loot_table_function
    {
        public string destination;
        public string decoration;
        public int? zoom;
        public int? search_radius;
        public bool? skip_existing_chunks;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_fill_player_head : Loot_table_function
    {
        public string entity;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_furnace_smelt : Loot_table_function
    {
        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_limit_count : Loot_table_function
    {
        public Loot_table_function_limit_count_limit limit;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_limit_count_limit
    {
        public int? min;
        public int? max;
    }

    public class Loot_table_function_looting_enchant : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public object count;  //Float or min max
        public int limit;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_attributes : Loot_table_function
    {
        public List<Loot_table_function_set_attributes_modifier> modifiers;
        public bool? replace;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_attributes_modifier
    {
        [JsonProperty(Required = Required.Always)]
        public string attribute;

        public string name;

        public string id;

        [JsonProperty(Required = Required.Always)]
        public object count;  //Float or min max

        public string operation;

        [JsonProperty(Required = Required.Always)]
        public object slot; //Either one or list of slots
    }

    public class Loot_table_function_set_contents : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_entry> entries;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_count : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public object count; //Int or object

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_damage : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public object count; //Float or object

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_loot_table : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        public long seed;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_lore : Loot_table_function
    {
        public string entity;

        //TODO how to get list of text here
        [JsonProperty(Required = Required.Always)]
        public object lore;

        public bool? replace;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_name : Loot_table_function
    {
        public string entity;
        [JsonProperty(Required = Required.Always)]
        public string name;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_nbt : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public string tag;

        public List<Predicate_15> conditions;
    }

    public class Loot_table_function_set_stew_effect : Loot_table_function
    {
        [JsonProperty(Required = Required.Always)]
        public List<Loot_table_function_set_stew_effect_effect> effects;
    }

    public class Loot_table_function_set_stew_effect_effect
    {
        [JsonProperty(Required = Required.Always)]
        public string type;

        [JsonProperty(Required = Required.Always)]
        public object duration; //Float or min max

        public List<Predicate_15> conditions;
    }
}
