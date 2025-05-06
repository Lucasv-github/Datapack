using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing.Validators
{
    public class Nbt_size_validator : Validator
    {
        private readonly HashSet<string> sizes = new() { "byte", "double", "float", "int", "long", "short" };

        public override void Validate(Command command, object external_data, string validator_params, out string error)
        {
            string alignment = (string)external_data;

            if (sizes.Contains(alignment))
            {
                error = "";
            }
            else
            {
                error = alignment + " is not a valid NBT size";
            }
        }
    }
}
