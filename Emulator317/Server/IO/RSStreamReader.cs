using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Emulator317.IO
{
    class RSStreamReader
    {
        public MemoryStream BaseStream { get; set; }

        public RSStreamReader(MemoryStream s)
        {
            this.BaseStream = s;
        }

        //java's byte is signed
        public sbyte ReadByte()
        {
            return (sbyte)BaseStream.ReadByte();
            //return payload[position++];
        }

        //Read an unsigned byte and return as an ubyte container
        public byte ReadByteAsUByte()
        {
            return (byte)BaseStream.ReadByte();
        }

        public byte ReadByteS()
        {
            return (byte)(128 - ReadByte());
        }

        public void ReadData(byte[] data, int offset, int length)
        {
            for (int i = offset; i < offset + length; i++)
                data[i] = (byte)ReadByte();
        }

        public int ReadIMEInt()
        { // V2
            return ((ReadByte() & 0xff) << 16) + ((ReadByte() & 0xff) << 24) +
                    (ReadByte() & 0xff) + ((ReadByte() & 0xff) << 8);
        }

        public int ReadInt()
        {
            return ((ReadByte() & 0xff) << 24) + ((ReadByte() & 0xff) << 16)
                    + ((ReadByte() & 0xff) << 8) + (ReadByte() & 0xff);
        }

        public int ReadLEShort()
        {
            int value = (ReadByte() & 0xff) + ((ReadByte() & 0xff) << 8);
            if (value > 32767)
                value -= 0x10000;
            return value;
        }

        public int ReadLEShortA()
        {
            int value = (ReadByte() - 128 & 0xff) + ((ReadByte() & 0xff) << 8);
            if (value > 32767)
                value -= 0x10000;
            return value;
        }

        public int ReadLEUShort()
        {
            return (ReadByte() & 0xff) + ((ReadByte() & 0xff) << 8);
        }

        public int ReadLEUShortA()
        {
            return (ReadByte() - 128 & 0xff) + ((ReadByte() & 0xff) << 8);
        }

        public long ReadLong()
        {
            long msi = ReadInt() & 0xFFFFFFFFL;
            long lsi = ReadInt() & 0xFFFFFFFFL;
            return (msi << 32) + lsi;
        }

        public int ReadMEInt()
        { // V1
            return ((ReadByte() & 0xff) << 8) + (ReadByte() & 0xff) +
                ((ReadByte() & 0xff) << 24) + ((ReadByte() & 0xff) << 16);
        }

        public byte ReadNegByte()
        {
            return (byte)-ReadByte();
        }

        public int ReadNegUByte()
        {
            return -ReadByte() & 0xff;
        }

        public void ReadReverseData(byte[] data, int offset, int length)
        {
            for (int i = length + offset - 1; i >= length; i--)
                data[i] = (byte)ReadByte();
        }

        public int ReadShort()
        {
            int value = ((ReadByte() & 0xff) << 8) + (ReadByte() & 0xff);
            if (value > 32767)
                value -= 0x10000;

            return value;
        }

        public int ReadUSmart()
        {
            int value = ReadByte() & 0xff;
            BaseStream.Position--;
            if (value < 128)
                return ReadUByte();

            return ReadUShort() - 32768;
        }

        public int ReadSmart()
        {
            int value = ReadByte() & 0xff;
            BaseStream.Position--;
            if (value < 128)
                return ReadUByte() - 64;

            return ReadUShort() - 49152;
        }

        public string ReadString()
        {
            return bytesToString(ReadStringBytes());
        }

        public sbyte[] ReadStringBytes()
        {
            List<sbyte> str = new List<sbyte>();
            sbyte b;
            do
            {
                b = ReadByte();
                str.Add(b);
            } while (b != 10);

            return str.ToArray();
        }

        public static sbyte[] stringToBytes(string input)
        {
            sbyte[] output = new sbyte[input.Length];
            for (int i = 0; i < input.Length; i++)
                output[i] = (sbyte)input[i];
            return output;
        }

        public static string bytesToString(sbyte[] input)
        {
            sbyte[] a = input;
            char[] result = new char[a.Length];
            for (int i = 0; i < a.Length; i++)
                result[i] = (char)a[i];
            return new string(result);
        }

        public int ReadUTriByte()
        {
            return ((ReadByte() & 0xff) << 16) + ((ReadByte() & 0xff) << 8) + (ReadByte() & 0xff);
        }

        public int ReadUByte()
        {
            return ReadByte() & 0xff;
        }

        public int ReadUByteA()
        {
            return ReadByte() - 128 & 0xff;
        }

        public int ReadUByteS()
        {
            return 128 - ReadByte() & 0xff;
        }

        public int ReadUShort()
        {
            return ((ReadByte() & 0xff) << 8) + (ReadByte() & 0xff);
        }

        public int ReadUShortA()
        {
            return ((ReadByte() & 0xff) << 8) + (ReadByte() - 128 & 0xff);
        }


        //Alternative Names
        public sbyte ReadSignedByte() { return ReadByte(); }

        public int ReadUnsignedByte() { return ReadUByte(); }

        public int ReadWord() { return ReadShort(); }

        public int ReadUnsignedWord() { return ReadUShort(); }

        public int ReadDWord() { return ReadInt(); }
        
        public string ReadNewString() { return ReadString(); }

    }
}
