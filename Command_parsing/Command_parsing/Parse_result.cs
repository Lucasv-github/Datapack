namespace Command_parsing
{
    public class Parse_result
    {
        public List<Command> Commands;
        public List<string> Called_functions;

        public Parse_result()
        {
            Commands = new();
            Called_functions = new();
        }
    }
}
