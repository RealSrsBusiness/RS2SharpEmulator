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
    struct Coordinate3
    {
        public int x, y, z;
    }

    struct Coordinate
    {
        public int x, y;
    }

    //this is all super messy, will be fixed at some point
    class Client
    {
        private readonly object _lock = new object();

        List<object> states = new List<object>();

        public Connection Con { get; private set; }
        public StaticPackets Packets { get; private set; }

        public Account acc { get; private set; }
        public Player localPlayer { get; private set; }

        List<Player> localPlayers = new List<Player>();
        List<Player> npcs = new List<Player>();

        DateTime loginTime = DateTime.Now;
        
        int activeInterface = -1;
        bool focused = true;
        

        public T GetState<T>(int index)
        {
            return (T)states[index];
        }

        public Client(Connection con, Account acc)
        {
            this.Con = con;
            this.acc = acc;
            Packets = new StaticPackets(con);
            localPlayer = new Player();
            Init();
        }

        private void Init()
        {
            Packets.RunEnergy(100);
            Packets.SetConfig(172, 0);
            
            Packets.SendMessage(WELCOME_MSG);
            Con.Send();
        }

        public void SetMovement(Coordinate[] coords)
        {
            List<Direction> path = new List<Direction>();

            var lastX = localPlayer.x;
            var lastY = localPlayer.y;

            for (int i = 0; i < coords.Length; i++)
            {
                var coord = coords[i];

                //tiles difference between waypoints
                int difX = coord.x - lastX;
                int difY = coord.y - lastY;

                var direction = DirectionType2(difX, difY);

                int steps = 0;
                //determine how many steps to make
                if (difX < 0)
                    steps = difX * (-1);
                else if (difX > 0)
                    steps = difX;
                else if (difY < 0)
                    steps = difY * (-1);
                else if (difY > 0)
                    steps = difY;

                for (int j = 0; j < steps; j++)
                    path.Add(direction);

                //this coord is compared to the next coord in the next iteration
                lastX = coord.x;
                lastY = coord.y;
            }

            localPlayer.SetMovement(path.ToArray());

            Program.Debug("Done");

        }

        public enum Direction : int
        {
            NONE = -1,
            NORTH_WEST = 0,
            NORTH = 1,
            NORTH_EAST = 2,
            EAST = 4,
            SOUTH_EAST = 7,
            SOUTH = 6,
            SOUTH_WEST = 5,
            WEST = 3,
        }

        public readonly static Coordinate[] directions = new Coordinate[] {
            new Coordinate() { x = -1, y = 1 },
            new Coordinate() { x = 0, y = 1 },
            new Coordinate() { x = 1, y = 1 },
            new Coordinate() { x = -1, y = 0 },
            new Coordinate() { x = 1, y = 0 },
            new Coordinate() { x = -1, y = -1 },
            new Coordinate() { x = 0, y = -1 },
            new Coordinate() { x = 1, y = -1 }
        };

        public static Direction DirectionType2(int deltaX, int deltaY)
        {
            if (deltaX < 0)
            {
                if (deltaY < 0)
                    return Direction.SOUTH_WEST;
                else if (deltaY > 0)
                    return Direction.NORTH_WEST;
                else
                    return Direction.WEST;
            }
            else if (deltaX > 0)
            {
                if (deltaY < 0)
                    return Direction.SOUTH_EAST;
                else if (deltaY > 0)
                    return Direction.NORTH_EAST;
                else
                    return Direction.EAST;
            }
            else
            {
                if (deltaY < 0)
                    return Direction.SOUTH;
                else if (deltaY > 0)
                    return Direction.NORTH;
                else
                    return Direction.NONE;
            }
        }

        public static int DirectionType(int deltaX, int deltaY)
        {
            if (deltaX < 0)
            {
                if (deltaY < 0)
                    return 5;
                else if (deltaY > 0)
                    return 0;
                else
                    return 3;
            }
            else if (deltaX > 0)
            {
                if (deltaY < 0)
                    return 7;
                else if (deltaY > 0)
                    return 2;
                else
                    return 4;
            }
            else
            {
                if (deltaY < 0)
                    return 6;
                else if (deltaY > 0)
                    return 1;
                else
                    return -1;
            }
        }


        public byte[] UpdateEntities()
        {
            MemoryStream ms = new MemoryStream();


            List<bool> bits = new List<bool>();
            localPlayer.WritePlayerMovement(ref bits);
            WriteLocalPlayers(ref bits);
            WritePlayerList(ref bits);
            byte[] bitArray = bits.ToByteArray();

            ms.Write(bitArray, 0, bitArray.Length);

            localPlayer.WriteAppearance(ref ms);


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
        public void Update()
        {
            if(localPlayer.needLocalUpdate)
                Packets.LoadRegion(localPlayer.RegionX, localPlayer.RegionY);

            byte[] update = UpdateEntities();
            Packets.PlayerUpdate(update);

            Con.Send();
        }

        public void Disconnect()
        {
            Packets.Logout();
        }

        

    }

    
}
