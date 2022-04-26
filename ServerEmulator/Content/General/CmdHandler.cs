using System;
using ServerEmulator.Core.Game;
using System.Reflection;
using static ServerEmulator.Core.Game.RSEModule;

namespace ServerEmulator.Content 
{
    [AttributeUsage(AttributeTargets.Method)]
    class Command : Attribute 
    {
        public string cmd;
        public int minArgs = -1; //unused for now
        public Command(string cmd, int minArgs = -1) 
        {
            this.cmd = cmd;
            this.minArgs = minArgs;
        }
    }
    
    class CmdHandler : RSEModule
    {
        Client c;
        string[] args;

        public override void Load() 
        {
            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                var att = method.GetCustomAttribute<Command>();
                if(att != null) 
                {
                    Commands.Add(att.cmd, (Client client, string[] arguments) => 
                    {
                        c = client;
                        args = arguments;
                        method.Invoke(this, null);
                    });
                }
            }

            Console.WriteLine("loaded cmdhandler. test commands available: ::chat ::hitsplat ::anim");
        }

        [Command("chat")]
        void Chat() {
            var chat = c.Player.effects.Chat;
            chat.text = "Hello World!";
        }

        [Command("hitsplat")]
        void HitSplat() {
            var dmg = c.Player.effects.Damage;
            dmg.damage = 20;
            dmg.type = 1; //red hitsplat
            dmg.health = 10;
            dmg.maxHealth = 50;
        }

        [Command("anim")]
        void Animation() {
            var animation = c.Player.effects.Animation;
            animation.animationId = 733; //lighting logs
            animation.delay = 0;
        }

        [Command("all")]
        void All() {
            var dmg = c.Player.effects.Damage;
            dmg.damage = 20;
            dmg.type = 2;
            dmg.maxHealth = 99;

            var animation = c.Player.effects.Animation;
            animation.animationId = 875;

            var chat = c.Player.effects.Chat;
            chat.text = "Hello World!";
        }

        [Command("walk")]
        void Walk() {
            c.Player.running = false;
        }

        [Command("run")]
        void Run() {
            c.Player.running = true;
        }

        [Command("obj")]
        void PlaceObject() {
            //c.SpawnObjectForClient(5628);
        }

        [Command("stump")]
        void Stump() {
            //c.SpawnObjectForClient(1341);
        }

        [Command("rem")]
        void RemoveObject() {
            //c.RemoveObject();
        }
    }

}