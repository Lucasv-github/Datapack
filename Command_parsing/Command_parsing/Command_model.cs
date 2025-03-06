using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command_parsing
{
    public class Command_model
    {
        public Command_part[] Parts;

        public Command_model(params Command_part[] parts) 
        {
            Parts = new Command_part[parts.Length];

            for(int i = 0; i < parts.Length; i++)
            {
                Parts[i] = (Command_part)parts.GetValue(i);
            }
        }
    }
}
