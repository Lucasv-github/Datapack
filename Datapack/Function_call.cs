using Minecraft_common;

namespace Datapack
{
    public class Function_call : Register_entry
    {
        public readonly string Path;
        public readonly string Short_path;

        public readonly Version_range Compatibility;

        public Function_call(string function_string) : base (function_string)
        {

        }

        public Function_call(Function_call parsed, Version_range compatibility, string path, string small_path) : base(parsed.String)
        {
            Compatibility = compatibility;

            Path = path;
            Short_path = small_path;
        }
    }
}
