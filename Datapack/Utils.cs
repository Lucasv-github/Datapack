using System.IO.Compression;

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

        //And for some reason ZipFile.ExtractToDirectory on linux will keep the permissions somehow, thus somehow we can extract a zip that manually even on linux extracts normally but the result without this own implementation would be unreadable files
        public static void Extract_zip(string zip_path, string result_path)
        {
            using ZipArchive archive = ZipFile.OpenRead(zip_path);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                //Reftify situation on linux
                entry.ExternalAttributes = 644;

                string entry_path = result_path + "/" + entry.FullName;

                if (Is_directory(entry))
                {
                    if (!Directory.Exists(entry_path))
                    {
                        Directory.CreateDirectory(entry_path);
                    }
                }
                else
                {
                    //For some reason the files might be before the folders
                    //I assume the folders could be left out entirely as well
                    string pre_path = Path.GetDirectoryName(entry_path);

                    if (!Directory.Exists(pre_path))
                    {
                        Directory.CreateDirectory(pre_path);
                    }

                    entry.ExtractToFile(entry_path);
                }
            }

            static bool Is_directory(ZipArchiveEntry entry)
            {
                return entry.Name == "" || entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\');
            }
        }
    }
}
