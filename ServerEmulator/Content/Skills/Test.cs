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


        public override void Load()
        {
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
                    return 88;
                }, TaskCreationOptions.LongRunning);

                Task.WaitAll(task);
                var res = task.Result;



                int? hhh = 66;


                

                int i5 = 0;

                Thread t = new Thread(() =>
                {
                    i5 = 7888;
                });

                Parallel.For(0, 100, ((int index) =>
                {

                }));

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
