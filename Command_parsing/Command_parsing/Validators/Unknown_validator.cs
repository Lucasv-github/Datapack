using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Validators
{
    public class Unknown_validator : Validator
    {
        private string expected_validator;

        public Unknown_validator(string expected_validator)
        {
            this.expected_validator = expected_validator;
        }

        public override void Validate(Command command, object external_data, out string error)
        {
            error = " Validator: " + expected_validator + " requested but doesn't exist, this fallback prevents crashing";
        }
    }
}
