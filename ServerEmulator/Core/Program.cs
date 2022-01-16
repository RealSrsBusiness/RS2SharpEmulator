using ServerEmulator.Core;
using ServerEmulator.Core.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using static ServerEmulator.Core.Extensions;

namespace ServerEmulator.Core
{
    /// <summary>
    /// Controls the server and loads and saves settings
    /// </summary>
    internal class Program
    {
        public static Processor Processor { get; private set; }
        public static bool DebugRunTime { get; private set; } = false;

        public static string windowTitle = $"RS2SharpEmulator v.{VERSION} - Rev #{Constants.SERVER_REV}";
        public const double VERSION = 0.7;

        //todo: disallow clicking in console window because it causes the server to pause
        static void Main(string[] args)
        {
            TestPlayground();

#if DEBUG
            windowTitle = "[DEBUG] " + windowTitle;
            DebugRunTime = true;
#endif     
            Console.Title = windowTitle;


            if (args.Length > 0)
                if (args[0].ToLower().Equals("debug"))
                    DebugRunTime = true;


            if (Debugger.IsAttached && !DebugRunTime)
                return;
                

            if(!Directory.Exists(Constants.DATA_PATH))
            {
                Directory.CreateDirectory(Constants.DATA_PATH);
                Program.Log("Content folder created.");
            }


            Processor = new Processor();
            Processor.Start();

            DataLoader.LoadContent();

            Log("Server successfully initialized @Thread " + Thread.CurrentThread.ManagedThreadId);

            while(Processor.Running)
            {
                string cmd = Console.ReadLine().ToLower();
                switch(cmd)
                {
                    case "exit":
                        Shutdown("Server shutdown by command.", false);
                        break;

                    case "help":
                        Console.WriteLine("Commands: exit, help.");
                        break;

                    case "thread":
                        Console.WriteLine("Thread id: " + Thread.CurrentThread.ManagedThreadId);
                        break;

                    default: Log("Unrecognized command, type 'help' for a list of commands.");
                        break;
                }
            }
        }

        public static void Log(string text, params object[] format) //todo: do it proper by replacing the standard output (Console.Out)
        {
            DateTime dt = DateTime.Now; //...also there's a better (already in-built) way to format time
            Console.WriteLine("[{0}:{1}:{2}] {3}", dt.Hour, dt.Minute, dt.Second, string.Format(text, format)); 
        }

        public static void Warning(string text, params object[] format)
        {
            ConsoleColor cc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log(text, format);
            Console.ForegroundColor = cc;
        }

        public static void Debug(string text, params object[] format)
        {
            if(DebugRunTime)
                Log($"[DEBUG] {text}", format);
        }

        public static void Shutdown(string exitMsg, bool exit = true, params object[] format)
        {
            Cleanup();
            Log(exitMsg, format);
            if(!exit)
                Console.ReadKey();
            Environment.Exit(0);
        }

        public static event Action Cleanup = delegate { };

        /* RS2SharpEmulator
         * [todo: Server command suggestions]:
         * status = shows the status including listening port and bound socket; connected clients
         * modules = show loaded modues
         * online = lists online players
         * count = number of players online
         * save = saves all players
         * restart = saves all players, disconnects them and restarts the server
         * close = shuts the server down
         * update <seconds> <restart/restartPC/restart+update>= update the server
         * kick <ip/p> [value] = kicks a player or ip address
         * ban <ip/p> [value] = bans a player or ip address
         * pause = pauses the listener; all open connections are kept open, however no new connections are established
         * purge <yes/no(listener)> = disconnects every player; the server keeps running
         * start = starts the listener
         * msg <message> = global message
         * limit = set a new connection limit for this session
         * [DEBUG] cycletime = display average cycletime (last 10 min)
         * [DEBUG] traffic = display average network traffic (last 10 min)
         *
         * Player commands:
         * skill <player> <skill> <xp>
         * give <player> <item> <count>
         * kill <player>
         * teleport <player> <coords/player>
         * rollback <player> <id>
         * 
         * */

