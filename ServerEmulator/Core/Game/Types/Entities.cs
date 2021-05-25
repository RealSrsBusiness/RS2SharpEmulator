using ServerEmulator.Core.NetworkProtocol;
using System;
using System.IO;
using ServerEmulator.Core.IO;

namespace ServerEmulator.Core.Game
{
    public class PlayerEntity : Actor, ICloneable
    {
        public bool teleported = true, running = true;
        public Direction[] LastSteps { get; private set; } = new Direction[2] { Direction.NONE, Direction.NONE };

        Direction[] movement;
        int walkingQueue = -1;

        internal AccessWatcher[] effects = new AccessWatcher[masks.Length];

        internal PlayerEntity(int id, PlayerAppearance initAppearance)
        {
            effects[APPEARANCE_CHANGED] = new AccessWatcher(initAppearance, true);
            effects[DAMAGE] = new AccessWatcher(new PlayerDamage());
            effects[CHAT_TEXT] = new AccessWatcher(new PlayerChat());
            effects[ANIMATION] = new AccessWatcher(new PlayerAnimation());

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

                        if (running && walkingQueue < movement.Length)
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

        internal void WriteEffects(RSStreamWriter data) 
        {
            int mask = 0x0;

            for (int i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if(effect == null)
                    continue;
                    
                mask |= masks[i];
                effect.Value<Effect>().Write(data);
            }
        }

        public void SetMovement(Coordinate[] waypointCoords) //todo: sometimes causes a race condition
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

        private object _lock = new object();
        public object Clone() => MemberwiseClone();


        public static int FORCED_MOVEMENT = 0, GRAPHIC = 1, ANIMATION = 2, FORCED_CHAT = 3, CHAT_TEXT = 4, INTERACTING_ENTITY = 5, 
        APPEARANCE_CHANGED = 6, FACING = 7, DAMAGE = 8, DAMAGE_2 = 9;

        public static int[] masks = { 0x400, 0x100, 0x8, 0x4, 0x80, 0x1, 0x10, 0x2, 0x20, 0x200 };
    }

    public class NPCEntity : Actor
    {

    }

    //movable, "living" entity
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
    }

    public class WorldEntity
    {
        public int id;
        public int x, y, z;

        public Action Update = delegate { };

        public ushort RegionX { get { return (ushort)(x >> 3); } }
        public ushort RegionY { get { return (ushort)(y >> 3); } }

        public byte LocalX { get { return (byte)(x - (RegionX - 6) * 8); } }
        public byte LocalY { get { return (byte)(y - (RegionY - 6) * 8); } }

        public Coordinate VerifyDistance(int x, int y, int steps = 16)
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
