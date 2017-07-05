using ServerEmulator.Core;
using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ServerEmulator.Core.Game.Client;

namespace ServerEmulator.Core.Game
{
    /// <summary>
    /// Represents a player in the world
    /// </summary>
    class Player
    {
        enum UpdateMask : int
        {
            NONE = 0x0,
            FORCED_MOVEMENT = 0x400,
            GRAPHIC = 0x100,
            ANIMATION = 0x8,
            FORCED_CHAT = 0x4,
            CHAT = 0x80,
            INTERACTING_ENTITIY = 0x1,
            APPEARANCE = 0x10,  //gender, headicon, bodyparts, idle animations
            FACING = 0x2,
            DAMAGE = 0x20,
            DAMAGE2 = 0x200
        }
        Queue<Coordinate> walkingQueue = new Queue<Coordinate>();
        UpdateMask updates = UpdateMask.APPEARANCE;

        public byte gender = 0, headicon = 0;
        public int animation;
        public long username;

        private int[] appearanceValues;
        private int[] colorValues;
        private int[] animations;

        public Player()
        {
            appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 };
            colorValues = new int[] { 7, 8, 9, 5, 0};
            animations = new int[] { 808, 823, 819, 820, 821, 822, 824 };
            username = Extensions.StringToLong("Player");
        }

        public int x = 3200, y = 3205;
        //8x8 in size
        public ushort RegionX { get { return (ushort)(x >> 3); } }
        public ushort RegionY { get { return (ushort)(y >> 3); } }
        byte localX { get { return (byte)(x - (RegionX - 6) * 8); } }
        byte localY { get { return (byte)(y - (RegionY - 6) * 8); } }
        //byte localX = 48, localY = 53; //48

        Direction[] movement = new Direction[0];
        int movementPos;

        int movedX = 0, movedY = 0;
        
        bool appearanceUpdate = true;
        public bool needLocalUpdate = true;
        bool running = false;
        bool teleported = false;

        int plane = 0;

        
        public void WritePlayerMovement(ref List<bool> bits)
        {
            bool didUpdate = false;

            bits.Add(false);

            if(needLocalUpdate)
            {
                didUpdate = true;
                bits.EncodeValue(2, 3); //type
                bits.EncodeValue(2, plane); //plane
                bits.EncodeValue(1, teleported ? 1 : 0);
                bits.EncodeValue(1, appearanceUpdate ? 1 : 0);

                bits.EncodeValue(7, localY);
                bits.EncodeValue(7, localX);
            }
            else
            {
                //var direction = DirectionType(movedX, movedY);
               // bool moved = direction != -1;

                bool moved = movementPos < movement.Length;

                if(moved)
                {
                    Direction direction = movement[movementPos];

                    var coord = Client.directions[(int)direction];
                    x += coord.x;
                    y += coord.y;

                    movementPos++;

                    didUpdate = true;
                    if(running)
                    {
                        bits.EncodeValue(2, 2); //type
                        bits.EncodeValue(3, (int)direction); //1st direction
                        bits.EncodeValue(3, (int)direction); //2nd dirction
                        bits.EncodeValue(1, appearanceUpdate ? 1 : 0);
                    }
                    else
                    {
                        bits.EncodeValue(2, 1); //type
                        bits.EncodeValue(3, (int)direction);
                        bits.EncodeValue(1, appearanceUpdate ? 1 : 0);
                    }
                }
            }

            if(!didUpdate)
            {
                if(appearanceUpdate)
                {
                    bits[0] = true;
                    bits.EncodeValue(2, 0); //type
                }
            }
            else
            {
                bits[0] = true;
            }

            needLocalUpdate = false;
            movedX = 0;
            movedY = 0;
        }



        public void SetMovement(Direction[] movementTypes)
        {
            movement = movementTypes;
            movementPos = 0;
        }

        public void WriteAppearance(ref MemoryStream ms)
        {
            if (!appearanceUpdate)
                return;

            RSStreamWriter sw = new RSStreamWriter(ms);

            int mask = 0x0;
            mask |= (int)updates;

            sw.WriteByte(mask);

            if((mask & (int)UpdateMask.APPEARANCE) == (int)UpdateMask.APPEARANCE)
            {
                long startPos = sw.BaseStream.Position;
                sw.WriteNegatedByte(0); //lenght

                sw.WriteByte(gender);
                sw.WriteByte(headicon);

                for (int i = 0; i < appearanceValues.Length; i++)
                {
                    int value = appearanceValues[i];
                    if (value > -1)
                    {
                        short v = (short)(0x100 | value);
                        sw.WriteShort(v);
                    }
                        
                    else
                        sw.WriteByte(0);
                }

                for (int i = 0; i < colorValues.Length; i++)
                {
                    int value = colorValues[i];
                    sw.WriteByte(value);
                }

                for (int i = 0; i < animations.Length; i++)
                {
                    int v = animations[i];
                    sw.WriteShort(v);
                }

                var stream = sw.BaseStream;

                sw.WriteLong(username);
                sw.WriteByte(CombatLevel);
                sw.WriteShort(TotalSkill);

                long endPos = sw.BaseStream.Position;
                byte size = (byte)(endPos - startPos - 1);
                sw.BaseStream.Position = startPos;
                sw.WriteNegatedByte(size);
                sw.BaseStream.Position = endPos;
            }

            appearanceUpdate = false;
            updates = UpdateMask.NONE;
        }

        public int DirectionType(int deltaX, int deltaY)
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


        public byte CombatLevel { get { return 3; } }
        public ushort TotalSkill { get { return 420; } }
    }
}
