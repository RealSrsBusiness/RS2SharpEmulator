using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ServerEmulator.Core.Constants.Frames;


namespace ServerEmulator.Core.Network
{
    class StaticPackets
    {
        Connection c;

        public StaticPackets(Connection c)
        {
            this.c = c;
        }

        public void SetConfig(ushort id, byte value)
        {
            c.WriteOpCode(CONFIG_SET);
            c.Writer.WriteLEShort(id);
            c.Writer.WriteByte(value);
        }

        public void SendMessage(string msg)
        {
            c.WriteOpCodeVarSize(MSG_SEND, (byte)(msg.Length + 1));
            c.Writer.WriteJString(msg);
        }

        public void SendRequest(string name, bool isTrade = true)
        {
            SendMessage(name + (isTrade ? ":tradereq:" : ":duelreq:"));
        }

        public void ChatMessage(string sentFrom, string msg)
        {
            SendMessage(sentFrom + ":" + msg + ":chalreq:");
        }

        public void SystemUpdate(ushort time)
        {
            c.WriteOpCode(SYSTEM_UPDATE_SECS);
            c.Writer.WriteShort(time);
        }

        public void Logout()
        {
            c.WriteOpCode(DISCONNECT);
        }

        public void PlayerUpdate(byte[] update)
        {
            c.WriteOpCode(PLAYER_UPDATE);
            c.Writer.BaseStream.Write(update, 0, update.Length);
        }      

        public void Status(bool isMember, ushort playerId)
        {
            c.WriteOpCode(PLAYER_STATUS);
            c.Writer.WriteByte(isMember ? 1 : 0);
            c.Writer.WriteShort(playerId);
        }

        public void SetSkill(byte skillId, int xp, byte lvl)
        {
            c.WriteOpCode(PLAYER_SKILL);
            c.Writer.WriteByte(skillId);
            c.Writer.WriteInt(xp);
            c.Writer.WriteByte(lvl);
        }

        public void LocalPosition(byte x, byte y)
        {
            c.WriteOpCode(PLAYER_LOCATION);
            c.Writer.WriteNegatedByte(y);
            c.Writer.WriteNegatedByte(x);
        }

        public void RunEnergy(byte energy)
        {
            c.WriteOpCode(PLAYER_RUN_ENERGY);
            c.Writer.WriteByte(energy);
        }

        public void Weight(short weight)
        {
            c.WriteOpCode(PLAYER_WEIGHT);
            c.Writer.WriteShort(weight);
        }

        public void SetPlayerContextMenu(byte indexId, bool isPrimary, string text)
        {
            c.WriteOpCode(PLAYER_RIGHTCLICK);
            c.Writer.WriteNegatedByte(indexId);
            c.Writer.WriteByte(isPrimary ? 1 : 0);
            c.Writer.WriteJString(text);
        }
    
        public void ClearInventory(ushort id)
        {
            c.WriteOpCode(ITEM_ALL_CLEAR);
            c.Writer.WriteShort(id);
        }

        public void SetItems(ushort id, byte[] slot, ushort[] ids, int[] amts)
        {
            c.WriteOpCode(ITEM_SET);

            int size = slot.Length;
            c.Writer.WriteLEShort(size);
            c.Writer.WriteLEShort(id);

            for (int i = 0; i < size; i++)
            {
                c.Writer.WriteByte(slot[i]);
                c.Writer.WriteShort(ids[i]);
                c.Writer.WriteInt(amts[i]);
            }
        }

        public void ResetAnimations()
        {
            c.WriteOpCode(ANIM_ALL_RESET);
        }

        public void SetGFX(byte offset, ushort gfx, byte renderOffset, ushort delay)
        {
            c.WriteOpCode(ANIM_SET);
            c.Writer.WriteByte(offset);
            c.Writer.WriteShort(gfx);
            c.Writer.WriteByte(renderOffset);
            c.Writer.WriteShort(delay);
        }

        public void Multicombat(bool set)
        {
            c.WriteOpCode(MULTICOMBAT);
            c.Writer.WriteByte(set ? 1 : 0);
        }

        public void NPCUpdate()
        {
            c.WriteOpCode(NPC_UPDATE);
        }

        public void AddObject(int position, ushort id, byte data)
        {
            c.WriteOpCode(OBJ_ADD);
            c.Writer.WriteByte(position);
            c.Writer.WriteShort(id);
            c.Writer.WriteByte(data);
        }

        public void CreateProjectile(
            byte angle, byte desX, byte desY, short target, ushort gfx,
            byte start, byte end, short time, short speed, byte slope, byte distance)
        {
            c.WriteOpCode(PROJECTILE);
        }

        public void AddFloorItem(int id, short amt, byte offset)
        {
            c.WriteOpCode(FLOORITEM_ADD);
            c.Writer.WriteShort(amt);
            c.Writer.WriteByte(offset);
        }

        public void LoadRegion(ushort regionX, ushort regionY)
        {
            c.WriteOpCode(REGION_LOAD);
            c.Writer.WriteShortA(regionX);
            c.Writer.WriteShort(regionY);
        }

        public void PlaySong(ushort id)
        {
            c.WriteOpCode(SONG_PLAY);
            c.Writer.WriteShort(id);
        }

        public void QueueSong(ushort id, ushort delay)
        {
            c.WriteOpCode(SONG_QUEUE);
            c.Writer.WriteShort(id);
            c.Writer.WriteShort(delay);
        }

        public void CameraShake(byte id, byte jitter, byte amp, byte freq)
        {
            c.WriteOpCode(CAMERA_SHAKE);
            c.Writer.WriteByte(id);
            c.Writer.WriteByte(jitter);
            c.Writer.WriteByte(amp);
            c.Writer.WriteByte(freq);
        }

        public void WelcomeMessage(
            byte daysSinceRec, ushort unreadMsg, 
            bool isMember, int lastIp, ushort daysLastLogin)
        {
            c.WriteOpCode(INTF_WELCOME);
            c.Writer.WriteByte(daysSinceRec);
            c.Writer.WriteShort(unreadMsg);
            c.Writer.WriteByte(isMember ? 1 : 0);
            c.Writer.WriteInt(lastIp);
            c.Writer.WriteShort(daysLastLogin);
        }

        public void ShowInterface(int intfId)
        {
            c.WriteOpCode(INTF_SHOW);
            c.Writer.WriteLEShort(intfId);
        }

        public void AssignSidebar(ushort sideBar, byte intfAssign)
        {
            c.WriteOpCode(SIDEBAR_INTF_ASSIGN);
            c.Writer.WriteShort(sideBar);
            c.Writer.WriteByte(intfAssign);
        }

        public void ClearInterfaces()
        {
            c.WriteOpCode(INTF_CLEAR);
        }

        public void InputBox(bool textInput)
        {
            c.WriteOpCode(textInput ? INTF_ENTER_NAME : INTF_ENTER_AMT);
        }

    }
}
