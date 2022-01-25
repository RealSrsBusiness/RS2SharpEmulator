﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerEmulator.Core.IO
{
    class RSStreamWriter //todo: make naming scheme consistant with client
    {
        public MemoryStream BaseStream { get; set; }

        public RSStreamWriter(MemoryStream s)
        {
            this.BaseStream = s;
        }

        public void WriteByte(int i)
        {
            BaseStream.WriteByte((byte)i);
        }

        public void WriteBytes(byte[] data, int offset, int length)
        {
            BaseStream.Write(data, offset, length);
        }

        public void WriteByteS(int i)
        {
            BaseStream.WriteByte((byte)(128 - i));
        }

        public void WriteByteA(int i) //readUByteA
        {
            BaseStream.WriteByte((byte)(128 + i));
        }

        //writeDWord
        public void WriteInt(int i) //big endian
        {
            BaseStream.WriteByte((byte)(i >> 24));
            BaseStream.WriteByte((byte)(i >> 16));
            BaseStream.WriteByte((byte)(i >> 8));
            BaseStream.WriteByte((byte)i);
        }

        public void WriteMEInt(int i) //middle endian
        {
            BaseStream.WriteByte((byte)(i >> 8));
            BaseStream.WriteByte((byte)(i));
            BaseStream.WriteByte((byte)(i >> 24));
            BaseStream.WriteByte((byte)(i >> 16));
        }

        public void WriteIMEInt(int i)
        {
            BaseStream.WriteByte((byte)(i >> 16));
            BaseStream.WriteByte((byte)(i >> 24));
            BaseStream.WriteByte((byte)(i));
            BaseStream.WriteByte((byte)(i >> 8));
        }

        public void WriteTerminatedString(string str)
        {
            byte[] bytes = new byte[str.Length];

            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)str[i];

            BaseStream.Write(bytes, 0, bytes.Length);
            BaseStream.WriteByte(10);
        }

        public void WriteLEInt(int i)
        {
            BaseStream.WriteByte((byte)i);
            BaseStream.WriteByte((byte)(i >> 8));
            BaseStream.WriteByte((byte)(i >> 16));
            BaseStream.WriteByte((byte)(i >> 24));
        }

        public void WriteLEShort(int i)
        {
            BaseStream.WriteByte((byte)i);
            BaseStream.WriteByte((byte)(i >> 8));
        }

        public void WriteLEShortA(int i)
        {
            BaseStream.WriteByte((byte)(i + 128));
            BaseStream.WriteByte((byte)(i >> 8));
        }

        public void WriteLong(long l)
        {
            BaseStream.WriteByte((byte)(int)(l >> 56));
            BaseStream.WriteByte((byte)(int)(l >> 48));
            BaseStream.WriteByte((byte)(int)(l >> 40));
            BaseStream.WriteByte((byte)(int)(l >> 32));
            BaseStream.WriteByte((byte)(int)(l >> 24));
            BaseStream.WriteByte((byte)(int)(l >> 16));
            BaseStream.WriteByte((byte)(int)(l >> 8));
            BaseStream.WriteByte((byte)(int)l);
        }

        public void WriteNegatedByte(int i)
        {
            BaseStream.WriteByte((byte)-i);
        }

        public void WriteReverseData(byte[] data, int length, int offset)
        {
            for (int i = (length + offset) - 1; i >= 0; i--)
                BaseStream.WriteByte((byte)(data[i]));
        }

        public void WriteShort(int i)
        {
            BaseStream.WriteByte((byte)(i >> 8));
            BaseStream.WriteByte((byte)i);
        }

        public void WriteShortA(int i)
        {
            BaseStream.WriteByte((byte)(i >> 8));
            BaseStream.WriteByte((byte)(i + 128));
        }

        public void WriteTriByte(int i)
        {
            BaseStream.WriteByte((byte)(i >> 16));
            BaseStream.WriteByte((byte)(i >> 8));
            BaseStream.WriteByte((byte)i);
        }

        //alternative names
        public void WriteWord(int i) { WriteShort(i); }

        public void WriteDWord(int i) { WriteInt(i); }

    }
}
