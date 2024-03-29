﻿using ServerEmulator.Core.NetworkProtocol;
using System;
using System.IO;
using ServerEmulator.Core.IO;

namespace ServerEmulator.Core.Game
{
    public class PlayerEntity : Actor, ICloneable
    {
        public bool teleported = true, running = true;
        public bool justLoggedIn { get => teleported; }

        public Direction[] LastSteps { get; private set; } = new Direction[2] { Direction.NONE, Direction.NONE };

        Direction[] movement;
        int walkingQueue = -1;

        internal BufferedEffectStates effects = new BufferedEffectStates();

        internal PlayerEntity(int id, Account account)
        {   
            base.id = id;
            var appear = effects.Appearance; //also sets the "needRefresh" flag

            //default values
            appear.appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 };
            appear.colorValues = new int[] { 7, 8, 9, 5, 0 };
            appear.idleAnimations = new int[] { 808, 823, 819, 820, 821, 822, 824 };
            appear.username = account.displayname.ToLong();

            PostProcess = () => effects.Reset();

            Update = () =>
            {
                lock(_lock)
                {
                    LastSteps[0] = Direction.NONE;
                    LastSteps[1] = Direction.NONE;

                    if (walkingQueue != -1)
                    {
                        LastSteps[0] = movement[walkingQueue++];
                        Coordinate moved = Movement.Directions[(int)LastSteps[0]];

                        if (running && walkingQueue < movement.Length) //can we run?
                        {
                            LastSteps[1] = movement[walkingQueue++];
                            moved += Movement.Directions[(int)LastSteps[1]];
                        }

                        if (walkingQueue >= movement.Length)
                            walkingQueue = -1;

                        x += moved.x;
                        y += moved.y;
                    }
                }
            };
        }

        void DoDamage() 
        {
            //effects.Damage.damage = 100;
        }

        public void SetMovement(Coordinate[] waypointCoords) //todo: sometimes causes a race condition, i think?
        {
            lock(_lock)
            {
                movement = Movement.InterpolateWaypoints(waypointCoords, x, y);

                if (movement.Length > 0)
                    walkingQueue = 0;
                else
                    walkingQueue = -1;
            }
        }

        /* 
            1200 = head, 1201 = cape, 1202 = amulet, 1203 = sword/weapon, 1204 = chest, 1205 = shield, 
            [1206], 1207 = legs, [1208], 1209 = gloves, 1210 = boots, [1211], 1212 = ring, 1213 = arrows, [1214] 
        */

        private object _lock = new object();
        public object Clone() => MemberwiseClone();

    }

    public class NPCEntity : Actor
    {

    }

    //moving, "living" entity
    public class Actor : WorldEntity
    {
        void ApplyDamage() { }

        void MoveTo() { }

        void SetAnimation() { }

        void InteractingEntity(Actor target) { }

        void Talk(string text) { }

    }
        
    public class GroundItemEntity : WorldEntity 
    { 
    
    }

    public class ObjectEntity : WorldEntity 
    {
        bool replaceObject;
        int cyclesToExpire;
    }

    public class WorldEntity
    {
        public int id;
        public int x, y, z;

        public Action Update = delegate { }; //todo: replace with event?
        internal Action PostProcess = delegate { }; //don't use this, this will be removed at one point

        //chunk of 8 tiles, also called "region" in client
        public ushort MapChunkX => (ushort)(x / 8); //same as "x >> 3", (base)2 ^ 3 = 8
        public ushort MapChunkY => (ushort)(y / 8); 

        //a map segment of 104x104 tiles consists of 13 chunks along each axis, this assumes the middle position and "goes back" 6 chunks to get to the origin
        public int SegmentOriginX => (MapChunkX - 6) * 8; //where the map segment would be at 0x0
        public int SegmentOriginY => (MapChunkY - 6) * 8; 

        //the X and Y within the middle(7th) chunk, can range from 48 to 55 (inclusive); first 6 chunks: 0-47, last 6 chunks: 56-103
        public byte XMiddleChunk => (byte)(x - SegmentOriginX);
        public byte YMiddleChunk => (byte)(y - SegmentOriginY);

        public Coordinate VerifyDistance(int x, int y, int steps = 15) //5 bits can only encode 32 values, from -16 to 15 and 0
        {
            int difX = this.x - x;
            int difY = this.y - y;

            int stepsN = -steps;

            if (difX > steps || difX < stepsN || difY > steps || difY < stepsN)
                return Coordinate.NONE;

            return new Coordinate() { x = difX, y = difY };
        }
    }

    
}
