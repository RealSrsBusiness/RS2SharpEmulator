using ServerEmulator.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using static ServerEmulator.Core.Constants;
using System.Text.Json.Serialization;
using System.Text.Json;

using System.Xml;

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

            int internalModules = LoadModules(Assembly.GetExecutingAssembly());
            Console.WriteLine($"Finished loading modules: {internalModules} (Of which internal: {internalModules}, external: {0})");

            LoadingComplete = true;
            Program.Log("All data loaded.");
        }

        class MyType {
            public int a { get; set; }
        }

        private static void Lists()
        {
            if(Program.DebugRunTime) 
            {
                Program.Warning("Content lists not loaded, using empty values instead");

                //default 317 amounts
                Items = new Item[6541]; //6541
                Npcs = new NPC[2633]; //2633
                Objects = new GameObject[9399]; //9399
            }

            var options = new JsonSerializerOptions 
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, //null or default value
                WriteIndented = true //pretty printing
            };
                
            var coolObject = new MyType[] {
                new MyType { a = 10 },
                new MyType { a = 20 },
                new MyType { a = 30 },
            };

            var result = JsonSerializer.Serialize(coolObject, options);
            //File.WriteAllText("coolfile.json", result);
            ;
        }

        
        private static int LoadModules(Assembly netAsm)
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

            return loaded;
        }

        private static void ObjectMap()
        {
            if(Constants.MAP_LOADING_METHOD == 0)
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
            catch (Exception)
            {
                throw new Exception("Mapdata");
            }
        }
    }
}
