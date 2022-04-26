using ServerEmulator.Core.NetworkProtocol;
using System;
using System.IO;
using ServerEmulator.Core.IO;
using System.Collections.Generic;

namespace ServerEmulator.Core.Game
{
    public class PlayerEntity : Actor, ICloneable
    {
        public bool teleported = true, running = true;
        public bool justLoggedIn { get => teleported; }

        public Direction[] LastSteps { get; private set; } = new Direction[2] { Direction.NONE, Direction.NONE };

        Direction[] movement;
        int walkingQueue = -1;

        internal BufferedEffectStates effects = new BufferedEffectStates();

        ItemContainer inventory = new ItemContainer(28), equipment = new ItemContainer(11);
        SkillTracker skills = new SkillTracker();

        internal PlayerEntity(int id, Account account)
        {   
            base.id = id;
            var appear = effects.Appearance; //also sets the "needRefresh" flag

            //default values
            appear.appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 };
            appear.colorValues = new int[] { 7, 8, 9, 5, 0 };
            appear.idleAnimations = new int[] { 808, 823, 819, 820, 821, 822, 824 };
            appear.username = account.displayname.ToLong();

            PostProcess = () => effects.Reset();

            Update = () =>
            {
                lock(_lock)
                {
                    LastSteps[0] = Direction.NONE;
                    LastSteps[1] = Direction.NONE;

                    if (walkingQueue != -1)
                    {
                        LastSteps[0] = movement[walkingQueue++];
                        Coordinate moved = Movement.Directions[(int)LastSteps[0]];

                        if (running && walkingQueue < movement.Length) //can we run?
                        {
                            LastSteps[1] = movement[walkingQueue++];
                            moved += Movement.Directions[(int)LastSteps[1]];
                        }

                        if (walkingQueue >= movement.Length)
                            walkingQueue = -1;

                        x += moved.x;
                        y += moved.y;
                    }
                }
            };
        }

        void DoDamage() 
        {
            //effects.Damage.damage = 100;
        }

        public void SetMovement(Coordinate[] waypointCoords) //todo: sometimes causes a race condition, i think?
        {
            lock(_lock)
            {
                movement = Movement.InterpolateWaypoints(waypointCoords, x, y);

                if (movement.Length > 0)
                    walkingQueue = 0;
                else
                    walkingQueue = -1;
            }
        }

        /* 
            0 = head
            1 = cape
            2 = amulet
            3 = sword/weapon
            4 = chest
            5 = shield
            6 = [empty] 
            7 = legs
            8 = [empty] 
            9 = gloves 
            10 = boots
            11 = [empty] 
            12 = ring
            13 = arrows
            14 = [empty] 
        */
        public const int HEAD = 0, CAPE = 1, AMULET = 2, SWORD = 3, CHEST = 4, SHIELD = 5, 
            LEGS = 6, GLOVES = 7, BOOTS = 8, RING = 9, ARROWS = 10;

        //needed because there are unused slots that don't align with the 'slot'-sprites on the equipment screen
        int[] EQUIPMENT_INF_SLOTID = new int[] { 0, 1, 2, 3, 4, 5, 7, 9, 10, 12, 13 }; 
        

