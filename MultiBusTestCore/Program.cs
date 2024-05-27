using MassTransit;
using Microsoft.Extensions.DependencyInjection;
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


        private static readonly string region = "eu-central-1"; // Set this!! 
        private static readonly string accessKey = "mykey";    // Set this!!
        private static readonly string secretKey = "mysecret"; // Set this!!

        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();


            serviceCollection.AddMassTransit(busRegistrationConfigurator =>
            {
                string serverName = "server1";

                busRegistrationConfigurator.SetEndpointNameFormatter(
                    new KebabCaseEndpointNameFormatter(prefix: serverName, includeNamespace: false)); // this might not be needed as we set endpoint name directly.
                busRegistrationConfigurator
                    .AddConsumer<MyMessageConsumer>(cfg =>
                    {
                        cfg.UseConcurrencyLimit(1);
                    }).Endpoint(c =>
                    {
                        c.Name = $"{serverName}-queue";
                    });
                busRegistrationConfigurator.UsingAmazonSqs((context, sqsBusFactoryConfigurator) =>
                {
                    sqsBusFactoryConfigurator.Host(region, h =>
                    {
                        h.AccessKey(accessKey);
                        h.SecretKey(secretKey);
                    });
                    var nameFormatter = new BusEnvironmentNameFormatter(
                        sqsBusFactoryConfigurator.MessageTopology.EntityNameFormatter, serverName);
                    sqsBusFactoryConfigurator.MessageTopology.SetEntityNameFormatter(nameFormatter);
                    sqsBusFactoryConfigurator.ConfigureEndpoints(context);
                });
            } );

            // this code is an exact copy of the above code. Only changes are the <IBus2> and the changed serverName.
            // we are using the SAME consumer of MyMessageConsumer.
            serviceCollection.AddMassTransit<ISecondBus>(busRegistrationConfigurator =>
            {
                var server2Name = "server2";

                busRegistrationConfigurator.SetEndpointNameFormatter(
                    new KebabCaseEndpointNameFormatter(prefix: server2Name, includeNamespace: false)); // this might not be needed as we set endpoint name directly.
                busRegistrationConfigurator
                    .AddConsumer<MyMessageConsumer>(cfg =>
                    {
                        cfg.UseConcurrencyLimit(1);
                    }).Endpoint(c =>
                    {
                        c.Name = $"{server2Name}-queue";
                    });
                busRegistrationConfigurator.UsingAmazonSqs((context, sqsBusFactoryConfigurator) =>
                {
                    sqsBusFactoryConfigurator.Host(region, h =>
                    {
                        h.AccessKey(accessKey);
                        h.SecretKey(secretKey);
                    });
                    var nameFormatter = new BusEnvironmentNameFormatter(
                        sqsBusFactoryConfigurator.MessageTopology.EntityNameFormatter, server2Name);
                    sqsBusFactoryConfigurator.MessageTopology.SetEntityNameFormatter(nameFormatter);
                    sqsBusFactoryConfigurator.ConfigureEndpoints(context);
                });
            });

            var sp = serviceCollection.BuildServiceProvider();
            var GlobalBusController = sp.GetService<IBusControl>()!;
            var bus1 = sp.GetService<IBus>();
            var bus2 = sp.GetService<ISecondBus>();
            await GlobalBusController.StartAsync();
            Console.Write("Started servers. Press Enter to shut down.");
            Console.ReadLine();
            await GlobalBusController.StopAsync();
            // What SHOULD have been created:
            //      Topic: server1-topic-MyMessage
            //      Topic: server2-topic-MyMessage
            //      Queue: server1-queue (with subscription to server1-topic-MyMessage)
            //      Queue: server2-queue (with subscription to server2-topic-MyMessage)
            
            // What was ACTUALLY created:
            //      Topic: server1-topic-MyMessage
            //      Queue: server2-queue (with subscription to server1-topic-MyMessage)
        }
    }
}
