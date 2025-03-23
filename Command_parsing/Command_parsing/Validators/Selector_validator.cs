using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;

namespace Command_parsing.Validators
{
    public class Selector_validator
    {
        private readonly bool entity_tags;
        private readonly bool predicates;
        public Selector_validator(bool entity_tags, bool predicates)
        { 
            this.entity_tags = entity_tags;
            this.predicates = predicates;
        }

        public void Validate(Command command, Command_entity model, Command_entity entity)
        {
            string text = entity.Entity_selector;

            string selector = text[..2];

            bool only_player = false;

            if (selector == "@a" || selector == "@p" || selector == "@s" || selector == "@r")  //@s as a player is fine here
            {
                only_player = true;
            }

            bool only_one = false;

            if (selector == "@s" || selector == "@p" || selector == "@r")  //@s as a player is fine here
            {
                only_one = true;
            }

            string inside;
            int start_index = text.IndexOf('[');
            int end_index = text.LastIndexOf(']');

            if (start_index == -1 && end_index == -1)
            {

            }
            else if (start_index != -1 && end_index != -1)
            {
                inside = text.Substring(start_index + 1, end_index - start_index - 1);

                string[] parts = Command_parser.Split_ignore(inside, ',').ToArray();

                foreach (string part in parts)
                {
                    string[] sub_parts = part.Split(new char[] { '=' }, 2);

                    if (sub_parts.Length != 2)
                    {
                        throw new Command_parse_exception("Cannot parse selector argument: " + part);
                    }

                    string type = sub_parts[0];
                    string follower = sub_parts[1];

                    if (follower.Length == 0)
                    {
                        throw new Command_parse_exception("Cannot parse selector argument: " + part);
                    }
                    //TODO some of these can also only be used once

                    switch (type)
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

                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

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

                            if (limit == 1)
                            {
                                only_one = true;
                            }

                            break;
                        case "name":
                            //Name can have "" and that allow many special chars such as ä

                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

                            //TODO some valid chars?
                            break;
                        case "nbt":
                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

                            //TODO nbt validator
                            break;
                        case "scores":
                            if (sub_parts.Length < 2)
                            {
                                throw new Command_parse_exception("Cannot parse: " + sub_parts + " as scores");
                            }

                            string part_no_bracket = follower[1..^1];
                            string[] all_scores = part_no_bracket.Split(',');

                            for (int i = 0; i < all_scores.Length; i++)
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
                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

                            //TODO some valid chars? not öäå
                            //Tag cannot have ""
                            break;
                        case "team":
                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

                            //TODO some valid chars?
                            break;
                        case "type":
                            if (selector == "@p" || selector == "@a")
                            {
                                throw new Command_parse_exception("Type cannot be used here");
                            }

                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

                            if (follower == "minecraft:player" || follower == "player")
                            {
                                only_player = true;
                            }

                            if(entity_tags)
                            {
                                if (!command.Parser.Get_collection("ENTITY_TAG").Contains(follower))
                                {
                                    throw new Command_parse_exception("Collection: ENTITY does not contain: " + follower);
                                }
                            }
                            else
                            {
                                if (!command.Parser.Get_collection("ENTITY").Contains(follower))
                                {
                                    throw new Command_parse_exception("Collection: ENTITY does not contain: " + follower);
                                }
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
                        case "predicate":
                            if(!predicates) goto default;

                            if (follower.StartsWith('!'))
                            {
                                follower = follower[1..];
                            }

                            //TODO some valid chars? not öäå
                            //Tag cannot have ""

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
    }
}