        private object _lock = new object();
        public object Clone() => MemberwiseClone();

    }

    public class SkillTracker
    {
        Skill[] skills = new Skill[SkillCount];

        
        public const int ATTACK = 0, DEFENSE = 1, STRENGTH = 2, HITPOINTS = 3, RANGED = 4, PRAYER = 5, MAGIC = 6,
            COOKING = 7, WOODCUTTING = 8, FLETCHING = 9, FISHING = 10, FIREMAKING = 11, CRAFTING = 12, 
            SMITHING = 13, MINING = 14, HERBLORE = 15, AGILITY = 16, THIEVING = 17, SLAYER = 18, FARMING = 19,
            RUNECRAFT = 20;
        public const int SkillCount = 21;
    }

    public class ItemContainer 
    {
        ItemStack[] items;
        internal List<(int slotId, Operation action)> actions { private set; get; } = new List<(int slotId, Operation action)>();

        public ItemContainer(int size) => items = new ItemStack[size];

        public int Count() => items.Length;
        
        public int GetFreeSlot() 
        {
            for (int i = 0; i < items.Length; i++)
                if(items[i] == null)
                    return i;
            return -1; //no free slot
        }

        public int FindItem(int id) //returns the index of the slot with that item in it
        {
            for (int i = 0; i < items.Length; i++)
                if(items[i].id == id)
                    return i;
            return -1;
        }

        internal int Add(int id, int count = 1) 
        {
            int slot = GetFreeSlot();

            if(slot != -1) 
            {
                var item = items[slot] = new ItemStack() { id = id, amount = count };
                actions.Add((slot, Operation.ADD));
            }
            return slot;
        }

        internal ref int Amount(int slot)
        { 
            actions.Add((slot, Operation.UPDATE_AMOUNT));
            return ref items[slot].amount; //reference, kinda like a pointer in C, allows you to modify the value from the "outside"
        }

        internal void Remove(int slot) 
        {
            items[slot] = null;
            actions.Add((slot, Operation.REMOVE));
        }

        internal (int, int) GetAtSlot(int slot) 
        {
            return (items[slot].id, items[slot].amount);
        }

        internal (int, int) InsertAtSlot(int slot, int id, int count = 1) 
        {
            var oldItem = GetAtSlot(slot);
            items[slot] = new ItemStack() { id = id, amount = count };
            return oldItem;
        }

        internal void SwapSlot(int slot1, int slot2) 
        {
            var temp = items[slot1];
            items[slot1] = items[slot2];
            items[slot2] = temp;
            actions.Add((slot1 << 16 | (slot2 & 0xffff), Operation.SWAP_SLOT)); //todo: bit shift, kinda awkward
        }

        internal void ClearActions() => actions.Clear();

        internal enum Operation { ADD, REMOVE, UPDATE_AMOUNT, SWAP_SLOT }
    }

    public class NPCEntity : Actor
    {

    }

    //moving, "living" entity
    public class Actor : WorldEntity
    {
        void ApplyDamage() { }

        void MoveTo() { }

        void SetAnimation() { }

        void InteractingEntity(Actor target) { }

        void Talk(string text) { }

    }
        
    public class GroundItemEntity : WorldEntity 
    { 
    
    }

    public class ObjectEntity : WorldEntity 
    {
        bool replaceObject;
        int cyclesToExpire;
    }

    public class WorldEntity
    {
        public int id;
        public int x, y, z;

        public Action Update = delegate { }; //todo: replace with event?
        internal Action PostProcess = delegate { }; //don't use this, this will be removed at one point

        //chunk of 8 tiles, also called "region" in client
        public ushort MapChunkX => (ushort)(x / 8); //same as "x >> 3", (base)2 ^ 3 = 8
        public ushort MapChunkY => (ushort)(y / 8); 

        //a map segment of 104x104 tiles consists of 13 chunks along each axis, this assumes the middle position and "goes back" 6 chunks to get to the origin
        public int SegmentOriginX => (MapChunkX - 6) * 8; //where the map segment would be at 0x0
        public int SegmentOriginY => (MapChunkY - 6) * 8; 

        //the X and Y within the middle(7th) chunk, can range from 48 to 55 (inclusive); first 6 chunks: 0-47, last 6 chunks: 56-103
        public byte XMiddleChunk => (byte)(x - SegmentOriginX);
        public byte YMiddleChunk => (byte)(y - SegmentOriginY);

        public Coordinate VerifyDistance(int x, int y, int steps = 15) //5 bits can only encode 32 values, from -16 to 15 and 0
        {
            int difX = this.x - x;
            int difY = this.y - y;

            int stepsN = -steps;

            if (difX > steps || difX < stepsN || difY > steps || difY < stepsN)
                return Coordinate.NONE;

            return new Coordinate() { x = difX, y = difY };
        }
    }

    
}
