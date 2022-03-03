namespace ServerUnity
{
    using System.Threading;
    using ServerUnity.Network;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            TcpServer.Listen("192.168.1.15", 8888);
            Thread.Sleep(-1);
        }
    }
}