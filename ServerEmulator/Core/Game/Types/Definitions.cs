using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerEmulator.Core.Game
{
    delegate void Interaction(Client c);
    delegate void Update(WorldEntity entity);

    class Definition
    {
        public Interaction[] OnAction { get; } = new Interaction[4];
        public Interaction OnExamine { get; }


    }

    class GameObject : Definition
    {
        int clippingMask;
        bool projectilesPassThrough;
        bool hasToBeClose; //based on actions?
        
        //group: WALL (0, includes doors), WALL_DECORATION(1), INTERACTABLE(2), FLOOR_DECORATION(3)
        //type: TYPE_TO_GROUP: { 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3 }
        //size?
    }

    class Item : Definition
    {
        bool noteable, tradeable, stackable, equipable, members;
        int weight, alching, dropType;
        string examine;
        //equipment stats (if equipable)
    }

    class NPC : Definition
    {
        int combat, health;

        public Update OnUpdate;
    }
}
