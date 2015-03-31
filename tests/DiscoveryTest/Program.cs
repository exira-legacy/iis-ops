namespace DiscoveryTest
{
    using System.Net;
    using Exira.EventStore;
    using Exira.EventStore.Owin;
    using Exira.IIS.Domain;
    using Owin;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            //var a = EventStore.connect(new Configuration(IPAddress.Parse("127.0.0.1"), ServerPort.NewServerPort(1113), "admin", "changeit"));

            //var b = new EventStoreOptions
            //{
            //    Configuration = new Configuration(IPAddress.Parse("127.0.0.1"), ServerPort.NewServerPort(1113), "admin", "changeit")
            //};

            //var c = ((IAppBuilder) null).UseEventStore(b);

            //var d = CommandHandler.parseCommand(5);
            //Console.WriteLine(d.IsFailure);
        }
    }
}
