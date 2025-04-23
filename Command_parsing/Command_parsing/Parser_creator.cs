using Command_parsing.Command_parts;
using Command_parsing.Validators;
using Minecraft_common;
using Newtonsoft.Json;

namespace Command_parsing
{
    public static class Parser_creator
    {
        private static readonly Dictionary<string, Action> creatable = new() { { "1.13", Create_1_13 }, { "1.13.1", Create_1_13_1 }, { "1.13.2", Create_1_13_2 }, { "1.14", Create_1_14 }, { "1.14.1", Create_1_14_1 }, { "1.14.2", Create_1_14_2 }, { "1.14.3", Create_1_14_3 }, { "1.14.4", Create_1_14_4 }, { "1.15", Create_1_15 }, { "1.15.1", Create_1_15_1 }, { "1.15.2", Create_1_15_2 }, { "1.16", Create_1_16 }, { "1.16.1", Create_1_16_1 }, { "1.16.2", Create_1_16_2 }, { "1.16.3", Create_1_16_3 }, { "1.16.4", Create_1_16_4 }, { "1.16.5", Create_1_16_5 }, { "1.17", Create_1_17 }, { "1.17.1", Create_1_17_1 }, { "1.18", Create_1_18 }, { "1.18.1", Create_1_18_1 }, { "1.18.2", Create_1_18_2 }, { "1.19", Create_1_19 }, { "1.19.1", Create_1_19_1 }, { "1.19.2", Create_1_19_2 }, { "1.19.3", Create_1_19_3 }, { "1.19.4", Create_1_19_4 }, { "1.20", Create_1_20 }, { "1.20.1", Create_1_20_1 }, { "1.20.2", Create_1_20_2 }, { "1.20.3", Create_1_20_3 }, { "1.20.4", Create_1_20_4 }, { "1.20.5", Create_1_20_5 }, { "1.20.6", Create_1_20_6 }, { "1.21", Create_1_21 }, { "1.21.1", Create_1_21_1 }, {"1.21.2",Create_1_21_2 }, { "1.21.3", Create_1_21_3 }, {"1.21.4", Create_1_21_4 },{"1.21.5", Create_1_21_5 } };

        private const int permission_level = 2;
        private static readonly Dictionary<string, Command_parser> Parser_collection = new();

        public static bool Get_parser(string version, out Command_parser parser)
        {
            if (Parser_collection.ContainsKey(version))
            {
                parser = new Command_parser(Parser_collection[version]);
                return true;
            }

            if (creatable.ContainsKey(version))
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
            if (Get_parser(version, out Command_parser parser))
            {
                return parser;
            }

            throw new Exception("Cannot get not create parser for version: " + version);
        }

