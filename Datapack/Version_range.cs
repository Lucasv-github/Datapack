using Command_parsing;

namespace Datapack
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

        public Version_range(int min_inclusive, int max_inclusive, bool supported = true)
        {
            versions = new int[Versions.Max_own + 1];
            Set(min_inclusive, max_inclusive, supported);
        }

        public bool Is_set(int i)
        {
            return versions[i] > 0;
        }

        public void Write(Action<string, ConsoleColor> output)
        {
            Write(1, output);
        }

        public void Write_scores(Action<string, ConsoleColor> output)
        {
            int max_score = Get_max();

            int start = 0;
            int score = versions[0];

            for (int i = 0; i < versions.Length; i++)
            {
                if (versions[i] != score)
                {
                    Color();
                    output.Invoke(Versions.Get_own_version(start) + "-" + Versions.Get_own_version(i - 1) + ": " + score + "\n", Console.ForegroundColor);
                    start = i;

                    score = versions[i];
                }
            }

            Color();
            output.Invoke(Versions.Get_own_version(start) + "-" + Versions.Get_own_version(versions.Length - 1) + ": " + versions[start] + "\n", Console.ForegroundColor);

            void Color()
            {
                if (score == max_score)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else if (score > max_score / 2)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (score > max_score / 4)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }
            }

            Console.ResetColor();
        }

        public void Write(int limit, Action<string, ConsoleColor> output)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            int start = -1;

            for (int i = 0; i < versions.Length; i++)
            {
                if (versions[i] >= limit)
                {
                    if (start == -1)
                    {
                        start = i;
                    }
                }
                else if (start != -1)
                {
                    output.Invoke(Versions.Get_own_version(start) + "-" + Versions.Get_own_version(i - 1), Console.ForegroundColor);
                    start = -1;
                }
            }

            if (start != -1)
            {
                output.Invoke(Versions.Get_own_version(start) + "-" + Versions.Get_own_version(versions.Length - 1), Console.ForegroundColor);
            }

            Console.ResetColor();
        }

        public void Unset()
        {
            for (int i = 0; i < versions.Length; i++)
            {
                versions[i] = 0;
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

        public void Set(int min_inclusive, int max_inclusive, bool supported)
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
