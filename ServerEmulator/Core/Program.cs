using ServerEmulator.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ServerEmulator.Core
{
    internal class Program
    {
        public static Processor Processor { get; private set; }
        const bool DEBUG = true;

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
           // public HashSet<Entitiy> entities;
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
                Program.Log("lol");
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
                        Console.WriteLine("lul" + i);
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
                        Console.WriteLine("lolol" + i);
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
                        Console.WriteLine("lul" + i);
                        Thread.Sleep(10);
                }

                mre.Set();
                
            }


            public void t2()
            {
                mre.WaitOne();

                for (int i = 0; i < 1000; i++)
                {
                    Console.WriteLine("lolol" + i);
                    Thread.Sleep(10);
                }
            }
        }


        static void Main(string[] args)
        {
            Console.Title = "Server Emulator Rev #" + Constants.SERVER_REV;

            if(!Directory.Exists(Constants.DATA_PATH))
            {
                Directory.CreateDirectory(Constants.DATA_PATH);
                Program.Log("Created content folder.");
            }

           // new tt2().start();


            Processor = new Processor();
            Processor.Start();

           // p6();

            //PerformanceTest();
            //Performance2();


            Log("Server successfully initialized");

            while(Processor.Runnning)
            {
                string cmd = Console.ReadLine().ToLower();
                switch(cmd)
                {
                    case "exit":
                        Shutdown("Server shutdown by command.", false);
                        break;

                    default: Log("Unrecognized command, type 'help' for a list of commands.");
                        break;
                }
            }
        }


        public static void Log(string text, params object[] format)
        {
            DateTime dt = DateTime.Now;
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
            if(DEBUG)
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

        /*
         * Server commands:
         * status = shows the status including listening port and bound socket; connected clients
         * online = lists online player
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
         * 
         * Player commands:
         * skill <player> <skill> <xp>
         * give <player> <item> <count>
         * kill <player>
         * teleport <player> <coords>
         * teletoplayer <player> <player>
         * rollback <player> <id>
         * 
         * */

        //higher rev implementations: a* pathfinding, xtea map files, "onExamine" packet 

    }
}
