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
            connection.onDisconnect += (Connection c) => OnDisconnect();
            Packets = new StaticPackets(connection);
            customStates = DataLoader.CreateCustomStates();

            Account = account;
            Player = new PlayerEntity();
            Player.id = AllocPlayerSlot();

            if (Player.id == -1)
                Program.Warning("Server Full");

            Player.appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 };
            Player.colorValues = new int[] { 7, 8, 9, 5, 0 };
            Player.animations = new int[] { 808, 823, 819, 820, 821, 822, 824 };
            Player.username = account.displayname.ToLong();
            Player.x = 3200;
            Player.y = 3200;

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
            var nearEntities = new List<(WorldEntity, Coordinate)>();

            //get nearest entities
            for (int i = 0; i < global.Count; i++)
            {
                var entity = global[i];
                var distance = entity.VerifyDistance(Player.x, Player.y);

                if (distance != Coordinate.NONE && entity != Player)
                    nearEntities.Add((entity, distance));
            }

            var toUpdateRemove = new List<EntityUpdates.EntityMovementUpdate>();
            var toAdd = new List<EntityUpdates.PlayerListEntry>();

            //remove old entities and update movement
            foreach (var local in localEntities)
            {
                var found = nearEntities.FindIndex(((WorldEntity, Coordinate) item) => item.Item1 == local);

                var update = new EntityUpdates.EntityMovementUpdate();

                if (found < 0) //local entity not found in nearEntities therefor delete it
                {
                    update.shouldRemove = true;
                    localEntities.Remove(local);
                }
                else //otherwise just update movement
                {
                    var movement = ((PlayerEntity)local).lastSteps;
                    update.firstDirection = (int)movement[0];
                    update.secondDirection = (int)movement[1];
                }

                toUpdateRemove.Add(update);
            }

            //add new entities
            foreach(var near in nearEntities)
            {
                var res = localEntities.Find((WorldEntity e) => e == near.Item1);
                if (res == null) //new entity which needs to be added to the locals
                {
                    var entity = near.Item1;

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
            {
                EntityUpdates.WritePlayerMovement(ref bits, Player.HasEffectUpdate, Player.LocalX, Player.LocalY, Player.z, Player.teleported);
                Player.justSpawned = false; //todo: move
            }
            else
            {
                var steps = Player.GetMovementSteps();
                EntityUpdates.WritePlayerMovement(ref bits, Player.HasEffectUpdate, (int)steps[0], (int)steps[1]);
            }
            EntityUpdates.WriteEntityMovement(ref bits, toUpdateRemove.ToArray());
            EntityUpdates.WriteNewPlayerList(ref bits, toAdd.ToArray());

            MemoryStream ms = new MemoryStream();
            byte[] bitBuffer = bits.ToByteArray();
            ms.Write(bitBuffer, 0, bitBuffer.Length);


            if (Player.effectBuffer != null)
            {
                var bytes = Player.effectBuffer;
                ms.Write(bytes, 0, bytes.Length);
            }

            foreach (var local in localEntities)
            {
                var player = (PlayerEntity)local;
                if(player.effectBuffer != null)
                {
                    var bytes = player.effectBuffer;
                    ms.Write(bytes, 0, bytes.Length);
                }
            }

            int mX = Player.LocalX, mY = Player.LocalY;
            //if (mX < 15 || mX > 88 || mY < 15 || mY > 88)
            if (mapUpdate)
            {
                Packets.LoadRegion(Player.RegionX, Player.RegionY);
                mapUpdate = false;
            }



            Packets.PlayerUpdate(ms.ToArray());

            //Packets.NPCUpdate(null);
            //Packets.RegionalUpdate(null);

            Packets.Send();
        }

        bool mapUpdate = true;



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

            Packets.Send();
        }

        public void SetMovement(Coordinate[] waypointCoords)
        {
            Player.movement = InterpolateWaypoints(waypointCoords, Player.x, Player.y);
            if(Player.movement.Length > 0)
                Player.walkingQueue = 0;
        }

        public T State<T>(int index)
        {
            return (T)customStates[index];
        }

        void OnDisconnect()
        {
            FreePlayerSlot(Player.id);
            World.UnregisterEntity(Player);
        }

        static int AllocPlayerSlot()
        {
            //find free player slot
            int pId = -1;
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (!playerSlots[i])
                {
                    pId = i;
                    playerSlots[i] = true;
                    break;
                }
            }
            return pId;
        }

        static void FreePlayerSlot(int id)
        {
            playerSlots[id] = false;
        }

        static bool[] playerSlots = new bool[2047];

        private readonly object _lock = new object();
    }

    
}