        //higher rev implementations: a* pathfinding, xtea map files, "onExamine" packet 



#region  -------------------------- Test/Playground-Region ----------------------------------
        
        interface Effe {

        }

        class Damage : Effe {

        }

        class Appear : Effe {

        }

        class MyInt 
        {
            public int value;

            public MyInt(int value) => this.value = value;
            public override string ToString() => value.ToString();
        }

        static void TestPlayground() {
            //ArraylistVsLinkedList();
            if(true) return;

            byte[] arr = "hello".ToJagString();

            sbyte[] arr2 = new sbyte[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                arr2[i] = (sbyte)arr[i];
            }



            if(true) return;

            var values = new MyInt[] {
                new MyInt(1),
                new MyInt(2),
                new MyInt(3),
                new MyInt(4),
                new MyInt(5),
                new MyInt(6),
                new MyInt(7),
                new MyInt(8),
                new MyInt(9),
                new MyInt(10),

                new MyInt(20),
                new MyInt(21),
                new MyInt(12)
            };


            var l1 = new MyInt[]{values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]};
            //var l2 = new MyInt[]{values[0], values[1], values[4], values[5], values[7], values[9], values[10], values[11], values[12]};
            
            var l2 = new MyInt[]{values[11], values[4], values[9], values[10], values[12], values[5], values[7], values[0], values[1]};
            
            ;
            var res = l1.Difference<MyInt>(l2);
            
            
            ;
            //-----------------------------------------------------------------
            
        
            BufferedEffectStates odes = new BufferedEffectStates();

            var appear = odes.Appearance;

            appear.appearanceValues = new int[] { -1, -1, -1, -1, 18, -1, 26, 36, 0, 33, 42, 10 };
            appear.colorValues = new int[] { 7, 8, 9, 5, 0 };
            appear.idleAnimations = new int[] { 808, 823, 819, 820, 821, 822, 824 };
            appear.gender = 69;
            appear.combatLevel = 69;
            appear.skill = 2056;

            for (int i = 0; i < 100; i++)
            {
                

                if(i < 20) 
                {
                    var inc = odes.Incremental;
                    odes.Reset();
                    ;
                } 
                else if(i < 40) 
                {
                    var inc = odes.Full;
                    odes.Reset();
                    ;
                }
                else if(i < 60) 
                {
                    odes.Reset();
                    break;
                }


            }
            
            //-----------------------------------------------------------------
            if(true) return;



            List<bool> bits = new List<bool>();
            bits.EncodeValue(8, 156);
            bits.OverwriteValueAt(0, 8, 0);
            bits.OverwriteValueAt(4, 4, 15);

            ;
    

/*
            Object[] objs = new Object[] {
                1, 2, 3, 5
            };

            Object[] src = {objs[0], objs[1], objs[2]};
            Object[] trg = {objs[0], objs[3]};

            var changes = src.Difference<Object>(trg);
            ;
*/
        }

        static void ArraylistVsLinkedList() {
            LinkedList<object> linkedlist = new LinkedList<object>();
            List<object> arraylist = new List<object>();

            var mynode = linkedlist.AddFirst(new object());

            Stopwatch sw = new Stopwatch();

            sw.Start();
            for (int i = 0; i < 100000000; i++)
            {
                //mynode = linkedlist.AddAfter(mynode, new object());
                //linkedlist.AddFirst()

                //new LinkedListNode<object>(new object());

                //arraylist.Add(new object());
            }
            sw.Stop();
            var res1 = sw.ElapsedMilliseconds; //20 seconds linkedlist vs 4 seconds arraylist
            ;
            
            

            foreach (var item in linkedlist)
            {
                var blah = item.ToString();
            }

    

            sw.Reset();
            sw.Start();
            foreach (var item in arraylist)
            {
                var blah = item.ToString();
            }

            var res2 = sw.ElapsedMilliseconds;
            ;
            

        }
        
       
        static void PerformanceTest()
        {
            byte[] data = new byte[100];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            MemoryStream ms;
            
            for (int i = 0; i < 10000000; i++)
            {
                ms = new MemoryStream(data);
            }

            sw.Stop();
            Log(sw.ElapsedMilliseconds.ToString());



            Stopwatch sw2 = new Stopwatch();
            sw2.Start();

            MemoryStream ms2 = new MemoryStream();
            for (int i = 0; i < 10000000; i++)
            {
                ms2.SetLength(0);
                ms2.Write(data, 0, data.Length);
            }

            sw2.Stop();
            Log(sw2.ElapsedMilliseconds.ToString());

        }

