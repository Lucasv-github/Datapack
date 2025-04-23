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

        public string[] Print(bool strip_empty, bool strip_comments)
        {
            List<string> lines = new();

            for(int i = 0; i < Commands.Count; i++)
            {
                string line = Commands[i].Print(strip_comments);

                if(line == null)  //If skipping comments we don't want to introduce a empty if we don't strip that
                {
                    continue;
                }

                if(line == "" && strip_empty)
                {
                    continue;
                }

                lines.Add(line);
            }

            return lines.ToArray();
        }
    }
}
