using System;
using ServerEmulator.Core.Game;

namespace ServerEmulator.Content {
    class CmdHandler : Core.Game.Content {
        
        public override void Load() {
            Console.WriteLine("loaded cmdhandler. ::chat, ::hitsplat and ::animation available");

            this.Commands.Add("chat", (Client c) => {
                
            });

            this.Commands.Add("hitsplat", (Client c) => {
                
            });

            this.Commands.Add("animation", (Client c) => {
                
            });
        }
    }

}