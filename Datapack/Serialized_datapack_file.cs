using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft_common;
using System.Xml.Linq;

namespace Datapack
{
    public class Serialized_datapack_file
    {
        /// <summary>
        /// The namespace in which the file was found
        /// </summary>
        public string Namespace;

        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name;

        /// <summary>
        /// The short path of the function starting with /data
        /// </summary>
        public string Short_path;

        /// <summary>
        /// The version that this serialization happned in, in ownversion is set even in case of failure
        /// </summary>
        public int Version;

        /// <summary>
        /// Whether or not the serialization was successfull
        /// </summary>
        public bool Success;

        public Serialized_datapack_file() 
        { 

        }

        public Serialized_datapack_file(Datapack_file file, int version, bool success)
        {
            Namespace = file.Namespace;
            Name = file.Name;

            Short_path = file.Short_path;
            Version = version;
            Success = success;
        }
    }
}
