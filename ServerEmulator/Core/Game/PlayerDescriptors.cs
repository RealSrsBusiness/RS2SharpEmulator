using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static ServerEmulator.Core.Constants;

namespace ServerEmulator.Core.Game
{ //some things in here should probably be removed and instead be moved to Content

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

        public int SlotOfItem(int id) //returns the index of the slot with that item in it
        {
            for (int i = 0; i < items.Length; i++)
                if(items[i].id == id)
                    return i;
            return -1;
        }

        internal int Add(int id, int count = 1) //returns the slot the item was added to; -1 if full
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
            return ref items[slot].amount; //reference, kinda like a pointer in C, allows you to modify the value from outside
        }

        internal void ClearSlot(int slot) 
        {
            items[slot] = null;
            actions.Add((slot, Operation.REMOVE));
        }

        internal (int id, int amt) GetAtSlot(int slot) 
        {
            return (items[slot].id, items[slot].amount);
        }

        internal (int id, int amt) PopulateSlot(int slot, int id, int count = 1) //populates a specific slot and returns the current
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

    public class SkillTracker
    {
        Skill[] skills = new Skill[SkillCount];

        
        public const int ATTACK = 0, DEFENSE = 1, STRENGTH = 2, HITPOINTS = 3, RANGED = 4, PRAYER = 5, MAGIC = 6,
            COOKING = 7, WOODCUTTING = 8, FLETCHING = 9, FISHING = 10, FIREMAKING = 11, CRAFTING = 12, 
            SMITHING = 13, MINING = 14, HERBLORE = 15, AGILITY = 16, THIEVING = 17, SLAYER = 18, FARMING = 19,
            RUNECRAFT = 20;
        public const int SkillCount = 21;
    }
}