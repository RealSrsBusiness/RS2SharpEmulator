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
        bool isFocused = true; //is window focused

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
                Player = new PlayerEntity(playerSlot, account);

                Player.x = 3200;
                Player.y = 3200;

                World.RegisterEntity(Player);
                loginTime = DateTime.Now;

                SendInitialState();
            }
            else 
            {
                Program.Warning("Server Full.");
                //todo: properly disconnect, send response code.
            }
        }

/*
        public void SpawnObjectForClient(int id) //todo: remove test
        { 
            Packets.LoadedMapPosition((byte)xOnMap, (byte)yOnMap);
            Packets.AddObject((ushort)id, 10, 0, 1);

            Packets.Send();
        }

        public void RemoveObject() 
        {
            Packets.LoadedMapPosition((byte)xOnMap, (byte)yOnMap);
            Packets.RemoveObject(10, 0, 1);
            Packets.Send();

        }
*/

        PlayerEntity[] localEntityList = new PlayerEntity[0];

        /// <summary>
        /// Builds all update packages that hold the current "screen" state
        /// </summary>
        //todo: this all feels needlessly complicated; build an alternative version that only filters for existing players (instead of all nearby player) and use the inbuilt "contains" function for lists, see Flare for a reference; use this version only if performance is much better
        public void RenderScreen()
        {
            CheckRegionChange(); //load new map if needed
         
            /* Player Updating Process:
             * 0: update movement of own player and (if needed) set a flag to update effects 
             * 1: update movement of other players that are already in the player list, determine if effect updates are needed or if players need to be removed
             * 2: add new players to the player list, determine if an effect update is needed, load the appearance (not effects) if it's still buffered
             * 3: based on previously set flags we know which players need effect updates, parse and apply all effects to own player and other players
             */
            var bits = new List<bool>(); //movement bits
            var allEffectUpdates = new List<byte[]>(); //effect updates of all players that are on screen

            //step 0
            var ownEffectUpdate = Player.effects.Incremental;
            var needOwnEffectUpdate = ownEffectUpdate[0] > 0; //array just contains 0 if effects haven't changed

            if(Player.justLoggedIn || Player.teleported) //redundant but more clear
            {
                EntityUpdates.LocalPlayerTeleported(ref bits, needOwnEffectUpdate, Player.XMiddleChunk, Player.YMiddleChunk, Player.z);
                Player.teleported = false;
            }
            else 
            { 
                var steps = Player.LastSteps;
                EntityUpdates.LocalPlayerMovement(ref bits, needOwnEffectUpdate, (int)steps[0], (int)steps[1]); //can pass -1 if not moving
            }

            if(needOwnEffectUpdate)
                allEffectUpdates.Add(ownEffectUpdate);


            //step 1 + 2 + 3
            var currentNearbyPlayers = World.FindEntities<PlayerEntity> //find all nearby entities, filter out own player
                ((PlayerEntity we) => { 
                    return we.VerifyDistance(Player.x, Player.y) != Coordinate.NONE && we.id != Player.id; //only filter for existing players maybe?
                }, -1);

            var otherPlayersMovement = new EntityUpdates.OtherEntitiesMovement(bits);
            var newOnScreenPlayers = new EntityUpdates.NewPlayerList(bits);

            /* "Difference" returns values in the same order as the source array 
                this is important because when existing players are updated the server doesn't send player indicies again
                results from "FindEntities" might be in the wrong order, so we copy results from "Difference" instead */
            var changes = localEntityList.Difference<PlayerEntity>(currentNearbyPlayers);
            var newEntityList = new PlayerEntity[changes.Unchanged + changes.Added];


            int position = 0;
            for (int i = 0; i < changes.entries.Length; i++)
            {
                var entity = changes.entries[i].entry;
                var changeType = changes.entries[i].changeType;

                switch(changeType) 
                {
                    case EntryChangeType.UNCHANGED: 
                        var otherEffectUpdate = entity.effects.Incremental;
                        var needEffectUpdate = otherEffectUpdate[0] > 0;

                        otherPlayersMovement.Add( (int)entity.LastSteps[0], (int)entity.LastSteps[1], needEffectUpdate);

                        if(needEffectUpdate)
                            allEffectUpdates.Add(otherEffectUpdate);

                        newEntityList[position++] = entity;
                    break;

                    case EntryChangeType.REMOVED: 
                        otherPlayersMovement.Add(remove: true);
                    break;

                    case EntryChangeType.ADDED: 
                        var distance = entity.VerifyDistance(Player.x, Player.y);
                        var update = entity.effects.Full;
                        var needUpdate = update[0] > 0; //is always true?

                        newOnScreenPlayers.Add(entity.id, distance.x, distance.y, needUpdate, entity.teleported); //distance can only be up to 15, check "VerifyDistance"

                        if(needUpdate)
                            allEffectUpdates.Add(update);

                        newEntityList[position++] = entity;
                    break;
                }
            }
            otherPlayersMovement.Finish();
            newOnScreenPlayers.Finish();

            localEntityList = newEntityList; //swap old entity list with new one
            Packets.PlayerUpdate(bits, allEffectUpdates); //packet 81
            //Packets.NPCUpdate(null); //packet 65
            //Packets.RegionalUpdate(null); //packet 60

            //todo: 01-14: fully test this function; implement chat, hitsplats and animations; 
            //todo: 01-17: implement inventory and equipment; woodcutting test, object replacement
            Packets.Send();
        }

        int mapOriginX, mapOriginY; //global origin(0x0) position for currently loaded map segment (104x104)
        int xOnMap, yOnMap; //player position on map

        private void CheckRegionChange()
        { 
            xOnMap = Player.x - mapOriginX;
            yOnMap = Player.y - mapOriginY;

            if(xOnMap < 13 || xOnMap > 90 || yOnMap < 13 || yOnMap > 90) //13 tiles away from the edge
            {
                Packets.LoadRegion(Player.MapChunkX, Player.MapChunkY);

                mapOriginX = Player.SegmentOriginX;
                mapOriginY = Player.SegmentOriginY;
            }
        }


        static int[] SIDE_BARS = { 2423, 3917, 638, 3213, 1644, 5608, 1151, -1, 5065, 5715, 2449, 904, 147, 962 };

        private void SendInitialState()
        {
            Packets.SetConfig(172, 0);
            Packets.RunEnergy(100);
            Packets.Weight(30);
            Packets.SendMessage(WELCOME_MSG);
            //Packets.SendMessage("[woodcut-test] Type ::axe to get an axe"); //todo: allow modules to have their own "on login" message
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
            Packets.FriendList(2); //status 2 == online
            Packets.SetInterfaceText(2426, "Some Weapon");

            //ItemStack[] inv = new ItemStack[28];

            //for (int i = 0; i < inv.Length; i++)
            //    inv[i] = new ItemStack() { id = 1511, amount = int.MaxValue };

            ItemStack[] inv = new ItemStack[3];
            inv[0] = new ItemStack() { id = 1511, amount = 1 };
            inv[1] = new ItemStack() { id = 590, amount = 1 };
            inv[2] = new ItemStack() { id = 882, amount = 1 };

            Packets.SetItems(3214, inv); //inventory

            ItemStack[] equip = new ItemStack[15];

            int equipId = 1200;
            for (int i = 0; i < equip.Length; i++)
                equip[i] = new ItemStack() { id = equipId++, amount = 1 };


            Packets.SetItems(1688, equip);
            Packets.SetPlayerContextMenu(1, false, "Attack");
            Packets.PlaySong(125);

            Packets.WelcomePopup(201, 2222, false, 100100, 6666);

            Packets.Send();
        }

        public T State<T>(int index) => (T)customStates[index];

        void OnDisconnect()
        {
            FreePlayerSlot(Player.id);
            World.UnregisterEntity(Player);
        }

        public static int AllocPlayerSlot() //find free player slot
        { //skip 0 because the player with that id cannot be interacted with (probably reserved for admins or debugging)
            for (int i = 1; i < playerSlots.Length; i++) 
            {
                if (!playerSlots[i])
                {
                    playerSlots[i] = true;
                    return i;
                }
            }
            return -1;
        }

        public static void FreePlayerSlot(int id) => playerSlots[id] = false;

        static bool[] playerSlots = new bool[Constants.MAX_PLAYER];

        private readonly object _lock = new object();
    }

    
}
