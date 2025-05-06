using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minecraft_common;

namespace Datapack
{
    public abstract class Content_serializer
    {
        /// <summary>
        /// Data read straight from the datapack
        /// </summary>
        protected readonly List<Datapack_file> files;

        /// <summary>
        /// Directives specifying if serialization should be tried in a specific version, used together with the context_compatibility
        /// </summary>
        public Version_range Serialization_directives;

        /// <summary>
        /// Each version has the value of the number of files that succesfully were deserialized in a version
        /// </summary>
        public Version_range Serialization_success;

        /// <summary>
        /// Instance to allow sending messages
        /// </summary>
        protected readonly Datapack_loader loader;

        public Content_serializer(Datapack_loader loader, Version_range serialization_directives, List<Datapack_file> files)
        {
            Serialization_directives = serialization_directives;
            Serialization_success = new();
            this.loader = loader;
            this.files = files;
        }

        public void Write_line(string text)
        {
            loader.Write_line(text);
        }

        public void Write(string text)
        {
            loader.Write(text);
        }

        /// <summary>
        /// Converts the serialized data back into text for a specific version
        /// </summary>
        /// <param name="root_directory"></param>
        /// <param name="version"></param>
        /// <param name="strip_empty"></param>
        /// <param name="strip_comments"></param>
        public abstract void Print(string root_directory, int version, bool strip_empty, bool strip_comments);
    }
}
