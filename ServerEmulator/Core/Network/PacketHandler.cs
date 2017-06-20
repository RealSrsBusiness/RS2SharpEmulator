using ServerEmulator.Core.Game;
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
               
        Action[] handles;
        int dataLeft = -1;

        public PacketHandler(Client client)
        {
            this.client = client;
            c = client.Con;

            handles = new Action[Constants.INCOMING_SIZES.Length];
            handles[IDLE] = Idle;
            handles[WALK] = Walk;
            handles[WALK_MINIMAP] = MinimapWalk;
            handles[WALK_ON_COMMAND] = WalkCMD;
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
        }

        public void Handle(byte opCode, bool firstByte)
        {
            if(firstByte)
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

        private void ChatMessage()
        {
            Program.Debug("Chat msg");
        }

        private void WalkCMD()
        {
            Program.Debug("Walking");
        }

        private void ActionButton()
        {
            Program.Debug("Action btn");
        }

        private void Idle()
        {
            Program.Debug("Idling...");
        }

        private void Walk()
        {
            Program.Debug("Walking Packet");
        }

        private void MinimapWalk()
        {
            Program.Debug("Minimap Packet");
        }

    }
}
