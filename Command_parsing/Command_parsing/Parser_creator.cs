using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace Command_parsing
{
    public class Parser_creator
    {
        public static Command_parser Create_1_13()
        {
            string version = "1.13";
            string version_path = version.Replace(".", "_");

            Console.WriteLine("Creating parser: " + version);

            Command_parser parser = new(version, Validate_selector);

            parser.Add_rule(new Command_model(new Command_name("advancement"), new Command_choice(new Command_choice_part("grant", new Command_entity(false,false,true), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text("ADVANCEMENT")), new Command_choice_part("only", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")), new Command_choice_part("until", new Command_text("ADVANCEMENT")))), new Command_choice_part("revoke", new Command_entity(), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text("ADVANCEMENT")), new Command_choice_part("only", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")))))));
            parser.Add_rule(new Command_model(new Command_name("bossbar"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text()), new Command_choice_part("get", new Command_choice(new string[] { "max", "players", "value", "visible" })), new Command_choice_part("list"), new Command_choice_part("remove", new Command_text()), new Command_choice_part("set", new Command_text()), new Command_choice_part("color"), new Command_choice_part("max", new Command_int()), new Command_choice_part("players", new Command_entity()), new Command_choice_part("style", new Command_choice(new string[] { "notched_10", "notched_12", "notched_20", "notched_6", "progress" })), new Command_choice_part("value", new Command_int()), new Command_choice_part("visible", new Command_bool()))));
            parser.Add_rule(new Command_model(new Command_name("clear"), new Command_entity(false,false,true), new Command_text("ITEM_TAG", true)));  //TODO tags as well
            parser.Add_rule(new Command_model(new Command_name("clone"), new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new Command_choice_part("filtered", new Command_text("ITEM_TAG")), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" })), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" })))));
            parser.Add_rule(new Command_model(new Command_name("data"), new Command_choice(new Command_choice_part("get", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt_path), new Command_float(true)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Validate_nbt_path), new Command_float(true)))), new Command_choice_part("merge", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt)), new Command_choice_part("entity", new Command_entity(false,true,false), new Command_text(Validate_nbt)))), new Command_choice_part("remove", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text(Validate_nbt)))))));
            parser.Add_rule(new Command_model(new Command_name("datapack"), new Command_choice(new Command_choice_part("disable", new Command_text()), new Command_choice_part("enable", new Command_text()), new Command_choice_part("list"))));
            parser.Add_rule(new Command_model(new Command_name("debug"), new Command_choice(new string[] { "start", "stop" })));
            parser.Add_rule(new Command_model(new Command_name("defaultgamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" })));
            parser.Add_rule(new Command_model(new Command_name("difficulty"), new Command_choice(new string[] { "easy", "hard", "normal", "peaceful" })));
            parser.Add_rule(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(), new Command_text(true)), new Command_choice_part("give", new Command_entity(), new Command_text(), new Command_int(true), new Command_int(true), new Command_bool(true)))));
            parser.Add_rule(new Command_model(new Command_name("enchant"), new Command_entity(false,false,true), new Command_text("ENCHANTMENT"), new Command_int(true)));

            Command_choice_part unless_row = new("if", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));
            Command_choice_part if_row = new("unless", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_blocks_tags_data_nbt), new Command_execute_stop()), new Command_choice_part("blocks", new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new string[] { "all", "masked" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_choice(/*TODO range*/new Command_choice_part("matches", new Command_text(), new Command_execute_stop()), new Command_choice_part(new string[] { "<=", "<", "=", ">", ">=" }, new Command_entity(), new Command_text(), new Command_execute_stop())))));


            parser.Add_rule(new Command_model(new Command_name("execute"), new Command_execute_setting(new Command_choice_part("align", new Command_text("ALIGNMENT"), new Command_execute_stop()), new Command_choice_part("anchored", new Command_choice(new string[] { "eyes", "feet" }), new Command_execute_stop()), new Command_choice_part("as", new Command_entity(), new Command_execute_stop()), new Command_choice_part("at", new Command_entity(), new Command_execute_stop()), new Command_choice_part("facing", new Command_choice(new Command_choice_part(new Command_pos(/*TODO this either the pos or the part with the name*/), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(), new Command_choice(new string[] { "eyes", "feet" }), new Command_execute_stop()))), new Command_choice_part("in", new Command_text("DIMENSION"), new Command_execute_stop()), new Command_choice_part("positioned", new Command_choice(new Command_choice_part(new Command_pos(), new Command_execute_stop()), new Command_choice_part("as", new Command_entity(), new Command_execute_stop()))), new Command_choice_part("rotated", new Command_choice(new string[] { /*TODO TWO coords as well*/ "as" }), new Command_entity(), new Command_execute_stop()), new Command_choice_part("store", new Command_choice(new string[] { "result", "success" }), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text(Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("bossbar", new Command_text(), new Command_choice(new string[] { "max", "value" }), new Command_execute_stop()), new Command_choice_part("entity", new Command_entity(false,true,false), new Command_text(Validate_nbt_path), new Command_text("NBT_SIZE"), new Command_float(), new Command_execute_stop()), new Command_choice_part("score", new Command_entity(), new Command_text(), new Command_execute_stop()))), unless_row, if_row)));


            parser.Add_rule(new Command_model(new Command_name("experience"), new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("set", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("query", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })))));
            parser.Add_rule(new Command_model(new Command_name("fill"), new Command_pos(), new Command_pos(), new Command_text(Validate_blocks_data_nbt), new Command_choice(true,new Command_choice_part("hollow"), new Command_choice_part("destroy"), new Command_choice_part("keep"), new Command_choice_part("outline"), new Command_choice_part("replace", new Command_text(Validate_blocks_tags_data_nbt))  )));
            parser.Add_rule(new Command_model(new Command_name("function"), new Command_text()));

            parser.Add_rule(new Command_model(new Command_name("gamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" }), new Command_entity(true,false,true)));
            parser.Add_rule(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)))));
            parser.Add_rule(new Command_model(new Command_name("give"), new Command_entity(false,false,true), new Command_text(Validate_items_nbt), new Command_int(true)));
            parser.Add_rule(new Command_model(new Command_name("help"), new Command_text()));
            parser.Add_rule(new Command_model(new Command_name("kick"), new Command_text_end(), new Command_text()));
            parser.Add_rule(new Command_model(new Command_name("kill"), new Command_entity()));
            parser.Add_rule(new Command_model(new Command_name("list"), new Command_choice(new string[] { "uuids" }, true)));
            parser.Add_rule(new Command_model(new Command_name("locate"), new Command_text("STRUCTURE")));
            parser.Add_rule(new Command_model(new Command_name("me"), new Command_text_end()));
            parser.Add_rule(new Command_model(new Command_name("msg"), new Command_entity(false,false,true), new Command_text_end()));
            parser.Add_rule(new Command_model(new Command_name("particle"), new Command_text("PARTICLE"), new Command_pos(true), new Command_pos(true), new Command_float(true), new Command_int(true), new Command_choice(new string[] { "force", "normal" }, true), new Command_entity(true)));

            //TODO Parts without anything action will have to continue root action
            parser.Add_rule(new Command_model(new Command_name("playsound"), new Command_text("SOUND"), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }), new Command_entity(), new Command_pos(true), new Command_int(true), new Command_float(2,true), new Command_float(1,true)));
            parser.Add_rule(new Command_model(new Command_name("publish"), new Command_int(65535)));
            parser.Add_rule(new Command_model(new Command_name("recipe"), new Command_choice(new Command_choice_part("give", new Command_entity(false, false, true), new Command_text("RECIPE")), new Command_choice_part("take", new Command_entity(false,false,true), new Command_text("RECIPE")))));
            parser.Add_rule(new Command_model(new Command_name("reload")));
            parser.Add_rule(new Command_model(new Command_name("replaceitem"), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text(Validate_items_nbt), new Command_int(true)), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), new Command_text(Validate_items_nbt), new Command_int(true)))));
            parser.Add_rule(new Command_model(new Command_name("say"), new Command_text_end()));

            //TODO two last are optional in pair
            parser.Add_rule(new Command_model(new Command_name("scoreboard"), new Command_choice(new Command_choice_part("objectives", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text("SCOREBOARD_CRITERIA",true), new Command_str_text(true)), new Command_choice_part("list"), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("displayname", new Command_str_text()), new Command_choice_part("rendertype", new Command_choice(new string[] { "hearts", "integer" })))), new Command_choice_part("remove", new Command_text()), new Command_choice_part("setdisplay", new Command_text("SCOREBOARD_DISPLAY"), new Command_text()))), new Command_choice_part("players", new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("enable", new Command_entity(), new Command_text()), new Command_choice_part("get", new Command_entity(), new Command_text()), new Command_choice_part("list", new Command_entity()), new Command_choice_part("operation", new Command_entity(), new Command_text(), new Command_choice(new string[] { "%=", "*=", "+=", "-=", "/=", "<", "=", ">", "><" }), new Command_entity(), new Command_text()), new Command_choice_part("remove", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("reset", new Command_entity(), new Command_text()), new Command_choice_part("set", new Command_entity(), new Command_text(), new Command_int()))))));
            parser.Add_rule(new Command_model(new Command_name("seed")));
            parser.Add_rule(new Command_model(new Command_name("setblock"), new Command_pos(), new Command_text(Validate_blocks_data_nbt), new Command_choice(new string[] { "replace","keep","destroy"},true)));
            parser.Add_rule(new Command_model(new Command_name("setworldspawn"), new Command_pos()));
            parser.Add_rule(new Command_model(new Command_name("spawnpoint"), new Command_entity(false, false, true), new Command_pos()));
            parser.Add_rule(new Command_model(new Command_name("stopsound"), new Command_entity(false, false, true), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }, true), new Command_text("SOUND", true)));
            parser.Add_rule(new Command_model(new Command_name("summon"), new Command_text("ENTITY"), new Command_pos(true), new Command_text(Validate_nbt, true)));
            parser.Add_rule(new Command_model(new Command_name("tag"), new Command_entity(), new Command_choice(new Command_choice_part("add", new Command_text()), new Command_choice_part("remove", new Command_text()), new Command_choice_part("list"))));
            parser.Add_rule(new Command_model(new Command_name("team"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_str_text()), new Command_choice_part("empty", new Command_text()), new Command_choice_part("join", new Command_text(), new Command_entity()), new Command_choice_part("leave", new Command_entity()), new Command_choice_part("list", new Command_text()), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("collisionRule", new Command_bool()), new Command_choice_part("color", new Command_text("TEAM_COLOR")), new Command_choice_part("deathMessageVisibility", new Command_bool()), new Command_choice_part("displayName", new Command_str_text()), new Command_choice_part("friendlyFire", new Command_bool()), new Command_choice_part("nametagVisible", new Command_bool()), new Command_choice_part("prefix", new Command_str_text()), new Command_choice_part("seeFriendlyInvisibles", new Command_bool()), new Command_choice_part("suffix", new Command_str_text()))), new Command_choice_part("remove", new Command_text()))));
            parser.Add_rule(new Command_model(new Command_name("teleport"), new Command_choice(new Command_choice_part(new Command_pos()), new Command_choice_part(new Command_entity(), new Command_choice(new Command_choice_part(new Command_pos(),/*TODO these optional should be grouped*/new Command_int(true), new Command_int(true)),new Command_choice_part(new Command_entity(false,true,false)))))));
            parser.Add_rule(new Command_model(new Command_name("tell"), new Command_entity(), new Command_text()));
            parser.Add_rule(new Command_model(new Command_name("tellraw"), new Command_entity(false, false, true), new Command_str_text()));
            parser.Add_rule(new Command_model(new Command_name("time"), new Command_choice(new Command_choice_part("add", new Command_int()), new Command_choice_part("query", new Command_choice(new string[] { "day", "daytime", "gametime" })), new Command_choice_part("set", new Command_choice(new string[] { "day", "midnight", "night", "noon" })))));
            parser.Add_rule(new Command_model(new Command_name("title"), new Command_entity(false, false, true), new Command_choice(new Command_choice_part("actionbar", new Command_str_text()), new Command_choice_part("clear"), new Command_choice_part("reset"), new Command_choice_part("subtitle", new Command_str_text()), new Command_choice_part("times", new Command_int(), new Command_int(), new Command_int()), new Command_choice_part("title", new Command_str_text()))));
            parser.Add_rule(new Command_model(new Command_name("trigger"), new Command_text(), new Command_choice(new string[] { "add", "set" }), new Command_int()));
            parser.Add_rule(new Command_model(new Command_name("weather"), new Command_text(), new Command_choice(new string[] { "clear", "rain", "thunder" }), new Command_int()));
            parser.Add_rule(new Command_model(new Command_name("worldborder"), new Command_choice(new Command_choice_part("add", new Command_int(), new Command_int()), new Command_choice_part("center", new Command_pos()), new Command_choice_part("damage", new Command_choice(new string[] { "amount", "buffer" }), new Command_int()), new Command_choice_part("get"), new Command_choice_part("set", new Command_int(), new Command_int()), new Command_choice_part("warning", new Command_choice(new string[] { "distance", "time" }), new Command_int()))));
            parser.Add_rule(new Command_model(new Command_name("experience"), new Command_choice(new string[] { "add", "query", "set" }), new Command_entity(false, false, true), new Command_int(), new Command_choice(new string[] { "points", "levels" })));

            //Aliases after commands
            parser.Add_alias("xp", "experience");
            parser.Add_alias("tp", "teleport");
            parser.Add_alias("w", "msg");

            string block_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Block/" + version_path + "b.json";
            string item_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Item/" + version_path + "i.json";
            string entity_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Entity/" + version_path + "e.json";
            string sound_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Sound/" + version_path + "s.json";
            string advancement_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Advancement/" + version_path + "a.json";
            string recipe_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Recipe/" + version_path + "r.json";
            string particle_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Particle/" + version_path + "p.json";
            string enchantment_path = AppDomain.CurrentDomain.BaseDirectory + "/Changes/Enchantment/" + version_path + "e.json";

            //The regular which can have a "minecraft:" before
            string[] namespaces = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path + "/Regular");
            foreach(string namespace_ in namespaces)
            {
                string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                List<string> entire = Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_)));
                parser.Add_collection(name,entire);
            }

            //The tags
            namespaces = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path + "/Regular/Tags");
            foreach (string namespace_ in namespaces)
            {
                string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                //Assuming the collection exists already
                List<string> tags_added = new (parser.Get_collection(name));

                List<string> tags = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));

                for(int i = 0; i < tags.Count; i++)
                {
                    tags[i] = "#" + tags[i];
                }

                tags_added.AddRange(tags);

                parser.Add_collection(name+ "_TAG", tags_added);
            }

            //The other which can't have a "miinecraft:" before
            namespaces = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path);
            foreach (string namespace_ in namespaces)
            {
                string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                List<string> entire = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                parser.Add_collection(name, entire);
            }

            //Some special handling

            List<string> scoreboard_criteria_list = parser.Get_collection("SCOREBOARD_CRITERIA");
            List<string> item = parser.Get_collection("ITEM");
            List<string> block = parser.Get_collection("BLOCK");
            List<string> entity = parser.Get_collection("ENTITY");

            foreach (string current_item in item)
            {
                scoreboard_criteria_list.Add("minecraft.crafted:"+ current_item.Replace(':','.'));
                scoreboard_criteria_list.Add("minecraft.dropped:" + current_item.Replace(':', '.'));
                scoreboard_criteria_list.Add("minecraft.picked_up:" + current_item.Replace(':', '.'));
                scoreboard_criteria_list.Add("minecraft.used:" + current_item.Replace(':', '.'));
            }

            foreach (string current_block in block)
            {
                scoreboard_criteria_list.Add("minecraft.broken:" + current_block.Replace(':', '.'));
                scoreboard_criteria_list.Add("minecraft.mineed:" + current_block.Replace(':', '.'));
            }

            foreach (string current_entity in entity)
            {
                scoreboard_criteria_list.Add("minecraft.killed:" + current_entity.Replace(':', '.'));
                scoreboard_criteria_list.Add("minecraft.killed_by:" + current_entity.Replace(':', '.'));
            }

            void Validate_blocks_data_nbt(Command_parser parser, string value)
            {
                Seperate(value, out string block, out string block_state, out string nbt_data);
                parser.Verify_collection("BLOCK", block);
            }


            void Validate_blocks_tags_data_nbt(Command_parser parser, string value)
            {
                Seperate(value, out string block, out string block_state, out string nbt_data);
                parser.Verify_collection("BLOCK_TAG", block);
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

                if (start_index == -1 && end_index == -1)
                {
                    
                }
                else if(start_index != -1 && end_index != -1)
                {
                    nbt_data = value.Substring(start_index, end_index - start_index + 1);
                }
                else
                {
                    throw new Command_parse_exception("Cannot get nbt data from: " + value);
                }

                parser.Verify_collection("ITEM", item);
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

                if (start_index == -1 && end_index == -1)
                {

                }
                else if (start_index != -1 && end_index != -1)
                {
                    block_state = text.Substring(start_index, end_index - start_index + 1);
                }
                else
                {
                    throw new Command_parse_exception("Cannot get block states from: " + text);
                }


                nbt_data = "";
                start_index = text.IndexOf('{');
                end_index = text.LastIndexOf('}');

                if (start_index == -1 && end_index == -1)
                {

                }
                else if (start_index != -1 && end_index != -1)
                {
                    nbt_data = text.Substring(start_index, end_index - start_index + 1);
                }
                else
                {
                    throw new Command_parse_exception("Cannot get nbt data from: " + text);
                }
            }

            void Validate_selector(Command_entity model ,Command_entity entity)
            {
                string text = entity.Entity_selector;

                string selector = text[..2];

                bool only_player = false;

                if(selector == "@a" || selector == "@p" || selector == "@s" || selector == "@r")  //@s as a player is fine here
                {
                    only_player = true;
                }

                bool only_one = false;

                if (selector == "@s" || selector == "@p" || selector == "@r")  //@s as a player is fine here
                {
                    only_one = true;
                }

                string inside = "";
                int start_index = text.IndexOf('[');
                int end_index = text.LastIndexOf(']');

                if (start_index == -1 && end_index == -1)
                {
                    
                }
                else if (start_index != -1 && end_index != -1)
                {
                    inside = text.Substring(start_index+1, end_index - start_index - 1); 

                    string[] parts = Command_parser.Split_ignore(inside,',').ToArray();

                    foreach (string part in parts)
                    {
                        string[] sub_parts = part.Split(new char[] { '=' }, 2);

                        if(sub_parts.Length != 2)
                        {
                            throw new Command_parse_exception("Cannot parse selector argument: " + part);
                        }

                        string type = sub_parts[0];
                        string follower = sub_parts[1];

                        if(follower.Length == 0)
                        {
                            throw new Command_parse_exception("Cannot parse selector argument: " + part);
                        }
                        //TODO some of these can also only be used once

                        switch(type)
                        {
                            case "advancements":
                                //TODO check with collection
                                break;
                            case "distance":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "dx":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "dy":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "dz":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "gamemode":
                                if (!(follower == "survival" || follower == "creative" || follower == "spectator" || follower == "adventure"))
                                {
                                    throw new Command_parse_exception("Cannot parse: " + follower + " as a gamemode");
                                }

                                break;
                            case "level":
                                Command_int.Validate_range(follower, out _, out _);
                                break;
                            case "limit":
                                if (!int.TryParse(follower, NumberStyles.Float, CultureInfo.InvariantCulture, out int limit))
                                {
                                    throw new Command_parse_exception("Cannot parse: " + follower + " as a number");
                                }

                                if(limit == 1)
                                {
                                    only_one = true;
                                }

                                break;
                            case "name":
                                //TODO some valid chars?
                                break;
                            case "nbt":
                                //TODO nbt validator
                                break;
                            case "scores":
                                if(sub_parts.Length < 2)
                                {
                                    throw new Command_parse_exception("Cannot parse: " + sub_parts + " as scores");
                                }

                                string part_no_bracket = follower[1..^1];
                                string[] all_scores = part_no_bracket.Split(',');
                                
                                for(int i = 0; i < all_scores.Length; i++)
                                {
                                    if (all_scores[i] == "")
                                    {
                                        //Last can apparently be empty, and only last
                                        if (i != all_scores.Length)
                                        {
                                            throw new Command_parse_exception("Cannot parse scores: " + part_no_bracket);
                                        }
                                    }

                                    string[] score_parts = all_scores[i].Split('=');
                                    string name = score_parts[0];
                                    string range = score_parts[1];

                                    Command_int.Validate_range(range, out _, out _);
                                }
                                

                                break;
                            case "sort":
                                if (!(follower == "nearest" || follower == "furthest" || follower == "arbitrary" || follower == "random"))
                                {
                                    throw new Command_parse_exception("Cannot parse: " + follower + " as a sort option");
                                }
                                break;
                            case "tag":
                                //TODO some valid chars?
                                break;
                            case "team":
                                //TODO some valid chars?
                                break;
                            case "type":
                                if(selector == "@p" || selector == "@a")
                                {
                                    throw new Command_parse_exception("Type cannot be used here");
                                }

                                if(follower == "minecraft:player" || follower == "player")
                                {
                                    only_player = true;
                                }

                                //TODO check with collection

                                break;
                            case "x":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "x_rotation":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "y":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "y_rotation":
                                Command_float.Validate_range(follower, out _, out _);
                                break;
                            case "z":
                                Command_float.Validate_range(follower, out _, out _);
                                break;

                            default:
                                throw new Command_parse_exception("Type: " + type + " is an unknown selector argument");
                        }
                    }
                }
                else
                {
                    throw new Command_parse_exception("Cannot get selector arguments from: " + selector);
                }

                if (model.Only_one && !only_one)
                {
                    throw new Command_parse_exception("Only one player can be used here, got: " + selector);
                }

                if (model.Only_player && !only_player)
                {
                    throw new Command_parse_exception("Only players can be used here, got: " + selector);
                }
            }

            Console.WriteLine("Parser " + version + " done");

            return parser;
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
