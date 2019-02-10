using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ServerEmulator.Core.Game
{
    [Obfuscation(Exclude = true)]
    abstract class Content
    {
        public abstract void Load();

        public static int AddState(Type state)
        {
            if (DataLoader.LoadingComplete)
                throw new InvalidOperationException("New states cannot be added, once loading is completed.");

            states.Add(state);
            return states.IndexOf(state);
        }

        public NPC[] Npcs => DataLoader.Npcs;
        public Item[] Items => DataLoader.Items;
        public GameObject[] Objects = DataLoader.Objects;

        public Dictionary<int, Action> ActionButtons => DataLoader.ActionButtons;
        public Definition PlayerActions => DataLoader.PlayerActions;

        private static List<Type> states = new List<Type>();
    }

}
