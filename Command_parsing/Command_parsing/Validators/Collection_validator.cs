namespace Command_parsing.Validators
{
    public class Collection_validator : Validator
    {
        private readonly string collection;
        public Collection_validator(string collection)
        {
            this.collection = collection;
        }

        public override void Validate(Command command, object external_data, string validator_params, out string error)
        {
            command.Parser.Verify_collection(collection, (string)external_data, out error);

            //TODO this in subfunction
            if(error != "" && Severity == Problem_severity.Warning)
            {
                command.Parser.Write_warning(command,error);
                error = "";
            }
        }
    }
}
