namespace Command_parsing.Validators
{
    public class Particle_validator : Validator
    {
        private readonly bool nbt;

        public Particle_validator(bool nbt)
        {
            this.nbt = nbt;
        }

        public override void Validate(Command command, object external_data, out string error)
        {
            string entire = (string)external_data;

            if (nbt)
            {
                int particlen_end_index = entire.IndexOf('{');

                string particle_type;

                if (particlen_end_index != -1)
                {
                    particle_type = entire[..particlen_end_index];
                }
                else
                {
                    particle_type = entire;
                }

                int start_index = entire.IndexOf('{');
                int end_index = entire.LastIndexOf('}');

                string nbt_data;

                if (start_index == -1 && end_index == -1)
                {
                    nbt_data = "";
                }
                else if (start_index != -1 && end_index != -1)
                {
                    nbt_data = entire.Substring(start_index, end_index - start_index + 1);
                }
                else
                {
                    error = "Cannot get nbt data from: " + entire;
                    return;
                }

                command.Parser.Verify_collection("PARTICLE", particle_type, out error);
            }
            else
            {
                string particle_type = entire;

                if (particle_type == "dust" || particle_type == "minecraft:dust")
                {
                    command.Read_next();
                    command.Read_next();
                    command.Read_next();
                    command.Read_next();
                }

                command.Parser.Verify_collection("PARTICLE", particle_type, out error);
            }
        }
    }
}
