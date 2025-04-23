namespace Minecraft_common
{
    public class Versions
    {
        //TODO these should at the very least be tested, perhaps auto generated from a single list as well

        public const int Min_own = 0;
        public const int Max_own = 39;

        public const string Min_minecraft = "1.13";
        public const string Max_minecraft = "1.21.5";

        public const int Min_minecraft_numeric = 4;
        public const int Max_minecraft_numeric = 71;

        private static readonly Dictionary<int, string> numeric_to_minecraft = new()
        {
            { 4, "1.13-1.14.4" },
            { 5, "1.15-1.16.1" },
            { 6, "1.16.2-1.16.5" },
            { 7, "1.17-1.17.1" },
            { 8, "1.18-1.18.1" },
            { 9, "1.18.2-1.18.2" },
            { 10, "1.19-1.19.3" },
            { 12, "1.19.4-1.19.4" },
            { 15, "1.20-1.20.1" },
            { 18, "1.20.2-1.20.2" },
            { 26, "1.20.3-1.20.4" },
            { 41, "1.20.5-1.20.6" },
            { 48, "1.21-1.21.1" },
            { 57, "1.21.2-1.21.3" },
            { 61, "1.21.4-1.21.4" },
            { 71, "1.21.5-1.21.5" }
    };


        public static string Get_minecraft_version(int numerical_version)
        {
            return Get_minecraft_version(numerical_version, out _);
        }

        public static string Get_minecraft_version(int numerical_version, out bool known, bool leaniant_next_higher = false)
        {
            if(numeric_to_minecraft.ContainsKey(numerical_version))
            {
                known = true;
                return numeric_to_minecraft[numerical_version];
            }

            if(leaniant_next_higher)
            {
                for(int i = numerical_version; i <= Max_minecraft_numeric; i++)
                {
                    if(numeric_to_minecraft.ContainsKey(i))
                    {
                        known = true;
                        return numeric_to_minecraft[i];
                    }
                }
            }

            known = false;
            return "Unknown version";

        }

        //Using own versions numbers as mincraft's isn't granular enough
        public static string Get_own_version(int number)
        {
            return number switch
            {
                0 => "1.13",
                1 => "1.13.1",
                2 => "1.13.2",
                3 => "1.14",
                4 => "1.14.1",
                5 => "1.14.2",
                6 => "1.14.3",
                7 => "1.14.4",
                8 => "1.15",
                9 => "1.15.1",
                10 => "1.15.2",
                11 => "1.16",
                12 => "1.16.1",
                13 => "1.16.2",
                14 => "1.16.3",
                15 => "1.16.4",
                16 => "1.16.5",
                17 => "1.17",
                18 => "1.17.1",
                19 => "1.18",
                20 => "1.18.1",
                21 => "1.18.2",
                22 => "1.19",
                23 => "1.19.1",
                24 => "1.19.2",
                25 => "1.19.3",
                26 => "1.19.4",
                27 => "1.20",
                28 => "1.20.1",
                29 => "1.20.2",
                30 => "1.20.3",
                31 => "1.20.4",
                32 => "1.20.5",
                33 => "1.20.6",
                34 => "1.21",
                35 => "1.21.1",
                36 => "1.21.2",
                37 => "1.21.3",
                38 => "1.21.4",
                39 => "1.21.5",
                _ => null,
            };
        }

        public static int Get_own_version(string version)
        {
            return version switch
            {
                "1.13" => 0,
                "1.13.1" => 1,
                "1.13.2" => 2,
                "1.14" => 3,
                "1.14.1" => 4,
                "1.14.2" => 5,
                "1.14.3" => 6,
                "1.14.4" => 7,
                "1.15" => 8,
                "1.15.1" => 9,
                "1.15.2" => 10,
                "1.16" => 11,
                "1.16.1" => 12,
                "1.16.2" => 13,
                "1.16.3" => 14,
                "1.16.4" => 15,
                "1.16.5" => 16,
                "1.17" => 17,
                "1.17.1" => 18,
                "1.18" => 19,
                "1.18.1" => 20,
                "1.18.2" => 21,
                "1.19" => 22,
                "1.19.1" => 23,
                "1.19.2" => 24,
                "1.19.3" => 25,
                "1.19.4" => 26,
                "1.20" => 27,
                "1.20.1" => 28,
                "1.20.2" => 29,
                "1.20.3" => 30,
                "1.20.4" => 31,
                "1.20.5" => 32,
                "1.20.6" => 33,
                "1.21" => 34,
                "1.21.1" => 35,
                "1.21.2" => 36,
                "1.21.3" => 37,
                "1.21.4" => 38,
                "1.21.5" => 39,
                _ => -1,
            };
        }

        public static string Get_min_minecraft_version(int numerical_version, bool leaniant_next_higher = false)
        {
            string version = Get_minecraft_version(numerical_version, out bool known, leaniant_next_higher);

            if (!known)
            {
                return "Unknown version";
            }

            return version.Split('-')[0];
        }

        public static string Get_max_minecraft_version(int numerical_version, bool leaniant_next_higher = false)
        {
            string version = Get_minecraft_version(numerical_version, out bool known, leaniant_next_higher);

            if (!known)
            {
                return "Unknown version";
            }

            return version.Split('-')[1];
        }
    }
}
