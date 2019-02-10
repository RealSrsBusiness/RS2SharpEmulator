using ServerEmulator.Core.Game;
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
        internal Connection C { get; private set; }

        public StaticPackets(Connection c)
        {
            C = c;
        }

        public void SetConfig(ushort id, byte value)
        {
            C.WriteOpCode(CONFIG_SET);
            C.Writer.WriteLEShort(id);
            C.Writer.WriteByte(value);
        }

        public void SendMessage(string msg)
        {
            var pos = C.WriteOpCodeVar(MSG_SEND);
            C.Writer.WriteJString(msg);
            C.FinishVarPacket(pos);
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
            C.WriteOpCode(SYSTEM_UPDATE_SECS);
            C.Writer.WriteLEShort(time);
        }

        public void Logout()
        {
            C.WriteOpCode(DISCONNECT);
        }

        public void Status(bool isMember, ushort playerId)
        {
            C.WriteOpCode(PLAYER_STATUS);
            C.Writer.WriteByte(isMember ? 1 : 0);
            C.Writer.WriteShort(playerId);
        }

        public void SetSkill(byte skillId, int xp, byte lvl)
        {
            C.WriteOpCode(PLAYER_SKILL);
            C.Writer.WriteByte(skillId);
            C.Writer.WriteMEInt(xp);
            C.Writer.WriteByte(lvl);
        }

        public void LocalPosition(byte x, byte y)
        {
            C.WriteOpCode(PLAYER_LOCATION);
            C.Writer.WriteNegatedByte(y);
            C.Writer.WriteNegatedByte(x);
        }

        public void RunEnergy(byte energy)
        {
            C.WriteOpCode(PLAYER_RUN_ENERGY);
            C.Writer.WriteByte(energy);
        }

        public void Weight(short weight)
        {
            C.WriteOpCode(PLAYER_WEIGHT);
            C.Writer.WriteShort(weight);
        }

        public void SetPlayerContextMenu(byte indexId, bool isPrimary, string text)
        {
            var p = C.WriteOpCodeVar(PLAYER_RIGHTCLICK);
            C.Writer.WriteNegatedByte(indexId);
            C.Writer.WriteByte(isPrimary ? 1 : 0);
            C.Writer.WriteJString(text);
            C.FinishVarPacket(p);
        }
    
        public void ClearInventory(ushort id)
        {
            C.WriteOpCode(ITEM_ALL_CLEAR);
            C.Writer.WriteShort(id);
        }

        public void SetItems(ushort intfId, ItemStack[] items)
        {
            var p = C.WriteOpCodeVar(ITEM_SET, VarSizePacket.Type.SHORT);

            C.Writer.WriteShort(intfId);
            C.Writer.WriteShort(items.Length);

            for (int i = 0; i < items.Length; i++)
            {
                ItemStack s = items[i];
                if(s.amount > 254)
                {
                    C.Writer.WriteByte(255);
                    C.Writer.WriteIMEInt(s.amount);
                }
                else
                {
                    C.Writer.WriteByte(s.amount);
                }
                C.Writer.WriteLEShortA(s.id + 1);
            }

            C.FinishVarPacket(p);
        }

        public void SetItemSlots()
        {
            var p = C.WriteOpCodeVar(ITEM_SLOT_SET, VarSizePacket.Type.SHORT);
        }

        public void ResetAnimations()
        {
            C.WriteOpCode(ANIM_ALL_RESET);
        }

        public void SetGFX(byte offset, ushort gfx, byte renderOffset, ushort delay)
        {
            C.WriteOpCode(ANIM_SET);
            C.Writer.WriteByte(offset);
            C.Writer.WriteShort(gfx);
            C.Writer.WriteByte(renderOffset);
            C.Writer.WriteShort(delay);
        }

        public void Multicombat(bool set)
        {
            C.WriteOpCode(MULTICOMBAT);
            C.Writer.WriteByte(set ? 1 : 0);
        }

        public void AddObject(int position, ushort id, byte data)
        {
            C.WriteOpCode(OBJ_ADD);
            C.Writer.WriteByte(position);
            C.Writer.WriteShort(id);
            C.Writer.WriteByte(data);
        }

        public void CreateProjectile(
            byte angle, byte desX, byte desY, short target, ushort gfx,
            byte start, byte end, short time, short speed, byte slope, byte distance)
        {
            C.WriteOpCode(PROJECTILE);
        }

        public void AddFloorItem(int id, short amt, byte offset)
        {
            C.WriteOpCode(FLOORITEM_ADD);
            C.Writer.WriteShort(amt);
            C.Writer.WriteByte(offset);
        }

        public void LoadRegion(ushort regionX, ushort regionY)
        {
            C.WriteOpCode(REGION_LOAD);
            C.Writer.WriteShortA(regionX);
            C.Writer.WriteShort(regionY);
        }

        public void ConstructRegion()
        {
            C.WriteOpCode(REGION_CONSTRUCT);
        }

        public void PlaySong(ushort id)
        {
            C.WriteOpCode(SONG_PLAY);
            C.Writer.WriteLEShort(id);
        }

        public void QueueSong(ushort id, ushort delay)
        {
            C.WriteOpCode(SONG_QUEUE);
            C.Writer.WriteShort(id);
            C.Writer.WriteShort(delay);
        }

        public void CameraShake(byte id, byte jitter, byte amp, byte freq)
        {
            C.WriteOpCode(CAMERA_SHAKE);
            C.Writer.WriteByte(id);
            C.Writer.WriteByte(jitter);
            C.Writer.WriteByte(amp);
            C.Writer.WriteByte(freq);
        }

        public void WelcomeMessage(
            byte daysSinceRec, ushort unreadMsg, 
            bool isMember, int lastIp, ushort daysLastLogin)
        {
            C.WriteOpCode(INTF_WELCOME);
            C.Writer.WriteByte(daysSinceRec);
            C.Writer.WriteShort(unreadMsg);
            C.Writer.WriteByte(isMember ? 1 : 0);
            C.Writer.WriteInt(lastIp);
            C.Writer.WriteShort(daysLastLogin);
        }

        public void ShowInterface(int intfId)
        {
            C.WriteOpCode(INTF_SHOW);
            C.Writer.WriteLEShort(intfId);
        }

        public void SetInterfaceText(int id, string text)
        {
            var p = C.WriteOpCodeVar(INTF_TEXT_ADD, VarSizePacket.Type.SHORT);
            C.Writer.WriteJString(text);
            C.Writer.WriteShortA(id);
            C.FinishVarPacket(p);
        }

        public void AssignSidebar(byte sideicon, ushort intf)
        {
            C.WriteOpCode(SIDEBAR_INTF_ASSIGN);
            C.Writer.WriteShort(intf);
            C.Writer.WriteByte(sideicon + 128);
        }

        public void ClearInterfaces()
        {
            C.WriteOpCode(INTF_CLEAR);
        }

        public void InputBox(bool textInput)
        {
            C.WriteOpCode(textInput ? INTF_ENTER_NAME : INTF_ENTER_AMT);
        }

        public void FriendList(byte status)
        {
            C.WriteOpCode(FRIENDLIST_STATUS);
            C.Writer.WriteByte(status);
        }

        public void SetFriend(string friend, short world)
        {
            C.WriteOpCode(FRIEND_ADD);
            C.Writer.WriteLong(Extensions.ToLong(friend));
            C.Writer.WriteByte(world < 0 ? 0 : (world + 9));
        }

        public void PlayerUpdate(byte[] update)
        {
            var p = C.WriteOpCodeVar(PLAYER_UPDATE, VarSizePacket.Type.SHORT);
            C.Writer.BaseStream.Write(update, 0, update.Length);
            C.FinishVarPacket(p);
        }

        public void NPCUpdate(byte[] data)
        {
            C.WriteOpCode(NPC_UPDATE);
        }

        public void RegionalUpdate(byte[] data)
        {
            C.WriteOpCodeVar(REGION_UPDATE);
        }

    }
}
