using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_entity : Command_part
    {
        //Model

        //Set 
        public string Entity_selector;
        public Entity_type Type;

        public Command_entity() { }

        public Command_entity(bool optional)
        { 
            Optional = optional;
        }

        public override string ToString()
        {
            return Entity_selector;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            string text = command.Read_next();

            if (text == null)
            {
                if(Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_excpetion("Expected entity, got nothing");
            }

            Command_entity entity = new();

            if (text.StartsWith("@a") || text.StartsWith("@e") || text.StartsWith("@s") || text.StartsWith("@p") || text.StartsWith("@r"))
            {
                entity.Entity_selector = text;
                entity.Type = Entity_type.Selector;
            }
            else
            {
                entity.Entity_selector = text;
                entity.Type = Entity_type.Fake_player;
            }
            

            done = false;
            return entity;
        }
    }

    public enum Entity_type
    {
        Selector = 0,
        Player = 1,
        Fake_player = 2,
        UUID = 3,
    }
}
