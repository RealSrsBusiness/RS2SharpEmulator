using ServerEmulator.Core;
using ServerEmulator.Core.IO;
using ServerEmulator.Core.Network;
using ServerEmulator.Core.NetworkProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using static ServerEmulator.Core.Constants;
using static ServerEmulator.Core.Game.PlayerEntity;

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
            this.Account = account;
            connection.onDisconnect += (Connection c) => OnDisconnect();
            Packets = new StaticPackets(connection);
            customStates = DataLoader.CreateCustomStates();

            var playerSlot = AllocPlayerSlot();

            if(playerSlot != -1) 
            {
                Player = new PlayerEntity(playerSlot, new PlayerAppearance 
                {
                    appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 },
                    colorValues = new int[] { 7, 8, 9, 5, 0 },
                    idleAnimations = new int[] { 808, 823, 819, 820, 821, 822, 824 },
                    username = account.displayname.ToLong()
                });

                Player.x = 3200;
                Player.y = 3200;

                World.RegisterEntity(Player);
                loginTime = DateTime.Now;

                Init();
            }
            else 
            {
                Program.Warning("Server Full.");
                //todo: properly disconnect, send response code.
            }
        }

        PlayerEntity[] localEntityList = new PlayerEntity[0];
        int[] playerIdList, npcIdList;

        /// <summary>
        /// Builds all update packages that hold the current "screen" state
        /// </summary>
        public void RenderScreen()
        {
            //var blah = new { blub = 100, test = "string" };

            var newNearEntities = World.FindEntities<PlayerEntity>((PlayerEntity we) => {
                return we.VerifyDistance(Player.x, Player.y) != Coordinate.NONE && we.id != Player.id;
            }, -1);

            var changes = localEntityList.Difference<PlayerEntity>(newNearEntities);

            //todo: do something with the changes.
         
            localEntityList = newNearEntities; //swap old entity list with new one

            CheckRegionChange();
            

            /* Player Updating Process:
             * 0: update the movement of the own player and/or update effects if needed
             * 1: update movement of other players that are in the player list, determine if effect updates are needed or players need to be removed
             * 2: add new players to the player list, determine if an effect update is needed, load the appearance (not effects!) if it's still buffered
             * 3: we now know what players need effect updates, parse and apply all effects to own player and other players
             */
            List<bool> bits = new List<bool>();
            var effectUpdates = new RSStreamWriter(new MemoryStream());
            var appearEffect = Player.effects[APPEARANCE_CHANGED];


            if(!Player.teleported) 
            {
                var steps = Player.LastSteps;
                EntityUpdates.LocalPlayerMovement(ref bits, appearEffect.Changed, (int)steps[0], (int)steps[1]);
            }
            else 
            { //either teleported or logged in, when logged in changed
                EntityUpdates.LocalPlayerTeleported(ref bits, appearEffect.Changed, Player.LocalX, Player.LocalY, Player.z);
                Player.teleported = false;
            }

            Player.WriteEffects(effectUpdates);


            var otherMovement = new EntityUpdates.OtherEntitiesMovement(bits);
            var playerList = new EntityUpdates.NewPlayerList(bits);

            otherMovement.Finish();
            playerList.Finish();


            Packets.PlayerUpdate(bits, effectUpdates.BaseStream.ToArray()); //81
            //Packets.NPCUpdate(null); //65
            //Packets.RegionalUpdate(null); //60

            Packets.Send();
        }


        int regionOriginX = 0, regionOriginY = 0;
        //int movedX = 0, movedY = 0;
        private void CheckRegionChange()
        {
            //how far are we from the region origin?
            int movedX = Player.x - regionOriginX;
            int movedY = Player.y - regionOriginY;

            //if we moved far enough, we need to load a new map.
            if (movedX < -35 || movedX > 42 || movedY < -35 || movedY > 42)
            {
                regionOriginX = Player.RegionX << 3;
                regionOriginY = Player.RegionY << 3;

                Packets.LoadRegion(Player.RegionX, Player.RegionY);
            }
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
            Packets.SetInterfaceText(2426, "Some Weapon");

            //ItemStack[] inv = new ItemStack[28];

            //for (int i = 0; i < inv.Length; i++)
            //    inv[i] = new ItemStack() { id = 1511, amount = int.MaxValue };

            ItemStack[] inv = new ItemStack[3];
            inv[0] = new ItemStack() { id = 1511, amount = 1 };
            inv[1] = new ItemStack() { id = 590, amount = 1 };
            inv[2] = new ItemStack() { id = 882, amount = 1 };

            Packets.SetItems(3214, inv); //inventory

            ItemStack[] equip = new ItemStack[14];

            for (int i = 0; i < equip.Length; i++)
                equip[i] = new ItemStack() { id = 1205, amount = 1 };


            Packets.SetItems(1688, equip);
            Packets.SetPlayerContextMenu(1, false, "Attack");
            Packets.PlaySong(125);

            Packets.WelcomeMessage(201, 2222, false, 100100, 6666);

            Packets.Send();
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

        public static int AllocPlayerSlot() //find free player slot
        {
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (!playerSlots[i])
                {
                    playerSlots[i] = true;
                    return i;
                }
            }
            return -1;
        }

        public static void FreePlayerSlot(int id)
        {
            playerSlots[id] = false;
        }

        static bool[] playerSlots = new bool[Constants.MAX_PLAYER];

        private readonly object _lock = new object();
    }

    
}
