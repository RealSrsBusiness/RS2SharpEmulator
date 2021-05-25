using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ServerEmulator.Core
{
    enum EntryChangeType : int { ADDED = 0, REMOVED = 1 }
    class ListEntryChange<T> {
        public T entry;
        internal EntryChangeType change;
    }

    internal static class Extensions
    {
        public static ListEntryChange<T>[] Difference<T>(this T[] source, T[] compareTo) where T : class 
        {
            var changes = new List<ListEntryChange<T>>();

            void Diff(T[] first, T[] second, EntryChangeType diffToSet) 
            {
                for (int i = 0; i < first.Length; i++)
                {
                    bool found = false;
                    for (int j = 0; j < second.Length; j++)
                    {
                        if(first[i] == second[j]) 
                        {
                            found = true;
                            break;
                        }
                    } 

                    if(!found) //not found in other list, therefor different
                    {
                        changes.Add(new ListEntryChange<T> {
                            change = diffToSet,
                            entry = first[i]
                        });
                    }
                }
            }

            Diff(source, compareTo, EntryChangeType.REMOVED);
            Diff(compareTo, source, EntryChangeType.ADDED);

            return changes.ToArray();
        }

        public static void EncodeValue(this List<bool> array, int count, int value)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                bool bit = ((value >> i) & 1) == 1;
                array.Add(bit);
            }
        }

        public static void OverwriteValueAt(this List<bool> array, int posAt, int count, int value)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                bool bit = ((value >> i) & 1) == 1;
                array[posAt++] = bit;
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

        public static string ToString(this long source)
        {
            if (source <= 0L || source >= 0x5b5b57f8a98a5dd1L)
                return null;
            if (source % 37L == 0L)
                return null;
            int i = 0;
            char[] ac = new char[12];
            while (source != 0L)
            {
                long l1 = source;
                source /= 37L;
                ac[11 - i++] = Constants.VALID_CHARS[(int)(l1 - source * 37L)];
            }
            return new string(ac, 12 - i, i);
        }

        public static long ToLong(this string source)
        {
            long l = 0L;
            for (int i = 0; i < source.Length && i < 12; i++)
            {
                char c = source[i];
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
