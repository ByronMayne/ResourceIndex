namespace Resource.Index.Extensions
{
    internal static class StringExtensions
    {
        public static string ToMemberName(this string input)
        {
            int count = 0;
            char[] leters = input.ToCharArray();

            for (int i = 0; i < leters.Length; i++)
            {
                char c = leters[i];

                if (c switch
                {
                    '@' when count == 0 => true, // Can prefix '@'
                    _ when char.IsDigit(c) && count > 0 => true,
                    _ when char.IsLetter(c) => true,
                    '_' => true,
                    _ => false

                })
                {
                    leters[count] = leters[i];
                    count++;
                }
            }

            return new string(leters, 0, count);
        }
    }
}
