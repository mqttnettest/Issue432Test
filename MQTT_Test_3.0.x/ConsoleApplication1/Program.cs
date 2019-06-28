using System;
using System.IO;
using System.Threading.Tasks;
using ClassLibrary1;
using log4net;
using log4net.Config;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load file Log4net.config
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            try
            {
                Broker broker = new Broker();

                broker.Start("127.0.0.1", 1883).Wait();

                Console.WriteLine("Broker started.");

                Task.Delay(30000).Wait();

                broker.Send("toto/a/la/plage", "Hello world!").Wait();

                Console.WriteLine("Message sended by broker on topic toto/a/la/plage.");

                Task.Delay(2000).Wait();

                //broker.Stop().Wait();

                Task.Delay(1000).Wait();

                Console.WriteLine("Finish !");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
            Console.ReadKey();

            LogManager.Shutdown();
        }
    }
}
