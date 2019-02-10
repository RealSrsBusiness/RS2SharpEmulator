using System;
using static ServerEmulator.Core.Game.Movement;

namespace ServerEmulator.Core.Game
{
    class WorldEntity
    {
        public int id;
        public int x, y, z;

        public Action Update;

        public ushort RegionX { get { return (ushort)(x >> 3); } }
        public ushort RegionY { get { return (ushort)(y >> 3); } }

        public byte LocalX { get { return (byte)(x - (RegionX - 6) * 8); } }
        public byte LocalY { get { return (byte)(y - (RegionY - 6) * 8); } }

        public Coordinate VerifyDistance(int x, int y, int steps = 16)
        {
            int difX = this.x - x;
            int difY = this.y - y;

            int stepsN = -steps;

            if (difX > steps || difX < stepsN || difY > steps || difY < stepsN)
                return Coordinate.NONE;

            return new Coordinate() { x = difX, y = difY };
        }

        public class PlayerEntity : WorldEntity
        {
            public EffectMaskPlayer effectUpdateMask = EffectMaskPlayer.NONE;
            public bool justSpawned, running = true, teleported = false;

            public Direction[] movement = new Direction[0];
            public int walkingQueue = 0;

            public byte gender = 0, headicon = 0;
            public long username;

            public int[] appearanceValues;
            public int[] colorValues;
            public int[] animations;

            public byte CombatLevel { get { return 3; } }
            public ushort Skill { get { return 0; } }
            public bool EffectUpdateRequired { get { return effectUpdateMask != EffectMaskPlayer.NONE; } }

            public enum EffectMaskPlayer : int
            {
                NONE = 0x0,
                FORCED_MOVEMENT = 0x400, //init x, y, dest x, y, start, end, direction
                GRAPHIC = 0x100, //graphic, info
                ANIMATION = 0x8, //animation, delay
                FORCED_CHAT = 0x4, //text
                CHAT_TEXT = 0x80, //textinfo, privilege, offset, off
                INTERACTING_ENTITIY = 0x1, //mob-id
                APPEARANCE = 0x10,  //gender, headicon, bodyparts, colors, idle and walk animations, name, combat, skill
                FACING = 0x2, //x, y
                DAMAGE = 0x20, //damage, type, curHealth, maxHealth
                DAMAGE2 = 0x200 //damage, type, curHealth, maxHealth
            }

            public static int AllocPlayerSlot()
            {
                //find free player slot
                int pId = -1;
                for (int i = 0; i < playerSlots.Length; i++)
                {
                    if (!playerSlots[i])
                    {
                        pId = i;
                        playerSlots[i] = true;
                        break;
                    }
                }
                return pId;
            }

            public static void FreePlayerSlot(int id)
            {
                playerSlots[id] = false;
            }

            static bool[] playerSlots = new bool[2047];


        }

        public class NPCEntity : WorldEntity
        {
            public EffectMaskNPC effectUpdateMask = EffectMaskNPC.NONE;

            public enum EffectMaskNPC : int
            {
                NONE = 0x0,
                ANIMATION = 0x10, //id, delay
                DAMAGE = 0x8, //value, type
                GRAPHIC = 0x80, //graphic, info
                INTERACTING_ENTITY = 0x20, //mob-id
                CHAT = 0x1, //text
                DAMAGE2 = 0x40, //value, type, curHealth, maxhealth
                APPEARANCE = 0x2,  //npcdefid (walking and idle animations, size)
                FACING = 0x4 //x, y
            }
        }
    }

    
}
