using System;
using ServerEmulator.Core.Game;

namespace ServerEmulator.Content {
    class CmdHandler : Core.Game.Content {
        
        public override void Load() {
            Console.WriteLine("loaded cmdhandler. test commands available: ::chat ::hitsplat ::animation");

            this.Commands.Add("chat", (Client c) => {
                var chat = c.Player.effects.Chat;
                chat.text = "Hello World!";
            });

            this.Commands.Add("hitsplat", (Client c) => {
                var dmg = c.Player.effects.Damage;
                dmg.damage = 20;
                dmg.type = 1; //red hitsplat
                dmg.health = 10;
                dmg.maxHealth = 50;
            });

            this.Commands.Add("animation", (Client c) => {
                var animation = c.Player.effects.Animation;
                animation.animationId = 733; //lighting logs
                animation.delay = 0;
            });

            this.Commands.Add("obj", (Client c) => {
                //c.SpawnObjectForClient(5628);
            });

            this.Commands.Add("stump", (Client c) => {
                //c.SpawnObjectForClient(1341);
            });

            this.Commands.Add("rem", (Client c) => {
                //c.RemoveObject();
            });

            this.Commands.Add("all", (Client c) => {
                var dmg = c.Player.effects.Damage;
                dmg.damage = 20;
                dmg.type = 2;
                dmg.maxHealth = 99;

                var animation = c.Player.effects.Animation;
                animation.animationId = 875;

                var chat = c.Player.effects.Chat;
                chat.text = "Hello World!";
            });


            this.Commands.Add("walk", (Client c) => {
                c.Player.running = false;
            });

            this.Commands.Add("run", (Client c) => {
                c.Player.running = true;
            });
        }
    }

}