using System;
using System.Collections.Generic;


namespace ServerEmulator.Core.NetworkProtocol
{
    internal class EntityUpdates
    {
        internal static void LocalPlayerMovement(ref List<bool> bits, bool effectsChanged, 
            int firstDir = -1, int secondDir = -1)
        {
            if(secondDir != -1) //running
            {
                bits.EncodeValue(1, 1);
                bits.EncodeValue(2, 2);
                bits.EncodeValue(3, firstDir);
                bits.EncodeValue(3, secondDir);
                bits.EncodeValue(1, effectsChanged ? 1 : 0);
            }
            else if(firstDir != -1) //walking
            {
                bits.EncodeValue(1, 1);
                bits.EncodeValue(2, 1);
                bits.EncodeValue(3, firstDir);
                bits.EncodeValue(1, effectsChanged ? 1 : 0);
            }
            else //no movement 
            {
                if(effectsChanged) //effect update only
                {
                    bits.EncodeValue(1, 1);
                    bits.EncodeValue(2, 0);
                }
                else //no update at all
                {
                    bits.EncodeValue(1, 0); //first bit= update=true/false
                }
            }
        }

        internal static void LocalPlayerTeleported(ref List<bool> bits, bool effectsChanged, 
            int localX, int localY, int planeZ, bool beam = false)
        {
            bits.EncodeValue(1, 1);
            bits.EncodeValue(2, 3);
            bits.EncodeValue(2, planeZ);
            bits.EncodeValue(1, beam ? 1 : 0);
            bits.EncodeValue(1, effectsChanged ? 1 : 0);
            bits.EncodeValue(7, localY);
            bits.EncodeValue(7, localX);
        }

        internal abstract class EntityUpdateList {
            protected int amount;
            protected List<bool> bits;
            internal abstract void Finish();
        }

        //used for both other players and npcs
        internal class OtherEntitiesMovement : EntityUpdateList
        {
            int startPos;
            internal OtherEntitiesMovement(List<bool> list) 
            {
                base.bits = list;
                startPos = bits.Count; 
                bits.EncodeValue(8, 0); //placeholder for length
            } 

            internal override void Finish() => bits.OverwriteValueAt(startPos, 8, amount);

            internal void Add(int firstDir = -1, int secondDir = -1, bool effectUpdate = false, bool remove = false)
            {
                if(secondDir != -1) //running
                {
                    bits.EncodeValue(1, 1);
                    bits.EncodeValue(2, 2);
                    bits.EncodeValue(3, firstDir);
                    bits.EncodeValue(3, secondDir);
                    bits.EncodeValue(1, effectUpdate ? 1 : 0);
                }
                else if(firstDir != -1) //walking
                {
                    bits.EncodeValue(1, 1);
                    bits.EncodeValue(2, 1);
                    bits.EncodeValue(3, firstDir);
                    bits.EncodeValue(1, effectUpdate ? 1 : 0);
                }
                else 
                {
                    if(effectUpdate) 
                    {
                        bits.EncodeValue(1, 1);
                        bits.EncodeValue(2, 0);
                    } 
                    else if (remove) //remove entity from screen
                    {
                        bits.EncodeValue(1, 1);
                        bits.EncodeValue(2, 3);
                    } 
                    else //do nothing
                    {
                        bits.EncodeValue(1, 0);
                    }
                }
                amount++;
            }
        }

        internal class NewPlayerList : EntityUpdateList
        {
            internal NewPlayerList(List<bool> list) => bits = list;

            internal void Add(int index, int x, int y, bool effect, bool teleport) 
            {
                bits.EncodeValue(11, index);
                bits.EncodeValue(1, effect ? 1 : 0);
                bits.EncodeValue(1, teleport ? 1 : 0);
                bits.EncodeValue(5, y); //relative to local player?
                bits.EncodeValue(5, x);
            }

            internal override void Finish() => bits.EncodeValue(11, 2047); //2047 = max value
        }

        internal class NewNPCList : EntityUpdateList
        {
            internal NewNPCList(List<bool> list) => bits = list;

            internal void Add(int index, int x, int y, int npcDef, bool effect, bool teleport) 
            {
                bits.EncodeValue(14, index);
                bits.EncodeValue(5, y);
                bits.EncodeValue(5, x);
                bits.EncodeValue(1, teleport ? 1 : 0);
                bits.EncodeValue(12, npcDef);
                bits.EncodeValue(1, effect ? 1 : 0);
            }

            internal override void Finish() => bits.EncodeValue(14, 16383); //16383 = max value
        }

    }
}
