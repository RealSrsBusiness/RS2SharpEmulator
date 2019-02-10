using ServerEmulator.Core.IO;
using System;
using System.IO;
using System.Net.Sockets;
using System.Timers;

namespace ServerEmulator.Core.Network
{
    delegate void ConnectionUpdate(Connection c);
    delegate void PacketHandle(byte opCode, bool firstByte);

    //toadd: encryption, connection timeout, limit login tries? disconnect when too much data is sent (ddos protection)
    //check for connection lost, valid data received, too much data received (put on a timer?)
    class Connection : IDisposable
    {
        Socket host; //remote host
        public event ConnectionUpdate onDisconnect;
        public PacketHandle handle { get; set; }

        public RSStreamReader Reader { get; private set; } //allows reading from the stream
        public RSStreamWriter Writer { get; private set; } //allows writing to the stream

        int OpCode = 0; //-1 means we expect next byte to be an opcode
        byte[] buffer = new byte[byte.MaxValue];
        int received = 0, expected = 0;

        public ISAACRng inRng, outRng;

        public Connection(Socket host)
        {
            this.host = host;
            this.host.NoDelay = true;
            EndPoint = host.RemoteEndPoint.ToString();
            Reader = new RSStreamReader(new MemoryStream());
            Writer = new RSStreamWriter(new MemoryStream());
        }

        /// <param name="expectedData">How much data to expect, -1 means an OpCode is expected</param>
        public void ReceiveData(int expectedData = -1)
        {
            try
            {
                if(expectedData < 0)
                {
                    OpCode = -1;
                    expected = 1;
                }
                else
                {
                    expected = expectedData;
                }

                if(received == 0)
                    Reader.BaseStream.SetLength(0);

                host.BeginReceive(buffer, received, expected - received, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch(Exception)
            {
                Dispose();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                received += host.EndReceive(ar);
            }
            catch(Exception)
            {
                Dispose();
                return;
            }

            if (received >= expected)
            {
                received = 0;
                bool readOpCode = OpCode < 0;

                if (readOpCode)
                    OpCode = (byte)(buffer[0] - inRng.Next());
                else
                {
                    Reader.BaseStream.Write(buffer, 0, expected);
                    Reader.BaseStream.Position = 0;
                }

                handle((byte)OpCode, readOpCode);
            }
            else
            {
                ReceiveData(expected);
            }
        }

        public void WriteOpCode(byte opCode)
        {
            Writer.WriteByte((byte)(opCode + outRng.Next()));;
        }

        public int WriteOpCodeVar(byte opcode, byte reserveBytes = 1)
        {
            WriteOpCode(opcode);
            for (int i = 0; i < reserveBytes; i++)
                Writer.WriteByte(0);
            return (int)Writer.BaseStream.Length;
        }

        public void FinishVarPacket(int startPos, byte reservedBytes = 1)
        {
            var endPos = Writer.BaseStream.Length;
            var packetSize = (int)(endPos - startPos);
            Writer.BaseStream.Position = startPos - reservedBytes;

            if (reservedBytes == 1)
                Writer.WriteByte(packetSize);
            else if (reservedBytes == 2)
                Writer.WriteShort(packetSize);
            else
                throw new NotSupportedException();

            Writer.BaseStream.Position = endPos;
        }

        /// <summary>
        /// Send buffered data
        /// </summary>
        public void Send()
        {
            try
            {
                byte[] buffer = Writer.BaseStream.GetBuffer();
                host.BeginSend(buffer, 0, (int)Writer.BaseStream.Length, SocketFlags.None, Sent, null);
            }
            catch (Exception)
            {
                Dispose();
            }
        }

        private void Sent(IAsyncResult ar)
        {
            //creating new memorystreams is expensive, so just clear the buffer
            Writer.BaseStream.SetLength(0); 
        }

        public void Dispose()
        {
            Reader.BaseStream.Dispose();
            Writer.BaseStream.Dispose();
            host.Dispose();
            onDisconnect(this);
        }

        public string EndPoint { get; private set; }
        public string IPAddress { get { return EndPoint.Split(':')[0]; } }
        public int Port { get { return int.Parse(EndPoint.Split(':')[1]); } }
    }
}
