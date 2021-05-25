using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static ServerEmulator.Core.Constants;

using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace ServerEmulator.Core.Game
{
    class DataLoader
    {
        public static bool LoadingComplete { get; private set; } = false;
        internal static Dictionary<short, int[]> mapXteas = new Dictionary<short, int[]>();

        internal static NPC[] Npcs;
        internal static Item[] Items;
        internal static GameObject[] Objects;

        internal static Dictionary<int, Action> ActionButtons;
        internal static Dictionary<string, Interaction> Commands = new Dictionary<string, Interaction>();
        internal static Definition PlayerActions;

        internal static object[] CreateCustomStates()
        {
            return null;
        }

        internal static void LoadContent()
        {
            ObjectMap();
            //MapXteas(); //untested
            Lists();
            LoadModules(Assembly.GetExecutingAssembly());

            LoadingComplete = true;

            Program.Log("All data loaded.");
        }

        private static void Lists()
        {
            if(Program.DEBUG)
                Program.Warning("Content lists could not be loaded.");
        }

        private static void LoadModules(Assembly netAsm)
        {
            var types = netAsm.GetTypes();
            int loaded = 0;

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];

                if(type.IsSubclassOf(typeof(Content))) {
                    var content = (Content)Activator.CreateInstance(type);
                    content.Load();
                    loaded++;
                }
            }

            Console.WriteLine($"Loaded {loaded} modules.");

        }

        private static void ObjectMap()
        {
            if(Constants.MAP_LOADING_METHOD == 0);
                Program.Warning("Map Verification deactivated.");
        }

        private static void MapXteas()
        {
            Program.Log("Loading Map Xtea Keys...");
            try
            {
                FileStream fs = new FileStream(DATA_PATH + "mapXteas.dat", FileMode.Open);
                RSStreamReader stream = null;// new RSStreamReader(fs);
                while (fs.Position < fs.Length)
                {
                    short area = (short)stream.ReadShort();
                    int[] parts = new int[4];
                    for (int i = 0; i < parts.Length; i++)
                        parts[i] = stream.ReadInt();
                    mapXteas.Add(area, parts);
                }
                fs.Close();
                fs.Dispose();
            }
            catch (Exception e)
            {
                throw new Exception("Mapdata");
            }
        }
    }
}
