using System.Globalization;

namespace Command_parsing.Command_parts
{
    public class Command_pos : Command_part
    {
        //Model
        public bool Small;

        public Command_pos(bool optional = false, bool small = false)
        {
            Optional = optional;
            Small = small;
        }

        public override string Get_nice_name()
        {
            if(Small)
            {
                return "Position: X Y";
            }

            return "Position: X Y Z";
        }
        public override string ToString()
        {
            return Value;
        }
        public override Command_part Validate(Command command, out string error)
        {
            string text_x = command.Read_next();

            //No pos at all
            if (text_x == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }

                error = "Expected a pos, got nothing";
                return null;
            }

            string text_y = command.Read_next();

            string text_z = null;

            if (!Small)
            {
                text_z = command.Read_next();
            }

            if (text_y == null || text_z == null)
            {
                if (text_y == null)
                {
                    error = "Expected a pos, got: " + text_x;
                    return null;
                }
                else if (!Small)  //Z was null then
                {
                    error = "Expected a pos, got: " + text_x + text_y;
                    return null;
                }
            }

            Command_pos position;

            if (Small)
            {
                position = new()
                {
                    Value = text_x + " " + text_y
                };
            }
            else
            {
                position = new()
                {
                    Value = text_x + " " + text_y + " " + text_z
                };
            }

            Parse_pos(text_x, out _, out _, out error);
            if (error != "") { return null; }

            Parse_pos(text_y, out _, out _, out error);
            if (error != "") { return null; }

            if (!Small)
            {
                Parse_pos(text_z, out _, out _, out error);
                if (error != "") { return null; }
            }

            return position;

            static void Parse_pos(string input, out Pos_type type, out float value, out string error)
            {
                if (input.StartsWith('~'))
                {
                    if (input.Length == 1)
                    {
                        value = 0;
                        type = Pos_type.Offset;
                        error = "";
                        return;
                    }

                    if (float.TryParse(input.AsSpan(1), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        type = Pos_type.Offset;
                        error = "";
                        return;
                    }
                }
                else if (input.StartsWith('^'))
                {
                    if (input.Length == 1)
                    {
                        value = 0;
                        type = Pos_type.Ray;
                        error = "";
                        return;
                    }

                    if (float.TryParse(input.AsSpan(1), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        type = Pos_type.Ray;
                        error = "";
                        return;
                    }
                }
                else
                {
                    if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        type = Pos_type.Absolue;
                        error = "";
                        return;
                    }
                }

                type = Pos_type.Absolue;
                error = "Can not parse: " + input + " as a position";
            }
        }
    }

    public enum Pos_type
    {
        Absolue = 0,
        Offset = 1,
        Ray = 2,
    }
}
