using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing;
using Minecraft_common;

namespace Datapack
{
    public class Datapack_file
    {
        public string Namespace;
        public string Name;

        /// <summary>
        /// The compatibility based solely on the files location (overlay) or name (plural/nonplural)
        /// </summary>
        public Version_range Context_compatibility;

        /// <summary>
        /// Absolute path of this file
        /// </summary>
        public string Path;

        /// <summary>
        /// Path that goes no higher than /data
        /// </summary>
        public string Short_path;

        /// <summary>
        /// The raw unserialized data straight from the file
        /// </summary>
        public string Data;

        public Datapack_file(string path, string short_path, string data, string namespace_, string name, Version_range compatibility)
        {
            Path = path;
            Short_path = short_path;

            Data = data;

            Namespace = namespace_;
            Name = name;
            Context_compatibility = compatibility;
        }

        public void Raw_print(string root_directory, int version)
        {
            string current_result_path = root_directory + Short_path;

            Directory.CreateDirectory(Directory.GetParent(current_result_path).ToString());
            File.WriteAllText(current_result_path, Data);
        }
    }
}
