namespace Command_parsing
{
    public abstract class Validator
    {
        public Problem_severity Severity;
        public Validator()
        {
            Severity = Problem_severity.Error;
        }

        //TODO better standardization than object if possible
        public abstract void Validate(Command command, object external_data, string validator_params, out string error);
    }
}
