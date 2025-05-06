using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Minecraft_common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Datapack.Content_serializers
{
    public class Dimension_serializer : Content_serializer
    {
        private readonly List<Serialized_dimension> serialized_dimensions;

        public Dimension_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files) : base(loader, serialization_directive, files)
        {
            serialized_dimensions = new();

            foreach (Datapack_file function_file in files)
            {
                Scan_dimension(function_file);
            }

            for (int i = 0; i < serialized_dimensions.Count; i++)
            {
                if (serialized_dimensions[i].Success)
                {
                    Serialization_success.Add(serialized_dimensions[i].Version);
                }
            }

            Write_line("\nTotal serialized dimension count: " + serialized_dimensions.Count);
            Write_line("Function serialization success ranges: \n" + Serialization_success.ToString());
        }

        private void Scan_dimension(Datapack_file dimension_file)
        {
            Write_line("Scanning dimension: " + dimension_file.Short_path);

            for (int i = 0; i <= Versions.Max_own; i++)
            {
                if (dimension_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    Dimension dimension;
                    bool success = true;

                    string version = Versions.Get_own_version(i);

                    dimension = Deserialize_dimension<Dimension_1_16>(dimension_file.Data, out string error);

                    if (error != "")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                        Console.ResetColor();
                        success = false;
                    }

                    //if (i < Versions.Get_own_version("1.17"))
                    //{
                    //    dimension_type = Deserialize_dimension<Dimension_type_1_16>(dimension_file.Data, out string error);

                    //    if (error != "")
                    //    {
                    //        Console.ForegroundColor = ConsoleColor.Red;
                    //        Write_line("Error serializing dimension_type in version: " + version + "  \n " + error);
                    //        Console.ResetColor();
                    //        success = false;
                    //    }
                    //}
                    //else if (i < Versions.Get_own_version("1.19"))
                    //{
                    //    dimension_type = Deserialize_dimension<Dimension_type_1_17>(dimension_file.Data, out string error);

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
                    //    dimension_type = Deserialize_dimension<Dimension_type_1_19>(dimension_file.Data, out string error);

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
                    //    Dimension_type_1_21_6 dimension_type = (Dimension_type_1_16)Try_serialize<Dimension_type_1_16>(dimension_file.Data, out string error);
                    //}

                    serialized_dimensions.Add(new Serialized_dimension(dimension_file, i, success, dimension));
                }
            }
        }

        private static T Deserialize_dimension<T>(string input, out string error) where T : Dimension
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
            throw new NotImplementedException();
        }
    }

    public class Serialized_dimension : Serialized_datapack_file
    {
        /// <summary>
        /// The result of the serialization
        /// </summary>
        public Dimension Serialization_result;

        public Serialized_dimension(Datapack_file file, int version, bool success, Dimension serialization_result) : base(file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }

    //TODO adding newer versions will be bad...

    public abstract class Dimension
    {

    }

    public class Dimension_1_16 : Dimension
    {
        [JsonProperty(Required = Required.Always)]
        public object type;

        [JsonProperty(Required = Required.Always)]
        public Dimension_generator generator;

        [JsonConstructor]
        public Dimension_1_16(JToken type, JToken generator)
        {
            if(type.Type == JTokenType.String)
            {
                this.type = type.ToString();
            }
            else
            {
                this.type = type.ToObject<Dimension_type_1_16>();
            }

            string generator_type = generator["type"].Value<string>();

            if (generator_type == "minecraft:debug" || generator_type == "debug")
            {
                this.generator = generator.ToObject<Dimension_generator>();
            }
            else if(generator_type == "minecraft:flat" || generator_type == "flat")
            {
                this.generator = generator.ToObject<Dimension_generator_flat>();
            }
            else if(generator_type == "minecraft:noise" || generator_type == "noise")
            {
                this.generator = generator.ToObject<Dimension_generator_noise>();
            }
            else
            {
                throw new JsonSerializationException("Unknown generator type: " + generator_type);
            }
        }
    }

    public class Dimension_generator
    {
        [JsonProperty(Required = Required.Always)]
        public string type;
    }
    public class Dimension_generator_flat : Dimension_generator
    {
        [JsonProperty(Required = Required.Always)]
        public Flat_settings settings;
    }

    public class Flat_settings
    {
        [JsonProperty(Required = Required.Always)]
        public string biome;

        [JsonProperty(Required = Required.Always)]
        public bool lakes;

        [JsonProperty(Required = Required.Always)]
        public bool features;

        [JsonProperty(Required = Required.Always)]
        public List<Dimension_layer> layers;

        [JsonProperty(Required = Required.Always)]
        public Dimension_structure structures;
    }

    public class Dimension_layer
    {
        [JsonProperty(Required = Required.Always)]
        public string block;

        [JsonProperty(Required = Required.Always)]
        public int height;
    }

    public class Dimension_structure
    {
        //This is not required
        public Structure_stronghold stronghold;

        //This is not required
        public Dictionary<string, Structure_detail> structures;
    }

    public class Structure_detail
    {
        [JsonProperty(Required = Required.Always)]
        public int spacing; //Between 0 and 4096

        [JsonProperty(Required = Required.Always)]
        public int separation; //Between 0 and 4096

        [JsonProperty(Required = Required.Always)]
        public ulong salt;
    }

    public class Structure_stronghold
    {
        [JsonProperty(Required = Required.Always)]
        public int distance; //Between 0 and 1023

        [JsonProperty(Required = Required.Always)]
        public int spread; //Between 0 and 1023

        [JsonProperty(Required = Required.Always)]
        public int count; //Between 1 and 4095

    }


    public class Dimension_generator_noise : Dimension_generator
    {
        [JsonProperty(Required = Required.Always)]
        public long seed;

        [JsonProperty(Required = Required.Always)]
        public object settings;

        public Biome_source biome_source;

        [JsonConstructor]
        public Dimension_generator_noise(JToken seed, JToken settings, JToken biome_source)
        {
            this.seed = seed.ToObject<long>();

            if (settings.Type == JTokenType.String)
            {
                this.settings = settings.ToString();
            }
            else
            {
                this.settings = settings.ToObject<Noise_settings>();
            }

            string biome_source_type = biome_source["type"].Value<string>();

            if (biome_source_type == "minecraft:checkerboard" || biome_source_type == "checkerboard")
            {
                this.biome_source = biome_source.ToObject<Biome_source_checkerboard>();
            }
            else if (biome_source_type == "minecraft:fixed" || biome_source_type == "fixed")
            {
                this.biome_source = biome_source.ToObject<Biome_source_fixed>();
            }
            else if (biome_source_type == "minecraft:multi_noise" || biome_source_type == "multi_noise")
            {
                string preset = biome_source["preset"].Value<string>();

                if(preset == "nether")
                {
                    this.biome_source = biome_source.ToObject<Biome_source_multi_noise_preset>();
                }
                else
                {
                    this.biome_source = biome_source.ToObject<Biome_source_multi_noise_no_preset>();
                }
            }
            else if (biome_source_type == "minecraft:the_end" || biome_source_type == "the_end")
            {
                this.biome_source = biome_source.ToObject<Biome_source_the_end>();
            }
            else if (biome_source_type == "minecraft:vanilla_layered" || biome_source_type == "vanilla_layered")
            {
                this.biome_source = biome_source.ToObject<Biome_source_vanilla_layered>();
            }
            else
            {
                throw new JsonSerializationException("Unknown generator type: " + biome_source_type);
            }
        }
    }

    public class Noise_settings
    {
        [JsonProperty(Required = Required.Always)]
        public string name;

        [JsonProperty(Required = Required.Always)]
        public Noise_default default_block;

        [JsonProperty(Required = Required.Always)]
        public Noise_default default_fluid;

        [JsonProperty(Required = Required.Always)]
        public int bedrock_roof_position; //Between -20 and 276

        [JsonProperty(Required = Required.Always)]
        public int bedrock_floor_position; //Between -20 and 276

        [JsonProperty(Required = Required.Always)]
        public int sea_leavel; //Between 0 and 255

        [JsonProperty(Required = Required.Always)]
        public bool disable_mob_generation;

        [JsonProperty(Required = Required.Always)]
        public Noise noise;

        [JsonProperty(Required = Required.Always)]
        public bool simplex_surface_noise;

        //This is not required
        public bool? random_density_offset;

        //This is not required
        public bool? island_noise_override;

        //This is not required
        public bool? amplified;

        //This is not required
        public Dictionary<string, Structure_detail> structures;
    }

    public class Noise
    {
        [JsonProperty(Required = Required.Always)]
        public int height; //Between 0 and 255

        [JsonProperty(Required = Required.Always)]
        public int size_horizontal; //Between 1 and 4

        [JsonProperty(Required = Required.Always)]
        public int size_vertical; //Between 1 and 4

        [JsonProperty(Required = Required.Always)]
        public long density_factor;

        [JsonProperty(Required = Required.Always)]
        public long density_offset;

        [JsonProperty(Required = Required.Always)]
        public Sampling sampling;

        [JsonProperty(Required = Required.Always)]
        public Slide top_slide;

        [JsonProperty(Required = Required.Always)]
        public Slide bottom_slide;
    }

    public class Sampling
    {
        [JsonProperty(Required = Required.Always)]
        public int xz_scale; //Between 0.001 and 1000

        [JsonProperty(Required = Required.Always)]
        public int y_scale; //Between 0.001 and 1000

        [JsonProperty(Required = Required.Always)]
        public int xz_factor; //Between 0.001 and 1000

        [JsonProperty(Required = Required.Always)]
        public int y_factor; //Between 0.001 and 1000
    }

    public class Slide
    {
        [JsonProperty(Required = Required.Always)]
        public long target;

        [JsonProperty(Required = Required.Always)]
        public int size; //Between 0 and 256

        [JsonProperty(Required = Required.Always)]
        public long offset;
    }

    public class Noise_default
    {
        [JsonProperty(Required = Required.Always)]
        public string name;
        public List<string> properties;
    }

    public class Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public string type;
    }

    public class Biome_source_checkerboard : Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public int scale; //Between 0 and 62

        [JsonProperty(Required = Required.Always)]
        public List<string> biomes;
    }

    public class Biome_source_fixed : Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public string biome;
    }

    public class Biome_source_multi_noise_preset : Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public long seed;

        public string preset;
    }
    public class Biome_source_multi_noise_no_preset : Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public long seed;

        public string preset;

        public Biome_noise temperature_noise;
        public Biome_noise humidity_noise;
        public Biome_noise altitude_noise;
        public Biome_noise weirdness_noise;

        public List<Biome> biomes;
    }



    public class Biome_noise
    {
        [JsonProperty(Required = Required.Always)]
        public long first_octave;

        [JsonProperty(Required = Required.Always)]
        public List<long> amplitudes;
    }

    public class Biome
    {
        public string biome;
        public Biome_parameters parameters;
    }

    public class Biome_parameters
    {
        public int temperature;  //Between -2 and 2
        public int humidity;  //Between -2 and 2
        public int altitude;  //Between -2 and 2
        public int weirdness;  //Between -2 and 2
        public int offset;  //Between 0 and 1

    }

    public class Biome_source_the_end : Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public long seed;
    }

    public class Biome_source_vanilla_layered : Biome_source
    {
        [JsonProperty(Required = Required.Always)]
        public long seed;

        //This is not required
        public bool? large_biomes;

        //This is not required
        public bool? legacy_biome_init_layer;
    }
}
