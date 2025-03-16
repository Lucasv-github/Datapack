using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Command_parsing.Command_parts
{
    public class Command_str_text : Command_part
    {
        public string Value;

        public override string ToString()
        {
            return Value;
        }

        public Command_str_text()
        {

        }

        public Command_str_text(bool optional)
        {
            Optional = optional;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_exception("Expected a str text, got nothing");
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
                    throw new Command_parse_exception("Expected a str text, got: " + text);
                //} 
            }

            Command_str_text str_text = new()
            {
                Value = text
            };

            done = false;
            return str_text;
        }
    }
}
