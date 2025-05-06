namespace Minecraft_common
{
    public class Version_range
    {
        private readonly int[] versions;

        public static Version_range All()
        {
            Version_range result = new();
            result.Set(0, Versions.Max_own, true);
            return result;
        }

        public Version_range()
        {
            versions = new int[Versions.Max_own + 1];
        }

        public Version_range(bool legacy)
        {
            versions = new int[Versions.Max_own + 1];

            if (legacy)
            {
                Set(Versions.Get_own_version("1.13"), Versions.Get_own_version("1.20.6"));
            }
            else
            {
                Set(Versions.Get_own_version("1.21"), Versions.Max_own);
            }
        }
        public Version_range(Version_range to_copy)
        {
            versions = new int[Versions.Max_own + 1];

            for(int i = 0; i < versions.Length; i++)
            {
                versions[i] = to_copy.Get_level(i);
            }
        }

        public Version_range(int min_inclusive, int max_inclusive, bool supported = true)
        {
            versions = new int[Versions.Max_own + 1];
            Set(min_inclusive, max_inclusive, supported);
        }

        public Version_range(string min_inclusive, string max_inclusive, bool supported = true)
        {
            versions = new int[Versions.Max_own + 1];
            Set(Versions.Get_own_version(min_inclusive), Versions.Get_own_version(max_inclusive), supported);
        }

        public bool Is_set(int i)
        {
            return versions[i] > 0;
        }

        public Version_range Get_common(Version_range external)
        {
            Version_range result = new(this);

            for (int i = 0; i < versions.Length; i++)
            {
                if(!external.Is_set(i))
                {
                    result.Unset(i);
                }
            }

            return result;
        }


        /// <summary>
        /// All will display the weights as well
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        public string ToString(bool all = true)
        {
            string to_returns = "";

            List<Tuple<string, int>> ranges = Get_ranges();

            foreach(Tuple<string,int> range in ranges)
            {
                if(all)
                {
                    to_returns += range.Item1 + ": " + range.Item2 + "\n";
                }
                else
                {
                    if(range.Item2 > 0)
                    {
                        to_returns += range.Item1 + "\n";
                    }
                }
            }

            //Removing the last \n
            if(to_returns.Length > 0)
            {
                to_returns = to_returns[..^1];
            }

            return to_returns;
        }

        public List<Tuple<string, int>> Get_ranges()
        {
            List<Tuple<string, int>> ranges = new();

            int version_index = 0;
            int score = versions[0];

            for (int i = 0; i < versions.Length; i++)
            {
                if (versions[i] != score)
                {
                    ranges.Add(new Tuple<string, int>(Versions.Get_own_version(version_index) + "-" + Versions.Get_own_version(i - 1), score));
                    version_index = i;

                    score = versions[i];
                }
            }

            ranges.Add(new Tuple<string, int>(Versions.Get_own_version(version_index) + "-" + Versions.Get_own_version(versions.Length - 1), versions[version_index]));
            return ranges;
        }

        public ConsoleColor Get_range_color(int score)
        {
            int max_score = Get_max();

            if (score == max_score)
            {
                return ConsoleColor.DarkGreen;
            }
            else if (score > max_score / 2)
            {
                return ConsoleColor.Yellow;
            }
            else if (score > max_score / 4)
            {
                return ConsoleColor.Red;
            }
            else
            {
                return ConsoleColor.DarkRed;
            }
        }

        //public string ToString(int limit)
        //{
        //    string result = "";

        //    int start = -1;

        //    for (int i = 0; i < versions.Length; i++)
        //    {
        //        if (versions[i] >= limit)
        //        {
        //            if (start == -1)
        //            {
        //                start = i;
        //            }
        //        }
        //        else if (start != -1)
        //        {
        //            result += Versions.Get_own_version(start) + "-" + Versions.Get_own_version(i - 1);
        //            start = -1;
        //        }
        //    }

        //    if (start != -1)
        //    {
        //        result += Versions.Get_own_version(start) + "-" + Versions.Get_own_version(versions.Length - 1);
        //    }

        //    return result;
        //}

        public void Unset()
        {
            for (int i = 0; i < versions.Length; i++)
            {
                versions[i] = 0;
            }
        }
        public void Unset(Version_range unset)
        {
            for (int i = 0; i < versions.Length; i++)
            {
                if(unset.Is_set(i))
                {
                    versions[i] = 0;
                }
            }
        }

        public void Set(Version_range unset)
        {
            for (int i = 0; i < versions.Length; i++)
            {
                if (unset.Is_set(i))
                {
                    versions[i] = 1;
                }
            }
        }

        public void Unset(int i)
        {
            versions[i] = 0;
        }

        public void Set(int i)
        {
            versions[i] = 1;
        }

        public void Set(int min_inclusive, int max_inclusive, bool supported = true)
        {
            for (int i = min_inclusive; i <= max_inclusive; i++)
            {
                if (supported)
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
            Set(0, min_inclusive - 1, supported);
            Set(max_inclusive + 1, Versions.Max_own, supported);
        }

        public void Add(int i)
        {
            versions[i]++;
        }

        public void Add(Version_range to_add)
        {
            for(int i = 0; i <= Versions.Max_own; i++)
            {
                versions[i] += to_add.Get_level(i);
            }
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

        public bool Is_entire(bool expected_state)
        {
            if(expected_state)
            {
                return Is_entire(1);
            }
            else
            {
                return Is_entire(0);
            }
        }

        public bool Is_entire(int expected_state)
        {
            for(int i = 0; i < versions.Length; i++)
            {
                if (versions[i] != expected_state)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Try_parse(string input, out Version_range range)
        {
            input = input.ToLower();

            if(input == "all")
            {
                range = All();
                return true; 
            }

            if (input == "none")
            {
                range = new();
                return true;
            }

            if (input.Contains(','))
            {
                range = new();

                string[] minecraft_Versions = input.Split(',');

                foreach(string minecraft_version in minecraft_Versions)
                {
                    int own_version = Versions.Get_own_version(minecraft_version);

                    if(own_version == -1)
                    {
                        range = null;
                        return false;
                    }

                    range.Set(own_version);
                }

                return true;
            }

            if(input.Contains('-'))
            {
                string[] start_stop = input.Split('-');

                if(start_stop.Length != 2)
                {
                    range = null;
                    return false;
                }

                int start_own_version = Versions.Get_own_version(start_stop[0]);
                int stop_own_version = Versions.Get_own_version(start_stop[1]);

                if(start_own_version == -1 || stop_own_version == -1)
                {
                    range = null;
                    return false;
                }

                range = new();

                for (int i = start_own_version; i <= stop_own_version; i++)
                {
                    range.Set(i);
                }

                return true;
            }

            int single_own_version = Versions.Get_own_version(input);

            if(single_own_version == -1)
            {
                range = null;
                return false;
            }

            range = new();
            range.Set(single_own_version);

            return true;
        }
    }
}
