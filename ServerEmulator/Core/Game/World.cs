using ServerEmulator.Core.Game;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using static ServerEmulator.Core.Game.WorldEntity;

namespace ServerEmulator.Core.Game
{
    /// <summary>
    /// Represents the entire gameworld and operations that can be done in it
    /// with all players, objects, npcs and items in it
    /// extra: projectiles?, gfx? sound effects?
    /// </summary>
    static class World
    {
        public class Region
        {
            Tile[] tiles = new Tile[8 * 8];
        }

        public class Tile
        {

        }

        static Region[] regions = new Region[375 * 375 * 4];


        internal static void Init()
        {
            for (int i = 0; i < regions.Length; i++)
            {
                regions[i] = new Region();
            }
        }

        //used for better npc walking and higher revision player walking
        //finding the target is guaranteed
        public static void FindPathAStar()
        {

        }

        //used for following, finding the target is not guaranteed  
        public static void FindPathSimple()
        {

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

        public static T[] FindEntities<T>(EntityFilter<T> filter, Region region = null, int limit = 1) where T : WorldEntity
        {
            List<T> result = new List<T>();
            for (int i = 0; i < globalEntities.Count; i++)
            {
                var entity = globalEntities[i];
                if (entity is T && filter((T)entity))
                    result.Add((T)entity);
                if (result.Count >= limit)
                    break;
            }
            return result.ToArray();
        }

        public static List<WorldEntity> globalEntities = new List<WorldEntity>();
    }

    delegate bool EntityFilter<T>(T we) where T : WorldEntity;
}
