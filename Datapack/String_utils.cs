namespace Datapack
{
    public class String_utils
    {
        public static bool Contains_not_middle(string input, string search)
        {
            int index = input.IndexOf(search);

            while (index != -1)
            {
                bool invalid_before = index > 0 && (char.IsLetter(input[index - 1]) || input[index - 1] == '_');
                bool invalid_after = index + search.Length < input.Length && (char.IsLetter(input[index + search.Length]) || input[index + search.Length] == '_');


                if (!invalid_before && !invalid_after)
                {
                    return true;
                }

                index = input.IndexOf(search, index + 1);
            }

            return false;
        }
    }
}