        public static void Create_1_13()
        {
            string version = "1.13";
            Console.WriteLine("Creating parser: " + version);
            Command_parser parser = new(version, permission_level);
            Tree_add(parser, version);

            parser.Add_validator("entity", new Selector_validator(false, false, false));

            parser.Add_validator("item", new Item_validator(false));
            parser.Add_validator("item_tag", new Item_tag_validator(false));
            parser.Add_validator("block_data", new Block_validator());
            parser.Add_validator("block_data_tag", new Block_data_tag_validator());

            parser.Add_validator("nbt", new Nbt_validator());
            parser.Add_validator("entity_nbt", new Entity_nbt_validator());
            parser.Add_validator("nbt_path", new Nbt_path_validator());

            parser.Add_validator("particle", new Particle_validator(false));

            parser.Add_validator("scoreboard_criteria", new Scoreboard_criteria_validator());

            parser.Add_validator("function_call", new Function_call_validator());

            parser.Add_validator("text_format", new Text_format_validator(false));

            parser.Set_validator_severity("SOUND", Problem_severity.Warning);
            parser.Set_validator_severity("STRUCTURE", Problem_severity.Warning);

            //Commands present on the client

            parser.Add_command(new Command_model(new Command_name("advancement"), new Command_choice(new Command_choice_part("grant", new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text("ADVANCEMENT")), new Command_choice_part("only", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")), new Command_choice_part("until", new Command_text("ADVANCEMENT")))), new Command_choice_part("revoke", new Command_entity(), new Command_choice(new Command_choice_part("everything"), new Command_choice_part("from", new Command_text("ADVANCEMENT")), new Command_choice_part("only", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")), new Command_choice_part("through", new Command_text("ADVANCEMENT")))))));
            parser.Add_command(new Command_model(new Command_name("bossbar"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text()), new Command_choice_part("get", new Command_choice(new string[] { "max", "players", "value", "visible" })), new Command_choice_part("list"), new Command_choice_part("remove", new Command_text()), new Command_choice_part("set", new Command_text()), new Command_choice_part("color"), new Command_choice_part("max", new Command_int()), new Command_choice_part("players", new Command_entity()), new Command_choice_part("style", new Command_choice(new string[] { "notched_10", "notched_12", "notched_20", "notched_6", "progress" })), new Command_choice_part("value", new Command_int()), new Command_choice_part("visible", new Command_bool()))));
            parser.Add_command(new Command_model(new Command_name("clear"), new Command_entity(true, false, Entity_type_limitation.Only_player), new Command_text("item_tag", true), new Command_int(true)));
            parser.Add_command(new Command_model(new Command_name("clone"), new Command_pos(), new Command_pos(), new Command_pos(), new Command_choice(true, new Command_choice_part("filtered", new Command_text("block_data_tag"), new Command_choice(new string[] { "force", "move", "normal" }, true)), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" }, true)), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" }, true)))));
            parser.Add_command(Command_model_builder.Get_data(Versions.Get_own_version(version)));
            parser.Add_command(new Command_model(new Command_name("datapack"), new Command_choice(new Command_choice_part("disable", new Command_text()), new Command_choice_part("enable", new Command_text()), new Command_choice_part("list"))));
            parser.Add_command(new Command_model(new Command_name("debug",3), new Command_choice(new string[] { "start", "stop" })));
            parser.Add_command(new Command_model(new Command_name("defaultgamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" })));
            parser.Add_command(new Command_model(new Command_name("difficulty"), new Command_choice(new string[] { "easy", "hard", "normal", "peaceful" })));
            parser.Add_command(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(), new Command_text("EFFECT", true)), new Command_choice_part("give", new Command_entity(), new Command_text("EFFECT"), new Command_int(true), new Command_int(true), new Command_bool(true)))));
            parser.Add_command(new Command_model(new Command_name("enchant"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text("ENCHANTMENT"), new Command_int(true)));

            parser.Add_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));

            //parser.Add_command(new Command_model(new Command_name("experience"), new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("set", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })), new Command_choice_part("query", new Command_entity(), new Command_int(), new Command_choice(new string[] { "levels", "points" })))));
            parser.Add_command(new Command_model(new Command_name("fill"), new Command_pos(), new Command_pos(), new Command_text("block_data"), new Command_choice(true, new Command_choice_part("hollow"), new Command_choice_part("destroy"), new Command_choice_part("keep"), new Command_choice_part("outline"), new Command_choice_part("replace", new Command_text("block_data_tag")))));
            parser.Add_command(new Command_model(new Command_name("function"), new Command_text("function_call")));

            parser.Add_command(new Command_model(new Command_name("gamemode"), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" }), new Command_entity(true, false, Entity_type_limitation.Only_player)));
            parser.Add_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)))));
            parser.Add_command(new Command_model(new Command_name("give"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text("item"), new Command_int(true)));
            parser.Add_command(new Command_model(new Command_name("help"), new Command_text()));
            parser.Add_command(new Command_model(new Command_name("kick", 3), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text(true, true)));
            parser.Add_command(new Command_model(new Command_name("kill"), new Command_entity()));
            parser.Add_command(new Command_model(new Command_name("list"), new Command_choice(new string[] { "uuids" }, true)));
            parser.Add_command(new Command_model(new Command_name("locate"), new Command_text("STRUCTURE")));
            parser.Add_command(new Command_model(new Command_name("me"), new Command_text(false, true)));
            parser.Add_command(new Command_model(new Command_name("msg"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text(false, true)));
            parser.Add_command(new Command_model(new Command_name("particle"), new Command_text("particle"), new Command_pos(true), new Command_pos(true), new Command_float(true), new Command_int(true), new Command_choice(new string[] { "force", "normal" }, true), new Command_entity(true)));

            parser.Add_command(new Command_model(new Command_name("playsound"), new Command_text("SOUND"), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }), new Command_entity(), new Command_pos(true), new Command_float(true), new Command_float(2, true), new Command_float(1, true)));
            parser.Add_command(new Command_model(new Command_name("publish", 4), new Command_int(65535, true)));
            parser.Add_command(new Command_model(new Command_name("recipe"), new Command_choice(new Command_choice_part("give", new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text("RECIPE")), new Command_choice_part("take", new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text("RECIPE")))));
            parser.Add_command(new Command_model(new Command_name("reload", 3)));
            parser.Add_command(new Command_model(new Command_name("replaceitem"), new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text("item"), new Command_int(true)), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), new Command_text("item"), new Command_int(true)))));
            parser.Add_command(new Command_model(new Command_name("say"), new Command_text(false, true)));
            parser.Add_command(new Command_model(new Command_name("scoreboard"), new Command_choice(new Command_choice_part("objectives", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text("scoreboard_criteria"), new Command_text("text_format", true)), new Command_choice_part("list"), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("displayname", new Command_text("text_format")), new Command_choice_part("rendertype", new Command_choice(new string[] { "hearts", "integer" })))), new Command_choice_part("remove", new Command_text()), new Command_choice_part("setdisplay", new Command_text("SCOREBOARD_DISPLAY"), new Command_text()))), new Command_choice_part("players", new Command_choice(new Command_choice_part("add", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("enable", new Command_entity(), new Command_text()), new Command_choice_part("get", new Command_entity(), new Command_text()), new Command_choice_part("list", new Command_entity()), new Command_choice_part("operation", new Command_entity(), new Command_text(), new Command_choice(new string[] { "%=", "*=", "+=", "-=", "/=", "<", "=", ">", "><" }), new Command_entity(), new Command_text()), new Command_choice_part("remove", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("reset", new Command_entity(), new Command_text(true)), new Command_choice_part("set", new Command_entity(), new Command_text(), new Command_int()))))));
            parser.Add_command(new Command_model(new Command_name("seed")));
            parser.Add_command(new Command_model(new Command_name("setblock"), new Command_pos(), new Command_text("block_data"), new Command_choice(new string[] { "replace", "keep", "destroy" }, true)));
            parser.Add_command(new Command_model(new Command_name("setworldspawn"), new Command_pos(true)));
            parser.Add_command(new Command_model(new Command_name("spawnpoint"), new Command_entity(true, false, Entity_type_limitation.Only_player), new Command_pos(true)));
            parser.Add_command(new Command_model(new Command_name("spreadplayers"), new Command_pos(false, true), new Command_float(), new Command_float(), new Command_bool(), new Command_entity()));
            parser.Add_command(new Command_model(new Command_name("stopsound"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_choice(new string[] { "*", "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }, true), new Command_text("SOUND", true)));
            parser.Add_command(new Command_model(new Command_name("summon"), new Command_text("ENTITY"), new Command_pos(true), new Command_text("entity_nbt", true)));
            parser.Add_command(new Command_model(new Command_name("tag"), new Command_entity(), new Command_choice(new Command_choice_part("add", new Command_text()), new Command_choice_part("remove", new Command_text()), new Command_choice_part("list"))));
            parser.Add_command(new Command_model(new Command_name("team"), new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text("text_format",true, true)), new Command_choice_part("empty", new Command_text()), new Command_choice_part("join", new Command_text(), new Command_entity()), new Command_choice_part("leave", new Command_entity()), new Command_choice_part("list", new Command_text()), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("collisionRule", new Command_bool()), new Command_choice_part("color", new Command_text("TEAM_COLOR")), new Command_choice_part("deathMessageVisibility", new Command_bool()), new Command_choice_part("displayName", new Command_text("text_format", false, true)), new Command_choice_part("friendlyFire", new Command_bool()), new Command_choice_part("nametagVisible", new Command_bool()), new Command_choice_part("prefix", new Command_text("text_format", false, true)), new Command_choice_part("seeFriendlyInvisibles", new Command_bool()), new Command_choice_part("suffix", new Command_text("text_format", false, true)))), new Command_choice_part("remove", new Command_text()))));
            parser.Add_command(new Command_model(new Command_name("teleport"), new Command_choice(new Command_choice_part(new Command_pos()), new Command_choice_part(new Command_entity(), new Command_choice(new Command_choice_part(new Command_pos(), new Command_pos(true, true)), new Command_choice_part(new Command_entity(false, true)))))));
            parser.Add_command(new Command_model(new Command_name("tell"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text()));
            parser.Add_command(new Command_model(new Command_name("tellraw"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text("text_format", false, true)));
            parser.Add_command(new Command_model(new Command_name("time"), new Command_choice(new Command_choice_part("add", new Command_int()), new Command_choice_part("query", new Command_choice(new string[] { "day", "daytime", "gametime" })), new Command_choice_part("set", new Command_choice(new Command_choice_part(new Command_int()), new Command_choice_part("day"), new Command_choice_part("midnight"), new Command_choice_part("night"), new Command_choice_part("noon"))))));
            parser.Add_command(new Command_model(new Command_name("title"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_choice(new Command_choice_part("actionbar", new Command_text("text_format", false, true)), new Command_choice_part("clear"), new Command_choice_part("reset"), new Command_choice_part("subtitle", new Command_text("text_format", false, true)), new Command_choice_part("times", new Command_int(), new Command_int(), new Command_int()), new Command_choice_part("title", new Command_text("text_format", false, true)))));
            parser.Add_command(new Command_model(new Command_name("trigger"), new Command_text(), new Command_choice(new string[] { "add", "set" }), new Command_int()));
            parser.Add_command(new Command_model(new Command_name("weather"), new Command_choice(new string[] { "clear", "rain", "thunder" }), new Command_int(true)));
            parser.Add_command(new Command_model(new Command_name("worldborder"), new Command_choice(new Command_choice_part("add", new Command_int(), new Command_int()), new Command_choice_part("center", new Command_pos()), new Command_choice_part("damage", new Command_choice(new string[] { "amount", "buffer" }), new Command_int()), new Command_choice_part("get"), new Command_choice_part("set", new Command_int(), new Command_int()), new Command_choice_part("warning", new Command_choice(new string[] { "distance", "time" }), new Command_int()))));
            parser.Add_command(new Command_model(new Command_name("experience"), new Command_choice(new string[] { "add", "query", "set" }), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_int(), new Command_choice(new string[] { "points", "levels" })));

            //Commands present on servers
            parser.Add_command(new Command_model(new Command_name("ban", 3), new Command_entity(false, false, Entity_type_limitation.Only_player_strict), new Command_text(true, true)));
            parser.Add_command(new Command_model(new Command_name("ban-ip", 3), new Command_entity(false, false, Entity_type_limitation.Only_player_strict), new Command_text(true, true)));
            parser.Add_command(new Command_model(new Command_name("banlist", 3), new Command_choice(new string[] { "ips", "players" }, true)));
            parser.Add_command(new Command_model(new Command_name("op", 4), new Command_entity(false, false, Entity_type_limitation.Only_player_strict)));
            parser.Add_command(new Command_model(new Command_name("deop", 4), new Command_entity(false, false, Entity_type_limitation.Only_player_strict)));
            parser.Add_command(new Command_model(new Command_name("pardon", 3), new Command_entity(false, false, Entity_type_limitation.Only_player_strict)));
            parser.Add_command(new Command_model(new Command_name("pardon-ip", 3), new Command_entity(false, false, Entity_type_limitation.Only_player_strict)));
            parser.Add_command(new Command_model(new Command_name("save-all", 4), new Command_choice(new string[] { "flush" }, true)));
            parser.Add_command(new Command_model(new Command_name("save-on", 4)));
            parser.Add_command(new Command_model(new Command_name("save-off", 4)));

            parser.Add_command(new Command_model(new Command_name("whitelist", 3), new Command_choice(new Command_choice_part("add", new Command_entity(false, false, Entity_type_limitation.Only_player)), new Command_choice_part("list"), new Command_choice_part("off"), new Command_choice_part("on"), new Command_choice_part("reload"), new Command_choice_part("remove", new Command_entity(false, false, Entity_type_limitation.Only_player)))));

            parser.Add_command(new Command_model(new Command_name("stop", 4)));
            //Aliases after commands
            parser.Add_alias("xp", "experience");
            parser.Add_alias("tp", "teleport");
            parser.Add_alias("w", "msg");

            parser.Add_replace_collection("NBT_SIZE", true, new List<string> { "byte", "double", "float", "int", "long", "short" });
            parser.Add_replace_collection("ALIGNMENT", true, new List<string> { "x", "y", "z", "xy", "xz", "yx", "yz", "zx", "zy", "xyz", "xzy", "yxz", "yzx", "zxy", "zyx" });

            Console.WriteLine("Parser " + version + " done");

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_13_1()
        {
            string version = "1.13.1";
            Command_parser parser = new(version, Get_parser("1.13"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("forceload", 3), new Command_choice(new Command_choice_part("add", new Command_pos(false, true), new Command_pos(true, true)), new Command_choice_part("query", new Command_pos(true,true)), new Command_choice_part("remove", new Command_choice(new Command_choice_part("all"), new Command_choice_part(new Command_pos(false, true), new Command_pos(true, true)))))));

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
            Command_parser parser = new(version, Get_parser("1.13.2"));
            Tree_add(parser, version);

            parser.Replace_validator("entity", new Selector_validator(true, false, false));
            parser.Set_validator_severity("LOOT_TABLE", Problem_severity.Warning);  //TODO this becomes a hard error at some point

            Command_choice nbt_hand = new(true, new Command_choice_part(new Command_text("item")), new Command_choice_part(new Command_choice(new string[] { "mainhand", "offhand" })));
            Command_choice loot_types = new(new Command_choice_part("fish", new Command_text("LOOT_TABLE"), new Command_pos(), nbt_hand), new Command_choice_part("kill", new Command_entity(false, true)), new Command_choice_part("loot", new Command_text("LOOT_TABLE")), new Command_choice_part("mine", new Command_pos(), nbt_hand));

            parser.Add_command(new Command_model(new Command_name("loot"), new Command_choice(new Command_choice_part("give", new Command_entity(false, false, Entity_type_limitation.Only_player), loot_types), new Command_choice_part("insert", new Command_pos(), loot_types), new Command_choice_part("replace", new Command_choice(new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), loot_types), new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), loot_types))), new Command_choice_part("spawn", new Command_pos(), loot_types))));
            parser.Add_command(new Command_model(new Command_name("teammsg"), new Command_text(false, true)));
            parser.Add_command(new Command_model(new Command_name("schedule"), new Command_choice(new Command_choice_part("function", new Command_text("function_call"), new Command_time()))));

            parser.Replace_command(new Command_model(new Command_name("time"), new Command_choice(new Command_choice_part("add", new Command_time()), new Command_choice_part("query", new Command_choice(new string[] { "day", "daytime", "gametime" })), new Command_choice_part("set", new Command_choice(new Command_choice_part(new Command_time()), new Command_choice_part("day"), new Command_choice_part("midnight"), new Command_choice_part("night"), new Command_choice_part("noon"))))));
            parser.Replace_command(Command_model_builder.Get_data(Versions.Get_own_version(version)));
            parser.Replace_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_1()
        {
            string version = "1.14.1";
            Command_parser parser = new(version, Get_parser("1.14"));
            Tree_add(parser, version);


            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_2()
        {
            string version = "1.14.2";
            Command_parser parser = new(version, Get_parser("1.14.1"));
            Tree_add(parser, version);


            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_3()
        {
            string version = "1.14.3";
            Command_parser parser = new(version, Get_parser("1.14.2"));
            Tree_add(parser, version);

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_14_4()
        {
            string version = "1.14.4";
            Command_parser parser = new(version, Get_parser("1.14.3"));
            Tree_add(parser, version);

            parser.Replace_command(new Command_model(new Command_name("reload", 2)));
            parser.Replace_command(new Command_model(new Command_name("debug",3), new Command_choice(new string[] { "start", "stop", "report"})));
            parser.Replace_command(new Command_model(new Command_name("forceload", 2), new Command_choice(new Command_choice_part("add", new Command_pos(false, true), new Command_pos(true, true)), new Command_choice_part("query", new Command_pos(true, true)), new Command_choice_part("remove", new Command_choice(new Command_choice_part("all"), new Command_choice_part(new Command_pos(false, true), new Command_pos(true, true)))))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_15()
        {
            string version = "1.15";
            Command_parser parser = new(version, Get_parser("1.14.4"));
            Tree_add(parser, version);

            parser.Replace_validator("entity", new Selector_validator(true, true, false));

            parser.Add_command(new Command_model(new Command_name("spectate"), new Command_entity(true, true), new Command_entity(true, true, Entity_type_limitation.Only_player)));

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)))));
            parser.Replace_command(new Command_model(new Command_name("schedule"), new Command_choice(new Command_choice_part("function", new Command_text("function_call"), new Command_time(), new Command_choice(new string[] { "append", "replace" }, true)), new Command_choice_part("clear", new Command_text("function_call", false, true)))));
            parser.Replace_command(new Command_model(new Command_name("kill"), new Command_entity(true)));
            parser.Replace_command(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(true), new Command_text("EFFECT", true)), new Command_choice_part("give", new Command_entity(), new Command_text("EFFECT"), new Command_int(true), new Command_int(true), new Command_bool(true)))));
            parser.Replace_command(Command_model_builder.Get_data(Versions.Get_own_version("1.15")));
            parser.Replace_command(Command_model_builder.Get_execute(Versions.Get_own_version("1.15")));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_15_1()
        {
            string version = "1.15.1";
            Command_parser parser = new(version, Get_parser("1.15"));
            Tree_add(parser, version);
            Parser_collection.Add(version, parser);
        }

        public static void Create_1_15_2()
        {
            string version = "1.15.2";
            Command_parser parser = new(version, Get_parser("1.15.1"));
            Tree_add(parser, version);
            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16()
        {
            string version = "1.16";
            Command_parser parser = new(version, Get_parser("1.15.2"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("locatebiome"), new Command_text("BIOME")));
            parser.Add_command(new Command_model(new Command_name("attribute"), new Command_entity(false, true), new Command_text("ATTRIBUTE"), new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("base", new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("set", new Command_float()))), new Command_choice_part("modifier", new Command_choice(new Command_choice_part("add", new Command_uuid(), new Command_text("text_format"), new Command_float(), new Command_choice(new string[] { "add", "multiply", "multiply_base" })), new Command_choice_part("remove", new Command_uuid()), new Command_choice_part("value", new Command_choice(new Command_choice_part("get", new Command_uuid(), new Command_float(true)))))))));

            parser.Replace_command(new Command_model(new Command_name("spreadplayers"), new Command_pos(false, true), new Command_float(), new Command_float(), new Command_choice(new Command_choice_part("false", new Command_entity()), new Command_choice_part("true", new Command_entity()), new Command_choice_part("under", new Command_float(), new Command_choice(new Command_choice_part("false", new Command_entity()), new Command_choice_part("true", new Command_entity()))))));
            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_1()
        {
            string version = "1.16.1";
            Command_parser parser = new(version, Get_parser("1.16"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_2()
        {
            string version = "1.16.2";
            Command_parser parser = new(version, Get_parser("1.16.1"));
            Tree_add(parser, version);

            parser.Replace_command(new Command_model(new Command_name("setworldspawn"), new Command_pos(true), new Command_float(true)));
            parser.Replace_command(new Command_model(new Command_name("spawnpoint"), new Command_entity(true, false, Entity_type_limitation.Only_player), new Command_pos(true), new Command_float(true)));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_3()
        {
            string version = "1.16.3";
            Command_parser parser = new(version, Get_parser("1.16.2"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_4()
        {
            string version = "1.16.4";
            Command_parser parser = new(version, Get_parser("1.16.3"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_16_5()
        {
            string version = "1.16.5";
            Command_parser parser = new(version, Get_parser("1.16.4"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_17()
        {
            string version = "1.17";
            Command_parser parser = new(version, Get_parser("1.16.5"));
            Tree_add(parser, version);

            Command_choice item_source = new(new Command_choice_part("with", new Command_text("item"), new Command_int(true)), new Command_choice_part("from", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text(true)), new Command_choice_part("entity", new Command_entity(false, true), new Command_text("SLOT"), new Command_text(true)))));
            parser.Replace_command(new Command_model(new Command_name("debug",3), new Command_choice(new string[] { "start", "stop"})));
            parser.Add_command(new Command_model(new Command_name("perf", 4), new Command_choice(new string[] { "start", "stop" })));
            parser.Add_command(new Command_model(new Command_name("item"), new Command_choice(new Command_choice_part("modify", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), new Command_text()), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), new Command_text()))), new Command_choice_part("replace", new Command_choice(new Command_choice_part("block", new Command_pos(), new Command_text("SLOT"), item_source), new Command_choice_part("entity", new Command_entity(), new Command_text("SLOT"), item_source))))));

            parser.Replace_command(new Command_model(new Command_name("debug", 3), new Command_choice(new Command_choice_part("start"), new Command_choice_part("stop"), new Command_choice_part("function", new Command_text("function_call",false,true)))));
            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()))));

            parser.Remove_command("replaceitem");

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_17_1()
        {
            string version = "1.17.1";
            Command_parser parser = new(version, Get_parser("1.17"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_18()
        {
            string version = "1.18";
            Command_parser parser = new(version, Get_parser("1.17.1"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("jfr"), new Command_choice(new string[] { "start", "stop" })));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_18_1()
        {
            string version = "1.18.1";
            Command_parser parser = new(version, Get_parser("1.18"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_18_2()
        {
            string version = "1.18.2";
            Command_parser parser = new(version, Get_parser("1.18.1"));
            Tree_add(parser, version);

            parser.Set_validator_severity("STRUCTURE_TAG", Problem_severity.Warning);

            parser.Add_command(new Command_model(new Command_name("placefeature"), new Command_text("FEATURE"), new Command_pos(true)));
            parser.Replace_command(new Command_model(new Command_name("locate"), new Command_text("STRUCTURE_TAG")));
            parser.Replace_command(new Command_model(new Command_name("locatebiome"), new Command_text("BIOME_TAG")));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_19()
        {
            string version = "1.19";
            Command_parser parser = new(version, Get_parser("1.18.2"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("place"), new Command_choice(new Command_choice_part("feature", new Command_text("FEATURE"), new Command_pos(true)), new Command_choice_part("jigsaw", new Command_text("JIGSAW"), new Command_float(), new Command_int(), new Command_pos(true)), new Command_choice_part("structure", new Command_text("STRUCTURE")), new Command_choice_part("template", new Command_text("TEMPLATE"), new Command_pos(true), new Command_choice(new string[] { "180", "clockwise_90", "counterclockwise_90", "none" }, true), new Command_choice(new string[] { "front_back", "left_right", "none" }, true), new Command_float(true), new Command_int(true)))));

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)))));
            //Locate structure aaa won't fail in 1.21.5, poi and biome will however
            parser.Replace_command(new Command_model(new Command_name("locate"), new Command_choice(new Command_choice_part("biome", new Command_text("BIOME_TAG")), new Command_choice_part("poi", new Command_text("POI_TAG")), new Command_choice_part("structure", new Command_text("STRUCTURE_TAG")))));

            //TODO at some point locate biome starts failing hard

            parser.Remove_command("placefeature");
            parser.Remove_command("locatebiome");

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_19_1()
        {
            string version = "1.19.1";
            Command_parser parser = new(version, Get_parser("1.19"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_19_2()
        {
            string version = "1.19.2";
            Command_parser parser = new(version, Get_parser("1.19.1"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_19_3()
        {
            string version = "1.19.3";
            Command_parser parser = new(version, Get_parser("1.19.2"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("fillbiome"), new Command_pos(), new Command_pos(), new Command_text("BIOME"), new Command_choice(true, new Command_choice_part("replace", new Command_text("BIOME_TAG")))));
            parser.Replace_command(new Command_model(new Command_name("publish", 4), new Command_bool(true), new Command_choice(new string[] { "adventure", "creative", "spectator", "survival" }), new Command_int(65535, true)));
            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)))));
            parser.Replace_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_19_4()
        {
            string version = "1.19.4";
            Command_parser parser = new(version, Get_parser("1.19.3"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("damage"), new Command_entity(false, true), new Command_float(), new Command_text("DAMAGE", true), new Command_choice(true, new Command_choice_part("by", new Command_entity(false, true), new Command_choice(true, new Command_choice_part("from", new Command_entity(false, true)))), new Command_choice_part("at", new Command_pos()))));
            parser.Add_command(new Command_model(new Command_name("ride"), new Command_entity(false, true), new Command_choice(new Command_choice_part("mount", new Command_entity(false, true)), new Command_choice_part("dismount"))));
            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)), new Command_choice_part("commandModificationBlockLimit", new Command_int(true)), new Command_choice_part("doVinesSpread", new Command_bool(true)))));

            parser.Replace_command(new Command_model(new Command_name("clone"), new Command_choice(true, new Command_choice_part(new Command_pos()), new Command_choice_part("from", new Command_text("DIMENSION"), new Command_pos())), new Command_pos(), new Command_choice(new Command_choice_part(new Command_pos()), new Command_choice_part("to", new Command_text("DIMENSION"), new Command_pos())), new Command_choice(true, new Command_choice_part("filtered", new Command_text("block_data_tag"), new Command_choice(new string[] { "force", "move", "normal" }, true)), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" }, true)), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" }, true)))));

            parser.Replace_command(new Command_model(new Command_name("weather"), new Command_choice(new string[] { "clear", "rain", "thunder" }), new Command_time(true)));
            parser.Replace_command(new Command_model(new Command_name("title"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_choice(new Command_choice_part("actionbar", new Command_text("text_format", false, true)), new Command_choice_part("clear"), new Command_choice_part("reset"), new Command_choice_part("subtitle", new Command_text("text_format", false, true)), new Command_choice_part("times", new Command_time(), new Command_int(), new Command_int()), new Command_choice_part("title", new Command_text("text_format", false, true)))));
            parser.Replace_command(new Command_model(new Command_name("effect"), new Command_choice(new Command_choice_part("clear", new Command_entity(true), new Command_text("EFFECT", true)), new Command_choice_part("give", new Command_entity(), new Command_text("EFFECT"), new Command_choice(true, new Command_choice_part(new Command_int(true)), new Command_choice_part("infinite")), new Command_int(true), new Command_bool(true)))));

            parser.Replace_command(Command_model_builder.Get_data(Versions.Get_own_version(version)));
            parser.Replace_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));

            Parser_collection.Add(version, parser);

        }

        public static void Create_1_20()
        {
            string version = "1.20";
            Command_parser parser = new(version, Get_parser("1.19.4"));
            Tree_add(parser, version);

            parser.Add_command(new Command_model(new Command_name("return"), new Command_int()));

            //TODO also negative index strings but that is currently not blocked in earlier

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_20_1()
        {
            string version = "1.20.1";
            Command_parser parser = new(version, Get_parser("1.20"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        //Macros added here
        //Multi lined functions
        public static void Create_1_20_2()
        {
            string version = "1.20.2";
            Command_parser parser = new(version, Get_parser("1.20.1"), true, true);
            Tree_add(parser, version);

            //These ranges seems to allow everything, intentinally, like random asd will generate minecraft:asd
            parser.Add_command(new Command_model(new Command_name("random"), new Command_choice(new Command_choice_part("reset", new Command_text(), new Command_int(true), new Command_bool(true), new Command_bool(true)), new Command_choice_part("value", new Command_int(false, true), new Command_text(true)), new Command_choice_part("roll", new Command_int(false,true), new Command_text(true)))));

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)), new Command_choice_part("commandModificationBlockLimit", new Command_int(true)), new Command_choice_part("doVinesSpread", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("enderPearlsVanishOnDeath", new Command_bool(true)))));
            parser.Replace_command(new Command_model(new Command_name("function"), new Command_text("function_call"), new Command_choice(true, new Command_choice_part("with", new Command_choice(new Command_choice_part("storage", new Command_text(), new Command_text("nbt_path", true)), new Command_choice_part("entity", new Command_entity(), new Command_text("nbt_path", true)), new Command_choice_part("block", new Command_pos(), new Command_text("nbt_path", true)))), new Command_choice_part(new Command_text("nbt")))));

            Parser_collection.Add(version, parser);
        }
        public static void Create_1_20_3()
        {
            string version = "1.20.3";
            Command_parser parser = new(version, Get_parser("1.20.2"));
            Tree_add(parser, version);

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)), new Command_choice_part("commandModificationBlockLimit", new Command_int(true)), new Command_choice_part("doVinesSpread", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("enderPearlsVanishOnDeath", new Command_bool(true)), new Command_choice_part("maxCommandForkCount", new Command_int(true)), new Command_choice_part("projectilesCanBreakBlocks", new Command_bool(true)), new Command_choice_part("playersNetherPortalDefaultDelay", new Command_bool(true)), new Command_choice_part("playersNetherPortalCreativeDelay", new Command_bool(true)) )));
            parser.Replace_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));
            parser.Replace_command(new Command_model(new Command_name("return"), new Command_choice(new Command_choice_part(new Command_int()), new Command_choice_part("fail"), new Command_choice_part(new Command_execute_stop(false, true)))));
            parser.Replace_command(new Command_model(new Command_name("scoreboard"), new Command_choice(new Command_choice_part("objectives", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_text("scoreboard_criteria"), new Command_text("text_format",true, true)), new Command_choice_part("list"), new Command_choice_part("modify", new Command_text(), new Command_choice(new Command_choice_part("numberformat", new Command_choice(new Command_choice_part("blank"), new Command_choice_part("fixed", new Command_text()), new Command_choice_part("styled", new Command_text()))), new Command_choice_part("displayautoupdate", new Command_bool()), new Command_choice_part("displayname", new Command_text("text_format")), new Command_choice_part("rendertype", new Command_choice(new string[] { "hearts", "integer" })))), new Command_choice_part("remove", new Command_text()), new Command_choice_part("setdisplay", new Command_text("SCOREBOARD_DISPLAY"), new Command_text()))), new Command_choice_part("players", new Command_choice(new Command_choice_part("display", new Command_choice(new Command_choice_part("name", new Command_entity(), new Command_text(), new Command_text("text_format",false, true)), new Command_choice_part("numberformat", new Command_entity(), new Command_text(), new Command_choice(new Command_choice_part("blank"), new Command_choice_part("fixed", new Command_text()), new Command_choice_part("styled", new Command_text()))))), new Command_choice_part("add", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("enable", new Command_entity(), new Command_text()), new Command_choice_part("get", new Command_entity(), new Command_text()), new Command_choice_part("list", new Command_entity()), new Command_choice_part("operation", new Command_entity(), new Command_text(), new Command_choice(new string[] { "%=", "*=", "+=", "-=", "/=", "<", "=", ">", "><" }), new Command_entity(), new Command_text()), new Command_choice_part("remove", new Command_entity(), new Command_text(), new Command_int()), new Command_choice_part("reset", new Command_entity(), new Command_text(true)), new Command_choice_part("set", new Command_entity(), new Command_text(), new Command_int()))))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_20_4()
        {
            string version = "1.20.4";
            Command_parser parser = new(version, Get_parser("1.20.3"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        //TODO massive changes to nbt, components introduced

        public static void Create_1_20_5()
        {
            //TODO allow inlined: loot item execute if predicate
            string version = "1.20.5";
            Command_parser parser = new(version, Get_parser("1.20.4"), null);
            Tree_add(parser, version);

            List<string> slot = parser.Get_collection("SLOT");

            slot.Add("armor.*");
            slot.Add("container.*");
            slot.Add("enderchest.*");
            slot.Add("horse.*");
            slot.Add("hotbar.*");
            slot.Add("inventory.*");
            slot.Add("player.crafting.*");
            slot.Add("villager.*");
            slot.Add("weapon.*");

            parser.Add_replace_collection("SLOT_MULTIPLE", false, slot);

            parser.Replace_validator("item", new Item_validator(true));
            parser.Replace_validator("item_tag", new Item_tag_validator(true));

            parser.Replace_validator("particle", new Particle_validator(true));

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)), new Command_choice_part("commandModificationBlockLimit", new Command_int(true)), new Command_choice_part("doVinesSpread", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("enderPearlsVanishOnDeath", new Command_bool(true)), new Command_choice_part("maxCommandForkCount", new Command_int(true)), new Command_choice_part("projectilesCanBreakBlocks", new Command_bool(true)), new Command_choice_part("playersNetherPortalDefaultDelay", new Command_bool(true)), new Command_choice_part("playersNetherPortalCreativeDelay", new Command_bool(true)), new Command_choice_part("spawnChunkRadius", new Command_int(true)))));
            parser.Replace_command(Command_model_builder.Get_execute(Versions.Get_own_version(version)));
            parser.Replace_command(new Command_model(new Command_name("attribute"), new Command_entity(false, true), new Command_text("ATTRIBUTE"), new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("base", new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("set", new Command_float()))), new Command_choice_part("modifier", new Command_choice(new Command_choice_part("add", new Command_uuid(), new Command_text("text_format"), new Command_float(), new Command_choice(new string[] { "add_value", "add_multiplied_base", "add_multiplied_total" })), new Command_choice_part("remove", new Command_uuid()), new Command_choice_part("value", new Command_choice(new Command_choice_part("get", new Command_uuid(), new Command_float(true)))))))));
            parser.Replace_command(new Command_model(new Command_name("playsound"), new Command_text("SOUND"), new Command_choice(new string[] { "ambient", "block", "hostile", "master", "music", "neutral", "player", "record", "voice", "weather" }, true), new Command_entity(true), new Command_pos(true), new Command_float(true), new Command_float(2, true), new Command_float(1, true)));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_20_6()
        {
            string version = "1.20.6";
            Command_parser parser = new(version, Get_parser("1.20.5"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        //TODO structure tag seems to be empty in reg
        public static void Create_1_21()
        {
            string version = "1.21";
            Command_parser parser = new(version, Get_parser("1.20.6"));
            Tree_add(parser, version);

            parser.Replace_validator("entity", new Selector_validator(true, true, true));

            parser.Replace_command(new Command_model(new Command_name("attribute"), new Command_entity(false, true), new Command_text("ATTRIBUTE"), new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("base", new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("set", new Command_float()))), new Command_choice_part("modifier", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_float(), new Command_choice(new string[] { "add_value", "add_multiplied_base", "add_multiplied_total" })), new Command_choice_part("remove", new Command_text()), new Command_choice_part("value", new Command_choice(new Command_choice_part("get", new Command_text(), new Command_float(true)))))))));

            Parser_collection.Add(version, parser);
        }

        //TODO structure tag seems to be empty in reg
        public static void Create_1_21_1()
        {
            string version = "1.21.1";
            Command_parser parser = new(version, Get_parser("1.21"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_21_2()
        {
            string version = "1.21.2";
            Command_parser parser = new(version, Get_parser("1.21.1"));
            Tree_add(parser, version);

            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)), new Command_choice_part("commandModificationBlockLimit", new Command_int(true)), new Command_choice_part("doVinesSpread", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("enderPearlsVanishOnDeath", new Command_bool(true)), new Command_choice_part("maxCommandForkCount", new Command_int(true)), new Command_choice_part("projectilesCanBreakBlocks", new Command_bool(true)), new Command_choice_part("playersNetherPortalDefaultDelay", new Command_bool(true)), new Command_choice_part("playersNetherPortalCreativeDelay", new Command_bool(true)), new Command_choice_part("spawnChunkRadius", new Command_int(true)), new Command_choice_part("minecartMaxSpeed", new Command_int(true)), new Command_choice_part("disablePlayerMovementCheck", new Command_bool(true)))));
            parser.Add_command(new Command_model(new Command_name("rotate"),new Command_entity(false,true), new Command_choice(new Command_choice_part(new Command_pos(false,true)), new Command_choice_part("facing", new Command_choice(new Command_choice_part(new Command_pos()), new Command_choice_part("entity", new Command_entity(false,true), new Command_choice(new string[] {"eyes","feet" },true)))))));

            //TODO new components

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_21_3()
        {
            string version = "1.21.3";
            Command_parser parser = new(version, Get_parser("1.21.2"));
            Tree_add(parser, version);

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_21_4()
        {
            string version = "1.21.4";
            Command_parser parser = new(version, Get_parser("1.21.3"));
            Tree_add(parser, version);

            //TODO custom model data changes not something that is relevant right now

            parser.Replace_command(new Command_model(new Command_name("attribute"), new Command_entity(false, true), new Command_text("ATTRIBUTE"), new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("base", new Command_choice(new Command_choice_part("get", new Command_float(true)), new Command_choice_part("set", new Command_float()), new Command_choice_part("reset"))), new Command_choice_part("modifier", new Command_choice(new Command_choice_part("add", new Command_text(), new Command_float(), new Command_choice(new string[] { "add_value", "add_multiplied_base", "add_multiplied_total" })), new Command_choice_part("remove", new Command_text()), new Command_choice_part("value", new Command_choice(new Command_choice_part("get", new Command_text(), new Command_float(true)))))))));

            Parser_collection.Add(version, parser);
        }

        public static void Create_1_21_5()
        {
            string version = "1.21.5";
            Command_parser parser = new(version, Get_parser("1.21.4"));
            Tree_add(parser, version);

            parser.Replace_validator("text_format", new Text_format_validator(true));

            parser.Add_command(new Command_model(new Command_name("test"), new Command_choice(new Command_choice_part("clearall", new Command_int(true)), new Command_choice_part("clearthat"), new Command_choice_part("clearthese"), new Command_choice_part("create", new Command_text(), new Command_int(true), new Command_choice(true, new Command_choice_part(new Command_int(), new Command_int()))), new Command_choice_part("locate", new Command_text()), new Command_choice_part("resetclosest"), new Command_choice_part("resetthese"), new Command_choice_part("resetthat"), new Command_choice_part("pos", new Command_text()), new Command_choice_part("run", new Command_text(), new Command_int(true), new Command_bool(true), new Command_int(true), new Command_int(true)), new Command_choice_part("runclosest", new Command_int(true), new Command_bool(true)), new Command_choice_part("runfailed", new Command_text(), new Command_int(true), new Command_bool(true), new Command_int(true), new Command_int(true)), new Command_choice_part("runmultiple", new Command_text(), new Command_int(true)), new Command_choice_part("runthese", new Command_int(true), new Command_bool(true)), new Command_choice_part("runtthat", new Command_int(true), new Command_bool(true)), new Command_choice_part("stop"), new Command_choice_part("verify", new Command_text()))));

            parser.Replace_command(new Command_model(new Command_name("tellraw"), new Command_entity(false, false, Entity_type_limitation.Only_player), new Command_text(false, true)));  //TODO bandaid solution
            parser.Replace_command(new Command_model(new Command_name("gamerule"), new Command_choice(new Command_choice_part("announceAdvancements", new Command_bool(true)), new Command_choice_part("commandBlockOutput", new Command_bool(true)), new Command_choice_part("disableElytraMovementCheck", new Command_bool(true)), new Command_choice_part("doDaylightCycle", new Command_bool(true)), new Command_choice_part("doEntityDrops", new Command_bool(true)), new Command_choice_part("doFireTick", new Command_bool(true)), new Command_choice_part("doLimitedCrafting", new Command_bool(true)), new Command_choice_part("doMobLoot", new Command_bool(true)), new Command_choice_part("doMobSpawning", new Command_bool(true)), new Command_choice_part("doTileDrops", new Command_bool(true)), new Command_choice_part("doWeatherCycle", new Command_bool(true)), new Command_choice_part("keepInventory", new Command_bool(true)), new Command_choice_part("logAdminCommands", new Command_bool(true)), new Command_choice_part("maxCommandChainLength", new Command_int(true)), new Command_choice_part("maxEntityCramming", new Command_int(true)), new Command_choice_part("mobGriefing", new Command_bool(true)), new Command_choice_part("naturalRegeneration", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("reducedDebugInfo", new Command_bool(true)), new Command_choice_part("sendCommandFeedback", new Command_bool(true)), new Command_choice_part("showDeathMessages", new Command_bool(true)), new Command_choice_part("spawnRadius", new Command_int(true)), new Command_choice_part("spectatorsGenerateChunks", new Command_bool(true)), new Command_choice_part("disableRaids", new Command_bool(true)), new Command_choice_part("doInsomna", new Command_bool(true)), new Command_choice_part("doImmediateRespawn", new Command_bool(true)), new Command_choice_part("drowningDamage", new Command_bool(true)), new Command_choice_part("fallDamage", new Command_bool(true)), new Command_choice_part("fireDamage", new Command_bool(true)), new Command_choice_part("doPatrolSpawning", new Command_bool(true)), new Command_choice_part("doTraderSpawning", new Command_bool()), new Command_choice_part("forgiveDeadPlayers", new Command_bool(true)), new Command_choice_part("universalAnger", new Command_bool(true)), new Command_choice_part("freezeDamage", new Command_bool()), new Command_choice_part("playersSleepingPercentage", new Command_int()), new Command_choice_part("doWardenSpawning", new Command_bool(true)), new Command_choice_part("blockExplosionDropDecay", new Command_bool(true)), new Command_choice_part("mobExplosionDropDecay", new Command_bool(true)), new Command_choice_part("tntExplosionDropDecay", new Command_bool(true)), new Command_choice_part("snowAccumulationHeight", new Command_int(true)), new Command_choice_part("waterSourceConversion", new Command_bool(true)), new Command_choice_part("lavaSourceConversion", new Command_bool(true)), new Command_choice_part("globalSoundEvents", new Command_bool(true)), new Command_choice_part("commandModificationBlockLimit", new Command_int(true)), new Command_choice_part("doVinesSpread", new Command_bool(true)), new Command_choice_part("randomTickSpeed", new Command_int(true)), new Command_choice_part("enderPearlsVanishOnDeath", new Command_bool(true)), new Command_choice_part("maxCommandForkCount", new Command_int(true)), new Command_choice_part("projectilesCanBreakBlocks", new Command_bool(true)), new Command_choice_part("playersNetherPortalDefaultDelay", new Command_bool(true)), new Command_choice_part("playersNetherPortalCreativeDelay", new Command_bool(true)), new Command_choice_part("spawnChunkRadius", new Command_int(true)), new Command_choice_part("minecartMaxSpeed", new Command_int(true)), new Command_choice_part("disablePlayerMovementCheck", new Command_bool(true)), new Command_choice_part("allowFireTicksAwayFromPlayer", new Command_bool(true)), new Command_choice_part("tntExplodes", new Command_bool(true)))));
            parser.Replace_command(new Command_model(new Command_name("fill"), new Command_pos(), new Command_pos(), new Command_text("block_data"), new Command_choice(true, new Command_choice_part("hollow"), new Command_choice_part("destroy"), new Command_choice_part("keep"), new Command_choice_part("outline"), new Command_choice_part("strict"), new Command_choice_part("replace", new Command_text("block_data_tag"), new Command_choice(true, new Command_choice_part("hollow"), new Command_choice_part("destroy"), new Command_choice_part("outline"), new Command_choice_part("strict"))))));
            parser.Replace_command(new Command_model(new Command_name("setblock"), new Command_pos(), new Command_text("block_data"), new Command_choice(new string[] { "replace", "keep", "destroy","strict" }, true)));
            parser.Replace_command(new Command_model(new Command_name("clone"), new Command_choice(true, new Command_choice_part(new Command_pos()), new Command_choice_part("from", new Command_text("DIMENSION"), new Command_pos())), new Command_pos(), new Command_choice(new Command_choice_part(new Command_pos()), new Command_choice_part("to", new Command_text("DIMENSION"), new Command_pos())), new Command_choice(true, new Command_choice_part("strict", new Command_choice(new Command_choice_part("filtered", new Command_text("block_data_tag"), new Command_choice(new string[] { "force", "move", "normal" },true)), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" },true)), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" },true)))), new Command_choice_part("filtered", new Command_text("block_data_tag"), new Command_choice(new string[] { "force", "move", "normal" },true)), new Command_choice_part("masked", new Command_choice(new string[] { "force", "move", "normal" }, true)), new Command_choice_part("replace", new Command_choice(new string[] { "force", "move", "normal" }, true)))));
            parser.Replace_command(new Command_model(new Command_name("place"), new Command_choice(new Command_choice_part("feature", new Command_text("FEATURE"), new Command_pos(true)), new Command_choice_part("jigsaw", new Command_text("JIGSAW"), new Command_float(), new Command_int(), new Command_pos(true)), new Command_choice_part("structure", new Command_text("STRUCTURE")), new Command_choice_part("template", new Command_text("TEMPLATE"), new Command_pos(true), new Command_choice(new string[] { "180", "clockwise_90", "counterclockwise_90", "none" }, true), new Command_choice(new string[] { "front_back", "left_right", "none" }, true), new Command_float(true), new Command_int(true), new Command_choice(new string[] { "strict"},true)))));


            Parser_collection.Add(version, parser);
        }

        private static void Tree_add(Command_parser parser, string version)
        {
            //The regular which can have a "minecraft:" before

            string current_register = Get_register(version);

            if (Directory.Exists(current_register + "/Regular"))
            {
                string[] namespaces = Directory.GetFiles(current_register + "/Regular");
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    List<string> entire = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    parser.Add_replace_collection(name, true, entire);
                }
            }

            if (Directory.Exists(current_register + "/Regular/Tags"))
            {
                //The tags
                string[] namespaces = Directory.GetFiles(current_register + "/Regular/Tags");

                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    //Assuming the validator_name exists already
                    List<string> tags_added = new(parser.Get_collection(name));

                    List<string> raw = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    List<string> tags = new();

                    for (int i = 0; i < raw.Count; i++)
                    {
                        tags.Add("#" + raw[i]);
                    }

                    tags_added.AddRange(tags);

                    parser.Add_replace_collection(name + "_TAG", true, tags_added);
                }
            }

            if (Directory.Exists(current_register))
            {
                //The other which can't have a "minecraft:" before
                //Better called the namespaceless
                string[] namespaces = Directory.GetFiles(current_register);
                foreach (string namespace_ in namespaces)
                {
                    string name = Path.GetFileNameWithoutExtension(namespace_).ToUpper();

                    List<string> entire = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(namespace_));
                    parser.Add_replace_collection(name, false, entire);
                }
            }

            //Some special handling

            //List<string> scoreboard_criteria_list = parser.Get_namespace("SCOREBOARD_CRITERIA");
            //List<string> item_hand = parser.Get_namespace("ITEM");
            //List<string> block = parser.Get_namespace("BLOCK");
            //List<string> entity = parser.Get_namespace("ENTITY");

            //foreach (string current_item in item_hand)
            //{
            //    scoreboard_criteria_list.Add("minecraft.crafted:" + current_item.Replace(':', '.'));
            //    scoreboard_criteria_list.Add("minecraft.dropped:" + current_item.Replace(':', '.'));
            //    scoreboard_criteria_list.Add("minecraft.picked_up:" + current_item.Replace(':', '.'));
            //    scoreboard_criteria_list.Add("minecraft.used:" + current_item.Replace(':', '.'));
            //}

            //foreach (string current_block in block)
            //{
            //    scoreboard_criteria_list.Add("minecraft.broken:" + current_block.Replace(':', '.'));
            //    scoreboard_criteria_list.Add("minecraft.mineed:" + current_block.Replace(':', '.'));
            //}

            //foreach (string current_entity in entity)
            //{
            //    scoreboard_criteria_list.Add("minecraft.killed:" + current_entity.Replace(':', '.'));
            //    scoreboard_criteria_list.Add("minecraft.killed_by:" + current_entity.Replace(':', '.'));
            //}
        }

        private static string Get_register(string version)
        {
            //Some are currently present internally in source (because 1 they are not present in the extracted data, 2 misode doesn't host them)

            string path = AppDomain.CurrentDomain.BaseDirectory + "/Registers/" + version.Replace('.', '_');

            //Is it already present (in the database, not just in source)
            if (File.Exists(path + "/Info.txt"))
            {
                return path;
            }

            Register_downloader.Download_to_directory(version, path);

            //TODO want this to fail safely somehow
            //Perhaps retries, then wait until working?

            return path;
        }
    }
}
