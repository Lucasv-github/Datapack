using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_common
{
    public class Pack_mcmeta_json
    {
        public Pack_json pack;
        public Overlays_json overlays;
    }

    public class Pack_json
    {
        public int pack_format;
        public object description;
        public object supported_formats;
    }

    public class Overlays_json
    {
        public List<Entry_json> entries;
    }
    public class Entry_json
    {
        public object formats;
        public string directory;
    }

    public class Description_json
    {
        public string text;
    }

    public class Supported_formats_json
    {
        public int min_inclusive;
        public int max_inclusive;
    }

    public class Tag_json
    {
        public bool replace;
        public List<object> values;
    }

    public class Tag_value_json
    {
        public string id;
        public bool required;
    }
}
