using System;
using System.Collections.Generic;
using System.Globalization;

namespace Scar.Common.Comparers;

public class AlphanumComparatorFast : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        var str1 = x;
        if (str1 == null)
        {
            return 0;
        }

        var str2 = y;
        if (str2 == null)
        {
            return 0;
        }

        var length1 = str1.Length;
        var length2 = str2.Length;
        var index1 = 0;
        var index2 = 0;
        while ((index1 < length1) && (index2 < length2))
        {
            var c1 = str1[index1];
            var c2 = str2[index2];
            var chArray1 = new char[length1];
            var num1 = 0;
            var chArray2 = new char[length2];
            var num2 = 0;
            do
            {
                chArray1[num1++] = c1;
                ++index1;
                if (index1 < length1)
                {
                    c1 = str1[index1];
                }
                else
                {
                    break;
                }
            }
            while (char.IsDigit(c1) == char.IsDigit(chArray1[0]));

            do
            {
                chArray2[num2++] = c2;
                ++index2;
                if (index2 < length2)
                {
                    c2 = str2[index2];
                }
                else
                {
                    break;
                }
            }
            while (char.IsDigit(c2) == char.IsDigit(chArray2[0]));

            var str3 = new string(chArray1);
            var str4 = new string(chArray2);
            var num3 = !char.IsDigit(chArray1[0]) || !char.IsDigit(chArray2[0])
                ? string.Compare(str3, str4, StringComparison.Ordinal)
                : int.Parse(str3, CultureInfo.InvariantCulture).CompareTo(int.Parse(str4, CultureInfo.InvariantCulture));
            if ((uint)num3 > 0U)
            {
                return num3;
            }
        }

        return length1 - length2;
    }
}