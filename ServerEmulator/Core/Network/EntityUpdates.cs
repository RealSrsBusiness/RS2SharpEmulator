using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static ServerEmulator.Core.Game.WorldEntity;
using static ServerEmulator.Core.Game.WorldEntity.PlayerEntity;

namespace ServerEmulator.Core.NetworkProtocol
{
    internal class EntityUpdates
    {
        internal struct PlayerListEntry
        {
            public int index, x, y;
            public bool effectUpdate, teleport;
        }

        //written when there are new players in the area
        internal static void WriteNewPlayerList(ref List<bool> data, PlayerListEntry[] newPlayers)
        {
            for (int i = 0; i < newPlayers.Length; i++)
            {
                var p = newPlayers[i];
                data.EncodeValue(11, p.index);
                data.EncodeValue(1, p.effectUpdate ? 1 : 0);
                data.EncodeValue(1, p.teleport ? 1 : 0);
                data.EncodeValue(5, p.y);
                data.EncodeValue(5, p.x);
            }

            data.EncodeValue(11, 2047); //2047 is the highest number in 11 bits
        }

        internal struct NPCListEntry
        {
            public int index, x, y;
            public bool effectUpdate, teleport;
            public int npcDef;
        }

        internal static void WriteNewNPCList(ref List<bool> data, NPCListEntry[] newNPCs)
        {
            for (int i = 0; i < newNPCs.Length; i++)
            {
                var npc = newNPCs[i];
                data.EncodeValue(14, newNPCs.Length);
                data.EncodeValue(5, npc.y);
                data.EncodeValue(5, npc.x);
                data.EncodeValue(1, npc.teleport ? 1 : 0);
                data.EncodeValue(12, npc.npcDef);
                data.EncodeValue(1, npc.effectUpdate ? 1 : 0);
            }
            data.EncodeValue(14, 16383); //max value
        }

        internal static void WritePlayerMovement(ref List<bool> bits, bool effectUpdateRequired, 
            int firstX = -1, int secondY = -1, int planeSpawn = -1, bool teleport = false)
        {
            if (!effectUpdateRequired && firstX == -1) //no update
            {
                bits.EncodeValue(1, 0);
            }
            else if(effectUpdateRequired && firstX == -1) //just effect update
            {
                bits.EncodeValue(1, 1);
                bits.EncodeValue(2, 0);
            }
            else
            {
                if(planeSpawn != -1) //movement update
                {
                    if(secondY != -1) //running
                    {
                        bits.EncodeValue(1, 1);
                        bits.EncodeValue(2, 2);
                        bits.EncodeValue(3, firstX);
                        bits.EncodeValue(3, secondY);
                        bits.EncodeValue(1, effectUpdateRequired ? 1 : 0);
                    }
                    else //walking
                    {
                        bits.EncodeValue(1, 1);
                        bits.EncodeValue(2, 1);
                        bits.EncodeValue(3, firstX);
                        bits.EncodeValue(1, effectUpdateRequired ? 1 : 0);
                    }
                }
                else //logged in or teleported
                {
                    bits.EncodeValue(1, 1);
                    bits.EncodeValue(2, 3);
                    bits.EncodeValue(2, planeSpawn);
                    bits.EncodeValue(1, teleport ? 1 : 0);
                    bits.EncodeValue(1, effectUpdateRequired ? 1 : 0);
                    bits.EncodeValue(7, secondY);
                    bits.EncodeValue(7, firstX);
                }
            }
        }

        internal struct EntityMovementUpdate
        {
            public bool effectUpdateRequired;

            public int firstDirection, secondDirection;
            public bool shouldRemove;
        }

        //walking and removing of both players and npcs
        internal static void WriteEntityMovement(ref List<bool> data, EntityMovementUpdate[] entities)
        {
            data.EncodeValue(8, entities.Length);

            for (int i = 0; i < entities.Length; i++)
            {
                var p = entities[i];

                if(!p.shouldRemove)
                {
                    if(p.firstDirection != -1)
                    {
                        if(p.secondDirection != -1) //running
                        {
                            data.EncodeValue(1, 1);
                            data.EncodeValue(2, 2);
                            data.EncodeValue(3, p.firstDirection);
                            data.EncodeValue(3, p.secondDirection);
                            data.EncodeValue(1, p.effectUpdateRequired ? 1 : 0);
                        }
                        else //walking
                        {
                            data.EncodeValue(1, 1);
                            data.EncodeValue(2, 1);
                            data.EncodeValue(3, p.firstDirection);
                            data.EncodeValue(1, p.effectUpdateRequired ? 1 : 0);
                        }
                    }
                    else if(p.effectUpdateRequired) //only effect update
                    {
                        data.EncodeValue(1, 1);
                        data.EncodeValue(2, 0);
                    }
                    else //no update
                    {
                        data.EncodeValue(1, 0);
                    }
                }
                else //remove
                {
                    data.EncodeValue(1, 1);
                    data.EncodeValue(2, 3);
                }
            }
        }

        internal static void WritePlayerEffectUpdate(ref MemoryStream ms, PlayerEntity player)
        {
            RSStreamWriter sw = new RSStreamWriter(ms);

            int mask = 0x0;
            mask |= (int)player.effectUpdateMask;

            sw.WriteByte(mask);

            if ((mask & (int)EffectMaskPlayer.APPEARANCE) == (int)EffectMaskPlayer.APPEARANCE)
            {
                sw.WriteNegatedByte(0); //length
                var startPos = sw.BaseStream.Position;

                sw.WriteByte(player.gender);
                sw.WriteByte(player.headicon);

                for (int i = 0; i < player.appearanceValues.Length; i++)
                {
                    int value = player.appearanceValues[i];
                    if (value > -1)
                    {
                        short v = (short)(0x100 | value);
                        sw.WriteShort(v);
                    }

                    else
                        sw.WriteByte(0);
                }

                for (int i = 0; i < player.colorValues.Length; i++)
                {
                    int value = player.colorValues[i];
                    sw.WriteByte(value);
                }

                for (int i = 0; i < player.animations.Length; i++)
                {
                    int v = player.animations[i];
                    sw.WriteShort(v);
                }

                var stream = sw.BaseStream;

                sw.WriteLong(player.username);
                sw.WriteByte(player.CombatLevel);
                sw.WriteShort(player.Skill);

                var endPos = sw.BaseStream.Position;
                byte size = (byte)(endPos - startPos);
                sw.BaseStream.Position = startPos - 1; //-1 to get to the size position
                sw.WriteNegatedByte(size);
                sw.BaseStream.Position = endPos;
            }

            player.effectUpdateMask = EffectMaskPlayer.NONE;
        }

    }
}
