using System;
using System.IO;
using System.Xml.Linq;
using Minecraft_common;
using Newtonsoft.Json;

namespace Command_parsing
{
    public class External_registers
    {
        private readonly List<Register_version> registers;
        private readonly string minecraft_path;

        //TODO we might want to get all the collection values directly, thus allowing us to validate them as well

        public External_registers(string minecraft_path, List<Overlay> overlays)
        {
            this.minecraft_path = minecraft_path;

            registers = new();

            string[] namespaces = Directory.GetDirectories(minecraft_path + "/data");

            Handle_namespaces(namespaces, Version_range.All());
            //Once we have the default we can start handling the overlays, but we do not carve out any compatibility

            if (overlays != null)
            {
                foreach (Overlay overlay in overlays)
                {
                    string current_overlay_path = minecraft_path + "/" + overlay.Path + "/data/";

                    if (!Directory.Exists(current_overlay_path))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Overlay doesn't exist: " + current_overlay_path);
                        Console.ResetColor();
                        continue;
                    }


                    namespaces = Directory.GetDirectories(current_overlay_path);

                    Handle_namespaces(namespaces, overlay.Compatibility, overlay.Path);

                    //old_defaul_registers.Unset(overlay.Compatibility);
                    //new_defaul_registers.Unset(overlay.Compatibility);
                }
            }

            //And here we do not warn about conflicting as the default will always stand

            void Handle_namespaces(string[] namespaces, Version_range external_compatibility, string overlay = null)
            {
                Version_range oldschool_compatibility = new(Versions.Get_own_version("1.13"), Versions.Get_own_version("1.20.6"));
                Version_range newschool_compatibility = new(Versions.Get_own_version("1.21"), Versions.Max_own);

                //And it is very possible that one of this will be totaly unset, we will still continue but warn if any registers belong to such
                Register_version oldschool_namespaces = new(external_compatibility.Get_common(oldschool_compatibility));
                Register_version newschool_namespaces = new(external_compatibility.Get_common(newschool_compatibility));

                if (oldschool_compatibility.Is_entire(false)) 
                {
                    oldschool_compatibility = null;
                }
                else
                {
                    registers.Add(oldschool_namespaces);
                }


                if (newschool_compatibility.Is_entire(false))
                {
                    newschool_compatibility = null;
                }
                else
                {
                    registers.Add(newschool_namespaces);
                }

                foreach (string namespace_ in namespaces)
                {
                    Handle_namespace(namespace_, overlay, oldschool_namespaces, newschool_namespaces);
                }
            }

            //Will sometimes produce 2, one pre 1.21 and one post
            void Handle_namespace(string namespace_, string overlay, Register_version oldschool_namespaces, Register_version newschool_namespaces)
            {
                string namespace_name = Path.GetFileName(namespace_);

                //Shipping this for now
                //if (namespace_name == "minecraft")
                //{
                //    return;
                //}

                //These are the oldschool

                Register_namespace one_namespace = new();

                

                Add_register(namespace_, overlay, one_namespace, "dimension", "DIMENSION");

                Add_register(namespace_, overlay, one_namespace, "tags/functions", "FUNCTION_TAG", true);
                Add_register(namespace_, overlay, one_namespace, "tags/blocks", "BLOCK_TAG", true);
                Add_register(namespace_, overlay, one_namespace, "tags/items", "ITEM_TAG", true);
                Add_register(namespace_, overlay, one_namespace, "tags/entity_types", "ENTITY_TAG", true);

                Add_register(namespace_, overlay, one_namespace, "loot_tables", "LOOT_TABLE", false);

                if(oldschool_namespaces == null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Oldschool namespace: " + namespace_ + " is never reachable with this overlay's range");
                }
                else
                {
                    oldschool_namespaces.Namespaces.Add(namespace_name, one_namespace);
                }
                

                //And these are the newschool
                one_namespace = new();

                Add_register(namespace_, overlay, one_namespace, "dimension", "DIMENSION");

                Add_register(namespace_, overlay, one_namespace, "tags/function", "FUNCTION_TAG", true);
                Add_register(namespace_, overlay, one_namespace, "tags/block", "BLOCK_TAG", true);
                Add_register(namespace_, overlay, one_namespace, "tags/item", "ITEM_TAG", true);
                Add_register(namespace_, overlay, one_namespace, "tags/entity_type", "ENTITY_TAG", true);

                Add_register(namespace_, overlay, one_namespace, "loot_table", "LOOT_TABLE", false);

                if (newschool_namespaces == null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Newschool namespace: " + namespace_ + " is never reachable with this overlay's range");
                }
                else
                {
                    newschool_namespaces.Namespaces.Add(namespace_name, one_namespace);
                }
            }

            static void Add_register(string namespace_, string overlay, Register_namespace one_namespace, string minecraft_path, string own_name, bool tag = false)
            {
                if (Directory.Exists(namespace_ + "/" + minecraft_path))
                {
                    string[] tag_paths = Directory.GetFiles(namespace_ + "/" + minecraft_path, "*.json", SearchOption.AllDirectories);
                    List<string> tags = new();

                    foreach (string path in tag_paths)
                    {
                        string full_subpath = Path.GetRelativePath(namespace_ + "/" + minecraft_path, path);
                        string subpath = Path.GetDirectoryName(full_subpath) + "/" + Path.GetFileNameWithoutExtension(full_subpath);

                        subpath = subpath.Replace('\\', '/');

                        if (subpath[0] == '/')
                        {
                            subpath = subpath[1..];
                        }

                        if (tag)
                        {
                            tags.Add("#" + subpath);

                        }
                        else
                        {
                            tags.Add(subpath);
                        }
                    }

                    one_namespace.Add(own_name, overlay, minecraft_path, tags);
                }
            }
        }

