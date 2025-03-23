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
        public bool Only_one;
        public bool Only_player;

        //Set 
        public string Entity_selector;
        public Entity_type Type;

        public Command_entity() { }

        public Command_entity(bool optional)
        { 
            Optional = optional;
        }

        public Command_entity(bool optional, bool only_one, bool only_player)
        {
            Optional = optional;

            Only_one = only_one;
            Only_player = only_player;  //TODO enum, false, true, strict
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

                throw new Command_parse_exception("Expected entity, got nothing");
            }

            Command_entity entity = new();

            if (text.StartsWith("@a") || text.StartsWith("@e") || text.StartsWith("@s") || text.StartsWith("@p") || text.StartsWith("@r"))
            {
                entity.Entity_selector = text;
                entity.Type = Entity_type.Selector;

                command.Parser.Selector_validator.Validate(command,this,entity);
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
