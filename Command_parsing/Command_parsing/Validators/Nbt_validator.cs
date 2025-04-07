namespace Command_parsing.Validators
{
    public class Nbt_validator : Validator
    {
        public Nbt_validator()
        {

        }

        //TODO implement
        public override void Validate(Command command, object external_data, out string error)
        {
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine((string)external_data);
            //Console.ResetColor();
            error = "";
        }
    }
    public class Nbt_path_validator : Validator
    {
        public Nbt_path_validator()
        {

        }

        //TODO implement
        public override void Validate(Command command, object external_data, out string error)
        {
            //Console.ForegroundColor = ConsoleColor.Magenta;
            //Console.WriteLine((string)external_data);
            //Console.ResetColor();
            error = "";
        }
    }

    public class Entity_nbt_validator : Validator
    {
        public Entity_nbt_validator()
        {

        }

        //TODO implement
        public override void Validate(Command command, object external_data, out string error)
        {
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine((string)external_data);
            //Console.ResetColor();
            error = "";
        }
    }
}
