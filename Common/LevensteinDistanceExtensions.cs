using System;

namespace Scar.Common
{
    public static class LevenshteinDistanceExtensions
    {
        public static int LevenshteinDistance(this string? input, string? comparedTo, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(comparedTo))
            {
                return -1;
            }

            if (!caseSensitive)
            {
                input = input.ToLower();
                comparedTo = comparedTo.ToLower();
            }

            var length1 = input.Length;
            var length2 = comparedTo.Length;
            var numArray = new int[length1 + 1, length2 + 1];
            if (length1 == 0)
            {
                return length2;
            }

            if (length2 == 0)
            {
                return length1;
            }

            var index1 = 0;
            while (index1 <= length1)
            {
                numArray[index1, 0] = index1++;
            }

            var index2 = 0;
            while (index2 <= length2)
            {
                numArray[0, index2] = index2++;
            }

            for (var index3 = 1; index3 <= length1; ++index3)
            for (var index4 = 1; index4 <= length2; ++index4)
            {
                var num = comparedTo[index4 - 1] == input[index3 - 1] ? 0 : 1;
                numArray[index3, index4] = Math.Min(Math.Min(numArray[index3 - 1, index4] + 1, numArray[index3, index4 - 1] + 1), numArray[index3 - 1, index4 - 1] + num);
            }

            return numArray[length1, length2];
        }
    }
}