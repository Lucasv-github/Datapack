namespace Command_parsing
{
    public class External_registers
    {
        private readonly Dictionary<string, Dictionary<string, List<string>>> external_registers;

        //TODO this needs to take versions into consideration as well which this currently doesn't
        public External_registers(string path)
        {
            //TODO will have to handle things like overlays here as well

            //THe datapack shouldn't pass pre handling if it doesn't have a data folder, thus should be able to start traversing that here without checking its presenc

            external_registers = new();

            string[] namespaces = Directory.GetDirectories(path + "/data");

            foreach (string namespace_ in namespaces)
            {
                string namespace_name = Path.GetFileName(namespace_);

                //Shipping this for now
                if (namespace_name == "minecraft")
                {
                    continue;
                }

                Dictionary<string, List<string>> namespace_registers = new();

                //TODO these are old school right now
                Handle_external("dimension", "DIMENSION");

                Handle_external("tags/blocks", "BLOCK_TAG", true);
                Handle_external("tags/items", "ITEM_TAG", true);
                Handle_external("tags/entity_types", "ENTITY_TAG", true);

                Handle_external("loot_tables", "LOOT_TABLE", false);

                //TODO here are the new schools, this needs to have a version range (which means we should probably move that to a command class)

                Handle_external("tags/block", "BLOCK_TAG", true);
                Handle_external("tags/item", "ITEM_TAG", true);
                Handle_external("tags/entity_type", "ENTITY_TAG", true);

                Handle_external("loot_table", "LOOT_TABLE", false);

                void Handle_external(string minecraft_path, string own_name, bool tag = false)
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

                            ;

                            if (tag)
                            {
                                tags.Add("#" + subpath);

                            }
                            else
                            {
                                tags.Add(subpath);
                            }
                        }

                        namespace_registers.Add(own_name, tags);
                    }
                }

                external_registers.Add(namespace_name, namespace_registers);
            }
        }

        public void Verify_collection(string collection, string namespace_, string item, out string error)
        {
            if (!external_registers.ContainsKey(namespace_))
            {
                error = "No external namespace by the name: " + namespace_ + " provided";
                return;
            }

            Dictionary<string, List<string>> collection_ = external_registers[namespace_];

            if (!collection_.ContainsKey(collection))
            {
                error = "No external collection by the name: " + collection + " provided";
                return;
            }

            List<string> collection_value = collection_[collection];

            if (!collection_value.Contains(item))
            {
                error = "External collection: " + collection + " does not contain: " + item;
                return;
            }

            error = "";
        }
    }
}
