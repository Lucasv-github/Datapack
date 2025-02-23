using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datapack
{
    public class Version_range
    {
        private readonly int[] Versions;

        public Version_range() 
        {
            Versions = new int[Datapack.Versions.Max + 1];
        }

        public Version_range(int min_inclusive, int max_inclusive, bool supported)
        {
            Versions = new int[Datapack.Versions.Max +  1];
            Set(min_inclusive,max_inclusive,supported);
        }

        public bool Is_set(int i)
        {
            return Versions[i] > 0;
        }

        public void Write()
        {
            Write(1);
        }

        public void Write(int limit)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            int start = -1;

            for(int i = 0; i < Versions.Length; i++)
            {
                if (Versions[i] >= limit)
                {
                    if(start == -1)
                    {
                        start = i;
                    }
                }
                else if(start != -1)
                {
                    Console.Write(Datapack.Versions.Get_own_version(start) + "-" + Datapack.Versions.Get_own_version(i - 1));
                    start = -1;
                }
            }
            
            if (start != -1)
            {
                Console.Write(Datapack.Versions.Get_own_version(start) + "-" + Datapack.Versions.Get_own_version(Versions.Length - 1));
            }

            Console.ResetColor();
        }

        public void Set(int min_inclusive, int max_inclusive, bool supported)
        {
            for(int i = min_inclusive; i <= max_inclusive; i++)
            {
                if(supported)
                {
                    Versions[i] = 1;
                }
                else
                {
                    Versions[i] = 0;
                }
            }
        }

        public void Add(int i)
        {
            Versions[i]++;
        }

        public int Get_max()
        {
            int max = 0;

            for (int i = 0; i < Versions.Length; i++)
            {
                if (Versions[i] > max)
                {
                    max = Versions[i];
                }
            }

            return max;
        }

        public int Get_level(int i)
        {
            return Versions[i];
        }
    }
}
