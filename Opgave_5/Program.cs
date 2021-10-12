using System;
using System.Net;

namespace Opgave_5 {
    public class Program {
        public static IPAddress IP = IPAddress.Any; //Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
        public static int port = 2121;

        public static void Main() {

            FootballPlayerServerSocket socket = new FootballPlayerServerSocket(IP, port);
            socket.Run();
        }
    }
}