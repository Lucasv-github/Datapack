using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_uuid : Command_part
    {
        //Set
        public string uuid;

        public override Command_part Validate(Command command, out bool done)
        {
            string uuid = command.Read_next();

            if(!Guid.TryParse(uuid, out _))
            {
                throw new Command_parse_exception("Cannot parser: " + uuid + " as an uuid");
            }

            Command_uuid command_uuid = new()
            {
                uuid = uuid
            };

            done = false;
            return command_uuid;
        }
    }
}
