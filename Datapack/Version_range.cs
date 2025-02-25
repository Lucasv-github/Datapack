using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datapack
{
    public class Version_range
    {
        private readonly int[] versions;

        public Version_range() 
        {
            versions = new int[Datapack.Versions.Max + 1];
        }

        public Version_range(int min_inclusive, int max_inclusive, bool supported)
        {
            versions = new int[Datapack.Versions.Max +  1];
            Set(min_inclusive,max_inclusive,supported);
        }

        public bool Is_set(int i)
        {
            return versions[i] > 0;
        }

        public void Write(Detector decector)
        {
            Write(1, decector);
        }

        public void Write(int limit, Detector detector)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            int start = -1;

            for(int i = 0; i < versions.Length; i++)
            {
                if (versions[i] >= limit)
                {
                    if(start == -1)
                    {
                        start = i;
                    }
                }
                else if(start != -1)
                {
                    detector.Write(Datapack.Versions.Get_own_version(start) + "-" + Datapack.Versions.Get_own_version(i - 1));
                    start = -1;
                }
            }
            
            if (start != -1)
            {
                detector.Write(Datapack.Versions.Get_own_version(start) + "-" + Datapack.Versions.Get_own_version(versions.Length - 1));
            }

            Console.ResetColor();
        }

        public void Unset()
        {
            for(int i = 0; i < versions.Length; i++)
            {
                versions[i] = 0;
            }
        }

        public void Set(int min_inclusive, int max_inclusive, bool supported)
        {
            for(int i = min_inclusive; i <= max_inclusive; i++)
            {
                if(supported)
                {
                    versions[i] = 1;
                }
                else
                {
                    versions[i] = 0;
                }
            }
        }

        public void Set_other(int min_inclusive, int max_inclusive, bool supported)
        {
            Set(0, min_inclusive - 1, false);
            Set(max_inclusive + 1, Versions.Max, false);
        }

        public void Add(int i)
        {
            versions[i]++;
        }

        public int Get_max()
        {
            int max = 0;

            for (int i = 0; i < versions.Length; i++)
            {
                if (versions[i] > max)
                {
                    max = versions[i];
                }
            }

            return max;
        }

        public int Get_level(int i)
        {
            return versions[i];
        }
    }
}
