using ServerEmulator.Core.Game;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using static ServerEmulator.Core.Game.WorldEntity;

namespace ServerEmulator.Core.Game
{
    delegate bool EntityFilter<T>(T we) where T : WorldEntity;

    /// <summary>
    /// Represents the entire gameworld and operations that can be done in it
    /// with all players, objects, npcs and grounditems
    /// extra: projectiles?, gfx? sound effects?
    /// </summary>
    static class World
    {
        public static List<WorldEntity> globalEntities = new List<WorldEntity>();

        internal static void Init()
        {
            if (Constants.MAP_LOADING_METHOD == 0 && !Program.DEBUG)
                Program.Warning("World Map is not loaded. The server cannot verify clipping and objects. Only use this setting for testing.");

            for (int i = 0; i < regions.Length; i++)
            {
                regions[i] = new Region();
            }

            //add some test npcs, grounditem and replace an object
            NPCEntity man = new NPCEntity() { id = 1, x = 3198, y = 3204, z = 0 };
            NPCEntity woman = new NPCEntity() { id = 4, x = 3194, y = 3202, z = 0 };
            NPCEntity guard = new NPCEntity() { id = 10, x = 3199, y = 3201, z = 0 };

            GroundItemEntity runeScim = new GroundItemEntity() { id = 1333, x = 3192, y = 3202, z = 0 };

            ObjectEntity replaceStandardWithCrate = new ObjectEntity() { id = 1, x = 3198, y = 3205, z = 0 };

            /* RegisterEntity(man);
             RegisterEntity(woman);
             RegisterEntity(guard);

             RegisterEntity(runeScim);
             
             RegisterEntity(replaceStandardWithCrate);
             */
        }

        public static T[] FindEntities<T>(EntityFilter<T> filter, int limit = 1, Region region = null) where T : WorldEntity
        {
            List<T> result = new List<T>();
            for (int i = 0; i < globalEntities.Count; i++)
            {
                var entity = globalEntities[i];
                
                if (entity is T && filter((T)entity))
                    result.Add((T)entity);

                if (limit != -1 && result.Count >= limit)
                    break;
            }
            return result.ToArray();
        }

        public static void ProcessWorld()
        {
            for (int i = 0; i < globalEntities.Count; i++)
                globalEntities[i].Update();
        }

        //if region is set, this entity will only be updated if it's seen by a client in that region
        public static void RegisterEntity(WorldEntity entity, Region updateRegion = null)
        {
            globalEntities.Add(entity);
        }

        public static void UnregisterEntity(WorldEntity entity)
        {
            globalEntities.Remove(entity);
        }

        //used for better npc walking and higher revision player walking
        //finding the target is guaranteed
        public static void FindPathAStar()
        {

        }

        //used for following, getting to the target is not guaranteed  
        public static void FindDirectPath()
        {

        }


        static Region[] regions = new Region[375 * 375 * 4];
        
    }

    public class Region
    {
        Tile[] tiles = new Tile[8 * 8];
    }

    public class Tile
    {

    }
}
