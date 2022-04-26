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
    delegate void CommandHandle(Client c, string[] args);

    [Obfuscation(Exclude = true)]
    abstract class RSEModule
    {
        public abstract void Load();

        public static int AddState(Type state)
        {
            if (ContentLoading.Completed)
                throw new InvalidOperationException("New states cannot be added once loading is complete.");

            customStates.Add(state);
            return customStates.IndexOf(state);
        }

        public static NPC[] Npcs;
        public static Item[] Items;
        public static GameObject[] Objects;

        public static Dictionary<int, Action> ActionButtons;
        public static Dictionary<string, CommandHandle> Commands = new Dictionary<string, CommandHandle>();
        public static Definition PlayerActions;

        static List<Type> customStates = new List<Type>();
    }

    static class ContentLoading 
    {
        public static bool Completed { get; private set; } = false;

        internal static void Load()
        {
            ObjectMap();
            //MapXteas(); //untested
            Lists();

            int internalModules = LoadModules(Assembly.GetExecutingAssembly());
            Console.WriteLine($"Finished loading modules: {internalModules} (Of which internal: {internalModules}, external: {0})");

            Completed = true;
            Program.Log("All data loaded.");
        }

        private static void Lists()
        {
            if(Program.DebugRunTime) 
            {
                Program.Warning("Content lists not loaded, using empty values instead");

                //default 317 amounts
                RSEModule.Items = new Item[6541]; //6541
                RSEModule.Npcs = new NPC[2633]; //2633
                RSEModule.Objects = new GameObject[9399]; //9399
            }

            var options = new JsonSerializerOptions 
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, //null or default value
                WriteIndented = true //pretty printing
            };
                
            var coolObject = new TestType[] {
                new TestType { a = 10 },
                new TestType { a = 20 },
                new TestType { a = 30 },
            };

            var result = JsonSerializer.Serialize(coolObject, options);
            //File.WriteAllText("coolfile.json", result);
            ;
        }

        class TestType {
            public int a { get; set; }
        }

        internal static object[] NewStateInstanceSet()
        {
            return null;
        }
        
        private static int LoadModules(Assembly netAsm)
        {
            var types = netAsm.GetTypes();
            int loaded = 0;

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];

                if(type.IsSubclassOf(typeof(RSEModule))) {
                    var content = (RSEModule)Activator.CreateInstance(type);
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

        internal static Dictionary<short, int[]> mapXteas = new Dictionary<short, int[]>();
    }

}
