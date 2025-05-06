using System.Linq.Expressions;
using Command_parsing;
using Minecraft_common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Datapack.Content_serializers
{
    public class Enchantment_serializer : Content_serializer
    {
        private readonly List<Serialized_enchantment> serialized_enchantments;

        public Enchantment_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files) : base(loader, serialization_directive, files)
        {
            serialized_enchantments = new();

            foreach (Datapack_file enchantment_file in files)
            {
                Scan_dimension_type(enchantment_file);
            }

            for (int i = 0; i < serialized_enchantments.Count; i++)
            {
                if (serialized_enchantments[i].Success)
                {
                    Serialization_success.Add(serialized_enchantments[i].Version);
                }
            }

            Write_line("\nTotal serialized enchantments count: " + serialized_enchantments.Count);
            Write_line("Enchantment serialization success ranges: \n" + Serialization_success.ToString());
        }

        private void Scan_dimension_type(Datapack_file enchantment_file)
        {
            Write_line("Scanning enchantment: " + enchantment_file.Short_path);

            for(int i = 0; i <= Versions.Max_own; i++)
            {
                if(enchantment_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    Enchantment enchantment;
                    bool success = true;

                    string version = Versions.Get_own_version(i);

                    enchantment = Deserialize_enchantment<Enchantment_1_21>(enchantment_file.Data, out string error);

                    if (error != "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error serializing enchantment in version: " + version + "  \n " + error);
                        Console.ResetColor();
                        success = false;
                    }

                    //if (i < Versions.Get_own_version("1.17"))
                    //{
                    //    enchantment = Deserialize_enchantment<Dimension_type_1_16>(enchantment_file.Data, out string error);

                    //    if(error != "")
                    //    {
                    //        Console.ForegroundColor = ConsoleColor.Red;
                    //        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                    //        Console.ResetColor();
                    //        success = false;
                    //    }
                    //}
                    //else if (i < Versions.Get_own_version("1.19"))
                    //{
                    //    enchantment = Deserialize_enchantment<Dimension_type_1_17>(enchantment_file.Data, out string error);

                    //    if (error != "")
                    //    {
                    //        Console.ForegroundColor = ConsoleColor.Red;
                    //        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                    //        Console.ResetColor();
                    //        success = false;
                    //    }
                    //}
                    //else /* if (i < Versions.Get_own_version("1.21.6"))*/
                    //{
                    //    enchantment = Deserialize_enchantment<Dimension_type_1_19>(enchantment_file.Data, out string error);

                    //    if (error != "")
                    //    {
                    //        Console.ForegroundColor = ConsoleColor.Red;
                    //        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                    //        Console.ResetColor();
                    //        success = false;
                    //    }
                    //}
                    //else
                    //{
                    //    Dimension_type_1_21_6 enchantment = (Dimension_type_1_16)Try_serialize<Dimension_type_1_16>(enchantment_file.Data, out string error);
                    //}

                    serialized_enchantments.Add(new Serialized_enchantment(enchantment_file, i, success, enchantment));
                }
            }
        }

        private static T Deserialize_enchantment<T>(string input, out string error) where T : Enchantment
        {
            try
            {
                T deserialized_enchantment = JsonConvert.DeserializeObject<T>(
                    input,
                    new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error }
                );

                error = "";
                return deserialized_enchantment;
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

    public class Serialized_enchantment : Serialized_datapack_file
    {
        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Enchantment Serialization_result;

        public Serialized_enchantment(Datapack_file file, int version, bool success, Enchantment serialization_result) : base(file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }


    public abstract class Enchantment
    {

    }

    public class Enchantment_1_21 : Enchantment
    {
        [JsonConverter(typeof(Text_converter))]
        public object description;

        public object exclusive_set;

        [JsonProperty(Required = Required.Always)]
        public object supported_items;

        public object primary_items;

        [JsonProperty(Required = Required.Always)]
        public int weight; //Between 0 and 1023

        [JsonProperty(Required = Required.Always)]
        public int max_level; //Between 1 and 255

        [JsonProperty(Required = Required.Always)]
        public Enchantment_cost min_cost;

        [JsonProperty(Required = Required.Always)]
        public Enchantment_cost max_cost;

        [JsonProperty(Required = Required.Always)]
        public int anvil_cost; //Min 0

        [JsonProperty(Required = Required.Always)]
        public List<string> slots;

        //TODO this will be horrid
        //This is not required
        public object effects;
    }

    public class Enchantment_cost
    {
        [JsonProperty("base", Required = Required.Always)]
        public int base_;

        [JsonProperty(Required = Required.Always)]
        public int per_level_above_first;

        //TODO need predicates for this
        public Dictionary<string, Structure_detail> effects;
    }

    //TODO move out

    public class Text_converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if(token.Type == JTokenType.String)
            {
                return token.ToString();
            }

            if(Utils.Try_Deserialize_multiple(token, new Type[] {typeof(Text_text), typeof(Click_event), typeof(Hover_event), typeof(Text_translate), typeof(Text_score), typeof(Score), typeof(Text_selector), typeof(Text_keybind), typeof(Text_block), typeof(Text_entity), typeof(Text_storage) }, out object result))
            {
                return result;
            }
            else
            {
                throw new JsonSerializationException("Could not parse: " + token.ToString() + " as text");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Text);
        }
    }

    public class Text
    {
        [JsonConverter(typeof(Text_converter))]
        public object with;

        public string type;

        [JsonConverter(typeof(Text_converter))]
        public object extra;

        public string color;

        public string font;

        public bool? bold;

        public bool? italic;

        public bool? underline;

        public bool? strikethrough;

        public bool? obfuscated;

        public string insertion;

        public Click_event click_event;

        public Hover_event hover_event;
    }

    public class Text_text : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string text;
    }

    public class Click_event
    {
        [JsonProperty(Required = Required.Always)]
        public string action;

        [JsonProperty(Required = Required.Always)]
        public string value;
    }

    public class Hover_event
    {
        [JsonProperty(Required = Required.Always)]
        public string action;

        [JsonConverter(typeof(Text_converter))]
        public object value;

        [JsonConverter(typeof(Text_converter))] 
        public object contants;
    }

    public class Text_translate : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string translate;

        public string fallback;
    }

    public class Text_score : Text
    {
        [JsonProperty(Required = Required.Always)]
        public Score score;
    }

    public class Score
    {
        [JsonProperty(Required = Required.Always)]
        public string objective;

        [JsonProperty(Required = Required.Always)]
        public string name;
    }

    public class Text_selector : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string selector;

        [JsonConverter(typeof(Text_converter))]
        public object separator;
    }

    public class Text_keybind : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string keybind;
    }

    public class Text_block : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string block;

        [JsonProperty(Required = Required.Always)]
        public string nbt;

        public string source;
    }

    public class Text_entity : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string entity;

        [JsonProperty(Required = Required.Always)]
        public string nbt;

        public string source;
    }

    public class Text_storage : Text
    {
        [JsonProperty(Required = Required.Always)]
        public string storage;

        [JsonProperty(Required = Required.Always)]
        public string nbt;

        public string source;
    }
}
