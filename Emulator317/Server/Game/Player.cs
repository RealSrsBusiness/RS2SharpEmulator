using System;
using System.Collections.Generic;
using System.Text;
using Emulator317.Server.Network;
using Emulator317.Core;

namespace Emulator317.Game.Definitions
{
    /// <summary>
    /// Represents a player in the world
    /// </summary>


    class Player
    {
        enum UpdateMask : int
        {
            FORCED_MOVEMENT = 0x400,
            GRAPHIC = 0x100,
            ANIMATION = 0x8,
            FORCED_CHAT = 0x4,
            CHAT = 0x80,
            INTERACTING_ENTITIY = 0x1,
            APPEARANCE = 0x10, 
            FACING = 0x2,
            DAMAGE = 0x20,
            DAMAGE2 = 0x200
        }

        Queue<Point> walkingQueue = new Queue<Point>();
        public int animation;
        UpdateMask updates = UpdateMask.APPEARANCE;

        int movedX, movedY;
        bool appearanceUpdate = true;
        bool needLocalUpdate = false;
        bool running = false;
        
        public void WritePlayerUpdate(ref List<bool> bits)
        {
            var direction = DirectionType(movedX, movedY);
            bool moved = direction != -1;

            if (!appearanceUpdate && !moved)
            {
                bits.EncodeValue(1, 0);
                return;
            }

            if(moved)
            {
                if(running)
                {
                    bits.EncodeValue(2, 2); //running
                    bits.EncodeValue(3, direction);
                    bits.EncodeValue(3, direction);
                }
                else
                {
                    bits.EncodeValue(2, 1); //walking
                    bits.EncodeValue(3, direction);
                }
                bits.EncodeValue(1, appearanceUpdate ? 1 : 0);
            }
            else
            {
                bits.EncodeValue(2, 0); //just update appearance
            }

            appearanceUpdate = false;
            movedX = 0;
            movedY = 0;
        }

        public void WriteUpdates(ref List<bool> data)
        {
            int mask = 0x0;
            mask |= (int)updates;


        }

        public int DirectionType(int deltaX, int deltaY)
        {
            if (deltaX < 0)
            {
                if (deltaY < 0)
                    return 5;
                else if (deltaY > 0)
                    return 0;
                else
                    return 3;
            }
            else if (deltaX > 0)
            {
                if (deltaY < 0)
                    return 7;
                else if (deltaY > 0)
                    return 2;
                else
                    return 4;
            }
            else
            {
                if (deltaY < 0)
                    return 6;
                else if (deltaY > 0)
                    return 1;
                else
                    return -1;
            }
        }




    }
}
