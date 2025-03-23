using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Command_parsing.Command_parts;
using Command_parsing.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace Command_parsing
{
    public static class Parser_creator
    {
        private static readonly Dictionary<string, Action> creatable = new() { { "1.13", Create_1_13 }, { "1.13.1", Create_1_13_1 }, { "1.13.2", Create_1_13_2 }, { "1.14", Create_1_14 }, { "1.14.1", Create_1_14_1 }, { "1.14.2", Create_1_14_2 }, { "1.14.3", Create_1_14_3 }, { "1.14.4", Create_1_14_4 }, { "1.15", Create_1_15 },{"1.15.1", Create_1_15_1 }, { "1.15.2", Create_1_15_2 }, {"1.16", Create_1_16 }, {"1.16.1", Create_1_16_1 }, { "1.16.2", Create_1_16_2 }, { "1.16.3", Create_1_16_3 }, { "1.16.4", Create_1_16_4 }, { "1.16.5", Create_1_16_5 }, { "1.17", Create_1_17 }, { "1.17.1", Create_1_17_1 }, {"1.18", Create_1_18 },{ "1.18.1", Create_1_18_1 },{ "1.18.2", Create_1_18_2} };

        private const int permission_level = 2;
        public static readonly Dictionary<string, Command_parser> Parser_collection = new();
    

        public static bool Get_parser(string version, out Command_parser parser)
        {
            if (Parser_collection.ContainsKey(version))
            {
                parser = new Command_parser(Parser_collection[version]);
                return true;
            }

            if(creatable.ContainsKey(version))
            {
                creatable[version].Invoke();
                parser = new Command_parser(Parser_collection[version]);
                return true;
            }

            parser = null;
            return false;
        }

        public static Command_parser Get_parser(string version)
        { 
            if(Get_parser(version, out Command_parser parser))
            {
                return parser;
            }

            throw new Exception("Cannot get not create parser for version: " + version);
        }

        //This is a bit bad but iterating through Parser_collection is cleaned than starting to mess with versions in Program.cs 
        public static void Create_all()
        {
            foreach(KeyValuePair<string,Action> version in creatable)
            {
                if (!Parser_collection.ContainsKey(version.Key))
                {
                    version.Value.Invoke();
                }
            }
        }

        public static void Create_1_13()
        {
            //TODO might want to use own version format
            string version = "1.13";
            
            Console.WriteLine("Creating parser: " + version);

            Command_parser parser = new(version, permission_level, new Selector_validator(false,false));

            //Commands present on the client

            parser.Add_command(new Command_model(new Command_name("advancement"), new Command_choice(new Command_choice_part("grant", new Command_entity(false,false,true), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text("ADVANCEMENT")), new Command_choice_part("only", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")), new Command_choice_part("until", new Command_text("ADVANCEMENT")))), new Command_choice_part("revoke", new Command_entity(), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text("ADVANCEMENT")), new Command_choice_part("only", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")))))));
            parser.Add_command(new Command_model(new Command_name("bossbar"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text()), new Command_choice_part("get", new Command_choice(new string[] { "max", "players", "value", "visible" })), new Command_choice_part("list"), new Command_choice_part("remove", new Command_text()), new Command_choice_part("set", new Command_text()), new Command_choice_part("color"), new Command_choice_part("max", new Command_int()), new Command_choice_part("players", new Command_entity()), new Command_choice_part("style", new Command_choice(new string[] { "notched_10", "notched_12", "notched_20", "notched_6", "progress" })), new Command_choice_part("value", new Command_int()), new Command_choice_part("visible", new Command_bool()))));
            parser.Add_command(new Command_model(new Command_name("clear"), new Command_entity(true,false,true), new Command_text("ITEM_TAG", true)));  //TODO raw as well
            parser.Add_command(new Command_model(new Command_name("clone"), new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(new Command_choice_part("filtered", new Command_text("ITEM_TAG")), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" })), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" })))));
            parser.Add_command(Command_model_builder.Get_data(Versions.Get_own_version(version)));
            parser.Add_command(new Command_model(new Command_name("datapack"), new Command_choice(new Command_choice_part("disable", new Command_text()), new Command_choice_part("enable", new Command_text()), new Command_choice_part("list"))));
            parser.Add_command(new Command_model(new Command_name("debug"), new Command_choice(new string[] { "start", "stop" })));
            parser.Add_command(new Command_model(new Command_name("defaultgamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" })));
            parser.Add_command(new Command_model(new Command_name("difficulty"), new Command_choice(new string[] { "easy", "hard", "normal", "peaceful" })));
            parser.Add_command(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(), new Command_text("EFFECT",true)), new Command_choice_part("give", new Command_entity(), new Command_text("EFFECT"), new Command_int(true), new Command_int(true), new Command_bool(true)))));
            parser.Add_command(new Command_model(new Command_name("enchant"), new Command_entity(false,false,true), new Command_text("ENCHANTMENT"), new Command_int(true)));

            parser.Add_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));

            //parser.Add_command(new Command_model(new Command_name("experience"), new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("set", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("query", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })))));
            parser.Add_command(new Command_model(new Command_name("fill"), new Command_pos(), new Command_pos(), new Command_text(Validate_blocks_data_nbt), new Command_choice(true,new Command_choice_part("hollow"), new Command_choice_part("destroy"), new Command_choice_part("keep"), new Command_choice_part("outline"), new Command_choice_part("replace", new Command_text(Validate_blocks_tags_data_nbt))  )));
            parser.Add_command(new Command_model(new Command_name("function"), new Command_text()));

            parser.Add_command(new Command_model(new Command_name("gamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" }), new Command_entity(true,false,true)));
            parser.Add_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)))));
            parser.Add_command(new Command_model(new Command_name("give"), new Command_entity(false,false,true), new Command_text(Validate_item_nbt), new Command_int(true)));
            parser.Add_command(new Command_model(new Command_name("help"), new Command_text()));
            parser.Add_command(new Command_model(new Command_name("kick",3), new Command_entity(false,false,true), new Command_text_end(true)));
            parser.Add_command(new Command_model(new Command_name("kill"), new Command_entity()));
            parser.Add_command(new Command_model(new Command_name("list"), new Command_choice(new string[] { "uuids" }, true)));
            parser.Add_command(new Command_model(new Command_name("locate"), new Command_text("STRUCTURE")));
            parser.Add_command(new Command_model(new Command_name("me"), new Command_text_end()));
            parser.Add_command(new Command_model(new Command_name("msg"), new Command_entity(false,false,true), new Command_text_end()));
            parser.Add_command(new Command_model(new Command_name("particle"), new Command_text("PARTICLE"), new Command_pos(true), new Command_pos(true), new Command_float(true), new Command_int(true), new Command_choice(new string[] { "force", "normal" }, true), new Command_entity(true)));

            //TODO Parts without anything action will have to continue root action
            parser.Add_command(new Command_model(new Command_name("playsound"), new Command_text("SOUND"), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }), new Command_entity(), new Command_pos(true), new Command_int(true), new Command_float(2,true), new Command_float(1,true)));
            parser.Add_command(new Command_model(new Command_name("publish"), new Command_int(65535)));
            parser.Add_command(new Command_model(new Command_name("recipe"), new Command_choice(new Command_choice_part("give", new Command_entity(false, false, true), new Command_text("RECIPE")), new Command_choice_part("take", new Command_entity(false,false,true), new Command_text("RECIPE")))));
            parser.Add_command(new Command_model(new Command_name("reload", 3)));
            parser.Add_command(new Command_model(new Command_name("replaceitem"), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text(Validate_item_nbt), new Command_int(true)), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), new Command_text(Validate_item_nbt), new Command_int(true)))));
            parser.Add_command(new Command_model(new Command_name("say"), new Command_text_end()));

            //TODO two last are optional in pair
            parser.Add_command(new Command_model(new Command_name("scoreboard"), new Command_choice(new Command_choice_part("objectives", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text("SCOREBOARD_CRITERIA",true), new Command_str_text(true)), new Command_choice_part("list"), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("displayname", new Command_str_text()), new Command_choice_part("rendertype", new Command_choice(new string[] { "hearts", "integer" })))), new Command_choice_part("remove", new Command_text()), new Command_choice_part("setdisplay", new Command_text("SCOREBOARD_DISPLAY"), new Command_text()))), new Command_choice_part("players", new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("enable", new Command_entity(), new Command_text()), new Command_choice_part("get", new Command_entity(), new Command_text()), new Command_choice_part("list", new Command_entity()), new Command_choice_part("operation", new Command_entity(), new Command_text(), new Command_choice(new string[] { "%=", "*=", "+=", "-=", "/=", "<", "=", ">", "><" }), new Command_entity(), new Command_text()), new Command_choice_part("remove", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("reset", new Command_entity(), new Command_text()), new Command_choice_part("set", new Command_entity(), new Command_text(), new Command_int()))))));
            parser.Add_command(new Command_model(new Command_name("seed")));
            parser.Add_command(new Command_model(new Command_name("setblock"), new Command_pos(), new Command_text(Validate_blocks_data_nbt), new Command_choice(new string[] { "replace","keep","destroy"},true)));
            parser.Add_command(new Command_model(new Command_name("setworldspawn"), new Command_pos(true)));
            parser.Add_command(new Command_model(new Command_name("spawnpoint"), new Command_entity(true, false, true), new Command_pos(true)));
            parser.Add_command(new Command_model(new Command_name("spreadplayers"), new Command_pos(false,true), new Command_float(), new Command_float(), new Command_bool(), new Command_entity()));
            parser.Add_command(new Command_model(new Command_name("stopsound"), new Command_entity(false, false, true), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }, true), new Command_text("SOUND", true)));
            parser.Add_command(new Command_model(new Command_name("summon"), new Command_text("ENTITY"), new Command_pos(true), new Command_text(Validate_nbt, true)));
            parser.Add_command(new Command_model(new Command_name("tag"), new Command_entity(), new Command_choice(new Command_choice_part("add", new Command_text()), new Command_choice_part("remove", new Command_text()), new Command_choice_part("list"))));
            parser.Add_command(new Command_model(new Command_name("team"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_str_text()), new Command_choice_part("empty", new Command_text()), new Command_choice_part("join", new Command_text(), new Command_entity()), new Command_choice_part("leave", new Command_entity()), new Command_choice_part("list", new Command_text()), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("collisionRule", new Command_bool()), new Command_choice_part("color", new Command_text("TEAM_COLOR")), new Command_choice_part("deathMessageVisibility", new Command_bool()), new Command_choice_part("displayName", new Command_str_text()), new Command_choice_part("friendlyFire", new Command_bool()), new Command_choice_part("nametagVisible", new Command_bool()), new Command_choice_part("prefix", new Command_str_text()), new Command_choice_part("seeFriendlyInvisibles", new Command_bool()), new Command_choice_part("suffix", new Command_str_text()))), new Command_choice_part("remove", new Command_text()))));
            parser.Add_command(new Command_model(new Command_name("teleport"), new Command_choice(new Command_choice_part(new Command_pos()), new Command_choice_part(new Command_entity(), new Command_choice(new Command_choice_part(new Command_pos(),/*TODO these optional should be grouped*/new Command_int(true), new Command_int(true)),new Command_choice_part(new Command_entity(false,true,false)))))));
            parser.Add_command(new Command_model(new Command_name("tell"), new Command_entity(false, false, true), new Command_text()));
            parser.Add_command(new Command_model(new Command_name("tellraw"), new Command_entity(false, false, true), new Command_str_text()));
            parser.Add_command(new Command_model(new Command_name("time"), new Command_choice(new Command_choice_part("add", new Command_int()), new Command_choice_part("query", new Command_choice(new string[] { "day", "daytime", "gametime" })), new Command_choice_part("set", new Command_choice(new Command_choice_part(new Command_int()), new Command_choice_part("day"), new Command_choice_part("midnight"), new Command_choice_part("night"), new Command_choice_part("noon"))))));
            parser.Add_command(new Command_model(new Command_name("title"), new Command_entity(false, false, true), new Command_choice(new Command_choice_part("actionbar", new Command_str_text()), new Command_choice_part("clear"), new Command_choice_part("reset"), new Command_choice_part("subtitle", new Command_str_text()), new Command_choice_part("times", new Command_int(), new Command_int(), new Command_int()), new Command_choice_part("title", new Command_str_text()))));
            parser.Add_command(new Command_model(new Command_name("trigger"), new Command_text(), new Command_choice(new string[] { "add", "set" }), new Command_int()));
            parser.Add_command(new Command_model(new Command_name("weather"), new Command_choice(new string[] { "clear", "rain", "thunder" }), new Command_int(true)));
            parser.Add_command(new Command_model(new Command_name("worldborder"), new Command_choice(new Command_choice_part("add", new Command_int(), new Command_int()), new Command_choice_part("center", new Command_pos()), new Command_choice_part("damage", new Command_choice(new string[] { "amount", "buffer" }), new Command_int()), new Command_choice_part("get"), new Command_choice_part("set", new Command_int(), new Command_int()), new Command_choice_part("warning", new Command_choice(new string[] { "distance", "time" }), new Command_int()))));
            parser.Add_command(new Command_model(new Command_name("experience"), new Command_choice(new string[] { "add", "query", "set" }), new Command_entity(false, false, true), new Command_int(), new Command_choice(new string[] { "points", "levels" })));

            //Commands present on servers
            parser.Add_command(new Command_model(new Command_name("ban",3), new Command_entity(false, false, true), new Command_text_end(true)));
            parser.Add_command(new Command_model(new Command_name("ban-ip",3), new Command_entity(false, false, true), new Command_text_end(true)));
            parser.Add_command(new Command_model(new Command_name("banlist",3), new Command_choice(new string[] {"ips","players" },true)));
            parser.Add_command(new Command_model(new Command_name("op",4), new Command_entity(false, false, true)));
            parser.Add_command(new Command_model(new Command_name("pardon",3), new Command_entity(false, false, true)));
            parser.Add_command(new Command_model(new Command_name("pardon-ip",3), new Command_entity(false, false, true)));
            parser.Add_command(new Command_model(new Command_name("save-all",4), new Command_choice(new string[] { "flush"}, true)));
            parser.Add_command(new Command_model(new Command_name("save-on",4)));
            parser.Add_command(new Command_model(new Command_name("save-off",4)));

            parser.Add_command(new Command_model(new Command_name("whitelist",3), new Command_choice(new Command_choice_part("add", new Command_entity(false, false, true)), new Command_choice_part("list"), new Command_choice_part("off"), new Command_choice_part("on"), new Command_choice_part("reload"), new Command_choice_part("remove", new Command_entity(false, false, true)))));

            parser.Add_command(new Command_model(new Command_name("stop",4)));
            //Aliases after commands
            parser.Add_alias("xp", "experience");
            parser.Add_alias("tp", "teleport");
            parser.Add_alias("w", "msg");

            Tree_add(parser,version);

            parser.Add_replace_collection("NBT_SIZE", new List<string> { "byte", "double", "float", "int", "long", "short" });

            void Validate_blocks_data_nbt(Command_parser parser, string value)
            {
                Separate_block(value, out string block, out string block_state, out string nbt_data);
                parser.Verify_collection("BLOCK", block);
            }

            Console.WriteLine("Parser " + version + " done");

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_13_1()
        {
            string version = "1.13.1";

            Command_parser parser = new(version, Get_parser("1.13"));

            parser.Add_command(new Command_model(new Command_name("forceload", 3), new Command_choice(new Command_choice_part("add", new Command_pos(false,true), new Command_pos(true,true)), new Command_choice_part("query"), new Command_choice_part("remove", new Command_choice(new Command_choice_part("all"), new Command_choice_part(new Command_pos(false,true), new Command_pos(true,true)))))));

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_13_2()
        {
            string version = "1.13.2";
            Command_parser parser = new(version, Get_parser("1.13.1"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14()
        {
            string version = "1.14";
            Command_parser parser = new(version, Get_parser("1.13.2"), new Selector_validator(true,false));

            Command_choice loot_types = new(new Command_choice_part("fish", new Command_text("LOOT_TABLE"), new Command_pos(), new Command_text("ITEM_HAND",true)), new Command_choice_part("kill", new Command_entity(false,true,false)), new Command_choice_part("loot", new Command_text("LOOT_TABLE")), new Command_choice_part("mine", new Command_text("ITEM_HAND", true)));

            parser.Add_command(new Command_model(new Command_name("loot"), new Command_choice(new Command_choice_part("give", new Command_entity(false,false,true), loot_types), new Command_choice_part("insert", new Command_pos(), loot_types), new Command_choice_part("replace", new Command_choice(new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), loot_types), new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), loot_types))), new Command_choice_part("spawn", new Command_pos(), loot_types))));
            parser.Add_command(new Command_model(new Command_name("teammsg"), new Command_text_end()));
            parser.Add_command(new Command_model(new Command_name("schedule"), new Command_choice(new Command_choice_part("function", new Command_text(), new Command_time()))));

            parser.Add_replace_command(new Command_model(new Command_name("time"), new Command_choice(new Command_choice_part("add", new Command_time()), new Command_choice_part("query", new Command_choice(new string[] { "day", "daytime", "gametime" })), new Command_choice_part("set", new Command_choice(new Command_choice_part(new Command_time()), new Command_choice_part("day"), new Command_choice_part("midnight"), new Command_choice_part("night"), new Command_choice_part("noon"))))));
            parser.Add_replace_command(Command_model_builder.Get_data(Versions.Get_own_version(version)));
            parser.Add_replace_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));

            Tree_add(parser, version);

            List<string> item_hand = new(parser.Get_collection("ITEM"))
            {
                "mainhand",
                "offhand"
            };

            parser.Add_replace_collection("ITEM_HAND",item_hand);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_1()
        {
            string version = "1.14.1";
            Command_parser parser = new(version, Get_parser("1.14"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_2()
        {
            string version = "1.14.2";
            Command_parser parser = new(version, Get_parser("1.14.1"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_3()
        {
            string version = "1.14.3";
            Command_parser parser = new(version, Get_parser("1.14.2"));

            parser.Add_replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids",new Command_bool(true)))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_4()
        {
            string version = "1.14.4";
            Command_parser parser = new(version, Get_parser("1.14.3"));

            //TODO debug report here?, also gone later for some reason

            parser.Add_replace_command(new Command_model(new Command_name("reload",2)));
            parser.Add_replace_command(new Command_model(new Command_name("forceload", 2), new Command_choice(new Command_choice_part("add", new Command_pos(false, true), new Command_pos(true, true)), new Command_choice_part("query"), new Command_choice_part("remove", new Command_choice(new Command_choice_part("all"), new Command_choice_part(new Command_pos(false, true), new Command_pos(true, true)))))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_15()
        {
            string version = "1.15";
            Command_parser parser = new(version, Get_parser("1.14.4"), new Selector_validator(true, true));

            parser.Add_command(new Command_model(new Command_name("spectate"), new Command_entity(true, true, false), new Command_entity(true, true, true)));

            parser.Add_replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)),new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmidiateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)))));
            parser.Add_replace_command(new Command_model(new Command_name("schedule"), new Command_choice(new Command_choice_part("function", new Command_text(), new Command_time(), new Command_choice(new string[] { "append","replace"}, true)), new Command_choice_part("clear", new Command_text_end()))));
            parser.Add_replace_command(new Command_model(new Command_name("kill"), new Command_entity(true)));
            parser.Add_replace_command(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(true), new Command_text("EFFECT", true)), new Command_choice_part("give", new Command_entity(), new Command_text("EFFECT"), new Command_int(true), new Command_int(true), new Command_bool(true)))));
            parser.Add_replace_command(Command_model_builder.Get_data(Versions.Get_own_version("1.15")));
            parser.Add_replace_command(Command_model_builder.Get_execute(Versions.Get_own_version("1.15")));

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_15_1()
        {
            string version = "1.15.1";
            Command_parser parser = new(version, Get_parser("1.15"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_15_2()
        {
            string version = "1.15.2";
            Command_parser parser = new(version, Get_parser("1.15.1"));
            parser.Add_replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmidiateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool())    )));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16()
        {
            string version = "1.16";

            Command_parser parser = new(version, Get_parser("1.15.2"));

            parser.Add_command(new Command_model(new Command_name("locatebiome"), new Command_text("BIOME")));
            parser.Add_command(new Command_model(new Command_name("attribute"), new Command_entity(false,true,false), new Command_text("ATTRIBUTE"), new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("base", new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("set", new Command_float()))), new Command_choice_part("modifier", new Command_choice(new Command_choice_part("add", new Command_uuid(), new Command_str_text(), new Command_float(), new Command_choice(new string[] { "add","multiply","multiply_base"})), new Command_choice_part("remove", new Command_uuid()), new Command_choice_part("value", new Command_choice(new Command_choice_part("get", new Command_uuid(), new Command_float(true)))))))));

            parser.Add_replace_command(new Command_model(new Command_name("spreadplayers"),new Command_pos(false, true), new Command_float(), new Command_float(), new Command_choice(new Command_choice_part("false", new Command_entity()), new Command_choice_part("true", new Command_entity()), new Command_choice_part("under", new Command_float(), new Command_choice(new Command_choice_part("false", new Command_entity()), new Command_choice_part("true", new Command_entity()))))));
            parser.Add_replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmidiateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true))  )));

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_1()
        {
            string version = "1.16.1";
            Command_parser parser = new(version, Get_parser("1.16"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_2()
        {
            string version = "1.16.2";
            Command_parser parser = new(version, Get_parser("1.16.1"));

            //TODO will have to
            //Tree add

            parser.Add_replace_command(new Command_model(new Command_name("setworldspawn"), new Command_pos(true), new Command_float(true)));
            parser.Add_replace_command(new Command_model(new Command_name("spawnpoint"), new Command_entity(true, false, true), new Command_pos(true), new Command_float(true)));  //TODO support relative angles with ~

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_3()
        {
            string version = "1.16.3";
            Command_parser parser = new(version, Get_parser("1.16.2"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_4()
        {
            string version = "1.16.4";
            Command_parser parser = new(version, Get_parser("1.16.3"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_5()
        {
            string version = "1.16.5";
            Command_parser parser = new(version, Get_parser("1.16.4"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_17()
        {
            string version = "1.17";
            Command_parser parser = new(version, Get_parser("1.16.5"));

            Command_choice item_source = new(new Command_choice_part("with", new Command_text(Validate_item_nbt), new Command_int(true)), new Command_choice_part("from", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text(true)), new Command_choice_part("entity", new Command_entity(false, true, false), new Command_text("SLOT"), new Command_text(true)))));
            parser.Add_command(new Command_model(new Command_name("perf", 4), new Command_choice(new string[] { "start", "stop" })));
            parser.Add_command(new Command_model(new Command_name("item"), new Command_choice(new Command_choice_part("modify", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text()), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), new Command_text()))), new Command_choice_part("replace", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), item_source), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), item_source))))));

            parser.Add_replace_command(new Command_model(new Command_name("debug", 3), new Command_choice(new Command_choice_part("start"), new Command_choice_part("stop"), new Command_choice_part("function", new Command_text()))));
            parser.Add_replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmidiateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()))));
            
            parser.Remove_command("replaceitem");

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_17_1()
        {
            string version = "1.17.1";
            Command_parser parser = new(version, Get_parser("1.17"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_18()
        {
            string version = "1.18";
            Command_parser parser = new(version, Get_parser("1.17.1"));

            parser.Add_command(new Command_model(new Command_name("jfr"), new Command_choice(new string[] { "start", "stop"}) ));

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_18_1()
        {
            string version = "1.18.1";
            Command_parser parser = new(version, Get_parser("1.18"));
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_18_2()
        {
            string version = "1.18.2";
            Command_parser parser = new(version, Get_parser("1.18.1"));

            //TODO these should be one less severity I think (I don't think locate aaaa fails)

            parser.Add_command(new Command_model(new Command_name("placefeature"), new Command_text("FEATURE"), new Command_pos(true)));
            parser.Add_replace_command(new Command_model(new Command_name("locate"), new Command_text("STRUCTURE_TAG")));
            parser.Add_replace_command(new Command_model(new Command_name("locatebiome"), new Command_text("BIOME_TAG")));

            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        //TODO support other versions or branch out (branch out is better)
        private static void Tree_add(Command_parser parser, string version)
        {
            string version_path = version.Replace(".", "_");

            //The regular which can have a "minecraft:" before

            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path + "/Regular"))
            {
                string[] namespaces = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path + "/Regular");
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    List<string> entire = Patch_minecraft(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_)));
                    parser.Add_replace_collection(name, entire);
                }
            }

            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path + "/Regular/Tags"))
            {
                //The tags
                string[] namespaces = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path + "/Regular/Tags");
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    //Assuming the collection exists already
                    List<string> tags_added = new(parser.Get_collection(name));

                    List<string> raw = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    List<string> tags = new();

                    for (int i = 0; i < raw.Count; i++)
                    {
                        tags.Add("#" + raw[i]);
                        tags.Add("#minecraft:" + raw[i]);
                    }

                    tags_added.AddRange(tags);

                    parser.Add_replace_collection(name + "_TAG", tags_added);
                }
            }
            
            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path))
            {
                //The other which can't have a "minecraft:" before
                string[] namespaces = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Changes/" + version_path);
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    List<string> entire = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    parser.Add_replace_collection(name, entire);
                }
            }

            //Some special handling

            List<string> scoreboard_criteria_list = parser.Get_collection("SCOREBOARD_CRITERIA");
            List<string> item = parser.Get_collection("ITEM");
            List<string> block = parser.Get_collection("BLOCK");
            List<string> entity = parser.Get_collection("ENTITY");

            foreach (string current_item in item)
            {
                scoreboard_criteria_list.Add("minecraft.crafted:" + current_item.Replace(':', '.'));
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

        //TODO probably move out to another class (nbt_validator)
        public static void Validate_nbt_path(Command_parser parser, string value)
        {

        }

        public static void Validate_nbt(Command_parser parser, string value)
        {

        }

        public static void Validate_nbt_value(Command_parser parser, string value)
        {

        }

        public static void Validate_blocks_tags_data_nbt(Command_parser parser, string value)
        {
            Separate_block(value, out string block, out string block_state, out string nbt_data);
            parser.Verify_collection("BLOCK_TAG", block);
        }

        public static void Validate_item_nbt(Command_parser parser, string value)
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
            else if (start_index != -1 && end_index != -1)
            {
                nbt_data = value.Substring(start_index, end_index - start_index + 1);
            }
            else
            {
                throw new Command_parse_exception("Cannot get nbt data from: " + value);
            }

            parser.Verify_collection("ITEM", item);
        }

        private static void Separate_block(string text, out string block, out string block_state, out string nbt_data)
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
    }
}
