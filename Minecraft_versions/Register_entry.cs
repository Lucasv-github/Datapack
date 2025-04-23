using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Minecraft_common
{
    public class Register_entry
    {
        public readonly bool Tag;
        public readonly string String;

        public readonly string Namespace;
        public readonly string Name;

        public Register_entry(string string_)
        {
            String = string_;
            Namespace = string_.Split(':')[0];
            Name = string_.Split(':')[1];

            if (Namespace.StartsWith('#'))
            {
                Tag = true;
                Name = "#" + Name;
                Namespace = Namespace[1..];
            }
        }
    }
}
