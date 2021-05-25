using ServerEmulator.Core.NetworkProtocol;
using System;
using ServerEmulator.Core.IO;
using System.IO;

namespace ServerEmulator.Core.Game
{
    class AccessWatcher {
        object _value;
        internal AccessWatcher(object obj, bool initChange = false) {
            _value = obj;
            Changed = initChange;
        }

        public void Reset() => Changed = false;
        internal T Value<T>() {
            Changed = true;
            return (T)_value;
        }
        public bool Changed { get; private set; }
    }


    interface Effect {
        void Write(RSStreamWriter sw);
    }

    class PlayerAppearance : Effect
    {
        public ushort skill;
        public long username;

        public byte combatLevel, gender, headicon;
        public int[] appearanceValues, colorValues, idleAnimations;

        private byte[] buffer;

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
        public int animationId = 0, delay = 0; 
        public void Write(RSStreamWriter sw)
        {
            sw.WriteLEShort(animationId);
            sw.WriteNegatedByte(delay);
        }
    }

    class PlayerChat : Effect 
    {
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