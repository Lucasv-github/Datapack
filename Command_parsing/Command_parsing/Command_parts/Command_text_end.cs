using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Command_parts
{
    public class Command_text_end : Command_part
    {
        //This text will read to the end (say test text)

        //Model

        //Set
        public string Value;
        public Command_text_end()
        {
            Value = "";
        }
        public Command_text_end(bool optional)
        {
            Optional = optional;
            Value = "";
        }

        //public Command_text_end(string collection, bool optional = false)
        //{
        //    Value = "";
        //    Optional = optional;
        //}

        public override string ToString()
        {
            return Value;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            Command_text return_text = new()
            {
                Value = ""
            };

            while (true)
            {
                string text = command.Read_next();

                if(text == null)
                {
                    break;
                }

                return_text.Value += text + " ";
            }

            if(return_text.Value.Length == 0)
            {
                if(Optional)
                {
                    done = false;
                    return return_text;
                }

                throw new Command_parse_exception("Expected a text, got nothing");
            }

            return_text.Value = return_text.Value.Remove(return_text.Value.Length - 1);

            done = true;
            return return_text;
        }
    }
}
