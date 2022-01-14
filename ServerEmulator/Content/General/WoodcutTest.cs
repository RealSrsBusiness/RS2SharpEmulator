using System;
using ServerEmulator.Core.Game;

namespace ServerEmulator.Content {
    class WoodcutTest : Core.Game.Content {

        public override void Load() {
            Console.WriteLine("loaded woodcut test. type ::axe to get an axe");

            this.Commands.Add("axe", (Client c) => {
                
            });
        }
    }

}