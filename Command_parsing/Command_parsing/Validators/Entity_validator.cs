using System.Globalization;
using Command_parsing.Command_parts;

namespace Command_parsing.Validators
{
    public class Entity_validator : Validator
    {
        private readonly bool entity_tags;
        private readonly bool predicates;
        private readonly bool no_length_limit;
        private readonly bool at_n;
        public Entity_validator(bool entity_tags, bool predicates, bool no_length_limit, bool at_n)
        {
            this.entity_tags = entity_tags;
            this.predicates = predicates;
            this.no_length_limit = no_length_limit;
            this.at_n = at_n;
        }

        public override void Validate(Command command, object data, out string error)
        {
            Tuple<Command_entity, Command_entity> data_tuple = (Tuple<Command_entity, Command_entity>)data;

            Command_entity model = data_tuple.Item1;
            Command_entity entity = data_tuple.Item2;

            if (entity.Value.StartsWith('@'))
            {
                string text = entity.Value;

                string selector;
                string selector_arguments;

                int bracket_start = text.IndexOf('[');
                int bracket_end = text.LastIndexOf(']');

                if (bracket_start == -1)
                {
                    selector = text;
                }
                else
                {
                    selector = text[..bracket_start];
                }

                Entity_type_limitation type_limitation = Entity_type_limitation.None;


                if (!(selector == "@a" || selector == "@e" || selector == "@s" || selector == "@p" || selector == "@r" || (selector == "@n" && at_n)))
                {
                    error = "Unknown selector: " + text;
                    return;
                }

                if (selector == "@a" || selector == "@p" || selector == "@r")  //@s as a player is fine here
                {
                    type_limitation = Entity_type_limitation.Only_player_strict;
                }

                if (selector == "@s")
                {
                    type_limitation = Entity_type_limitation.Only_player;
                }

                bool only_one = false;

                if (selector == "@s" || selector == "@p" || selector == "@r" || selector == "@n")  //@s as a player is fine here
                {
                    only_one = true;
                }

                if (bracket_start == -1 && bracket_end == -1)
                {

                }
                else if (bracket_start != -1 && bracket_end != -1)
                {
                    selector_arguments = text.Substring(bracket_start + 1, bracket_end - bracket_start - 1);

                    string[] parts = Command_parser.Split_ignore(selector_arguments, ',').ToArray();

                    HashSet<string> taken = new();
                    List<string> type_modifiers = new();
                    List<string> gamemode_modifiers = new();

                    foreach (string part in parts)
                    {
                        string[] sub_parts = part.Split(new char[] { '=' }, 2);

                        if (sub_parts.Length != 2)
                        {
                            error = "Cannot parse selector argument: " + part;
                            return;
                        }

                        string type = sub_parts[0];
                        string follower = sub_parts[1];

                        if (follower.Length == 0)
                        {
                            error = "Cannot parse selector argument: " + part;
                            return;
                        }

                        //Some bypass the single limit
                        if (!(type == "tag" || type == "gamemode" || type == "nbt" || type == "predicate" || type == "type"))
                        {
                            if (taken.Contains(type))
                            {
                                error = "Selector argument: " + type + " already used";
                                return;
                            }
                        }

                        taken.Add(type);

                        switch (type)
                        {
                            case "advancements":  //Can not be empty
                                                  //TODO check with validator_name

                                //TODO validates like scoreboard
                                //[advancements={ asd = true}

                                break;
                            case "distance":  //Can not be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "dx":  //Can not be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "dy":  //Can not be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "dz":  //Can not be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "gamemode":  //Can not be empty
                                gamemode_modifiers.Add(follower);

                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                if (!(follower == "survival" || follower == "creative" || follower == "spectator" || follower == "adventure"))
                                {
                                    error = "Cannot parse: " + follower + " as a gamemode";
                                    return;
                                }

                                break;
                            case "level":  //Can no be empty
                                Command_int.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "limit":
                                if (!int.TryParse(follower, NumberStyles.Float, CultureInfo.InvariantCulture, out int limit))
                                {
                                    error = "Cannot parse: " + follower + " as a number";
                                    return;
                                }

                                if (limit == 1)
                                {
                                    only_one = true;
                                }

                                break;
                            case "name":  //Can be empty, can have ""
                                          //Name can have "" and that allow many special chars such as ä

                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                //TODO some valid chars?
                                break;
                            case "nbt":  //Can not be empty
                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                //TODO nbt validator_name
                                break;
                            case "scores":  //Can not be empty
                                if (sub_parts.Length < 2)
                                {
                                    error = "Cannot parse: " + sub_parts + " as scores";
                                }

                                string part_no_bracket = follower[1..^1];
                                string[] all_scores = part_no_bracket.Split(',');

                                for (int i = 0; i < all_scores.Length; i++)
                                {
                                    if (all_scores[i] == "")
                                    {
                                        //Last can apparently be empty, and only last
                                        if (i == all_scores.Length - 1)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            error = "Cannot parse scores: " + part_no_bracket;
                                            return;
                                        }
                                    }

                                    string[] score_parts = all_scores[i].Split('=');
                                    string name = score_parts[0];
                                    string range = score_parts[1];

                                    Command_int.Validate_range(range, out _, out _, out error);
                                    if (error != "") { return; }
                                }


                                break;
                            case "sort":  //Cannot be empty
                                if (selector == "@s")  //This only applies for @s for some reason
                                {
                                    error = "Sort cannot be used here";
                                    return;
                                }

                                if (!(follower == "nearest" || follower == "furthest" || follower == "arbitrary" || follower == "random"))
                                {
                                    error = "Cannot parse: " + follower + " as a sort option";
                                    return;
                                }
                                break;
                            case "tag":  //Can be empty
                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                //TODO some valid chars? not öäå
                                //Tag cannot have ""
                                break;
                            case "team":  //Can be empty
                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                //TODO some valid chars?
                                break;
                            case "type":  //Cannot be empty
                                type_modifiers.Add(follower);

                                if (selector == "@p" || selector == "@a")
                                {
                                    error = "Type cannot be used here";
                                    return;
                                }

                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                if (follower == "minecraft:player" || follower == "player")
                                {
                                    type_limitation = Entity_type_limitation.Only_player_strict;
                                }

                                if (entity_tags)
                                {
                                    command.Parser.Verify_collection("ENTITY_TAG", follower, out error);
                                    if (error != "") { return; }
                                }
                                else
                                {
                                    command.Parser.Verify_collection("ENTITY", follower, out error);
                                    if (error != "") { return; }
                                }

                                //TODO check with validator_name

                                break;
                            case "x": //Cannot be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "x_rotation": //Cannot be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "y": //Cannot be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "y_rotation": //Cannot be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "z":  //Cannot be empty
                                Command_float.Validate_range(follower, out _, out _, out error);
                                if (error != "") { return; }
                                break;
                            case "predicate":  //Can be empty
                                if (!predicates) goto default;

                                if (follower.StartsWith('!'))
                                {
                                    follower = follower[1..];
                                }

                                //TODO some valid chars? not öäå
                                //Tag cannot have ""

                                break;

                            default:
                                error = "Type: " + type + " is an unknown selector argument";
                                return;
                        }
                    }

                    if (gamemode_modifiers.Count > 1)  //If we have two or more selectors ALL need to be negative
                    {
                        foreach (string modifier in gamemode_modifiers)
                        {
                            if (!modifier.StartsWith('!'))
                            {
                                error = "Only negative gamemodes can be used here, got: " + modifier;
                                return;
                            }
                        }
                    }

                    if (type_modifiers.Count > 1)  //If we have two or more selectors ALL need to be negative
                    {
                        foreach (string modifier in type_modifiers)
                        {
                            if (!modifier.StartsWith('!'))
                            {
                                error = "Only negative types can be used here, got: " + modifier;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    error = "Cannot get selector arguments from: " + selector;
                    return;
                }

                if (model.Only_one && !only_one)
                {
                    error = "Only one entity can be used here, got: " + selector;
                    return;
                }

                if (model.Type_limitation == Entity_type_limitation.Only_player && type_limitation == Entity_type_limitation.None)
                {
                    error = "Only players can be used here, got: " + selector;
                    return;
                }

                if (model.Type_limitation == Entity_type_limitation.Only_player_strict && (type_limitation == Entity_type_limitation.None || type_limitation == Entity_type_limitation.Only_player))
                {
                    error = "Only players can be used here (strict), got: " + selector;
                    return;
                }

                error = "";
                return;
            }

            if (Guid.TryParse(entity.Value, out _))
            {
                error = "";
                return;
            }

            if(!no_length_limit && entity.Value.Length > 16)
            {
                error = "Entity name: " + entity.Value + " is longer than 16 chars";
                return;
            }

            error = "";
            return;
        }
    }
}
