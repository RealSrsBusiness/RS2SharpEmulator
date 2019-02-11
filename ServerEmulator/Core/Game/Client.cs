using ServerEmulator.Core;
using ServerEmulator.Core.IO;
using ServerEmulator.Core.Network;
using ServerEmulator.Core.NetworkProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using static ServerEmulator.Core.Constants;
using static ServerEmulator.Core.Game.Movement;
using static ServerEmulator.Core.Game.WorldEntity;
using static ServerEmulator.Core.Game.WorldEntity.PlayerEntity;

namespace ServerEmulator.Core.Game
{
    /// <summary>
    /// represents a single client, holds entities seen by a client
    /// </summary>
    class Client
    {
        public StaticPackets Packets { get; private set; }

        public PlayerEntity Player { get; private set; }
        public Account Account { get; private set; }

        DateTime loginTime;
        int activeInterface = -1;
        bool focused = true;

        object[] customStates;

        public Client(Connection connection, Account account)
        {
            connection.onDisconnect += (Connection c) => Disconnect();
            Packets = new StaticPackets(connection);
            customStates = DataLoader.CreateCustomStates();

            Account = account;
            Player = new PlayerEntity();
            Player.id = AllocPlayerSlot();

            if (Player.id == -1)
                Program.Warning("Server Full");

            Player.Update = Update;
            Player.appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 };
            Player.colorValues = new int[] { 7, 8, 9, 5, 0 };
            Player.animations = new int[] { 808, 823, 819, 820, 821, 822, 824 };
            Player.username = "Player".ToLong();

            World.RegisterEntity(Player);
            loginTime = DateTime.Now;

            Init();
        }

        List<WorldEntity> localEntities = new List<WorldEntity>();

        /// <summary>
        /// Builds all update packages that hold the current "screen" state
        /// </summary>
        public void RenderScreen()
        {
            var global = World.globalEntities;
            var nearBy = new Dictionary<WorldEntity, Coordinate>();

            //get nearest entities
            for (int i = 0; i < global.Count; i++)
            {
                var entity = global[i];
                var distance = entity.VerifyDistance(Player.x, Player.y);

                if (distance != Coordinate.NONE)
                    nearBy.Add(entity, distance);
            }

            var toUpdateRemove = new List<EntityUpdates.EntityMovementUpdate>();
            var toAdd = new List<EntityUpdates.PlayerListEntry>();

            //remove old entities and update movement
            foreach (var local in localEntities)
            {
                bool found = false;
                foreach(var near in nearBy.Keys)
                {
                    if (near == local)
                    {
                        found = true;
                        break;
                    } 
                }

                bool remove = false;

                if (!found)
                {
                    remove = true;
                    localEntities.Remove(local);
                }

                var update = new EntityUpdates.EntityMovementUpdate();
                update.shouldRemove = remove;
                //get movement

                toUpdateRemove.Add(update);
            }

            //add new entities
            foreach(var near in nearBy)
            {
                var res = localEntities.Find((WorldEntity e) => e == near.Key);
                if (res == null)
                {
                    var entity = near.Key;

                    var addPlayer = new EntityUpdates.PlayerListEntry();
                    addPlayer.index = entity.id;
                    addPlayer.x = entity.LocalX;
                    addPlayer.y = entity.LocalY;
                    //addPlayer.teleport = entity.teleport;
                    //addPlayer.effectUpdate = entity.effect;

                    toAdd.Add(addPlayer);
                    localEntities.Add(entity);
                }
            }

            //write updates
            List<bool> bits = new List<bool>();

            if (Player.justSpawned)
                EntityUpdates.WritePlayerMovement(ref bits, Player.EffectUpdateRequired, Player.LocalX, Player.LocalY, Player.z, Player.teleported);
            else if (Player.walkingQueue != -1)
            {
                var nextStep = Player.movement[Player.walkingQueue];
                EntityUpdates.WritePlayerMovement(ref bits, Player.EffectUpdateRequired, (int)nextStep);
            }

            EntityUpdates.WriteEntityMovement(ref bits, toUpdateRemove.ToArray());
            EntityUpdates.WriteNewPlayerList(ref bits, toAdd.ToArray());



            int mX = Player.LocalX, mY = Player.LocalY;
            if (mX < 15 || mX > 88 || mY < 15 || mY > 88)
                Packets.LoadRegion(Player.RegionX, Player.RegionY);

            Packets.PlayerUpdate(null);
            Packets.NPCUpdate(null);
            Packets.RegionalUpdate(null);

            Packets.C.Send();
        }

        /// <summary>
        /// Processes all queued up tasks
        /// </summary>
        private void Update()
        {

        }

        static int[] SIDE_BARS = { 2423, 3917, 638, 3213, 1644, 5608, 1151, -1, 5065, 5715, 2449, 904, 147, 962 };

        private void Init()
        {
            Packets.SetConfig(172, 0);
            Packets.RunEnergy(100);
            Packets.Weight(30);
            Packets.SendMessage(WELCOME_MSG);
            //
            for (int i = 0; i < SIDE_BARS.Length; i++)
            {
                Packets.AssignSidebar((byte)i, (ushort)SIDE_BARS[i]);
            }

            for (int i = 0; i < Account.skills.Length; i++)
            {
                var skill = Account.skills[i];
                Packets.SetSkill((byte)i, skill.xp, (byte)skill.level);
            }

            Packets.SetFriend("TestFriend", 80);
            Packets.FriendList(2);
            Packets.SetInterfaceText(2426, "A Weapon");

            //ItemStack[] inv = new ItemStack[28];

            //for (int i = 0; i < inv.Length; i++)
            //    inv[i] = new ItemStack() { id = 1511, amount = int.MaxValue };

            ItemStack[] inv = new ItemStack[3];
            inv[0] = new ItemStack() { id = 1511, amount = 1 };
            inv[1] = new ItemStack() { id = 590, amount = 1 };
            inv[2] = new ItemStack() { id = 1205, amount = 1 };

            Packets.SetItems(3214, inv); //inventory

            ItemStack[] equip = new ItemStack[14];

            for (int i = 0; i < equip.Length; i++)
                equip[i] = new ItemStack() { id = 1205, amount = 1 };


            Packets.SetItems(1688, equip);
            Packets.SetPlayerContextMenu(1, false, "Attack");
            Packets.PlaySong(125);

            Packets.C.Send();
        }

        public void SetMovement(Coordinate[] waypointCoords)
        {
            Player.movement = InterpolateWaypoints(waypointCoords, Player.x, Player.y);
            Player.walkingQueue = 0;
        }

        public T GetState<T>(int index)
        {
            return (T)customStates[index];
        }

        public void Disconnect()
        {
            Packets.Logout();
            FreePlayerSlot(Player.id);
            World.UnregisterEntity(Player);
        }

        private readonly object _lock = new object();
    }

    
}
