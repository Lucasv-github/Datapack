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

        private readonly int min_inc_version;
        private readonly int max_inc_version;

        private readonly Change_types type;

        private readonly Func<string,bool> detection_function;

        private bool detected;

        public Change(string description, int min_inc_version, int max_inc_version, Change_types type, Func<string,bool> detection_function)
        {
            this.description = description;

            this.min_inc_version = min_inc_version;
            this.max_inc_version = max_inc_version;

            this.type = type;

            detected = false;

            this.detection_function = detection_function;
        }

        public void Purge()
        {
            detected = false;
        }

        public void Check(string line)
        {
            if(detected)
            {
                return;
            }

            if(detection_function.Invoke(line))
            {
                detected = true;
            }
        }

        public void Apply(ref bool[] versions)
        {
            if(!detected)
            {
                return;
            }
            if (type is Change_types.allow)
            {
                throw new NotImplementedException();
            }
            else if(type is Change_types.block)
            {
                if(min_inc_version == 0)
                {
                    Set(min_inc_version, max_inc_version + 1, false, ref versions);
                    Console.WriteLine(description + " point to: >=" + Versions.Get_own_version(max_inc_version + 1));
                }
                else if(max_inc_version == Versions.Max)
                {
                    Set(min_inc_version, max_inc_version, false, ref versions);
                    Console.WriteLine(description + " point to: <=" + Versions.Get_own_version(min_inc_version - 1));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (type is Change_types.block_other)
            {
                //Should block others not including min/max
                Set(0, min_inc_version, false, ref versions);
                Set(max_inc_version+1, Versions.Max, false, ref versions);
                Console.WriteLine(description + " point to: <=" + Versions.Get_own_version(max_inc_version) + " >=" + Versions.Get_own_version(min_inc_version));
            }

            //void Set_below_inc(int index, bool value, ref bool[] versions)
            //{
            //    for (int i = index; i >= 0; i--)
            //    {
            //        versions[i] = value;
            //    }
            //}

            //void Set_above_inc(int index, bool value, ref bool[] versions)
            //{
            //    for (int i = index; i < versions.Length; i++)
            //    {
            //        versions[i] = value;
            //    }
            //}

            //Lover is included but not max
            void Set(int start, int end, bool value, ref bool[] versions)
            {
                for (int i = start; i < Math.Min(end, versions.Length); i++)
                {
                    versions[i] = value;
                }
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
