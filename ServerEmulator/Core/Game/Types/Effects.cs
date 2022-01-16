using ServerEmulator.Core.NetworkProtocol;
using System;
using ServerEmulator.Core.IO;
using System.IO;

namespace ServerEmulator.Core.Game
{
    //holds effect data, buffers it and keeps track if something changed
    class BufferedEffectStates //todo: optimization idea: save all buffers into a list shared by all clients and just clear it per cycle (instead of post processing)
    {
        public PlayerAnimation Animation => Get<PlayerAnimation>(ANIMATION);
        public PlayerChat Chat => Get<PlayerChat>(CHAT);
        public PlayerAppearance Appearance => Get<PlayerAppearance>(APPEARANCE);
        public PlayerDamage Damage => Get<PlayerDamage>(DAMAGE);

        private T Get<T>(int index) where T : Effect, new() 
        {
            if(effects[index] == null)
                effects[index] = new T();
            
            effects[index].needRefresh = true;
            return (T)effects[index];
        }

        public byte[] Full {
        get 
        {
            if(bufferFull == null)
                bufferFull = BuildBuffer(true);
            return bufferFull;
        }}
        
        public byte[] Incremental {   
        get 
        {
            if(bufferIncremental == null)
                bufferIncremental = BuildBuffer(false);
            return bufferIncremental;
        }}

        byte[] bufferFull, bufferIncremental; //incremental = only what changed from last cycle

        private byte[] BuildBuffer(bool full) 
        {
            short mask = 0x0;

            var writer = new RSStreamWriter(new MemoryStream()); //todo: optimize?
            for (int i = 0; i < effects.Length; i++)
            {
                var curEffect = effects[i];
                if(curEffect == null)
                    continue;

                if(curEffect.needRefresh || (curEffect.Persistant && full))
                {
                    mask |= masks[i];
                    curEffect.Write(writer);
                }
            }

            int maskLength = ((mask >> 8) > 0) ? 2 : 1;
            byte[] buffer = new byte[writer.BaseStream.Length + maskLength];
            buffer[0] = (byte)mask;

            if(maskLength == 2) {
                buffer[0] |= DOUBLE_BYTE_MASK;
                buffer[1] = (byte)(mask >> 8);
            } 

            writer.BaseStream.Position = 0;
            writer.BaseStream.Read(buffer, maskLength, (int)writer.BaseStream.Length);

            return buffer;
        }

        public void Reset() 
        {
            for (int i = 0; i < effects.Length; i++)
            {
                var curEffect = effects[i];
                if(curEffect == null)
                    continue;

                if(curEffect.Persistant)
                    curEffect.needRefresh = false;
                else
                    effects[i] = null;                    
            }
            bufferFull = null;
            bufferIncremental = null;
        }

        Effect[] effects = new Effect[masks.Length];

        const byte DOUBLE_BYTE_MASK = 0x40; //indicates that the mask should be encoded in 2 bytes instead of 1
        static short[] masks = 
            { 0x400, 0x100, 0x8, 0x4, 0x80, 0x1, 0x10, 0x2, 0x20, 0x200 }; //important: needs to be in the right order as it's specified in the client

        const int FORCED_MOVEMENT = 0, GRAPHIC = 1, ANIMATION = 2, FORCED_CHAT = 3, CHAT = 4, //corrosponds to "masks"
        INTERACTING_ENTITY = 5, APPEARANCE = 6, FACING = 7, DAMAGE = 8, DAMAGE_2 = 9; 
    }

    abstract class Effect 
    {
        public abstract bool Persistant { get; } //does this effect last over multiple cycles? e.g. appearance, animation
        public bool needRefresh; //a value changed, so the effect needs to be resent to the client
        public abstract void Write(RSStreamWriter sw);
    }

    class PlayerAppearance : Effect
    {
        public ushort skill;
        public long username;

        public byte combatLevel = 10, gender, headicon;
        public int[] appearanceValues = new int[12], colorValues = new int[5], idleAnimations = new int[7];

        public override void Write(RSStreamWriter sw)
        {
            long orgPosition = sw.BaseStream.Position; //start position
            sw.BaseStream.Position++; //reserve 1 byte for length

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

            sw.BaseStream.Position = orgPosition; //to get to the size position
            byte size = (byte)(sw.BaseStream.Length - orgPosition - 1);
            sw.WriteNegatedByte(size);

            sw.BaseStream.Position = orgPosition + size + 1;
        }

        public override bool Persistant => true;
    }

    class PlayerAnimation : Effect 
    {
        public int animationId = 0, delay = 0;

        public override void Write(RSStreamWriter sw)
        {
            sw.WriteLEShort(animationId);
            sw.WriteNegatedByte(delay);
        }

        public override bool Persistant => true;
    }

    class PlayerChat : Effect 
    {
        public string text;
        public int color = 0, animationEffect = 0, playerRights = 0;

        public override void Write(RSStreamWriter sw)
        {
            var bytesText = text.ToJagString();

            sw.WriteLEShort(((color & 0xFF) << 8) + (animationEffect & 0xFF));
            sw.WriteByte(playerRights); //privilege
            sw.WriteNegatedByte(bytesText.Length);
            sw.WriteReverseData(bytesText, bytesText.Length, 0);
        }

        public override bool Persistant => false;
    }

    class PlayerDamage : Effect
    {
        public int damage = 0, type = 0, health = 0, maxHealth = 0;

        public override void Write(RSStreamWriter sw)
        {
            sw.WriteByte(damage);
            sw.WriteByteA(type); //0 to 4; blue, red, green, orange, sickness?
            sw.WriteNegatedByte(health);
            sw.WriteByte(maxHealth);
        }

        public override bool Persistant => false;
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