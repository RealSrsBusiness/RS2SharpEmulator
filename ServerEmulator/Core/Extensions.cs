using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ServerEmulator.Core
{
    enum EntryChangeType : int { UNCHANGED = 0, REMOVED = 1, ADDED = 2 }

    class ComparedListResult<T> 
    {
        public (T entry, EntryChangeType changeType)[] entries; //tuple
        public int Unchanged, Removed, Added;
    }

    internal static class Extensions
    {
        public static ComparedListResult<T> Difference<T>(this T[] source, T[] compareTo, bool checkUnchanged = true) where T : class 
        {
            var res = new ComparedListResult<T>();
            var entries = new List<(T entry, EntryChangeType changeType)>(source.Length + compareTo.Length);

            int Diff(T[] first, T[] second, EntryChangeType diffToSet, bool addUnchanged = false) 
            {
                int count = 0;

                for (int i = 0; i < first.Length; i++)
                {
                    bool found = false;
                    for (int j = 0; j < second.Length; j++)
                    {
                        if(first[i] == second[j]) 
                        {
                            found = true;
                            if(addUnchanged) 
                            {
                                entries.Add( (first[i], EntryChangeType.UNCHANGED) );
                                res.Unchanged++;
                            }
                            break;
                        }
                    } 

                    if(!found) //not found in other list, therefor different
                    {
                        count++;
                        entries.Add( (first[i], diffToSet) );
                    }
                }
                return count;
            }

            res.Removed = Diff(source, compareTo, EntryChangeType.REMOVED, checkUnchanged);
            res.Added = Diff(compareTo, source, EntryChangeType.ADDED);

            res.entries = entries.ToArray();
            return res;
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
                ac[11 - i++] = Constants.CHARACTER_MAP_INT64[(int)(l1 - source * 37L)];
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

        public static byte[] ToRSChatString(this string src) 
        {
            if (src.Length > 80)
                src = src.Substring(0, 80);

            MemoryStream stream = new MemoryStream();

            src = src.ToLower();
            int next = -1;

            for (int index = 0; index < src.Length; index++) 
            {
                char character = src[index];
                int charIndex = 0;

                for (int idx = 0; idx < Constants.CHARACTER_MAP_CHAT.Length; idx++) 
                {
                    if (character != Constants.CHARACTER_MAP_CHAT[idx])
                        continue;

                    charIndex = idx;
                    break;
                }

                if (charIndex > 12)
                    charIndex += 195;

                if (next == -1) 
                {
                    if (charIndex < 13)
                        next = charIndex;
                    else
                        stream.WriteByte((byte)charIndex);
                } 
                else if (charIndex < 13) 
                {
                    stream.WriteByte((byte)((next << 4) + charIndex));
                    next = -1;
                } 
                else 
                {
                    stream.WriteByte((byte)((next << 4) + (charIndex >> 4)));
                    next = charIndex & 0xF;
                }
            }

            if (next != -1)
                stream.WriteByte((byte)(next << 4));

            return stream.ToArray();
        }


    }
}