        static void Performance2()
        {
            Tile[] world = new Tile[4000 * 4000 * 4];

            Stopwatch sw = new Stopwatch();
            sw.Start();

            byte[] b;
            for (int i = 0; i < 10000000; i++)
            {
                b = new byte[128];
            }

            sw.Stop();


            Program.Debug("Performance {0}", sw.ElapsedMilliseconds);
        }

        public delegate void TestDele();

        static event TestDele events;

        static void p3()
        {

            TestDele[] tttt = new TestDele[338000];

            for (int i = 0; i < tttt.Length; i++)
            {
                tttt[i] += delegate
                {
                    Console.WriteLine("yo");
                };
            }

            for (int i = 0; i < tttt.Length; i++)
            {
                events += tttt[i];
            }

            //events();


            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 50; i++)
            {
                events -= tttt[i];
            }

            Program.Debug("removed all");

          

            sw.Stop();
            Program.Debug("Performance {0}", sw.ElapsedMilliseconds);
        }
   
        //static HashSet<Entitiy> entities = new HashSet<Entitiy>();

        struct Tile
        {
            public HashSet<Entitiy> entities;
            public byte CollisionMap;

            //public object[] obs;

        }
        /*

        static void p4()
        {

            for (int i = 0; i < 1000; i++)
            {
                entities.Add(new Entitiy());
            }
            

            Tile[] world = new Tile[4000 * 4000 * 4];
            for (int i = 0; i < world.Length; i++)
            {
                world[i] = new Tile();
            }

            int tiles = 104 * 104;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            HashSet<Entitiy>[] hs = new HashSet<Entitiy>[10000];

            for (int i = 0; i < hs.Length; i++)
            {
                hs[i] = new HashSet<Entitiy>();
            }

            


            sw.Stop();
            Program.Warning("world iteration {0}", sw.ElapsedMilliseconds);


            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < tiles; j++)
                {
                    Tile t = world[j];
                    //if(t.updated)
                    //{
                    //    break;
                    //}
                }
            }
            
        }

        */

        class Entitiy
        {
            public int i = 6;
        }

        class Region
        {
            public List<Entitiy> entities = new List<Entitiy>();

            public Region()
            {
                for (int i = 0; i < 100; i++)
                {
                    entities.Add(new Entitiy());
                }
            }

        }

        static void p5()
        {
            int nmbRegions = (4000 / 8) * (4000 / 8);
            Program.Log("Regions: {0}", nmbRegions);

            Region[] r = new Region[nmbRegions];

            for (int i = 0; i < r.Length; i++)
            {
                r[i] = new Region();
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 2000; i++)
            {
                for (int j = 0; j < 169; j++)
                {
                    Region h = r[j];
                    var entities = h.entities;
                    for (int k = 0; k < entities.Count; k++)
                    {
                        if (entities[k].i == 4)
                            break;
                    }
                }
            }

            sw.Stop();

            Program.Warning("update time {0}", sw.ElapsedMilliseconds);
        }

        static void p6()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            System.Timers.Timer[] ts = new System.Timers.Timer[3000];

            for (int i = 0; i < ts.Length; i++)
            {
                ts[i] = new System.Timers.Timer(3000);
                ts[i].Elapsed += Program_Elapsed;
                ts[i].Start();

               // ts[i].Stop();
            }

            sw.Stop();

            Program.Warning("time elapsed {0}", sw.ElapsedMilliseconds);

        }

        static int lolLevel = 0;

        private static void Program_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lolLevel++;
            if (lolLevel >= 9000)
            {
                Program.Log("blah");
                lolLevel = 0;
            }
            //t.Stop();
        }

        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Program.Warning("-----Elapsed----");
            var s = sender as System.Timers.Timer;
            s.Stop();

            sw.Stop();

            Program.Warning("lul2 {0}", sw.ElapsedMilliseconds);
        }

        static Stopwatch sw = new Stopwatch();

        class ThreadTest
        {
            private readonly object _locker = new object();

            public void start()
            {
                Thread tt1 = new Thread(t1);
                Thread tt2 = new Thread(t2);

                tt1.Start();
                Thread.Sleep(100);
                tt2.Start();

            }

            public void t1()
            {
                lock(_locker)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        Console.WriteLine("blub" + i);
                        Thread.Sleep(10);
                    }
                }
            }
            

            public void t2()
            {
                lock(_locker)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        Console.WriteLine("blah" + i);
                        Thread.Sleep(10);
                    }
                }
            }

        }

        class tt2
        {
            private ManualResetEvent mre = new ManualResetEvent(false);

            public void start()
            {
                Thread tt1 = new Thread(t1);
                Thread tt2 = new Thread(t2);

                tt1.Start();
                Thread.Sleep(100);
                tt2.Start();

            }

            public void t1()
            {
                
                for (int i = 0; i < 1000; i++)
                {
                        Console.WriteLine("blah1" + i);
                        Thread.Sleep(10);
                }

                mre.Set();
                
            }


            public void t2()
            {
                mre.WaitOne();

                for (int i = 0; i < 1000; i++)
                {
                    Console.WriteLine("blah2" + i);
                    Thread.Sleep(10);
                }
            }
        }

        partial class Clienta
        {
            public object[] states = new object[1000];
            ItemStack[] inventory = new ItemStack[28];

            public object this[int i] { get{ return states[i]; } }

            public Clienta()
            {
                states[5] = new WCContext() { axeTier = 5 };
                inventory[20] = new ItemStack() { id = 4 };
                axeTier = 5;
            }

            public T GetState<T>(int index)
            {
                return (T)states[index];
            }

            public object GetState2(int index)
            {
                return states[index];
            }

            public bool HasItem(int id)
            {
                for (int i = 0; i < inventory.Length; i++)
                    if (inventory[i].id == id)
                        return true;
                return false;
            }
        }

        partial class Clienta
        {
            public int axeTier;
        }

        struct WCContext
        {
            public int axeTier;
        }

        public static class ContextPTest
        {
            public static void Run()
            {
                Stopwatch sw = new Stopwatch();
                Clienta c = new Clienta();

                int[] axes = new int[] { 1, 2, 3, 4, 5 };


                sw.Start();

                for (int i = 0; i < 10000000; i++) //~280ms
                {
                    // WCContext cxt = c.GetState<WCContext>(5);
                    for (int j = 0; j < 3; j++)
                    {
                        //WCContext cxt = (WCContext)c[5];

                        WCContext cxt = (WCContext)c.GetState2(5); //non generic version is even faster
                        int res = cxt.axeTier;
                    }
                    
                }

                sw.Stop();

                Program.Debug("Res: {0}", sw.ElapsedMilliseconds);
                sw.Reset();


                sw.Start();
                for (int i = 0; i < 10000000; i++) //~280ms
                {
                    // WCContext cxt = c.GetState<WCContext>(5);
                    int cxt = c.axeTier; //non generic version is even faster
                    int res = cxt;
                }
                sw.Stop();
                Program.Debug("Res: {0}", sw.ElapsedMilliseconds);
                sw.Reset();

                sw.Start();

                for (int i = 0; i < 10000000; i++)
                {
                    int tier = 0;
                    for (int j = 0; j < axes.Length; j++)
                    {
                        if(c.HasItem(axes[j]))
                        {
                            tier = j;
                            break;
                        }
                    }
                    int res = tier;
                }

                sw.Stop();

                Program.Debug("Res: {0}", sw.ElapsedMilliseconds);
            }

        }


#endregion


    }
}