        //This takes care to expand tags in tags, thus this should NEVER give out a taged value
        //Should be easier to detect circular definitions here
        public List<string> Get_tag_value(string minecraft_version, string collection, string namespace_, string tag, out string error, bool missing_fail = false, List<string> previous_tags = null)
        {
            Verify_collection(minecraft_version, collection, namespace_, tag, out error);

            if(error != "")
            {
                return null;
            }

            List<string> values = new();

            Register_namespace data = Get_namespace(minecraft_version, collection, namespace_, out _);

            //This shouldn't fail
            string minecraft_subpath = data.Collections[collection].Item1;
            tag = tag[1..];

            //The file should already be here

            Tag_json root;

            if (data.Overlay != null)
            {
                root = JsonConvert.DeserializeObject<Tag_json>(File.ReadAllText(minecraft_path + "/" + data.Overlay + "/data/" + namespace_ + "/" + minecraft_subpath + "/" + tag + ".json"));
            }
            else
            {
                root = JsonConvert.DeserializeObject<Tag_json>(File.ReadAllText(minecraft_path + "/data/" + namespace_ + "/" + minecraft_subpath + "/" + tag + ".json"));
            }

            
            foreach (object unserialized in root.values)
            {
                string to_add;

                try
                {
                    //Is it as an object?
                    Tag_value_json tag_value = JsonConvert.DeserializeObject<Tag_value_json>(unserialized.ToString());
                    to_add = tag_value.id;
                }
                catch
                {
                    //Apparently as a string
                    to_add = unserialized.ToString();
                }

                if (to_add.StartsWith('#'))
                {
                    if (previous_tags != null && previous_tags.Contains(to_add))
                    {
                        error = "Circular tag definition";
                        return null;
                    }

                    //Apparently we have a tag in a tag

                    Register_entry current_tag = new(to_add);

                    previous_tags ??= new() { };
                    previous_tags.Add(to_add);

                    List<string> tag_tag_data = Get_tag_value(minecraft_version, collection, current_tag.Namespace, current_tag.Name, out error, previous_tags: previous_tags);

                    if(error != "")
                    {
                        if(missing_fail || error == "Circular tag definition")
                        {
                            return null;
                        }

                        continue;
                    }

                    values.AddRange(tag_tag_data);

                    ;
                }
                else
                {
                    values.Add(to_add);
                }
            }

            error = "";
            return values;
        }

        public void Verify_collection(string minecraft_version, string collection, string namespace_, string item, out string error)
        {
            Register_namespace data = Get_namespace(minecraft_version, collection, namespace_, out error);

            if(error != "")
            {
                return;
            }

            if (!data.Collections[collection].Item2.Contains(item))
            {
                error = "External collection: " + collection + " does not contain: " + item;
                return;
            }

            error = "";
        }

        public Register_namespace Get_namespace(string minecraft_version, string collection, string namespace_, out string error)
        {
            int version_index = registers.FindIndex(r => r.Compatibility.Is_set(Versions.Get_own_version(minecraft_version)) && r.Namespaces.ContainsKey(namespace_) && r.Namespaces[namespace_].Collections.ContainsKey(collection));

            List<Register_version> version_filtered = registers.FindAll(r => r.Compatibility.Is_set(Versions.Get_own_version(minecraft_version)));

            if (version_filtered.Count == 0)
            {
                error = "No external register for version: " + minecraft_version + " provided";
                return null;
            }

            List<Register_version> namespace_filtered = version_filtered.FindAll(r => r.Namespaces.ContainsKey(namespace_));

            if (namespace_filtered.Count == 0)
            {
                error = "No external namespace by the name: " + namespace_ + " provided for version: " + minecraft_version;
                return null;
            }

            List<Register_version> collection_filtered = namespace_filtered.FindAll(r => r.Namespaces.Any(n => n.Value.Collections.ContainsKey(collection)));

            if (collection_filtered.Count == 0)
            {
                error = "No external collection by the name: " + collection + " provided for version: " + minecraft_version;
                return null;
            }

            //Starting by returning the overlays
            //TODO if multiple OVERLAYS exists we might want to warn
            foreach (Register_version version in collection_filtered)
            {
                if(version.Namespaces[namespace_].Overlay != null)
                {
                    error = "";
                    return version.Namespaces[namespace_];
                }
            }

            //This should never fail as this is the second time this is ran
            error = "";
            return collection_filtered.Find(r => r.Namespaces.ContainsKey(namespace_)).Namespaces[namespace_];
        }
    }

    public class Register_version
    {
        public Version_range Compatibility;
        public Dictionary<string, Register_namespace> Namespaces;
        

        public Register_version(Version_range compatibility)
        {
            Compatibility = compatibility;
            Namespaces = new();
        }
    }

    public class Register_namespace
    {
        public Dictionary<string, Tuple<string, List<string>>> Collections;
        public string Overlay;

        public Register_namespace()
        {
            Collections = new();
        }

        public void Add(string own_name, string overlay, string minecraft_path, List<string> tags)
        {
            Collections.Add(own_name, new Tuple<string, List<string>>(minecraft_path,tags));
            Overlay = overlay;
        }
    }
}
