﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datapack
{
    public class Versions
    {
        public const int Min = 0;
        public const int Max = 38;

        public static string Ger_minecraft_version(int numerical_version)
        {
            return Get_minecraft_version(numerical_version, out _);
        }

        public static string Get_minecraft_version(int numerical_version, out bool known)
        {
            known = true;

            switch (numerical_version)
            {
                case 4:
                    return "1.13-1.14.4"; //Added the initial pack format versions of 4.
                case 5:
                    return "1.15-1.16.1"; //Added predicates.
                case 6:
                    return "1.16.2-1.16.5"; //Added experimental support for custom world generation.
                case 7:
                    return "1.17-1.17.1"; //The /replaceitem command was replaced with /item. The set_damage loot function now requires a valid [String] type field.
                case 8:
                    return "1.18-1.18.1"; //Loot table functions set_contents and set_loot_table now require a [String] type field. Removed length limits for scoreboards, score holders, and team names.
                case 9:
                    return "1.18.2-1.18.2"; //The /locate command now takes a configured structure as its first parameter rather than a structure type, so many grouped structures now require a structure type tag.
                case 10:
                    return "1.19-1.19.3"; //Data packs can now have a [NBT Compound / JSON Object] filter section in pack.mcmeta. Merged /locatebiome with /locate, changing its syntax.
                case 12:
                    return "1.19.4-1.19.4"; //Added damage types. Removed all boolean flags in damage predicates, instead damage type tags can now be tested for.
                case 15:
                    return "1.20-1.20.1"; //Changed sign NBT. All fields in placed_block, item_used_on_block, and allay_drop_item_on_block advancement triggers have been collapsed to a single location field.
                case 18:
                    return "1.20.2-1.20.2"; //Added function macros. Effects now use namespaced IDs rather than numeric values in NBT.
                case 26:
                    return "1.20.3-1.20.4"; //Text components are parsed more strictly. Renamed grass block and item to short_grass. Added scoreboard display names and number formats.
                case 41:
                    return "1.20.5-1.20.6"; //Renamed the sweeping enchantment to sweeping_edge. Changed the behavior of the item_used_on_block advancement trigger.
                case 48:
                    return "1.21-1.21.1"; //Added data driven enchantments. Renamed the enchantment field to enchantments in the item sub predicate.
                case 57:
                    return "1.21.2-1.21.3"; //Removed attribute ID prefixes such as generic.. Changed formats of data components, loot tables, and predicates.
                case 61:
                    return "1.21.4-1.21.4"; //Renamed tnt minecart TNTFuse to fuse. Added required field duration to trail particle.
                case 67:
                    return "1.21.5-1.21.5"; //Text components are now saved as objects in NBT rather than strings containing JSON.
                default:
                    known = false;
                    return "Unknown version";
            }
        }

        //Using own versions numbers as mincraft's isn't granular enough
        public static string Get_own_version(int number)
        {
            switch (number)
            {
                case 0: return "1.13";
                case 1: return "1.13.1";
                case 2: return "1.13.2";
                case 3: return "1.14";
                case 4: return "1.14.1";
                case 5: return "1.14.2";
                case 6: return "1.14.3";
                case 7: return "1.14.4";
                case 8: return "1.15";
                case 9: return "1.15.1";
                case 10: return "1.15.2";
                case 11: return "1.16";
                case 12: return "1.16.1";
                case 13: return "1.16.2";
                case 14: return "1.16.3";
                case 15: return "1.16.4";
                case 16: return "1.16.5";
                case 17: return "1.17";
                case 18: return "1.17.1";
                case 19: return "1.18";
                case 20: return "1.18.1";
                case 21: return "1.18.2";
                case 22: return "1.19";
                case 23: return "1.19.1";
                case 24: return "1.19.2";
                case 25: return "1.19.3";
                case 26: return "1.19.4";
                case 27: return "1.20";
                case 28: return "1.20.1";
                case 29: return "1.20.2";
                case 30: return "1.20.3";
                case 31: return "1.20.4";
                case 32: return "1.20.5";
                case 33: return "1.20.6";
                case 34: return "1.21";
                case 35: return "1.21.1";
                case 36: return "1.21.2";
                case 37: return "1.21.3";
                case 38: return "1.21.4";
                default: return null;
            }
        }

        public static int Get_own_version(string version)
        {
            switch (version)
            {
                case "1.13": return 0;
                case "1.13.1": return 1;
                case "1.13.2": return 2;
                case "1.14": return 3;
                case "1.14.1": return 4;
                case "1.14.2": return 5;
                case "1.14.3": return 6;
                case "1.14.4": return 7;
                case "1.15": return 8;
                case "1.15.1": return 9;
                case "1.15.2": return 10;
                case "1.16": return 11;
                case "1.16.1": return 12;
                case "1.16.2": return 13;
                case "1.16.3": return 14;
                case "1.16.4": return 15;
                case "1.16.5": return 16;
                case "1.17": return 17;
                case "1.17.1": return 18;
                case "1.18": return 19;
                case "1.18.1": return 20;
                case "1.18.2": return 21;
                case "1.19": return 22;
                case "1.19.1": return 23;
                case "1.19.2": return 24;
                case "1.19.3": return 25;
                case "1.19.4": return 26;
                case "1.20": return 27;
                case "1.20.1": return 28;
                case "1.20.2": return 29;
                case "1.20.3": return 30;
                case "1.20.4": return 31;
                case "1.20.5": return 32;
                case "1.20.6": return 33;
                case "1.21": return 34;
                case "1.21.1": return 35;
                case "1.21.2": return 36;
                case "1.21.3": return 37;
                case "1.21.4": return 38;
                default: return -1;
            }
        }

        public static string Get_min_minecraft_version(int numerical_version)
        {
            string version = Get_minecraft_version(numerical_version, out bool known);

            if (!known)
            {
                return "Unknown version";
            }

            return version.Split('-')[0];
        }

        public static string Get_max_minecraft_version(int numerical_version)
        {
            string version = Get_minecraft_version(numerical_version, out bool known);

            if (!known)
            {
                return "Unknown version";
            }

            return version.Split('-')[1];
        }
    }
}
