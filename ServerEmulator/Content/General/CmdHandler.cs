using System;
using ServerEmulator.Core.Game;
using System.Reflection;
using static ServerEmulator.Core.Game.RSEModule;

namespace ServerEmulator.Content 
{  
    [Obfuscation(Exclude = true)]
    class CmdHandler : RSEModule //method name determines to command to type
    {
        Client c;
        string[] args;

        [Command]
        void Give() { //give an item

            int? id = null, amt = null;

            try {
                id = int.Parse(args[0]);
                amt = int.Parse(args[1]);
            }catch { }
                
            if(id != null)
                c.Player.inventory.Add((int)id, amt != null ? (int)amt : 1);
        }

        [Command]
        void Take() { //take item away

        }

        [Command]
        void remOne() {
            var items = new (short slot, ushort? id, int amt)[] {
                (0, null, 0), //remove?
            };

            c.Packets.SetItemsBySlots(3214, items);
        }

        [Command]
        void Item() {
            c.Packets.ClearItemContainer(3214);

            var items = new (short slot, ushort? id, int amt)[] {
                (0, 995, 6000), //coins
                (3, 1351, 1), //axe
                (10, 590, 1), //tinderbox
                (20, 78, Int32.MaxValue)
            };

            c.Packets.SetItemsBySlots(3214, items);

            Console.WriteLine("inventory cleared....");
        }

        [Command]
        void Chat() {
            var chat = c.Player.effects.Chat;
            chat.text = "Hello World!";
        }

        [Command]
        void HitSplat() {
            var dmg = c.Player.effects.Damage;
            dmg.damage = 20;
            dmg.type = 1; //red hitsplat
            dmg.health = 10;
            dmg.maxHealth = 50;
        }

        [Command]
        void Anim() {
            var animation = c.Player.effects.Animation;
            animation.animationId = 733; //lighting logs
            animation.delay = 0;
        }

        [Command]
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

        [Command]
        void Walk() {
            c.Player.running = false;
        }

        [Command]
        void Run() {
            c.Player.running = true;
        }

        [Command]
        void Obj() { //PlaceObject
            //c.SpawnObjectForClient(5628);
        }

        [Command]
        void Stump() {
            //c.SpawnObjectForClient(1341);
        }

        [Command]
        void Rem() { //RemoveObject
            //c.RemoveObject();
        }

        public override void Load()
        {
            var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            var commands = 0;
            
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                var att = method.GetCustomAttribute<Command>();
                
                if(att != null) 
                {
                    var command = method.Name.ToLower();
                    Commands.Add(command, (Client client, string[] arguments) => 
                    {
                        c = client;
                        args = arguments;
                        method.Invoke(this, null);
                    });
                    commands++;
                }
            }
            Console.WriteLine($"Loaded cmdhandler. Total loaded: {commands}; examples: ::chat ::hitsplat ::anim");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    class Command : Attribute 
    {
        public int minArgs = -1; //unused for now
        public Command(int minArgs = -1) 
        {
            this.minArgs = minArgs;
        }
    }

}