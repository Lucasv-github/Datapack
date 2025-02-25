using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datapack
{
    public class Change
    {
        private readonly string description;

        public readonly int Min_inc_version;
        public readonly int Max_inc_version;

        private readonly Change_types type;

        private readonly Func<string, Change, bool> detection_function;

        public Change(string description, int min_inc_version, int max_inc_version, Change_types type, Func<string, Change, bool> detection_function)
        {
            this.description = description;

            Min_inc_version = min_inc_version;
            Max_inc_version = max_inc_version;

            this.type = type;
            this.detection_function = detection_function;
        }

        public void Check(string line, Version_range version_range, bool output)
        {
            if (!detection_function.Invoke(line, this))
            {
                return;
            }
            if (type is Change_types.allow)
            {
                throw new NotImplementedException();
            }
            else if (type is Change_types.block)
            {
                if (Min_inc_version == 0)
                {
                    version_range.Set(Min_inc_version, Max_inc_version, false);
                    if (output) Console.WriteLine(description + " point to: >=" + Versions.Get_own_version(Max_inc_version + 1));
                }
                else if (Max_inc_version == Versions.Max)
                {
                    version_range.Set(Min_inc_version, Max_inc_version, false);
                    if (output) Console.WriteLine(description + " point to: <=" + Versions.Get_own_version(Min_inc_version - 1));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (type is Change_types.block_other)
            {
                //Should block others not including min/max
                version_range.Set_other(Min_inc_version,Max_inc_version,false);
                if (output) Console.WriteLine(description + " point to: <=" + Versions.Get_own_version(Max_inc_version - 1) + " >=" + Versions.Get_own_version(Min_inc_version + 1));
            }
        }
    }
    public enum Change_types
    {
        allow = 1,
        block = 2,
        block_other = 3,
    }
}
