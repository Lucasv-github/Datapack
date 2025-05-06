namespace Command_parsing.Validators
{
    public class Scoreboard_criteria_validator : Validator
    {
        public Scoreboard_criteria_validator()
        {

        }

        public override void Validate(Command command, object external_data, string validator_params, out string error)
        {
            string value = (string)external_data;

            command.Parser.Verify_collection("SCOREBOARD_CRITERIA", value, out error);

            if (error == "")
            {
                return;
            }

            string[] parts = value.Split(':');

            if (parts.Length != 2)
            {
                error = "Cannot parse: " + value + " as a scoreboard criteria";
                return;
            }

            string prefix = parts[0];
            string suffix = parts[1];

            suffix = suffix.Replace('.', ':');

            if (prefix == "minecraft.crafted" || prefix == "crafted" || prefix == "minecraft.dropped" || prefix == "dropped" || prefix == "minecraft.picked_up" || prefix == "picked_up" || prefix == "minecraft.used" || prefix == "used")
            {
                command.Parser.Verify_collection("ITEM", suffix, out error);

                if (error == "")
                {
                    return;
                }
            }

            if (prefix == "minecraft.broken" || prefix == "broken" || prefix == "minecraft.mined" || prefix == "mined")
            {
                command.Parser.Verify_collection("BLOCK", suffix, out error);

                if (error == "")
                {
                    return;
                }
            }

            if (prefix == "minecraft.killed" || prefix == "killed" || prefix == "minecraft.killed_by" || prefix == "killed_by")
            {
                command.Parser.Verify_collection("ENTITY", suffix, out error);

                if (error == "")
                {
                    return;
                }
            }

            error = "Cannot parse: " + value + " as a scoreboard criteria";
        }
    }
}
