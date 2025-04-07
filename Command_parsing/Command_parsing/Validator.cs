namespace Command_parsing
{
    public class Validator
    {
        public Problem_severity Severity;
        public Validator()
        {
            Severity = Problem_severity.Error;
        }

        //TODO better standardization than object if possible
        public virtual void Validate(Command command, object external_data, out string error)
        {
            throw new NotSupportedException();
        }
    }
}
