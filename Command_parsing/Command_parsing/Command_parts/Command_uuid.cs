namespace Command_parsing.Command_parts
{
    public class Command_uuid : Command_part
    {
        //Set
        public string uuid;


        public override string Get_nice_name()
        {
            return "UUID";
        }
        public override Command_part Validate(Command command, out string error)
        {
            string uuid = command.Read_next();

            if (!Guid.TryParse(uuid, out _))
            {
                error = "Cannot parser: " + uuid + " as an uuid";
                return null;
            }

            Command_uuid command_uuid = new()
            {
                uuid = uuid
            };

            error = "";
            return command_uuid;
        }
    }
}
