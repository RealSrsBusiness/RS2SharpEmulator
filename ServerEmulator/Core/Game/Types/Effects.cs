using ServerEmulator.Core.NetworkProtocol;
using System;
using ServerEmulator.Core.IO;
using System.IO;

namespace ServerEmulator.Core.Game
{
    class EffectStates //WatchedEffectStates
    {
        AccessWatcher[] effects = new AccessWatcher[] { 
            null, //forced_movement
            null, //graphic
            new AccessWatcher(new PlayerAnimation()), 
            null, //forced chat
            new AccessWatcher(new PlayerChat()), 
            null, //interacting_entity
            new AccessWatcher(new PlayerAppearance()), 
            null, //facing
            new AccessWatcher(new PlayerDamage()), 
            null, //damage2
        };

        public PlayerAnimation Animation => effects[ANIMATION].Value<PlayerAnimation>();
        public PlayerChat Chat => effects[CHAT_TEXT].Value<PlayerChat>();
        public PlayerAppearance Appearance => effects[APPEARANCE_CHANGED].Value<PlayerAppearance>();
        public PlayerDamage Damage => effects[DAMAGE].Value<PlayerDamage>();

        


        const int FORCED_MOVEMENT = 0, GRAPHIC = 1, ANIMATION = 2, FORCED_CHAT = 3, CHAT_TEXT = 4, INTERACTING_ENTITY = 5, 
        APPEARANCE_CHANGED = 6, FACING = 7, DAMAGE = 8, DAMAGE_2 = 9;


        static int[] masks = { 0x400, 0x100, 0x8, 0x4, 0x80, 0x1, 0x10, 0x2, 0x20, 0x200 };
    }

    class AccessWatcher //todo: probably can be simplified and merged with EffectStates
    {
        internal AccessWatcher(object value, bool initChange = false) 
        {
            _value = value;
            Changed = initChange;
        }

        public void Reset() => Changed = false;

        internal T Value<T>() 
        {
            Changed = true;
            return (T)_value;
        }

        object _value;
        public bool Changed { get; private set; }
    }

    interface Effect 
    {
        int cycles { get; } //how long should the effect appear for; -1 = unlimited until changed
        void Write(RSStreamWriter sw);
    }

    class PlayerAppearance : Effect
    {
        public ushort skill;
        public long username;

        public byte combatLevel, gender, headicon;
        public int[] appearanceValues, colorValues, idleAnimations;

        private byte[] buffer;

        public int cycles => -1;

        public void Write(RSStreamWriter sw_)
        {
            if(buffer != null) {
                sw_.WriteBytes(buffer, 0, buffer.Length);
                return;
            }

            var sw = new RSStreamWriter(new MemoryStream());
            sw.BaseStream.Position = 1; //reserve for length

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

            for (int i = 0; i < idleAnimations.Length; i++)
            {
                int v = idleAnimations[i];
                sw.WriteShort(v);
            }

            sw.WriteLong(username);
            sw.WriteByte(combatLevel);
            sw.WriteShort(skill);

            sw.BaseStream.Position = 0; //to get to the size position
            byte size = (byte)(sw.BaseStream.Length - 1);
            sw.WriteNegatedByte(size);

            buffer = sw.BaseStream.ToArray();
            sw_.WriteBytes(buffer, 0, buffer.Length);
        }

        public void Clear() => buffer = null;
    }

    class PlayerAnimation : Effect 
    {
        public int cycles => -1;
        public int animationId = 0, delay = 0;

        public void Write(RSStreamWriter sw)
        {
            sw.WriteLEShort(animationId);
            sw.WriteNegatedByte(delay);
        }
    }

    class PlayerChat : Effect 
    {
        public int cycles => 3;
        public int textInfo = 0, privilage = 0, offset = 0;

        public void Write(RSStreamWriter sw)
        {
            sw.WriteLEShort(textInfo);
            sw.WriteByte(privilage);
            sw.WriteNegatedByte(offset);
            sw.WriteReverseDataA(new byte[0], 0, 0);
        }
    }

    class PlayerDamage : Effect
    {
        public int cycles => 1;
        public int damage = 0, type = 0, health = 0, maxHealth = 0;

        public void Write(RSStreamWriter sw)
        {
            sw.WriteByte(damage);
            sw.WriteByte(type);
            sw.WriteByte(health);
            sw.WriteByte(maxHealth);
        }
    }

    /*
            //Player
        FORCED_MOVEMENT = 0x400, //init x, y, dest x, y, start, end, direction
        GRAPHIC = 0x100, //graphic, info
        ANIMATION = 0x8, //animation, delay
        FORCED_CHAT = 0x4, //text
        CHAT_TEXT = 0x80, //textinfo, privilege, offset, off
        INTERACTING_ENTITY = 0x1, //mob-id
        APPEARANCE_CHANGED = 0x10,  //gender, headicon, bodyparts, colors, idle and walk animations, name, combat, skill
        FACING = 0x2, //x, y
        DAMAGE = 0x20, //damage, type, curHealth, maxHealth
        DAMAGE_2 = 0x200 //damage, type, curHealth, maxHealth

            //NPC
        ANIMATION = 0x10, //id, delay
        DAMAGE = 0x8, //value, type
        GRAPHIC = 0x80, //graphic, info
        INTERACTING_ENTITY = 0x20, //mob-id
        CHAT = 0x1, //text
        DAMAGE2 = 0x40, //value, type, curHealth, maxhealth
        APPEARANCE_CHANGED = 0x2,  //npcdefid (walking and idle animations, size)
        FACING = 0x4 //x, y
    */
}