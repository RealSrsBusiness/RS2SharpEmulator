using ServerEmulator.Core.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerEmulator.Content.Skills
{
    class Test : Core.Game.Content
    {


        public override void Load()
        {

            for (int i = 0; i < 1000; i++)
            {
                
                Objects[i].OnAction[0] = (Client c) =>
                {
                    c.GetState<Cxt>(0).test = 66;

                    c.State().test = 66;

                    //c.NextRandom();
                    //c.ValueRandom();


                };

            }
        }
    }

    internal class Cxt
    {
        public int test;
    }

    static class Ex { public static Cxt State(this Client c) => c.GetState<Cxt>(0); }
}
