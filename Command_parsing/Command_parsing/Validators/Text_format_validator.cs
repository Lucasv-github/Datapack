using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Command_parsing.Validators
{
    public class Text_format_validator : Validator
    {
        private readonly bool component;

        public Text_format_validator(bool component)
        {
            this.component = component;
        }

        public override void Validate(Command command, object external_data, out string error)
        {
            string text = (string)external_data;

            if(component)
            {

            }
            else
            {
                try
                {
                    JsonNode.Parse(text);
                }
                catch /*(Exception ex)*/
                {
                    //Does this catch everything?
                    //if (ex is JsonReaderException || ex is FormatException)
                    //{
                    error = "Expected json text, got: " + text;
                    return;
                    //} 
                }
            }

            error = "";
        }
    }
}
