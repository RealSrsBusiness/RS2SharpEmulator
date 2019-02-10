using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerEmulator.Core.Game
{
    delegate void Interaction(Client c);

    class Definition
    {
        public Interaction[] OnAction { get; } = new Interaction[4];
        public Interaction OnExamine { get; }


    }

    class GameObject : Definition
    {
        int clippingMask;
        bool hasToBeClose;
    }

    class Item : Definition
    {
        bool noteable, tradeable, stackable, equipable, members;
        int weight, alching, dropType;
        string examine;
    }

    class NPC : Definition
    {
        int combat, health;
    }
}
