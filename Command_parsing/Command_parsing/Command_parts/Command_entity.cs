namespace Command_parsing.Command_parts
{
    public class Command_entity : Command_part
    {
        //Model
        public bool Only_one;
        public Entity_type_limitation Type_limitation;

        //Set 
        public string Entity_selector;
        public Entity_type Type;

        public Command_entity() { }

        //public Command_entity(bool optional)
        //{
        //    Optional = optional;
        //    Type_limitation = Entity_type_limitation.None;
        //}

        public Command_entity(bool optional, bool only_one = false, Entity_type_limitation only_player = Entity_type_limitation.None)
        {
            Optional = optional;

            Only_one = only_one;
            Type_limitation = only_player;
        }

        public override string Get_nice_name()
        {
            return "Entity";
        }

        public override string ToString()
        {
            return Entity_selector;
        }

        public override Command_part Validate(Command command, out string error)
        {
            string text = command.Read_next();
            if (text == null)
            {
                if (Optional)
                {
                    error = "";
                    return null;
                }

                error = "Expected entity, got nothing";
                return null;
            }

            Command_entity entity = new();

            if (text.StartsWith('@'))
            {
                entity.Entity_selector = text;
                entity.Type = Entity_type.Selector;

                command.Parser.Get_validator("entity").Validate(command, new Tuple<Command_entity, Command_entity>(this, entity), out error);

                if (error != "")
                {
                    return null;
                }
            }
            else
            {
                entity.Entity_selector = text;
                entity.Type = Entity_type.Fake_player;
            }

            error = "";
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

    public enum Entity_type_limitation
    {
        None = 1,
        Only_player = 2,
        Only_player_strict = 3, //Ban doesn't allow @s without a @s[type=!player]
    }
}
