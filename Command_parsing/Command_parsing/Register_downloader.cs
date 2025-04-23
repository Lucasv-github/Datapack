namespace Command_parsing
{
    public class Register_downloader
    {
        public static void Download_to_directory(string minecraft_version, string result_path)
        {
            Console.WriteLine("Downloading registers for: " + minecraft_version + " to path: " + result_path);

            string temp_folder = AppDomain.CurrentDomain.BaseDirectory + "/Temp";
            string download_link = "https://github.com/misode/mcmeta/archive/refs/tags/" + minecraft_version + "-registries.zip";

            string zip_location = temp_folder + "/" + minecraft_version + ".zip";

            Task downloader = Download_file_async(download_link, zip_location);
            downloader.Wait();

            string extract_location = temp_folder + "/" + minecraft_version;

            //Want to remove everything old in folder
            if (Directory.Exists(extract_location))
            {
                Console.WriteLine("Removing: " + extract_location);
                Directory.Delete(extract_location, true);
            }

            System.IO.Compression.ZipFile.ExtractToDirectory(zip_location, extract_location);

            string all_data_path = Directory.GetDirectories(extract_location)[0];

            //foreach(string path in Directory.GetDirectories(all_data_path))
            //{
            //    Console.WriteLine(path);
            //}

            Directory.CreateDirectory(result_path);

            string regular_result = result_path + "/Regular/";
            Directory.CreateDirectory(regular_result);

            string tag_result = result_path + "/Regular/Tags/";
            Directory.CreateDirectory(tag_result);

            //Blocking redownload, allowing us to add folders that don't block it
            File.Create(result_path + "/Info.txt").Close();

            Move_out("/advancement/data.json", "/advancement.json");
            Move_out("/attribute/data.json", "/attribute.json");
            Move_out("/worldgen/biome/data.json", "/biome.json");
            Move_out("/block/data.json", "/block.json");
            Move_out("/damage_type/data.json", "/damage.json");
            Move_out("/dimension_type/data.json", "/dimension.json");
            Move_out("/mob_effect/data.json", "/effect.json");
            Move_out("/enchantment/data.json", "/enchantment.json");
            Move_out("/entity_type/data.json", "/entity.json");
            Move_out("/worldgen/configured_feature/data.json", "/feature.json");
            Move_out("/item/data.json", "/item.json");
            Move_out("/worldgen/template_pool/data.json", "/jigsaw.json");
            Move_out("/loot_table/data.json", "/loot_table.json");
            Move_out("/particle_type/data.json", "/particle.json");
            Move_out("/point_of_interest_type/data.json", "/poi.json");
            Move_out("/recipe/data.json", "/recipe.json");
            Move_out("/sound_event/data.json", "/sound.json");
            Move_out("/structure/data.json", "/template.json");

            //First this
            Move_out("/structure_feature/data.json", "/structure.json", Register_type.Other);

            //Replaced by this
            Move_out("/worldgen/structure_feature/data.json", "/structure.json");

            //Then by this
            Move_out("/worldgen/structure/data.json", "/structure.json");

            Move_out("/tag/block/data.json", "/block.json", Register_type.Tag);
            Move_out("/tag/item/data.json", "/item.json", Register_type.Tag);
            Move_out("/tag/entity_type/data.json", "/entity.json", Register_type.Tag);
            Move_out("/tag/worldgen/biome/data.json", "/biome.json", Register_type.Tag);


            Move_out("/tag/worldgen/configured_structure_feature/data.json", "/structure.json", Register_type.Tag);
            Move_out("/tag/worldgen/structure/data.json", "/structure.json", Register_type.Tag);

            //TODO will need to add rest
            //Move_out("/custom_stat/data.json", "/scoreboard_criteria.json", Register_type.Other);

            Console.WriteLine("Downloading done for: " + minecraft_version);

            void Move_out(string source_sub, string destination_name, Register_type type = Register_type.Regular)
            {
                if (!File.Exists(all_data_path + source_sub))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Sub: " + source_sub + " not present in version: " + minecraft_version);
                    Console.ResetColor();
                    return;
                }

                switch (type)
                {
                    case Register_type.Regular:
                        File.Copy(all_data_path + source_sub, regular_result + destination_name,true);
                        break;

                    case Register_type.Tag:
                        File.Copy(all_data_path + source_sub, tag_result + destination_name, true);
                        break;

                    case Register_type.Other:
                        File.Copy(all_data_path + source_sub, result_path + destination_name, true);
                        break;

                    default:
                        break;
                }
            }
        }

        private static async Task Download_file_async(string url, string path)
        {
            try
            {
                using HttpClient client = new();
                using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fileStream);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error downloading changes: " + ex);
                Console.ResetColor();
            }
        }
    }

    enum Register_type
    {
        Regular = 0,
        Tag = 1,
        Other = 2,
    }
}
