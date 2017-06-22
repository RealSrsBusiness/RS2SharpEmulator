using ServerEmulator.Core;
using ServerEmulator.Core.IO;
using ServerEmulator.Core.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using static ServerEmulator.Core.Constants;

namespace ServerEmulator.Core.Game
{
    /// <summary>
    /// represents a single client, holds entities seen by a client
    /// </summary>
    /// 
    struct Point
    {
        int x, y, z;
    }

    //this is all super messy, will be fixed at some point
    class Client
    {
        private object _lock = new object();
        public Connection Con { get; private set; }
        public StaticPackets Packets { get; private set; }

        public Account acc { get; private set; }
        public Player localPLayer { get; private set; }

        List<Player> localPlayers = new List<Player>();
        List<Player> npcs = new List<Player>();

        DateTime loginTime = DateTime.Now;
        
        int activeInterface = -1;
        bool focused = true;
        

        public Client(Connection con, Account acc)
        {
            this.Con = con;
            this.acc = acc;
            Packets = new StaticPackets(con);
            localPLayer = new Player();
        }


        public byte[] UpdateEntities()
        {
            MemoryStream ms = new MemoryStream();


            List<bool> bits = new List<bool>();
            localPLayer.WritePlayerMovement(ref bits);
            WriteLocalPlayers(ref bits);
            WritePlayerList(ref bits);
            byte[] bitArray = bits.ToByteArray();

            ms.Write(bitArray, 0, bitArray.Length);

            localPLayer.WriteAppearance(ref ms);


            byte[] output = new byte[ms.Length];

            ms.Position = 0;
            ms.Read(output, 0, output.Length);

            return output;
        }

        public void WriteLocalPlayers(ref List<bool> data)
        {
            data.EncodeValue(8, 0);
        }

        //gender, headicon, bodyparts, idle animations
        public void WritePlayerList(ref List<bool> data)
        {
            data.EncodeValue(11, 2047);
        }


        /// <summary>
        /// Update this client
        /// </summary>
        /// <returns>Determines whether there is buffered data to be sent out</returns>
        public bool Update()
        {
            lock(_lock)
            {
                //if (needsUpdate)
                //{
                //    needsUpdate = false;
                //    return true;
                //}
                return false;
            }
        }

        public void Flush()
        {
           // Con.Send();
        }

        public void Disconnect()
        {
            Packets.Logout();
        }

        public void InitData()
        {
            Packets.RunEnergy(100);
            Packets.SetConfig(172, 0);
            Packets.SendMessage(WELCOME_MSG);
            Packets.LoadRegion(localPLayer.RegionX, localPLayer.RegionY);

            byte[] data = UpdateEntities();
            Packets.PlayerUpdate(data);
        }


    }

    
}
