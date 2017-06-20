using Emulator317.Game;
using Emulator317.Game.Definitions;
using Emulator317.IO;
using Emulator317.Network;
using Emulator317.Server.Game.Definitions;
using Emulator317.Server.IO;
using System;
using System.Collections.Generic;
using System.Text;

using static Emulator317.Constants.LoginResponse;

namespace Emulator317.Server.Network
{
    enum LoginStage
    {
        INIT, SIZE, HANDSHAKE
    }

    internal delegate void Response(sbyte response, Client client);

    class LoginHandler
    {
        public static ISAACRng sessionKeyRNG;
        Connection c;
        StaticPackets packets;

        static LoginHandler()
        {
            int[] seed = new int[2];
            seed[0] = Guid.NewGuid().GetHashCode();
            seed[1] = (int)DateTime.Now.Ticks;
            sessionKeyRNG = new ISAACRng(seed);
        }

        public LoginHandler(Connection c)
        {
            this.c = c;
            packets = new StaticPackets(c);
        }

        LoginStage stage = LoginStage.INIT;

        public void Handle(byte opCode, bool firstByte)
        {
            if (stage == LoginStage.INIT)
            {
                byte connectStatus = c.Reader.ReadByteAsUByte();
                byte nameHash = c.Reader.ReadByteAsUByte();

                for (int i = 0; i < 8; i++)
                    c.Writer.WriteByte(0);

                c.Writer.WriteByte(LOGGING_IN);

                int k1 = sessionKeyRNG.Next(), k2 = sessionKeyRNG.Next();

                c.Writer.WriteInt(k1);
                c.Writer.WriteInt(k2);

                c.Send();

                stage = LoginStage.SIZE;
                c.ReceiveData(2);
            }
            else if(stage == LoginStage.SIZE)
            {
                byte connectStatus = c.Reader.ReadByteAsUByte();
                byte size = c.Reader.ReadByteAsUByte();

                stage = LoginStage.HANDSHAKE;
                c.ReceiveData(size);
            }
            else if(stage == LoginStage.HANDSHAKE)
            {
                byte magicNumber = c.Reader.ReadByteAsUByte(); //255
                short revision = (short)c.Reader.ReadShort(); //317
                byte isHighMem = c.Reader.ReadByteAsUByte();

                int[] crcs = new int[9];

                for (int i = 0; i < crcs.Length; i++)
                    crcs[i] = c.Reader.ReadInt();

                byte sizeLeft = c.Reader.ReadByteAsUByte();
                byte delimitter = c.Reader.ReadByteAsUByte(); //10

                int[] isaacSeed = new int[4];
                for (int i = 0; i < isaacSeed.Length; i++)
                    isaacSeed[i] = c.Reader.ReadInt();

                c.inRng = new ISAACRng(isaacSeed);

                for (int i = 0; i < isaacSeed.Length; i++)
                    isaacSeed[i] += 50;

                c.outRng = new ISAACRng(isaacSeed);

                int uId = c.Reader.ReadInt();

                string username = c.Reader.ReadString();
                string password = c.Reader.ReadString();

                bool allRead = c.Reader.BaseStream.Position == c.Reader.BaseStream.Length;


                Account acc = null;
                var response = Account.Load(username, password, ref acc);
                c.Writer.WriteByte(response);
                if(response == LOGIN_OK)
                {
                    c.Writer.WriteByte((byte)acc.rights);
                    c.Writer.WriteByte(acc.flagged ? 1 : 0);
                    

                    Client client = new Client(c, acc);
                    client.InitData();
                    c.handle = new PacketHandler(client).Handle;
                    c.ReceiveData();
                }
                else
                {
                    c.Dispose();
                }

                c.Send();
            }   

        }

    }
}
