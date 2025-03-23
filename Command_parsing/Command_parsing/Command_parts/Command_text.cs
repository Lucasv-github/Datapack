using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Command_parsing.Command_parts
{
    public class Command_text : Command_part
    {
        //This text will read to the next space

        //Model
        public readonly string Collection;
        private readonly Action<Command_parser,string> validator;

        //Set
        public string Value;
        public Command_text() 
        {
        }
        public Command_text(bool optional) 
        {
            Optional = optional;
        }

        public override string ToString()
        {
            return Value;
        }
        public Command_text(string collection, bool optional = false)
        {
            if(collection.EndsWith('S'))
            {
                throw new ArgumentException(nameof(collection) + " should probably not be plural");
            }

            Optional = optional;
            Collection = collection;
        }

        public Command_text(Action<Command_parser,string> validator, bool optional = false)
        {
            Optional = optional;
            this.validator = validator;
        }

        public override Command_part Validate(Command command, out bool done)
        {
            Command_text return_text = new();

            string value = command.Read_next();

            if (value == null)
            {
                if (Optional)
                {
                    done = false;
                    return null;
                }

                throw new Command_parse_exception("Expected a text, got nothing");
            }

            validator?.Invoke(command.Parser,value);

            if (Collection != null)
            {
                command.Parser.Verify_collection(Collection,value);
            }

            return_text.Value = value;

            done = false;
            return return_text;
        }
    }
}
