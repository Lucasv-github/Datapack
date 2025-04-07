namespace Command_parsing.Command_parts
{
    public class Command_name : Command_part
    {
        //Both
        public string Name;
        public int Permission_level;

        public Command_name(string name, int permission_level = 2)
        {
            Name = name;
            Permission_level = permission_level;
        }

        public override string ToString()
        {
            return Name;
        }

        public override string Get_nice_name()
        {
            return "Command";
        }

        public override Command_part Validate(Command command, out string error)
        {
            //Have already found it by name, so no reason to validate that

            string name = command.Read_next();

            if (Permission_level > command.Parser.Permission_level)
            {
                //For some reason someone thought it would be a good idea to support only "forceload query"
                if (name == "forceload" && (command.Parser.Version == "1.14" || command.Parser.Version == "1.14.1" || command.Parser.Version == "1.14.2" || command.Parser.Version == "1.14.3"))
                {
                    string next = command.Read_next();
                    command.Read_index--;

                    if (next == "query")
                    {
                        error = "";
                        return new Command_text(name);
                    }
                }

                error = "Command: " + name + " expected a permission level of: " + Permission_level + " but current is: " + command.Parser.Permission_level;
                return null;
            }

            error = "";
            return new Command_text(name);
        }
    }
}
