using ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Opgave_5 {
    public class FootballPlayerServerSocket {
        public TcpListener Server;

        public static List<FootballPlayer> _players = new List<FootballPlayer>( new FootballPlayer[3]{ new FootballPlayer(1, "Name", 100, 1), new FootballPlayer(2, "Anouther Name", 1000, 2), new FootballPlayer(3, "Yet Anouther Name", 10000, 3) } );

        public FootballPlayerServerSocket(IPAddress ip, int port) {
            Console.WriteLine("This is the server");

            Server = new TcpListener(ip, port);
        }

        public void Run() {
            Server.Start();

            Console.WriteLine("Server ready");
            while (true) {
                TcpClient client = Server.AcceptTcpClient();
                Console.WriteLine("Client connected");
                Task.Run(() => ServiceClient(client));
            }
        }

        private void ServiceClient(TcpClient client) {
            NetworkStream ns = client.GetStream();
            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);

            bool end = false;

            string message = "";
            while (!end) {

                if (message.Length > 0) {
                    message += "\n" + ReadLines(reader);
                } else {
                    message += ReadLines(reader);
                }

                if(message.Contains("\n")) {
                    try {
                        if (message.ToLower().Contains("getall") || message.ToLower().Contains("hentalle")) {
                            SendAll(writer);
                        } else if (message.ToLower().Contains("get") || message.ToLower().Contains("hent")) {
                            Send(writer, message.Split("\n")[1]);
                        } else if(message.ToLower().Contains("put") || message.ToLower().Contains("update")) {
                            Put(writer, message.Split("\n")[1]);
                        } else if(message.ToLower().Contains("add") || message.ToLower().Contains("tilføj")) {
                            Add(writer, message.Split("\n")[1]);
                        } else if(message.ToLower().Contains("delete") || message.ToLower().Contains("fjern")) {
                            Delete(writer, message.Split("\n")[1]);
                        } else {
                            writer.WriteLine("Didn't understand, try again.");
                        }

                        message = "";

                        writer.Flush();
                    } catch (Exception e) {
                        Console.WriteLine(e.Message);
                        if (e != null) {
                            end = true;
                            writer.WriteLine("Server connection severed");
                            writer.Flush();
                        }
                    }
                }
            }
            client.Close();
        }

        private void Delete(StreamWriter writer, string input) {
            FootballPlayer toDelete = null;
            foreach (FootballPlayer player in _players) {
                if(player.Name.ToLower().Contains(input.ToLower())) {
                    toDelete = player;
                    writer.WriteLine("You have deleted: " + ToJson(player));
                }
            }
            if(toDelete != null) {
                _players.Remove(toDelete);
            }
        }

        private void Add(StreamWriter writer, string input) {
            FootballPlayer player = FromJson(input);

            _players.Add(player);

            writer.WriteLine("You have added: " + ToJson(player));
        }

        private void Put(StreamWriter writer, string input) {
            string[] inputs = input.Split(";");

            FootballPlayer player = FromJson(inputs[1].Trim());

            FootballPlayer deleted = _players[int.Parse(inputs[0].Trim())-1];
            _players[int.Parse(inputs[0].Trim())-1] = player;

            writer.WriteLine("You have updated: " + ToJson(deleted) + " with " + ToJson(player));
        }

        private void Send(StreamWriter writer, string input) {
            foreach (FootballPlayer player in _players) {
                if (player.Name.ToLower().Contains(input.ToLower())) {
                    writer.WriteLine(ToJson(player));
                }
            }
        }

        private void SendAll(StreamWriter writer) {
            foreach(FootballPlayer player in _players) {
                writer.WriteLine(ToJson(player));
            }
        }

        private void WriteObject(string message) {
            try {
                FootballPlayer player = FromJson(message);

                Console.WriteLine("Server received: " + player.ToString());
            } catch (Exception e) { }
        }

        private string ReadLines(StreamReader Reader, string re = "") {
            //Console.WriteLine("Receiving: " + Reader.ReadLine());
            re += Reader.ReadLine();

            if (Reader.Peek() >= 0) {
                re = ReadLines(Reader, re);
            }

            return re;
        }

        private FootballPlayer FromJson(string messsage) {
            Object o = JsonSerializer.Deserialize(messsage, typeof(FootballPlayer));
            if (o is FootballPlayer) {
                FootballPlayer type = (FootballPlayer)o;
                return type;
            }
            throw new Exception("Could not create FootballPlayer from JSON");
        }

        private string ToJson(FootballPlayer player) {
            return JsonSerializer.Serialize(player, typeof(FootballPlayer));
        }
    }
}