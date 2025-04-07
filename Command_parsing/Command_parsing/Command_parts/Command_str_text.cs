using System.Text.Json.Nodes;

namespace Command_parsing.Command_parts
{
    public class Command_str_text : Command_part
    {
        public string Value;
        public bool To_end;

        public override string ToString()
        {
            return Value;
        }

        public Command_str_text()
        {

        }

        public Command_str_text(bool optional, bool to_end = true)
        {
            Optional = optional;
            To_end = to_end;
        }

        public override string Get_nice_name()
        {
            return "String text";
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

                error = "Expected a str text, got nothing";
                return null;
            }

            if (To_end)
            {
                string next = command.Read_next();

                while (next != null)
                {
                    text += " " + next;
                    next = command.Read_next();
                }
            }

            //Split will not split anything inside ""

            try
            {
                JsonNode.Parse(text);
            }
            catch /*(Exception ex)*/
            {
                //Does this catch everything?
                //if (ex is JsonReaderException || ex is FormatException)
                //{
                error = "Expected a str text, got: " + text;
                return null;
                //} 
            }

            Command_str_text str_text = new()
            {
                Value = text
            };

            error = "";
            return str_text;
        }
    }
}
