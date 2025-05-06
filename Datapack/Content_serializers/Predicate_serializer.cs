using System;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Command_parsing;
using Minecraft_common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Datapack.Content_serializers
{
    public class Predicate_serializer : Content_serializer
    {
        private readonly List<Serialized_predicate> serialized_predicates;

        public Predicate_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files) : base(loader, serialization_directive, files)
        {
            serialized_predicates = new();

            foreach (Datapack_file file_function in files)
            {
                Scan_predicate(file_function);
            }

            for (int i = 0; i < serialized_predicates.Count; i++)
            {
                if (serialized_predicates[i].Success)
                {
                    Serialization_success.Add(serialized_predicates[i].Version);
                }
            }

            Write_line("\nTotal serialized predicate count: " + serialized_predicates.Count);
            Write_line("Predicate serialization success ranges: \n" + Serialization_success.ToString());
        }

        private void Scan_predicate(Datapack_file predicate_file)
        {
            Write_line("Scanning predicate: " + predicate_file.Short_path);

            for(int i = 0; i <= Versions.Max_own; i++)
            {
                if(predicate_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    Predicate_15 predicate;
                    bool success = true;

                    string version = Versions.Get_own_version(i);

                    predicate = Deserialize_predicate<Predicate_15>(predicate_file.Data, out string error);

                    if (error != "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                        Console.ResetColor();
                        success = false;
                    }

                    serialized_predicates.Add(new Serialized_predicate(predicate_file, i, success, predicate));
                }
            }
        }

        //TODO could we do like in enchantments?
        private static T Deserialize_predicate<T>(string input, out string error) where T : Mc_predicate
        {
            try
            {
                T deserialized_predicate = JsonConvert.DeserializeObject<T>(
                    input,
                    new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error }
                );

                error = "";
                return deserialized_predicate;
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

    public class Serialized_predicate : Serialized_datapack_file
    {
        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Mc_predicate Serialization_result;

        public Serialized_predicate(Datapack_file file, int version, bool success, Mc_predicate serialization_result) : base(file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }

    public abstract class Mc_predicate
    {

    }

    [JsonConverter(typeof(Predicate_converter))]
    public class Predicate_15 : Mc_predicate
    {
        [JsonProperty(Required = Required.Always)]
        public string condition;
    }

    public class Predicate_converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            string condition = token["condition"].Value<string>();

            string[] parts = condition.Split(':');

            if (parts.Length == 2 && parts[0] == "minecraft")
            {
                condition = parts[1];
            }

            Predicate_15 target = condition switch
            {
                "alternative" => new Alternative(),
                "block_state_property" => new Block_state_property(),
                "damage_source_properties" => new Damage_source_properties(),
                "entity_properties" => new Entity_properties(),
                "entity_scores" => new Entity_scores(),
                "inverted" => new Inverted(),
                "killed_by_player" => new Killed_by_player(),
                "location_check" => new Location_check(),
                "match_tool" => new Match_tool(),
                "random_chance" => new Random_chance(),
                "random_chance_with_looting" => new Random_chance_with_looting(),
                "reference" => new Reference(),
                "survives_explosion" => new Survives_explosion(),
                "table_bonus" => new Table_bonus(),
                "time_check" => new Time_check(),
                "weather_check" => new Weather_check(),
                _ => throw new JsonSerializationException("Unknown predicate condition: " + condition),
            };

            serializer.Populate(token.CreateReader(), target);
            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Predicate_15).IsAssignableFrom(objectType);
        }
    }

    public class Alternative : Predicate_15
    {
        public List<Predicate_15> terms;
    }

    public class Block_state_property : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public string block;

        public Dictionary<string, string> properties; //TODO will need external table to validate this
    }

    public class Damage_source_properties : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public Damage_source_properties_predicate predicate;
    }

    public class Damage_source_properties_predicate
    {
        public bool? is_explosion;
        public bool? is_fire;
        public bool? is_magic;
        public bool? is_projective;
        public bool? is_lightning;
        public bool? bypasses_armor;
        public bool? bypasses_invulnerability;

        public Predicate_entity source_entity;
        public Predicate_entity direct_entity;
    }

    public class Entity_properties : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public string entity;

        [JsonProperty(Required = Required.Always)]
        public Predicate_entity predicate;
    }

    public class Entity_scores : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public string entity;

        public Dictionary<string, object> scores;  //TODO either int or min max
    }

    public class Inverted : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public Predicate_15 term;
    }

    public class Killed_by_player : Predicate_15
    {
        public bool? inverse;
    }

    public class Location_check : Predicate_15
    {
        public int offset_x;
        public int offset_y;
        public int offset_z;

        [JsonProperty(Required = Required.Always)]
        public Predicate_location predicate;
    }

    public class Match_tool : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public Predicate_item predicate;
    }

    public class Random_chance : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public int chance; //Between 0 and 1
    }

    public class Random_chance_with_looting : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public int chance; //Between 0 and 1

        [JsonProperty(Required = Required.Always)]
        public int looting_multiplier;
    }

    public class Reference : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public string name;
    }

    public class Survives_explosion : Predicate_15
    {

    }

    public class Table_bonus : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public string enchantment;

        public List<int> chances;  //Between 0 and 1
    }

    public class Time_check : Predicate_15
    {
        [JsonProperty(Required = Required.Always)]
        public object value;  //Float or min max
        public long period;
    }

    public class Weather_check : Predicate_15
    {
        public bool? raining;
        public bool? thundering;
    }

    public class Predicate_entity
    {
        public string type;
        public string team;
        public string nbt;
        public Predicate_location location;
        public Predicate_distance distance;
        public Predicate_flags flags;
        public Dictionary<string, Predicate_item> equipment;
        public string cat_type;
        public Predicate_player player;
        public Dictionary<string, Predicate_effect> effects;

    }

    public class Predicate_location
    {
        public Predicate_position position;
        public string biome;
        public string feature;
        public string dimension;
        public object light; //TODO int or min max
        public Predicate_block block;
    }

    public class Predicate_block
    {
        public string block;
        public string tag;
        public Dictionary<string, string> state;
        public string nbt;
    }

    //TODO fix objects
    public class Predicate_distance
    {
        public object x; //Float or min max
        public object y; //Float or min max
        public object z; //Float or min max
        public object absolute; //Float or min max
        public object horizontal; //Float or min max
    }

    public class Predicate_flags
    {
        public bool? is_on_fire;
        public bool? is_sneaking;
        public bool? is_sprinting;
        public bool? is_swamming;
        public bool? is_baby;
    }

    public class Predicate_item
    {
        public string item;
        public string tag;
        public object durability; //Int or min max
        public string potion;
        public List<Predicate_enchantment> enchantmnets;
        public List<Predicate_enchantment> stored_enchantmnets;
        public string nbt;
    }

    public class Predicate_player
    {
        public List<object> advancements; //TODO missing datapack file
        public string gamemode;
        public object level;  //Int or min max
        public List<object> recipes; //TODO missing datapack file
        public List<Predicate_stat> stats;
        public Predicate_entity looking_at;
    }

    public class Predicate_stat
    {
        public string type;
        public string stat;
        public object value; //Int or min max
    }

    public class Predicate_enchantment
    {
        public string enchantment;
        public object levels; //Int or min max
    }

    //TODO fix objects
    public class Predicate_position
    {
        public object x;  //Float or min max
        public object y;  //Float or min max
        public object z;  //Float or min max
    }

    //TODO fix objects
    public class Predicate_effect
    {
        public object amplifier;  //Int or min max
        public object duration;  //Int or min max
        public bool? ambient;
        public bool? visible;
    }
}
