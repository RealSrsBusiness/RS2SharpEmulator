using ServerEmulator.Core.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerEmulator.Content.Skills
{
    class Test : Core.Game.Content
    {

#if (_TYPE_DEFINE)
        Tree{ int logId, int level, xp; }
        Tree[] trees = {
            Normal { logId: 1, level: 10, xp: 100 },
            
        }
#endif

        delegate void PerformCmd(string[] args);
        Dictionary<string, PerformCmd> cmds = new Dictionary<string, PerformCmd>();


        public override void Load()
        {
            if(true) {
                Console.WriteLine("laoded test.");
                return;
            }

            Dictionary<(int, int), string> dict = new Dictionary<(int, int), string>();




            cmds.Add("item", (string[] args) =>
            {

            });

            string cmd = "";

            var data = cmd.Split(' ');
            string mycmd = data[0];
            var args_ = data.Skip(1);

            cmds[mycmd]((string[])args_);



            for (int i = 0; i < 1000; i++)
            {

                Objects[i].OnAction[0] = (Client c) =>
                {
                    c.State<Cxt>(0).test = 66;

                    c.State().test = 66;

                    //c.NextRandom();
                    //c.ValueRandom();

                };

                Task.Run(() => Console.WriteLine("hello world"));

                var task = Task.Factory.StartNew(() =>
                {
                    return 33;
                }, TaskCreationOptions.LongRunning);

                Task.WaitAll(task);
                var res = task.Result;



                int? hhh = 66;


                int[] iiii = new int[5555];
                object o = iiii;

                int[] back = (int[])o;


                

                int i5 = 0;

                Thread t = new Thread(() =>
                {
                    i5 = 7888;
                });

                Parallel.For(0, 100, ((int index) =>
                {

                }));

                var gg = localFunction();

                bool localFunction()
                {
                    return true;
                }

                t.Start();
                t.Join();


                MemoryStream[] u = new MemoryStream[100];

                u.Last();
                u.First();
                u.Random();

                WebClient wc = new WebClient();
                wc.DownloadStringAsync(new Uri("sdffsddf"));

            }
        }
    }

    internal class Cxt
    {
        public int test;
    }

    static class Ex { public static Cxt State(this Client c) => c.State<Cxt>(0); }


    static class Ex2 {
        static Random ran = new Random();
        public static T Random<T>(this ICollection<T> c)
        {
            return c.ElementAt<T>(ran.Next(0, c.Count));
        }
    }
}
