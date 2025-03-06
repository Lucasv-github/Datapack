using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_pos : Command_part
    {
        public int X;
        public Pos_type Type_x;

        public int Y;
        public Pos_type Type_y;

        public int Z;
        public Pos_type Type_z;

        public Command_pos(bool optional = false) 
        {
            Optional = optional;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            string text_x = command.Read_next();

            //No pos at all
            if (text_x == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_excpetion("Expected a pos, got nothing");
            }

            string text_y = command.Read_next();
            string text_z = command.Read_next();

            if (text_y == null || text_z == null)
            {
                if(text_y == null)
                {
                    throw new Command_parse_excpetion("Expected a pos, got: " + text_x);
                }
                else  //Z was null then
                {
                    throw new Command_parse_excpetion("Expected a pos, got: " + text_x + text_y);
                }
            }

            Command_pos position = new();

            Parse_pos(text_x, out position.Type_x, out position.X);
            Parse_pos(text_y, out position.Type_y, out position.Y);
            Parse_pos(text_z, out position.Type_z, out position.Z);

            done = false;
            return position;

            void Parse_pos(string input, out Pos_type type, out int value)
            {
                if (int.TryParse(input, out value))
                {
                    type = Pos_type.Absolue;
                    return;
                }
                else if (input.StartsWith('~'))
                {
                    if(input.Length == 1)
                    {
                        value = 0;
                        type = Pos_type.Offset;
                        return;
                    }
                    else if(int.TryParse(input.Substring(1), out value))
                    {
                        type = Pos_type.Offset;
                        return;
                    }
                }
                else if (input.StartsWith('^'))
                {
                    if (input.Length == 1)
                    {
                        value = 0;
                        type = Pos_type.Ray;
                        return;
                    }
                    else if (int.TryParse(input.Substring(1), out value))
                    {
                        type = Pos_type.Ray;
                        return;
                    }
                }

                throw new Command_parse_excpetion("Can not parse: " + input + " as a position");
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
