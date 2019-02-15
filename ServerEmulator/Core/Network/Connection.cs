using ServerEmulator.Core.IO;
using System;
using System.IO;
using System.Net.Sockets;
using System.Timers;

namespace ServerEmulator.Core.Network
{
    delegate void ConnectionUpdate(Connection c);
    delegate void PacketHandle(byte opCode, bool firstByte);

    struct VarSizePacket
    {
        public int type;
        public long position;
        public enum Type : int { BYTE = 1, SHORT = 2 }
    }

    //toadd: encryption, connection timeout, limit login tries? disconnect when too much data is sent (ddos protection)
    //check for connection lost, valid data received, too much data received (put on a timer?)
    class Connection : IDisposable
    {
        Socket host; //remote host
        public event ConnectionUpdate onDisconnect;
        public PacketHandle handle { private get; set; }

        public RSStreamReader Reader { get; private set; } //allows reading from the stream
        public RSStreamWriter Writer { get; private set; } //allows writing to the stream

        int opCode = 0; //-1 means we expect next byte to be an opcode
        byte[] buffer = new byte[byte.MaxValue];
        int received = 0, expected = 0;

        public ISAACRng inRng { private get; set; }
        public ISAACRng outRng { private get; set; }

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
                    opCode = -1;
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
                bool readOpCode = opCode < 0;

                if (readOpCode)
                    opCode = (byte)(buffer[0] - inRng.Next());
                else
                {
                    Reader.BaseStream.Write(buffer, 0, expected);
                    Reader.BaseStream.Position = 0;
                }

                handle((byte)opCode, readOpCode);
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

        public VarSizePacket WriteOpCodeVar(byte opcode, VarSizePacket.Type type = VarSizePacket.Type.BYTE)
        {
            WriteOpCode(opcode);
            for (int i = 0; i < (int)type; i++)
                Writer.WriteByte(0);

            var packet = new VarSizePacket() { position = Writer.BaseStream.Length, type = (int)type };

            return packet;
        }

        public void FinishVarPacket(VarSizePacket p)
        {
            var endPos = Writer.BaseStream.Length;
            var packetSize = endPos - p.position;

            Writer.BaseStream.Position = p.position - p.type;

            if (p.type == 1)
                Writer.WriteByte((int)packetSize);
            else
                Writer.WriteShort((int)packetSize);

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
            if(Writer != null)
            {
                host.Dispose();
                onDisconnect(this);
                Reader.BaseStream.Dispose();
                Writer.BaseStream.Dispose();
                Writer = null;
                Reader = null;
            }
        }

        public string EndPoint { get; private set; }
        public string IPAddress { get { return EndPoint.Split(':')[0]; } }
        public int Port { get { return int.Parse(EndPoint.Split(':')[1]); } }
    }
}
