using ServerEmulator.Core.Game;
using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;
using static ServerEmulator.Core.Constants.Packets;

namespace ServerEmulator.Core.Network
{
    //delegate void Action();

    class PacketHandler
    {
        Connection c;
        Client client;
        RSStreamReader reader;
               
        Action[] handles;
        int dataLeft = -1;

        public PacketHandler(Client client)
        {
            this.client = client;
            c = client.Con;

            handles = new Action[Constants.INCOMING_SIZES.Length];
            handles[IDLE] = Idle;
            handles[MOUSE_CLICK] = MouseClick;
            handles[CAMERA_MOVE] = Camera;

            handles[WALK] = () => Walk();
            handles[WALK_MINIMAP] = () => Walk(14);
            handles[WALK_ON_COMMAND] = () => Walk();

            handles[NPC_OPT_1] = () => NpcAction(1);
            handles[OBJ_OPT_1] = () => ObjectAction(1);
            handles[FLOORITEM_OPT_1] = () => GroundItemAction(1);
               

            handles[INTF_ACTION_BTN] = ActionButton;
            handles[CHAT_MSG_SEND] = ChatMessage;
            

            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i] == null)
                {
                    int j = i; //copy currect index
                    handles[i] = () => { Program.Debug("Unhandled Packet {0}", j); };
                }
            }

            reader = c.Reader;
        }

        private void GroundItemAction(int option)
        {
            //throw new NotImplementedException();
        }

        private void ObjectAction(int option)
        {
            //throw new NotImplementedException();
        }

        private void NpcAction(int option)
        {
            //throw new NotImplementedException();
        }

        public void Handle(byte opCode, bool firstByte)
        {
            if (firstByte)
                dataLeft = Constants.INCOMING_SIZES[opCode];

            if (!firstByte)
            {
                if (dataLeft < 0)
                    dataLeft = c.Reader.ReadByteAsUByte();
                else
                    dataLeft = 0;
            }

            if (dataLeft < 0) //-1 = variable size; receive the size byte
                c.ReceiveData(1);
            else if (dataLeft > 0)
                c.ReceiveData(dataLeft);
            else
            {
                handles[opCode]();
                c.ReceiveData(); //receive next opcode
            }
        }

        private void Idle()
        {
            // Program.Debug("Idling...");
        }

        private void MouseClick()
        {
            // Program.Debug("Mouseclick");
        }

        private void Camera()
        {
            // Program.Debug("Camera");
        }

        private void ChatMessage()
        {
            Program.Debug("Chat msg");
        }

        private void Walk(int ignore = 0)
        {
            int count = (int)((reader.BaseStream.Length - 5 - ignore) / 2);
            int[] xs = new int[count];
            int[] ys = new int[count];

            short x = (short)reader.ReadLEShortA();
            for (int i = 0; i < count; i++)
            {
                xs[i] = x + reader.ReadByte();
                ys[i] = reader.ReadByte();
            }
            short y = (short)reader.ReadLEShort();

            Coordinate[] waypoints = new Coordinate[count + 1];
            waypoints[0] = new Coordinate() { x = x, y = y };

            for (int i = 1; i < waypoints.Length; i++)
            {
                var wp = new Coordinate();
                wp.x = xs[i - 1];
                wp.y = ys[i - 1] + y;
                waypoints[i] = wp;
            }

            sbyte keyStat = reader.ReadNegByte();
            client.SetMovement(waypoints);

            Program.Debug("Do Walking");
        }


        private void ActionButton()
        {
            Program.Debug("Action btn");
        }

        
    }
}
