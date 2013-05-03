using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Serialization
{
    public class StringSplitter
    {
        public static IEnumerable<string> SplitString(string input, int substringLength)
        {
            if (substringLength <= 0)
                throw new InvalidOperationException("Substring length must be greater than 0.");

            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(input))
                return result;

            for (int i = 0; i < input.Length;)
            {
                int end = substringLength;
                if (i + substringLength >= input.Length)
                    end = input.Length - i;

                result.Add(input.Substring(i, end));

                i+= substringLength;
            }

            return result;
        }
    }
}
