using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Minecraft_common.Resources
{
    /// <summary>
    /// This handles the resources in the server.jar that may or may not be present in the vanilla resource pack as well 
    /// </summary>
    public class Resource_handler
    {
        /// <summary>
        /// The resources in different versions, observ that they can be empty and Get_resource will use something older if that is the case
        /// </summary>
        private static Dictionary<string, Dictionary<string, Tuple<bool, List<string>>>> resources;

        //TODO might want to limit to 1 version instead of all directly

        /// <summary>
        /// Adds all the resources from all versions
        /// </summary>
        private static void Add_resources()
        {
            resources = new();

            for(int i = 0; i <= Versions.Max_own; i++)
            {
                Add_resources(Versions.Get_own_version(i));
            }
        }

        public static List<string> Get_resource_names(string version)
        {
            if (resources == null)
            {
                Add_resources();
            }

            return resources[version].Keys.ToList();
        }

        public static Tuple<bool, List<string>> Get_resource(string version, string collection)
        {
            if(resources == null)
            {
                Add_resources();
            }

            //Backtracking if not adding in 
            for(int i = Versions.Get_own_version(version); i >= 0; i--)
            {
                if (resources[Versions.Get_own_version(i)].TryGetValue(collection, out Tuple<bool, List<string>> resource))
                {
                    return resource;
                }
            }

            throw new Exception("Could not find collection: " + collection + " in: " + version + " or in any below");
        }

        /// <summary>
        /// Adds all the resources from a specific version
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="version"></param>
        private static void Add_resources(string version)
        {
            resources.Add(version, new Dictionary<string, Tuple<bool, List<string>>>());

            //The regular which can have a "minecraft:" before

            string current_register = Get_resource_path(version);

            if (Directory.Exists(current_register + "/Regular"))
            {
                string[] namespaces = Directory.GetFiles(current_register + "/Regular");
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    List<string> entire = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));

                    resources[version].Add(name, new Tuple<bool, List<string>>(true, entire));
                }
            }

            //TODO should this be shifted to only handle things that aren't also in datapacks, like strip tags from this

            //TODO we currently have ITEM with items and ITEM_TAG with items and tags. Is this the way we want it?
            //Previously made sense with very simple validators, perhaps not much anymore

            if (Directory.Exists(current_register + "/Regular/Tags"))
            {
                //The tags
                string[] namespaces = Directory.GetFiles(current_register + "/Regular/Tags");

                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    //Assuming the validator_name exists already
                    List<string> tags_added = new(Get_resource(version,name).Item2);

                    List<string> raw = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    List<string> tags = new();

                    for (int i = 0; i < raw.Count; i++)
                    {
                        tags.Add("#" + raw[i]);
                    }

                    tags_added.AddRange(tags);

                    resources[version].Add(name + "_TAG", new Tuple<bool, List<string>>(true, tags_added));
                }
            }

            if (Directory.Exists(current_register))
            {
                //The other which can't have a "minecraft:" before
                //Better called the namespaceless
                string[] namespaces = Directory.GetFiles(current_register);
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    List<string> entire = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    resources[version].Add(name, new Tuple<bool, List<string>>(false, entire));
                }
            }
        }

        /// <summary>
        /// Will check if the resource is available locally else it will download the rereource from the web
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string Get_resource_path(string version)
        {
            //Some are currently present internally in source (because 1 they are not present in the extracted data, 2 misode doesn't host them)

            string path = AppDomain.CurrentDomain.BaseDirectory + "/Registers/" + version.Replace('.', '_');

            //Is it already present (in the database, not just in source)
            if (File.Exists(path + "/Info.txt"))
            {
                return path;
            }

            Resource_downloader.Download_to_directory(version, path);

            //TODO want this to fail safely somehow
            //Perhaps retries, then wait until working?

            return path;
        }
    }
}
