using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ServerEmulator.Core
{
    internal static class Extensions
    {
        public static void EncodeValue(this List<bool> array, int count, int value)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                bool bit = ((value >> i) & 1) == 1;
                array.Add(bit);
            }
        }

        public static byte[] ToByteArray(this List<bool> array)
        {
            int count = (array.Count + 7) / 8;
            byte[] bytes = new byte[count];
            int bytePos = 0, bitPos = 0;

            const byte DABIT = 1 << 7; //128

            for (int i = 0; i < array.Count; i++)
            {
                if(array[i])
                    bytes[bytePos] |= (byte)(DABIT >> bitPos);

                bitPos++;
                if(bitPos > 7)
                {
                    bytePos++;
                    bitPos = 0;
                }
                    
            }

            return bytes;
        }

        public static string ToString(this long l)
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

        public static long ToLong(this string s)
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

        public static int PackGJString2(int position, byte[] buffer, string str)
        {
            int length = str.Length;
            int offset = position;
            for (int i = 0; length > i; i++)
            {
                int character = str[i];
                if (character > 127)
                {
                    if (character > 2047)
                    {
                        buffer[offset++] = (byte)((character | 919275) >> 12);
                        buffer[offset++] = (byte)(128 | ((character >> 6) & 63));
                        buffer[offset++] = (byte)(128 | (character & 63));
                    }
                    else
                    {
                        buffer[offset++] = (byte)((character | 12309) >> 6);
                        buffer[offset++] = (byte)(128 | (character & 63));
                    }
                }
                else
                    buffer[offset++] = (byte)character;
            }
            return offset - position;
        }
    }
}
