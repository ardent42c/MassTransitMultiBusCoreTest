using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBusTest
{
    public interface ISecondBus : IBus { }

    internal class Program
    {

        // this example uses the same region, accessKey and secretKey for all servers, but obviously, each server can get different ones.
        private static readonly string region = "eu-central-1"; // Set this!! 
        private static readonly string accessKey = "mykey";    // Set this!!
        private static readonly string secretKey = "mysecret"; // Set this!!

        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            var servers = new string[] { "server1", "server2", "server3" };

            foreach (var server in servers)
            {
                // serverName is used to register one of the IBusX types to this name and is also sent to the bus configurator.
                // serverData can be used to pass any data to lazy called bus configurator.
                serviceCollection.AddMassTransit(serverName: server, serverData: null, 
                    (busRegistrationConfigurator, serverName, data) =>
                {
                    busRegistrationConfigurator
                        .AddConsumer<MyMessageConsumer>(cfg =>
                        {
                            cfg.UseConcurrencyLimit(1);
                        }); 
                    busRegistrationConfigurator.UsingAmazonSqs((context, sqsBusFactoryConfigurator) =>
                    {
                        sqsBusFactoryConfigurator.LocalstackHost();
                        sqsBusFactoryConfigurator.Host(region, h =>
                        {
                            h.AccessKey(accessKey);
                            h.SecretKey(secretKey);
                        });
                        var nameFormatter = new BusEnvironmentNameFormatter(
                            sqsBusFactoryConfigurator.MessageTopology.EntityNameFormatter, serverName);
                        sqsBusFactoryConfigurator.MessageTopology.SetEntityNameFormatter(nameFormatter);
                        sqsBusFactoryConfigurator.ConfigureEndpoints(context,
                            new KebabCaseEndpointNameFormatter(prefix: $"{serverName}-queue", includeNamespace: false));
                    });
                });
            }

            var sp = serviceCollection.BuildServiceProvider();
            List<IBus> busses = new List<IBus>();
            foreach (var server in servers)
            {
                var bus = sp.GetMassTransitBus(server);
                if (bus != null)
                    busses.Add(bus);
            }
            var hostedServices = sp.GetServices<IHostedService>();
            var cancelToken = new CancellationToken();
            foreach (var service in hostedServices)
            {
                Console.WriteLine($"starting service {service.GetType().Name}");
                await service.StartAsync(cancelToken);
            }

            Console.Write("Started servers. Press Enter to shut down.");
            Console.ReadLine();
            foreach (var service in hostedServices)
                await service.StopAsync(cancelToken);

            // What SHOULD be created (now works):
            //      Topic: server1-topic-MyMessage
            //      Topic: server2-topic-MyMessage
            //      Topic: server3-topic-MyMessage
            //      Queue: server1-queue-MyMessage (with subscription to server1-topic-MyMessage)
            //      Queue: server2-queue-MyMessage (with subscription to server2-topic-MyMessage)
            //      Queue: server4-queue-MyMessage (with subscription to server3-topic-MyMessage)
        }
    }
}
