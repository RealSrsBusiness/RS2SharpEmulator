using ServerEmulator.Core.NetworkProtocol;
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

        public Action Update = delegate { }, PostProcess = delegate { }; //todo: replace with event?

        public ushort RegionX { get { return (ushort)(x >> 3); } }
        public ushort RegionY { get { return (ushort)(y >> 3); } }

        public byte LocalX { get { return (byte)(x - (RegionX - 6) * 8); } }
        public byte LocalY { get { return (byte)(y - (RegionY - 6) * 8); } }

        public Coordinate VerifyDistance(int x, int y, int steps = 15) //5 bits can only encode 32 values from from -16 to 15, counting zero
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
