using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Validators
{
    public class Scoreboard_validator : Validator
    {
        private readonly bool no_length_limit;

        public Scoreboard_validator(bool no_length_limit)
        {
            this.no_length_limit = no_length_limit;
        }
        public override void Validate(Command command, object external_data, string validator_params, out string error)
        {
            string objective = (string)external_data;

            if(!no_length_limit && objective.Length > 16)
            {
                error = "Scoreboard objective: " + objective + " is longer than 16 chars";
                return;
            }

            error = "";
            return;
        }
    }
}
