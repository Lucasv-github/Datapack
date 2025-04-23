using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_common
{
    public class Overlay
    {
        public Version_range Compatibility;
        public string Path;

        public Overlay(Version_range compatibility, string path)
        {
            Compatibility = compatibility;
            Path = path;
        }
    }
}
