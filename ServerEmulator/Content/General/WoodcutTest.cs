using System;
using ServerEmulator.Core.Game;
using static ServerEmulator.Core.Game.RSEModule;

namespace ServerEmulator.Content 
{
    class WoodcutTest : RSEModule 
    {
        
        //tree stump: 1341
        public override void Load() 
        {
            Console.WriteLine("loaded woodcut test. (not working yet)");

            Commands.Add("axe", (Client c, string[] args) => {

            });

            Objects[100] = new GameObject();

            Objects[100].OnAction[0] = (Client c) => {

            };

        }
    }

}