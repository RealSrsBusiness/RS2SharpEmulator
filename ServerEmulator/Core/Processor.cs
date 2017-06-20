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
        List<Client> clients = new List<Client>(MAX_PLAYER);
        Socket listener;

        private int delayedTicks;
        private object _lockObj = new object();
        public bool Runnning { get; set; } = true;

        public void Start()
        {
            clientAcceptor = new Thread(SocketListener);
            gameProcessing = new Thread(GameLoop);

            gameProcessing.Start();
            clientAcceptor.Start();
        }

        public void RegisterClient(Client client)
        {
            lock(_lockObj)
            {
                clients.Add(client);
            }
        }

        void ScheduleUpdate()
        {

        }

        void GameLoop()
        {
            Stopwatch sw = new Stopwatch();
            while (Runnning)
            {
                sw.Start();

                //do all queries and build all delegates that command the server to send out the packages (?)

                for (int i = 0; i < clients.Count; i++)
                {
                    Client client = clients[i];
                    if(client.Update())
                        client.Flush();
                }

                //raise events

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

        void SocketListener()
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, PORT));
                listener.NoDelay = true;
                listener.Listen(100);
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
            Socket remoteHost = listener.EndAccept(ar);
            remoteHost.NoDelay = true;
            Connection c = new Connection(remoteHost);
            c.onDisconnect += Disconnect_Client;
            connections.Add(c);
            c.handle = new LoginHandler(c).Handle;
            c.ReceiveData(2);
            Program.Log("New Client has been connected {0}", c.EndPoint);
            listener.BeginAccept(Accept_Client, null);
        }

        void Disconnect_Client(Connection c)
        {
            connections.Remove(c);
            Program.Log("Connection lost {0}", c.EndPoint);
        }
    }
}
