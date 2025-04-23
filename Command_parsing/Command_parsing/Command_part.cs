namespace Command_parsing
{
    public abstract class Command_part
    {
        public bool Optional;
        public string Value;

        //Uses self as a model and returns new
        public virtual Command_part Validate(Command command, out string error)
        {
            throw new NotSupportedException();
        }

        //public static void Validate_macro(string value)
        //{
        //    
        //    if(!Is_macro(value))
        //    {
        //        throw new Command_parse_exception("Cannot validate: " + value + " as a macro");
        //    }
        //}

        //public static bool Is_macro(string value)
        //{
        //    int macro_start = value.IndexOf("$(");

        //    if (macro_start == -1)
        //    {
        //        return false;
        //    }

        //    int macro_end = value.IndexOf(")");

        //    if (macro_end < macro_start)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //Sets self
        public virtual bool Set_validate(string part)
        {
            throw new NotSupportedException();
        }
        public virtual string Get_nice_name()
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            throw new NotSupportedException();
        }
    }
}
