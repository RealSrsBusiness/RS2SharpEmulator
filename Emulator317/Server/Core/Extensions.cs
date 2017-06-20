using System;
using System.Collections.Generic;
using System.Text;

namespace Emulator317.Core
{
    public static class Extensions
    {
        public static void EncodeValue(this List<bool> array, int count, int value)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                bool bit = ((value >> i) & 1) == 1;
                array.Add(bit);
            }
        }

        public static void DecodeValue()
        {

        }

        public static string LongToString(long l)
        {
            if (l <= 0L || l >= 0x5b5b57f8a98a5dd1L)
                return null;
            if (l % 37L == 0L)
                return null;
            int i = 0;
            char[] ac = new char[12];
            while (l != 0L)
            {
                long l1 = l;
                l /= 37L;
                ac[11 - i++] = Constants.VALID_CHARS[(int)(l1 - l * 37L)];
            }
            return new string(ac, 12 - i, i);
        }

        public static long StringToLong(string s)
        {
            long l = 0L;
            for (int i = 0; i < s.Length && i < 12; i++)
            {
                char c = s[i];
                l *= 37L;
                if (c >= 'A' && c <= 'Z')
                    l += (1 + c) - 65;
                else if (c >= 'a' && c <= 'z')
                    l += (1 + c) - 97;
                else if (c >= '0' && c <= '9')
                    l += (27 + c) - 48;
            }
            while (l % 37L == 0L && l != 0L)
                l /= 37L;
            return l;
        }
    }
}
