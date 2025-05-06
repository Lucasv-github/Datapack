using System.Linq.Expressions;
using Command_parsing;
using Minecraft_common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Datapack.Content_serializers
{
    public class Dimension_type_serializer : Content_serializer
    {
        private readonly List<Serialized_dimension_type> serialized_dimension_types;

        public Dimension_type_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files) : base(loader, serialization_directive, files)
        {
            serialized_dimension_types = new();

            foreach (Datapack_file file_function in files)
            {
                Scan_dimension_type(file_function);
            }

            for (int i = 0; i < serialized_dimension_types.Count; i++)
            {
                if (serialized_dimension_types[i].Success)
                {
                    Serialization_success.Add(serialized_dimension_types[i].Version);
                }
            }

            Write_line("\nTotal serialized dimension types count: " + serialized_dimension_types.Count);
            Write_line("Dimension serialization success ranges: \n" + Serialization_success.ToString());
        }

        private void Scan_dimension_type(Datapack_file dimension_type_file)
        {
            Write_line("Scanning dimension type: " + dimension_type_file.Short_path);

            for(int i = 0; i <= Versions.Max_own; i++)
            {
                if(dimension_type_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    Dimension_type dimension_type;
                    bool success = true;

                    string version = Versions.Get_own_version(i);

                    if (i < Versions.Get_own_version("1.17"))
                    {
                        dimension_type = Deserialize_dimension_type<Dimension_type_1_16>(dimension_type_file.Data, out string error);

                        if(error != "")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                            Console.ResetColor();
                            success = false;
                        }
                    }
                    else if (i < Versions.Get_own_version("1.19"))
                    {
                        dimension_type = Deserialize_dimension_type<Dimension_type_1_17>(dimension_type_file.Data, out string error);

                        if (error != "")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                            Console.ResetColor();
                            success = false;
                        }
                    }
                    else /* if (i < Versions.Get_own_version("1.21.6"))*/
                    {
                        dimension_type = Deserialize_dimension_type<Dimension_type_1_19>(dimension_type_file.Data, out string error);

                        if (error != "")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                            Console.ResetColor();
                            success = false;
                        }
                    }
                    //else
                    //{
                    //    Dimension_type_1_21_6 dimension_type = (Dimension_type_1_16)Try_serialize<Dimension_type_1_16>(dimension_type_file.Data, out string error);
                    //}

                    serialized_dimension_types.Add(new Serialized_dimension_type(dimension_type_file, i, success, dimension_type));
                }
            }
        }

        private static T Deserialize_dimension_type<T>(string input, out string error) where T : Dimension_type
        {
            try
            {
                T deserialized_dimension_type = JsonConvert.DeserializeObject<T>(
                    input,
                    new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error }
                );

                error = "";
                return deserialized_dimension_type;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        public override void Print(string root_directory, int version, bool strip_empty, bool strip_comments)
        {
            foreach (Datapack_file dimension_type_file in files)
            {
                List<Serialized_dimension_type> correct_dimension_type = serialized_dimension_types.FindAll(f => f.Short_path == dimension_type_file.Short_path);

                if (correct_dimension_type.Count == 0)
                {
                    //And no we do not output the unserialized here, the places we do that are when we haven't had time for implement a serializer, to get it to work temporarily.
                    //If serialization fails when it shouldn't thats a bug, not a missing feature

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Dimension type: " + dimension_type_file.Short_path + " hasn't been serialized at all");
                    Console.ResetColor();
                }

                correct_dimension_type = correct_dimension_type.FindAll(f => f.Version == version);

                if (correct_dimension_type.Count > 1)
                {
                    throw new Exception("Something is wrong, " + correct_dimension_type.Count + " serializations for version: " + version + " of the dimension type: " + dimension_type_file.Short_path);
                }

                if (correct_dimension_type.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Dimension type: " + dimension_type_file.Short_path + " hasn't been serialized at all in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                if (!correct_dimension_type[0].Success)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Dimension type: " + dimension_type_file.Short_path + " hasn't been serialized succesfully in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                string current_result_path = root_directory + dimension_type_file.Short_path;

                Directory.CreateDirectory(Directory.GetParent(current_result_path).ToString());
                File.WriteAllText(current_result_path, JsonConvert.SerializeObject(correct_dimension_type[0].Serialization_result));
            }
        }
    }

    public class Serialized_dimension_type : Serialized_datapack_file
    {
        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Dimension_type Serialization_result;

        public Serialized_dimension_type(Datapack_file file, int version, bool success, Dimension_type serialization_result) : base(file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }

    public abstract class Dimension_type
    {

    }

    //TODO only work here is the limits described down

    public class Dimension_type_1_16 : Dimension_type
    {
        [JsonProperty(Required = Required.Always)]
        public bool ultrawarm;

        [JsonProperty(Required = Required.Always)]
        public bool natural;

        [JsonProperty(Required = Required.Always)]
        public bool piglin_safe;

        [JsonProperty(Required = Required.Always)]
        public bool respawn_anchor_works;

        [JsonProperty(Required = Required.Always)]
        public bool bed_works;

        [JsonProperty(Required = Required.Always)]
        public bool has_raids;

        [JsonProperty(Required = Required.Always)]
        public bool has_skylight;

        [JsonProperty(Required = Required.Always)]
        public bool has_ceiling;

        [JsonProperty(Required = Required.Always)]
        public float coordinate_scale;

        [JsonProperty(Required = Required.Always)]
        public int ambient_light;  //Between min 0 and 1

        [JsonProperty(Required = Required.Always)]
        public int fixed_time;

        [JsonProperty(Required = Required.Always)]
        public int logical_height;  //Between 0 and 256

        [JsonProperty(Required = Required.Always)]
        public string effects;

        [JsonProperty(Required = Required.Always)]
        public string infiniburn;
    }

    public class Dimension_type_1_17 : Dimension_type
    {
        [JsonProperty(Required = Required.Always)]
        public bool ultrawarm;

        [JsonProperty(Required = Required.Always)]
        public bool natural;

        [JsonProperty(Required = Required.Always)]
        public bool piglin_safe;

        [JsonProperty(Required = Required.Always)]
        public bool respawn_anchor_works;

        [JsonProperty(Required = Required.Always)]
        public bool bed_works;

        [JsonProperty(Required = Required.Always)]
        public bool has_raids;

        [JsonProperty(Required = Required.Always)]
        public bool has_skylight;

        [JsonProperty(Required = Required.Always)]
        public bool has_ceiling;

        [JsonProperty(Required = Required.Always)]
        public float coordinate_scale;

        [JsonProperty(Required = Required.Always)]
        public int ambient_light;  //Between 0 and 1

        [JsonProperty(Required = Required.Always)]
        public int fixed_time;

        [JsonProperty(Required = Required.Always)]
        public int logical_height;  //Between 0 and 256

        [JsonProperty(Required = Required.Always)]
        public string effects;

        [JsonProperty(Required = Required.Always)]
        public string infiniburn;

        [JsonProperty(Required = Required.Always)]
        public int min_y; //Between -2032 and 2031

        [JsonProperty(Required = Required.Always)]
        public int height; //Between -2032 and 2031
    }

    public class Dimension_type_1_19 : Dimension_type
    {
        [JsonProperty(Required = Required.Always)]
        public bool ultrawarm;

        [JsonProperty(Required = Required.Always)]
        public bool natural;

        [JsonProperty(Required = Required.Always)]
        public bool piglin_safe;

        [JsonProperty(Required = Required.Always)]
        public bool respawn_anchor_works;

        [JsonProperty(Required = Required.Always)]
        public bool bed_works;

        [JsonProperty(Required = Required.Always)]
        public bool has_raids;

        [JsonProperty(Required = Required.Always)]
        public bool has_skylight;

        [JsonProperty(Required = Required.Always)]
        public bool has_ceiling;

        [JsonProperty(Required = Required.Always)]
        public float coordinate_scale;

        [JsonProperty(Required = Required.Always)]
        public int ambient_light;  //Between 0 and 1

        [JsonProperty(Required = Required.Always)]
        public int fixed_time;

        [JsonProperty(Required = Required.Always)]
        public int logical_height;  //Between 0 and 256

        [JsonProperty(Required = Required.Always)]
        public string effects;

        [JsonProperty(Required = Required.Always)]
        public string infiniburn;

        [JsonProperty(Required = Required.Always)]
        public int min_y; //Between -2032 and 2031

        [JsonProperty(Required = Required.Always)]
        public int height; //Between -2032 and 2031

        [JsonProperty(Required = Required.Always)]
        public object monster_spawn_light_level; //Either int between 0 and 15 or complex object

        [JsonProperty(Required = Required.Always)]
        public int monster_spawn_block_light_limit; //Between 0 and 15
    }

    public class Dimension_type_1_21_6 : Dimension_type
    {
        [JsonProperty(Required = Required.Always)]
        public bool ultrawarm;

        [JsonProperty(Required = Required.Always)]
        public bool natural;

        [JsonProperty(Required = Required.Always)]
        public bool piglin_safe;

        [JsonProperty(Required = Required.Always)]
        public bool respawn_anchor_works;

        [JsonProperty(Required = Required.Always)]
        public bool bed_works;

        [JsonProperty(Required = Required.Always)]
        public bool has_raids;

        [JsonProperty(Required = Required.Always)]
        public bool has_skylight;

        [JsonProperty(Required = Required.Always)]
        public bool has_ceiling;

        [JsonProperty(Required = Required.Always)]
        public float coordinate_scale;

        [JsonProperty(Required = Required.Always)]
        public int ambient_light;  //Between 0 and 1

        [JsonProperty(Required = Required.Always)]
        public int fixed_time;

        [JsonProperty(Required = Required.Always)]
        public int logical_height;  //Between 0 and 256

        [JsonProperty(Required = Required.Always)]
        public string effects;

        [JsonProperty(Required = Required.Always)]
        public string infiniburn;

        [JsonProperty(Required = Required.Always)]
        public int min_y; //Between -2032 and 2031

        [JsonProperty(Required = Required.Always)]
        public int height; //Between -2032 and 2031

        [JsonProperty(Required = Required.Always)]
        public object monster_spawn_light_level; //Either int between 0 and 15 or complex object

        [JsonProperty(Required = Required.Always)]
        public int monster_spawn_block_light_limit; //Between 0 and 15

        public int? cloud_height; //Between -2032 and 2031, only one that can be unset
    }
}
