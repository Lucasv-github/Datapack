using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing;
using Minecraft_common;
using Minecraft_common.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Datapack.Content_serializers
{
    public class Tag_serializer : Content_serializer
    {
        private readonly List<Serialized_tag> serialized_tags;

        public Tag_serializer(Datapack_loader loader, Version_range serialization_directive, List<Datapack_file> files, string collection = null) : base(loader, serialization_directive, files)
        {
            serialized_tags = new();

            foreach (Datapack_file tag_file in files)
            {
                Serialize_tag(tag_file);
            }

            for (int i = 0; i < serialized_tags.Count; i++)
            {
                if (serialized_tags[i].Success)
                {
                    Serialization_success.Add(serialized_tags[i].Version);
                }
            }


            for (int i = 0; i < serialized_tags.Count; i++)
            {
                if (!serialized_tags[i].Success)
                {
                    continue;
                }

                string version = Versions.Get_own_version(serialized_tags[i].Version);

                foreach (object tag in serialized_tags[i].Serialization_result.values)
                {
                    string value;

                    if (tag is Tag_value_json object_tag)
                    {
                        value = object_tag.id;
                    }
                    else
                    {
                        value = (string)tag;
                    }

                    //Verify that tags in a tag exists and that the definition isn't circular

                    if(value.StartsWith('#'))
                    {
                        if(!value.StartsWith("#minecraft"))  //TODO will have to think about this do we want to inject minecraft vanilla datapack it or fall back to the resources (right now we are using the fall back down below)
                        {
                            Traverse_tag(version, new List<string> { string.Concat("#", serialized_tags[i].Namespace, ":", serialized_tags[i].Name.AsSpan(1)) }, value, out string error);
                            if (error != "")
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Write_line("Error loading tag: " + serialized_tags[i].Short_path + "\n  " + error);
                                Console.ResetColor();
                                serialized_tags[i].Success = false;
                                break;
                            }
                        }
                    }

                    //Verify the contents of the tag against a collection
                    if (collection != null)
                    {
                        string error;
                        try
                        {
                            error = loader.Verify_collection(version, collection, value);
                        }
                        catch
                        {
                            error = "BANDAID TO PREVENT CRASH";
                        }

                        if (error != "")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Write_line("Error loading tag: " + serialized_tags[i].Short_path + "\n  " + error);
                            Console.ResetColor();
                            serialized_tags[i].Success = false;
                            break;
                        }
                    }
                }

            }

            Write_line("\nTotal serialized tags count: " + files.Count);
            Write_line("Tag serialization success ranges: \n" + Serialization_success.ToString());
        }

        /// <summary>
        /// Will check that subtags in the tag exists as well as detect circular definitions
        /// </summary>
        /// <param name="parent_tags"></param>
        /// <param name="tag_value"></param>
        /// <param name="error"></param>
        private void Traverse_tag(string version, List<string> parent_tags, string tag_value, out string error)
        {
            string[] parts = tag_value.Substring(1).Split(':');
            string namespace_ = parts[0];
            string name = "#" + parts[1];

            List<object> tag_values = Get_tag_values(namespace_, name, version, out error);

            if (error != "")
            {
                return;
            }

            foreach (object sub_tag in tag_values)
            {
                string value;

                if (sub_tag is Tag_value_json object_tag)
                {
                    value = object_tag.id;
                }
                else
                {
                    value = (string)sub_tag;
                }

                if (value.StartsWith('#'))
                {
                    if (!value.StartsWith("#minecraft"))  //This skip is explained higher up but no this doesn't leave out scanning as we both traverse tags and also scan them
                    {
                        if (parent_tags.Contains(value))
                        {
                            error = "Cirulat tag definition detected";
                            return;
                        }

                    }

                    parent_tags.Add(value);

                    Traverse_tag(version, parent_tags, value, out error);

                    if (error != "")
                    {
                        return;
                    }
                }
            }
        }

        //TODO this should obviously fail if tag has failed
        //TODO to fix this and other error Traverse_tag should probably also scan the non tag things and set flag to prevent wasteing resorces by validaing twice

        //Register_verifyer in Datapack_loader only deals with unserialized, can't be used here
        public List<object> Get_tag_values(string namespace_, string name, string version, out string error)
        {
            List<Serialized_tag> version_filtered = serialized_tags.FindAll(r => r.Version == Versions.Get_own_version(version));

            if (version_filtered.Count == 0)
            {
                error = "No external register for version: " + version + " provided";
                return null;
            }

            List<Serialized_tag> namespace_filtered = version_filtered.FindAll(r => r.Namespace == namespace_);

            if (namespace_filtered.Count == 0)
            {
                error = "Namespace: " + namespace_ + " not provided for version: " + version;
                return null;
            }

            List<Serialized_tag> name_filtered = version_filtered.FindAll(r => r.Name == name);

            if(name_filtered.Count == 0)
            {
                error = "Namespace: " + namespace_ + " does not contain: " + name + " in version: " + version;
                return null;
            }

            error = "";
            return name_filtered[0].Serialization_result.values;
        }

        /// <summary>
        /// Converts the raw data into json
        /// </summary>
        private void Serialize_tag(Datapack_file tag_file)
        {
            Write_line("Serializing tag: " + tag_file.Short_path);

            Tag_json minecraft_tag = JsonConvert.DeserializeObject<Tag_json>(tag_file.Data);

            for (int i = 0; i <= Versions.Max_own; i++)
            {
                if (tag_file.Context_compatibility.Is_set(i) && (Serialization_directives == null || Serialization_directives.Is_set(i)))
                {
                    bool success = true;

                    try
                    {
                        for (int j = 0; j < minecraft_tag.values.Count; j++)
                        {
                            string tag_value_string = minecraft_tag.values[j].ToString();
                            string tag_value_trimmed = tag_value_string.Trim();

                            if (i >= Versions.Get_own_version("1.16"))  //Tags with objects from 1.16 and onwards
                            {
                                //We cannot rely on exceptions as they are very slow when possible tens of thousands will be fired
                                if (tag_value_trimmed.StartsWith('{') && tag_value_trimmed.EndsWith('}'))
                                {
                                    minecraft_tag.values[j] = JsonConvert.DeserializeObject<Tag_value_json>(tag_value_string);
                                    continue;
                                }
                            }
                            
                            minecraft_tag.values[j] = tag_value_string;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Write_line("Error serializing tag: " + ex);
                        Console.ResetColor();
                        success = false;
                    }

                    serialized_tags.Add(new Serialized_tag(tag_file, i, success, minecraft_tag));
                }
            }
        }

        public override void Print(string root_directory, int version, bool strip_empty, bool strip_comments)
        {
            foreach (Datapack_file tag_file in files)
            {
                List<Serialized_tag> correct_tag = serialized_tags.FindAll(f => f.Short_path == tag_file.Short_path);

                if (correct_tag.Count == 0)
                {
                    //And no we do not output the unserialized here, the places we do that are when we haven't had time for implement a serializer, to get it to work temporarily.
                    //If serialization fails when it shouldn't thats a bug, not a missing feature

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Tag: " + tag_file.Short_path + " hasn't been serialized at all");
                    Console.ResetColor();
                }

                correct_tag = correct_tag.FindAll(f => f.Version == version);

                if (correct_tag.Count > 1)
                {
                    throw new Exception("Something is wrong, " + correct_tag.Count + " serializations for version: " + version + " of the tag: " + tag_file.Short_path);
                }

                if (correct_tag.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Tag: " + tag_file.Short_path + " hasn't been serialized at all in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                if (!correct_tag[0].Success)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Write_line("Tag: " + tag_file.Short_path + " hasn't been serialized succesfully in version: " + version);
                    Console.ResetColor();
                    continue;
                }

                string current_result_path = root_directory + tag_file.Short_path;

                Directory.CreateDirectory(Directory.GetParent(current_result_path).ToString());
                File.WriteAllText(current_result_path, JsonConvert.SerializeObject(correct_tag[0].Serialization_result));
            }
        }
    }

    public class Serialized_tag : Serialized_datapack_file
    {
        /// <summary>
        /// The actual serialized strucuture of the minecraft tag, see https://minecraft.wiki/w/Tag#List_of_tags
        /// </summary>
        public Tag_json Serialization_result;

        public Serialized_tag(Datapack_file file, int version, bool success, Tag_json serialization_result) : base (file, version, success)
        {
            Serialization_result = serialization_result;
        }
    }
}
