using ServerEmulator.Core.Game;
using ServerEmulator.Core.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static ServerEmulator.Core.Constants;

namespace ServerEmulator.Core
{
    internal class Processor
    {
        Thread clientAcceptor, gameProcessing;
        List<Connection> connections = new List<Connection>(MAX_CONNECTIONS);
        List<Client> establishedClients = new List<Client>(MAX_PLAYER);
        Socket listener;

        private int delayedTicks;
        private object _lock = new object();
        public bool Running { get; set; }

        public void Start()
        {
            Running = true;
            World.Init();

            clientAcceptor = new Thread(SocketListener);
            gameProcessing = new Thread(GameLoop);

            gameProcessing.Start();
            clientAcceptor.Start();
        }

        void GameLoop()
        {
            Stopwatch sw = new Stopwatch();
            while (Running)
            {
                sw.Start();

                //update all entities (actions, movement of players, npcs etc) and build packages
                World.ProcessWorld(); //I don't think this is needed, can probably be combined with finalizeWorld()

                //update what all the clients see and send out packages
                for (int i = 0; i < establishedClients.Count; i++)
                {
                    Client client = establishedClients[i];
                    client.RenderScreen();
                }

                World.FinalizeWorld();

                sw.Stop();
                int remainingSleep = CYCLE_TIME - (int)sw.ElapsedMilliseconds;
                if (remainingSleep > 0)
                {
                    Thread.Sleep(remainingSleep);
                }
                else
                {
                    delayedTicks++;
                    Program.Warning("Server can't keep up. Delayed Ticks so far: {0}", delayedTicks);
                }
                sw.Reset();
            }
        }

        public void RegisterClient(Client client)
        {
            lock (_lock)
            {
                establishedClients.Add(client);
            }
        }

        public void UnregisterClient(Client client)
        {
            lock (_lock)
            {
                establishedClients.Remove(client);
            }
        }

        void ScheduleUpdate()
        {

        }

        void SocketListener()
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, PORT));
                listener.NoDelay = true;
                listener.Listen(20);
                listener.BeginAccept(Accept_Client, null);
                Program.Log("Listening on Port {0}", PORT);
            }
            catch
            {
                Program.Shutdown("Could not listen on Port {0}. System hold.", false, PORT);
            }
        }

        void Accept_Client(IAsyncResult ar)
        {
            Connection host = new Connection(listener.EndAccept(ar));
            host.onDisconnect += Disconnect_Client;
            host.handle = new LoginHandler(host).Handle;
            host.ReceiveData(2); //todo, check if ip is banned before receiving, if so, let connection time out

            connections.Add(host);
            Program.Log("New Client has been connected {0}", host.EndPoint);
            listener.BeginAccept(Accept_Client, null);
        }

        void Disconnect_Client(Connection c)
        {
            connections.Remove(c);
            establishedClients.RemoveAll(client => client.Packets.C == c);
            Program.Log("Connection lost {0}", c.EndPoint);
        }
    }
}
