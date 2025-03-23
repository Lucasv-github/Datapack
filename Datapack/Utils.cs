using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datapack
{
    public class Utils
    {
        public static void Copy(string source_directory, string target_directory)
        {
            DirectoryInfo source = new(source_directory);
            DirectoryInfo target = new(target_directory);

            Copy_all(source, target);
        }

        public static void Copy_all(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            //Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            //Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                Copy_all(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}
