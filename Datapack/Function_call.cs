using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datapack
{
    public class Function_call
    {
        public bool Legacy; //<=1.20.6 (in functions instead of function)
        public string Function;
        public string Namespace;
        public string Name;

        public Version_range Compatibility;

        public Function_call(bool legacy, string function, Version_range compatibility)
        {
            Legacy = legacy;
            Function = function;
            Namespace = function.Split(':')[0];
            Name = function.Split(':')[1];
            Compatibility = compatibility;
        }

        public Function_call(bool legacy, string function) 
        {
            Legacy = legacy;
            Function = function;
            Namespace = function.Split(':')[0];
            Name = function.Split(':')[1];

            if (legacy)
            {
                Compatibility = new Version_range(0, Versions.Get_own_version("1.20.6"), true);
            }
            else
            {
                Compatibility = new Version_range(Versions.Get_own_version("1.21"), Versions.Max,true);
            }
        }
    }
}
