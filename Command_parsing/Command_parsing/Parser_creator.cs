using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Command_parsing
{
    public class Parser_creator
    {
        public static void Create_1_13(Command_parser parser)
        {
            parser.Add_rule(new Command_model(new Command_name("advancement"), new Command_choice(new Command_choice_part("grant", new Command_entity(), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text()), new Command_choice_part("only", new Command_text()), new Command_choice_part("through", new Command_text()))), new Command_choice_part("revoke", new Command_entity(), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text()), new Command_choice_part("only", new Command_text()), new Command_choice_part("through", new Command_text()))))));
            parser.Add_rule(new Command_model(new Command_name("bossbar"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text()), new Command_choice_part("get", new Command_choice(new string[] { "max", "players", "value", "visible" })), new Command_choice_part("list"), new Command_choice_part("remove", new Command_text()), new Command_choice_part("set", new Command_text()), new Command_choice_part("color"), new Command_choice_part("max", new Command_int()), new Command_choice_part("players", new Command_entity()), new Command_choice_part("style", new Command_choice(new string[] { "notched_10", "notched_12", "notched_20", "notched_6", "progress" })), new Command_choice_part("value", new Command_int()), new Command_choice_part("visible", new Command_bool()))));
            parser.Add_rule(new Command_model(new Command_name("clear"), new Command_entity(), new Command_text("ITEMS_TAGS", true)));  //TODO tags as well
            parser.Add_rule(new Command_model(new Command_name("clone"), new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new Command_choice_part("filtered", new Command_text("ITEMS_TAGS")), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" })), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" })))));
            parser.Add_rule(new Command_model(new Command_name("data"), new Command_choice(new Command_choice_part("get", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt_path), new Command_int(true)), new Command_choice_part("entity", new Command_entity(), new Command_text(Validate_nbt_path), new Command_int(true)))), new Command_choice_part("merge", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt)), new Command_choice_part("entity", new Command_entity(), new Command_text(Validate_nbt)))), new Command_choice_part("remove", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt)), new Command_choice_part("entity", new Command_entity(), new Command_text(Validate_nbt)))))));
            parser.Add_rule(new Command_model(new Command_name("datapack"), new Command_choice(new Command_choice_part("disable", new Command_text()), new Command_choice_part("enable", new Command_text()), new Command_choice_part("list"))));
            parser.Add_rule(new Command_model(new Command_name("debug"), new Command_choice(new string[] { "start", "stop" })));
            parser.Add_rule(new Command_model(new Command_name("defaultgamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" })));
            parser.Add_rule(new Command_model(new Command_name("difficulty"), new Command_choice(new string[] { "easy", "hard", "normal", "peaceful" })));
            parser.Add_rule(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(), new Command_text(true)), new Command_choice_part("give", new Command_entity(), new Command_text(), new Command_int(true), new Command_int(true), new Command_bool(true)))));
            parser.Add_rule(new Command_model(new Command_name("enchant"), new Command_entity(), new Command_text(), new Command_int(true)));

            Command_choice_part unless_row = new("if", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));
            Command_choice_part if_row = new("unless", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));


            parser.Add_rule(new Command_model(new Command_name("execute"), new Command_execute_setting(new Command_choice_part("align", new Command_text("ALIGNMENTS"), new Command_execute_stop()), new Command_choice_part("anchored", new Command_choice(new string[] { "eyes", "feet" }), new Command_execute_stop()), new Command_choice_part("as", new Command_entity(), new Command_execute_stop()), new Command_choice_part("at", new Command_entity(), new Command_execute_stop()), new Command_choice_part("facing", new Command_choice(new Command_choice_part(new Command_pos(/*TODO this either the pos or the part with the name*/), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_choice(new string[] { "eyes", "feet" }), new Command_execute_stop()))), new Command_choice_part("in", new Command_text("DIMENSIONS"), new Command_execute_stop()), new Command_choice_part("positioned", new Command_choice(new Command_choice_part(new Command_pos(), new Command_execute_stop()), new Command_choice_part("as", new Command_entity(), new Command_execute_stop()))), new Command_choice_part("rotated", new Command_choice(new string[] { /*TODO TWO coords as well*/ "as" }), new Command_entity(), new Command_execute_stop()), new Command_choice_part("store", new Command_choice(new string[] { "result", "success" }), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("bossbar", new Command_text(), new Command_choice(new string[] { "max", "value" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_text(Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_execute_stop()))), unless_row, if_row)));


            parser.Add_rule(new Command_model(new Command_name("experience"), new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("set", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("query", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })))));
            parser.Add_rule(new Command_model(new Command_name("fill"), new Command_pos(), new Command_pos(), new Command_text(Validate_blocks_data_nbt), new Command_choice(true,new Command_choice_part("hollow"), new Command_choice_part("destroy"), new Command_choice_part("keep"), new Command_choice_part("outline"), new Command_choice_part("replace", new Command_text(Validate_blocks_tags_data_nbt))  )));
            parser.Add_rule(new Command_model(new Command_name("function"), new Command_text()));

            parser.Add_rule(new Command_model(new Command_name("gamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" }), new Command_entity(true)));
            parser.Add_rule(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)))));
            parser.Add_rule(new Command_model(new Command_name("give"), new Command_entity(), new Command_text(Validate_items_nbt), new Command_int(true)));
            parser.Add_rule(new Command_model(new Command_name("help"), new Command_text()));
            parser.Add_rule(new Command_model(new Command_name("kick"), new Command_text_end(), new Command_text()));
            parser.Add_rule(new Command_model(new Command_name("kill"), new Command_entity()));
            parser.Add_rule(new Command_model(new Command_name("list"), new Command_choice(new string[] { "uuids" }, true)));
            parser.Add_rule(new Command_model(new Command_name("locate"), new Command_text("STRUCTURES")));
            parser.Add_rule(new Command_model(new Command_name("me"), new Command_text_end()));
            parser.Add_rule(new Command_model(new Command_name("msg"), new Command_entity(), new Command_text_end()));
            parser.Add_rule(new Command_model(new Command_name("particle"), new Command_text("PARTICLES"), new Command_pos(true), new Command_pos(true), new Command_int(true), new Command_int(true), new Command_choice(new string[] { "force", "normal" }, true), new Command_entity(true)));

            //TODO Parts without anything action will have to continue root action
            parser.Add_rule(new Command_model(new Command_name("playsound"), new Command_text("SOUNDS"), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }), new Command_entity(), new Command_pos(true), new Command_int(true), new Command_float(true), new Command_float(true)));
            parser.Add_rule(new Command_model(new Command_name("publish"), new Command_int()));
            parser.Add_rule(new Command_model(new Command_name("recipe"), new Command_choice(new Command_choice_part("give", new Command_entity(), new Command_text("RECIPES")), new Command_choice_part("take", new Command_entity(), new Command_text("RECIPES")))));
            parser.Add_rule(new Command_model(new Command_name("reload")));
            parser.Add_rule(new Command_model(new Command_name("replaceitem"), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text(Validate_items_nbt), new Command_int(false)), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), new Command_text(Validate_items_nbt), new Command_int(false)))));
            parser.Add_rule(new Command_model(new Command_name("say"), new Command_text_end()));

            //TODO two last are optional in pair
            parser.Add_rule(new Command_model(new Command_name("scoreboard"), new Command_choice(new Command_choice_part("objectives", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text("SCOREBOARD_CRITERIA"), new Command_str_text()), new Command_choice_part("list"), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("displayname", new Command_str_text()), new Command_choice_part("rendertype", new Command_choice(new string[] { "hearts", "integer" })))), new Command_choice_part("remove", new Command_text()), new Command_choice_part("setdisplay", new Command_text("SCOREBOARD_DISPLAYS"), new Command_text()))), new Command_choice_part("players", new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("enable", new Command_entity(), new Command_text()), new Command_choice_part("get", new Command_entity(), new Command_text()), new Command_choice_part("list", new Command_entity()), new Command_choice_part("operation", new Command_entity(), new Command_text(), new Command_choice(new string[] { "%=", "*=", "+=", "-=", "/=", "<", "=", ">", "><" }), new Command_entity(), new Command_text()), new Command_choice_part("remove", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("reset", new Command_entity(), new Command_text()), new Command_choice_part("set", new Command_entity(), new Command_text(), new Command_int()))))));
            parser.Add_rule(new Command_model(new Command_name("seed")));
            parser.Add_rule(new Command_model(new Command_name("setblock"), new Command_pos(), new Command_text(Validate_blocks_data_nbt), new Command_choice(new string[] { "replace","keep","destroy"})));
            parser.Add_rule(new Command_model(new Command_name("setworldspawn"), new Command_pos()));
            parser.Add_rule(new Command_model(new Command_name("spawnpoint"), new Command_entity(), new Command_pos()));
            parser.Add_rule(new Command_model(new Command_name("stopsound"), new Command_entity(), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }, true), new Command_text("SOUNDS", true)));
            parser.Add_rule(new Command_model(new Command_name("summon"), new Command_text("ENTITIES"), new Command_pos(true), new Command_text(Validate_nbt, true)));
            parser.Add_rule(new Command_model(new Command_name("tag"), new Command_entity(), new Command_choice(new Command_choice_part("add", new Command_text()), new Command_choice_part("remove", new Command_text()), new Command_choice_part("list"))));
            parser.Add_rule(new Command_model(new Command_name("team"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_str_text()), new Command_choice_part("empty", new Command_text()), new Command_choice_part("join", new Command_text(), new Command_entity()), new Command_choice_part("leave", new Command_entity()), new Command_choice_part("list", new Command_text()), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("collisionRule", new Command_bool()), new Command_choice_part("color", new Command_text("TEAM_COLORS")), new Command_choice_part("deathMessageVisibility", new Command_bool()), new Command_choice_part("displayName", new Command_str_text()), new Command_choice_part("friendlyFire", new Command_bool()), new Command_choice_part("nametagVisible", new Command_bool()), new Command_choice_part("prefix", new Command_str_text()), new Command_choice_part("seeFriendlyInvisibles", new Command_bool()), new Command_choice_part("suffix", new Command_str_text()))), new Command_choice_part("remove", new Command_text()))));
            parser.Add_rule(new Command_model(new Command_name("teleport"), new Command_choice(new Command_choice_part(new Command_entity(), new Command_pos()), new Command_choice_part(new Command_pos()))));
            parser.Add_rule(new Command_model(new Command_name("tell"), new Command_entity(), new Command_text()));
            parser.Add_rule(new Command_model(new Command_name("tellraw"), new Command_entity(), new Command_str_text()));
            parser.Add_rule(new Command_model(new Command_name("time"), new Command_choice(new Command_choice_part("add", new Command_int()), new Command_choice_part("query", new Command_choice(new string[] { "day", "daytime", "gametime" })), new Command_choice_part("set", new Command_choice(new string[] { "day", "midnight", "night", "noon" })))));
            parser.Add_rule(new Command_model(new Command_name("title"), new Command_entity(), new Command_choice(new Command_choice_part("actionbar", new Command_str_text()), new Command_choice_part("clear"), new Command_choice_part("reset"), new Command_choice_part("subtitle", new Command_str_text()), new Command_choice_part("times", new Command_int(), new Command_int(), new Command_int()), new Command_choice_part("title", new Command_str_text()))));
            parser.Add_rule(new Command_model(new Command_name("trigger"), new Command_text(), new Command_choice(new string[] { "add", "set" }), new Command_int()));
            parser.Add_rule(new Command_model(new Command_name("weather"), new Command_text(), new Command_choice(new string[] { "clear", "rain", "thunder" }), new Command_int()));
            parser.Add_rule(new Command_model(new Command_name("worldborder"), new Command_choice(new Command_choice_part("add", new Command_int(), new Command_int()), new Command_choice_part("center", new Command_pos()), new Command_choice_part("damage", new Command_choice(new string[] { "amount", "buffer" }), new Command_int()), new Command_choice_part("get"), new Command_choice_part("set", new Command_int(), new Command_int()), new Command_choice_part("warning", new Command_choice(new string[] { "distance", "time" }), new Command_int()))));
            parser.Add_rule(new Command_model(new Command_name("experience"), new Command_choice(new string[] { "add", "query", "set" }), new Command_entity(), new Command_int(), new Command_choice(new string[] { "points", "levels" })));

            //Aliases after commands
            parser.Add_alias("xp", "experience");
            parser.Add_alias("tp", "teleport");
            parser.Add_alias("w", "msg");

            string block_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Block/1_13b.json";
            string item_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Item/1_13i.json";
            string entity_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Entity/1_13e.json";
            string sound_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Sound/1_13s.json";

            parser.Add_collection("BLOCKS", Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(block_path))));
            parser.Add_collection("ITEMS", Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(item_path))));

            parser.Add_collection("BLOCKS_TAGS", Add_tags(Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(block_path))), "minecraft",new string[] { "acacia_logs", "anvil", "banners", "birch_logs", "buttons", "carpets", "coral_blocks", "corals", "dark_oak_logs", "doors", "enderman_holdable", "flower_pots", "ice", "impermeable", "jungle_logs", "leaves", "logs", "oak_logs", "planks", "rails", "sand", "saplings", "slabs", "spruce_logs", "stairs", "stone_bricks", "trapdoors", "valid_spawn", "wall_corals", "wooden_buttons", "wooden_doors", "wooden_pressure_plates", "wooden_slabs", "wooden_stairs", "wooden_trapdoors", "wool" }));
            parser.Add_collection("ITEMS_TAGS", Add_tags(Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(item_path))), "minecraft",new string[] { "acacia_logs", "anvil", "banners", "birch_logs", "boats", "buttons", "carpets", "dark_oak_logs", "doors", "fishes", "jungle_logs", "leaves", "logs", "oak_logs", "planks", "rails", "sand", "saplings", "slabs", "spruce_logs", "stairs", "stone_bricks", "trapdoors", "wooden_buttons", "wooden_doors", "wooden_pressure_plates", "wooden_slabs", "wooden_stairs", "wooden_trapdoors", "wool" }));

            parser.Add_collection("DIMENSIONS", new List<string> { "the_nether", "the_end","overworld" });

            parser.Add_collection("ENTITIES", Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(entity_path))));
            parser.Add_collection("SOUNDS", Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(sound_path))));

            void Validate_blocks_data_nbt(Command_parser parser, string value)
            {
                Seperate(value, out string block, out string block_state, out string nbt_data);
                parser.Verify_collection("BLOCKS", block);
            }


            void Validate_blocks_tags_data_nbt(Command_parser parser, string value)
            {
                Seperate(value, out string block, out string block_state, out string nbt_data);
                parser.Verify_collection("BLOCKS_TAGS", block);
            }

            void Validate_nbt_path(Command_parser parser, string value)
            {

            }

            void Validate_nbt(Command_parser parser, string value)
            {

            }

            void Validate_items_nbt(Command_parser parser, string value)
            {
                int block_end_index = value.IndexOfAny(new char[] { '[', '{' });

                string item;

                if (block_end_index != -1)
                {
                    item = value[..block_end_index];
                }
                else
                {
                    item = value;
                }

                string nbt_data = "";
                int start_index = value.IndexOf('{');
                int end_index = value.LastIndexOf('}');

                if (start_index != -1 && end_index != -1 && end_index > start_index)
                {
                    nbt_data = value.Substring(start_index, end_index - start_index + 1);
                }

                parser.Verify_collection("ITEMS", item);
            }

            void Seperate(string text, out string block, out string block_state, out string nbt_data)
            {
                int block_end_index = text.IndexOfAny(new char[] { '[', '{' });

                if (block_end_index != -1)
                {
                    block = text[..block_end_index];
                }
                else
                {
                    block = text;
                }

                block_state = "";
                int start_index = text.IndexOf('[');
                int end_index = text.LastIndexOf(']');

                if (start_index != -1 && end_index != -1 && end_index > start_index)
                {
                    block_state = text.Substring(start_index, end_index - start_index + 1);
                }


                nbt_data = "";
                start_index = text.IndexOf('{');
                end_index = text.LastIndexOf('}');

                if (start_index != -1 && end_index != -1 && end_index > start_index)
                {
                    nbt_data = text.Substring(start_index, end_index - start_index + 1);
                }
            }
        }

        private static List<string> Patch_minecraft(List<string> all_data)
        {
            int original_length = all_data.Count;

            for(int i = 0; i < original_length; i++)
            {
                all_data.Add("minecraft:" + all_data[i]);
            }

            return all_data;
        }

        private static List<string> Add_tags(List<string> all_data,string prefix ,string[] tags)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                //No prefix would be minecraft
                if(prefix == "minecraft")
                {
                    all_data.Add("#" + tags[i]);
                }
                all_data.Add("#" + prefix +":" +tags[i]);
            }

            return all_data;
        }
    }
}
