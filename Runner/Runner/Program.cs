using Datapack;

namespace Runner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Please input the datapack for processing: ");

                string location = Console.ReadLine();

                //Windows puts "" around path with spaces
                location = location.Replace("\"", "");

                Detector current;

                //Was a directory that isn't a datapack provided?
                if (!File.Exists(location) && Directory.Exists(location) && !File.Exists(location + "/pack.mcmeta"))
                {
                    Console.WriteLine("Folder with no pack.mcmeta, assuming every pack in folder");

                    string[] files = Directory.GetFiles(location);

                    foreach (string file in files)
                    {
                        current = new (file);
                    }
                    return;
                }

                _ = new Detector(location);
            }
        }
    }
}
