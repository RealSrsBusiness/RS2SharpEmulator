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
        internal Connection c;
        public StaticPackets(Connection c) => this.c = c;

        public void Send() => c.Send();
        public void Clear() => c.Writer.BaseStream.SetLength(0);

        public void SetConfig(ushort id, byte value)
        {
            c.WriteOpCode(CONFIG_SET);
            c.Writer.WriteLEShort(id);
            c.Writer.WriteByte(value);
        }

        public void SendMessage(string msg)
        {
            var pos = c.WriteOpCodeVar(MSG_SEND);
            c.Writer.WriteTerminatedString(msg);
            c.FinishVarPacket(pos);
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
            c.Writer.WriteLEShort(time);
        }

        public void Logout()
        {
            c.WriteOpCode(DISCONNECT);
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
            c.Writer.WriteMEInt(xp);
            c.Writer.WriteByte(lvl);
        }

        public void LoadedMapPosition(byte x, byte y) //position on a 104x104 map segment (equates to exacly 1 map file)
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

        public void SpecialAttackAmt(int barId, int amount) 
        { //kind of a weird one because there's no dedicated variable/packet to it
            c.WriteOpCode(INTF_HIDDEN);
            c.Writer.WriteByte(1);
            c.Writer.WriteShort(barId);

            c.WriteOpCode(BTN_SET);
            c.Writer.WriteShort(300);
            c.Writer.WriteInt(amount);
        }

        public void SetPlayerContextMenu(byte indexId, bool isPrimary, string text)
        {
            var p = c.WriteOpCodeVar(PLAYER_RIGHTCLICK);
            c.Writer.WriteNegatedByte(indexId);
            c.Writer.WriteByte(isPrimary ? 1 : 0);
            c.Writer.WriteTerminatedString(text);
            c.FinishVarPacket(p);
        }

        public void ClearItemContainer(ushort intfId)
        {
            c.WriteOpCode(ITEM_ALL_CLEAR);
            c.Writer.WriteLEShort(intfId);
        }

        public void SetItemsBySlots(ushort intfId, (short slot, ushort? id, int amt)[] items)
        {
            var p = c.WriteOpCodeVar(ITEM_SLOT_SET, VarSizePacket.Type.SHORT);

            c.Writer.WriteShort(intfId);

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                c.Writer.WriteDynamicShort(item.slot);
                c.Writer.WriteShort(item.id != null ? (int)item.id + 1 : 0); //+1 because 0 is reserved for "empty"

                if(item.amt > 254) 
                {
                    c.Writer.WriteByte(255);
                    c.Writer.WriteInt(item.amt);
                }
                else 
                {
                    c.Writer.WriteByte(item.amt);
                }
            }

            c.FinishVarPacket(p);
        }

        public void SetItems(ushort intfId, ItemStack[] items)
        {
            var p = c.WriteOpCodeVar(ITEM_SET, VarSizePacket.Type.SHORT);

            c.Writer.WriteShort(intfId);
            c.Writer.WriteShort(items.Length);

            for (int i = 0; i < items.Length; i++)
            {
                ItemStack s = items[i];
                if(s.amount > 254)
                {
                    c.Writer.WriteByte(255);
                    c.Writer.WriteIMEInt(s.amount);
                }
                else
                {
                    c.Writer.WriteByte(s.amount);
                }
                c.Writer.WriteLEShortA(s.id + 1); 
            }

            c.FinishVarPacket(p);
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

        public void AddObject(ushort id, byte type, byte orientation /*from 0 to 3*/, byte offsetX = 0, byte offsetY = 0)
        {
            c.WriteOpCode(OBJ_ADD);
            c.Writer.WriteByte(((offsetX & 7) << 4) | (offsetY & 7));
            c.Writer.WriteLEShort(id);
            c.Writer.WriteByteS((type << 2) | (orientation & 3));
        }

        public void RemoveObject(byte type, byte orientation, byte offsetX = 0, byte offsetY = 0) 
        {
            c.WriteOpCode(OBJ_REMOVE);
            c.Writer.WriteNegatedByte((type << 2) | (orientation & 3));
            c.Writer.WriteByte(((offsetX & 7) << 4) | (offsetY & 7));
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

        public void ConstructRegion()
        {
            c.WriteOpCode(REGION_CONSTRUCT);
        }

        public void PlaySong(ushort id)
        {
            c.WriteOpCode(SONG_PLAY);
            c.Writer.WriteLEShort(id);
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

        public void WelcomePopup(
            byte daysSinceRec, ushort unreadMsg, 
            bool isMember, byte[] ip, ushort daysLastLogin)
        {
            int lastIp = ip[0] << 24 | ip[1] << 16 | ip[2] << 8 | ip[3];

            c.WriteOpCode(INTF_WELCOME);
            c.Writer.WriteNegatedByte(daysSinceRec);
            c.Writer.WriteShortA(unreadMsg);
            c.Writer.WriteByte(isMember ? 1 : 0);
            c.Writer.WriteIMEInt(lastIp);
            c.Writer.WriteShort(daysLastLogin);
        }

        public void ShowInterface(int intfId)
        {
            c.WriteOpCode(INTF_SHOW);
            c.Writer.WriteLEShort(intfId);
        }

        public void SetInterfaceText(int id, string text)
        {
            var p = c.WriteOpCodeVar(INTF_TEXT_ADD, VarSizePacket.Type.SHORT);
            c.Writer.WriteTerminatedString(text);
            c.Writer.WriteShortA(id);
            c.FinishVarPacket(p);
        }

        public void AssignSidebar(byte sideicon, ushort intf)
        {
            c.WriteOpCode(SIDEBAR_INTF_ASSIGN);
            c.Writer.WriteShort(intf);
            c.Writer.WriteByte(sideicon + 128);
        }

        public void ClearInterfaces()
        {
            c.WriteOpCode(INTF_CLEAR);
        }

        public void InputBox(bool textInput)
        {
            c.WriteOpCode(textInput ? INTF_ENTER_NAME : INTF_ENTER_AMT);
        }

        public void FriendList(byte status)
        {
            c.WriteOpCode(FRIENDLIST_STATUS);
            c.Writer.WriteByte(status);
        }

        public void SetFriend(string friend, short world)
        {
            c.WriteOpCode(FRIEND_ADD);
            c.Writer.WriteLong(Extensions.ToLong(friend));
            c.Writer.WriteByte(world < 0 ? 0 : (world + 9));
        }

        public void PlayerUpdate(List<bool> movementPlayerList, List<byte[]> effectUpdates)
        {
            var p = c.WriteOpCodeVar(PLAYER_UPDATE, VarSizePacket.Type.SHORT);

            var bitArray = movementPlayerList.ToByteArray();
            c.Writer.BaseStream.Write(bitArray, 0, bitArray.Length);

            for (int i = 0; i < effectUpdates.Count; i++)
            {
                var byteChunk = effectUpdates[i];
                c.Writer.BaseStream.Write(byteChunk, 0, byteChunk.Length);
            }

            c.FinishVarPacket(p); //todo: optimize: constantly copying arrays is probably not such a good idea (check Client.RenderScreen)
        }

        public void NPCUpdate(byte[] data)
        {
            c.WriteOpCode(NPC_UPDATE);
        }

        public void RegionalUpdate(byte[] data)
        {
            c.WriteOpCodeVar(REGION_UPDATE);
        }

    }
}
