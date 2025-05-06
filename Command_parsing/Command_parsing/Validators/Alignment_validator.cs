using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Validators
{
    public class Alignment_validator : Validator
    {
        private readonly HashSet<string> alignments = new() { "x", "y", "z", "xy", "xz", "yx", "yz", "zx", "zy", "xyz", "xzy", "yxz", "yzx", "zxy", "zyx" };

        public override void Validate(Command command, object external_data, string validator_params, out string error)
        {
            string alignment = (string)external_data;

            if(alignments.Contains(alignment))
            {
                error = "";
            }
            else
            {
                error = alignment + " is not a valid alignment";
            }
        }
    }
}
