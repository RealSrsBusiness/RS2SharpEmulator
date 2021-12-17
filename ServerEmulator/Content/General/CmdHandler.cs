using System;
using ServerEmulator.Core.Game;

namespace ServerEmulator.Content {
    class CmdHandler : Core.Game.Content {
        
        public override void Load() {
            Console.WriteLine("loaded cmdhandler.");

            this.Commands.Add("testnpc", (Client c) => {
                
            });
        }
    }

}