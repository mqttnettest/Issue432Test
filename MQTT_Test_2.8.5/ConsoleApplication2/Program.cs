using System;
using System.IO;
using System.Threading;
using ClassLibrary1;
using log4net;
using log4net.Config;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load file Log4net.config
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            string clientName = "toto";
            string topic = "toto/a/la/plage";

            Thread.Sleep(5000);

            Client client1 = new Client();
            client1.Connect(clientName + "1", topic);
            Console.WriteLine("Client 1 connected.");
            client1.MessageReceived += (sender, eventArgs) => { Console.WriteLine("Message received on client 1"); };

            client1.Begin(topic);
            Console.WriteLine("Client 1 subscription at toto/a/la/plage");

            Thread.Sleep(1000);

            Client client2 = new Client();
            client2.Connect(clientName + "2", topic);
            Console.WriteLine("Client 2 connected.");
            client2.Begin(topic);
            Console.WriteLine("Client 2 subscription at toto/a/la/plage");
            client2.MessageReceived += (sender, eventArgs) =>
            {
                Console.WriteLine("Message received on client 2");
                Console.WriteLine("Ask client 2 unsubscription at toto/a/la/plage");
                client2.End(topic);
                Console.WriteLine("Client 2 unsubscription at toto/a/la/plage");
            };//*/

            Thread.Sleep(25000);
            Console.WriteLine("Ask client 1 unsubscription at toto/a/la/plage");
            client1.End(topic);
            Console.WriteLine("Client 1 unsubscription at toto/a/la/plage");//*/

            Console.ReadKey();

            LogManager.Shutdown();
        }
    }
}
