using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Command_parsing.Command_parts;

namespace Command_parsing.Validators
{
    public class Function_call_validator : Validator
    {
        public override void Validate(Command command, object external_data, out string error)
        {
            string function = (string)external_data;

            string[] parts = function.Split(':');

            if(parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
            {
                error = "Cannot parse: " + function + " as a function name";
                return;
            }

            command.Parser.Result.Called_functions.Add(function);

            error = "";
        }
    }
}
