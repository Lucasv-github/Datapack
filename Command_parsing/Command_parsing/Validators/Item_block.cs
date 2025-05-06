namespace Command_parsing.Validators
{
    public class Item_block_utils
    {
        public static void Separate_nbt_item(bool components, string entire, out string item, out string nbt_data, out string error)
        {
            error = "";
            nbt_data = "";

            if (components)
            {
                int iten_end_index = entire.IndexOf('[');

                if (iten_end_index != -1)
                {
                    item = entire[..iten_end_index];
                }
                else
                {
                    item = entire;
                }

                int start_index = entire.IndexOf('[');
                int end_index = entire.LastIndexOf(']');

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
            }
            else
            {
                int iten_end_index = entire.IndexOf('{');

                if (iten_end_index != -1)
                {
                    item = entire[..iten_end_index];
                }
                else
                {
                    item = entire;
                }

                int start_index = entire.IndexOf('{');
                int end_index = entire.LastIndexOf('}');

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
            }
        }

        public static void Separate_nbt_data_block(string text, out string block, out string block_state, out string nbt_data, out string error)
        {
            nbt_data = "";
            error = "";

            int block_end_index = text.IndexOfAny(new char[] { '[', '{' });

            if (block_end_index != -1)
            {
                block = text[..block_end_index];
            }
            else
            {
                block = text;
            }

            block_state = "";
            int start_index = text.IndexOf('[');
            int end_index = text.LastIndexOf(']');

            if (start_index == -1 && end_index == -1)
            {

            }
            else if (start_index != -1 && end_index != -1)
            {
                block_state = text.Substring(start_index, end_index - start_index + 1);
            }
            else
            {
                error = "Cannot get block states from: " + text;
                return;
            }


            nbt_data = "";
            start_index = text.IndexOf('{');
            end_index = text.LastIndexOf('}');

            if (start_index == -1 && end_index == -1)
            {

            }
            else if (start_index != -1 && end_index != -1)
            {
                nbt_data = text.Substring(start_index, end_index - start_index + 1);
            }
            else
            {
                error = "Cannot get nbt data from: " + text;
                return;
            }
        }
    }

    public class Item_validator : Validator
    {
        private readonly bool components;
        public Item_validator(bool components)
        {
            this.components = components;
        }
        public override void Validate(Command command, object value, string validator_params, out string error)
        {
            Item_block_utils.Separate_nbt_item(components, (string)value, out string item, out string _, out error);

            if (error != "")
            {
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine(nbt);
            //Console.ResetColor();

            command.Parser.Verify_collection("ITEM", item, out error);
        }

    }

    public class Item_tag_validator : Validator
    {
        private readonly bool components;
        public Item_tag_validator(bool components)
        {
            this.components = components;
        }

        public override void Validate(Command command, object value, string validator_params, out string error)
        {
            Item_block_utils.Separate_nbt_item(components, (string)value, out string item, out string _, out error);

            if (error != "")
            {
                return;
            }

            //Seeing as tags are allowed here * is also allowed after 1.20.5
            if (components && item == "*")
            {
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine(nbt);
            //Console.ResetColor();

            command.Parser.Verify_collection("ITEM_TAG", item, out error);
        }
    }


    public class Block_validator : Validator
    {
        public Block_validator()
        {

        }
        public override void Validate(Command command, object value, string validator_params, out string error)
        {
            Item_block_utils.Separate_nbt_data_block((string)value, out string item, out _, out _, out error);

            if (error != "")
            {
                return;
            }

            command.Parser.Verify_collection("BLOCK", item, out error);
        }
    }

    public class Block_data_tag_validator : Validator
    {
        public Block_data_tag_validator()
        {

        }
        public override void Validate(Command command, object value, string validator_params, out string error)
        {
            Item_block_utils.Separate_nbt_data_block((string)value, out string item, out _, out _, out error);

            if (error != "")
            {
                return;
            }

            command.Parser.Verify_collection("BLOCK_TAG", item, out error);
        }
    }
}
