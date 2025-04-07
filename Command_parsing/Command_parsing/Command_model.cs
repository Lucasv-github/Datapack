namespace Command_parsing
{
    public class Command_model : ICloneable
    {
        public Command_part[] Parts;

        public Command_model() { }

        public Command_model(params Command_part[] parts)
        {
            Parts = new Command_part[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                Parts[i] = (Command_part)parts.GetValue(i);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
