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

        public override Command_part Validate(Command command, out bool done)
        {
            Command_str_text return_text = new();

            string text = command.Read_next();

            if (text == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_excpetion("Expected a str text, got nothing");
            }

            //Split will not split anything inside ""

            try
            {
                JsonNode.Parse(text);
            }
            catch (Exception ex)
            {
                //Does this catch everything?
                if (ex is JsonReaderException || ex is FormatException)
                {
                    throw new Command_parse_excpetion("Expected a str text, got: " + text);
                } 
            }

            Command_str_text str_text = new();
            str_text.Value = text;

            done = false;
            return return_text;
        }
    }
}
