using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MultiBusTest
{
    public static class MassTransitExtensions
    {

        public static IBus? GetMassTransitBus(this IServiceProvider serviceProvider, string serverName)
        {
            var serverNum = GetServerNum(serverName);
            if (serverNum < 0)
                return null;
            switch (serverNum)
            {
                case 0:
                    return serviceProvider.GetService<IBus>();
                case 1:
                    return serviceProvider.GetService<IBus1>();
                case 2:
                    return serviceProvider.GetService<IBus2>();
                case 3:
                    return serviceProvider.GetService<IBus3>();
                case 4:
                    return serviceProvider.GetService<IBus4>();
                case 5:
                    return serviceProvider.GetService<IBus5>();
                case 6:
                    return serviceProvider.GetService<IBus6>();
                case 7:
                    return serviceProvider.GetService<IBus7>();
                case 8:
                    return serviceProvider.GetService<IBus8>();
                case 9:
                    return serviceProvider.GetService<IBus9>();
            }
            return null;
        }

        public static IServiceCollection AddMassTransit(this IServiceCollection collection, string serverName, object? serverData,
            Action<IBusRegistrationConfigurator, string, object> configure)
        {
            var serverNum = GetServerNum(serverName, true);
            switch (serverNum)
            {
                case 0:
                    collection.AddMassTransit(cfg => configure(cfg, serverName, serverData));
                    break;
                case 1:
                    collection.AddMassTransit<IBus1>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 2:
                    collection.AddMassTransit<IBus2>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 3:
                    collection.AddMassTransit<IBus3>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 4:
                    collection.AddMassTransit<IBus4>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 5:
                    collection.AddMassTransit<IBus5>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 6:
                    collection.AddMassTransit<IBus6>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 7:
                    collection.AddMassTransit<IBus7>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 8:
                    collection.AddMassTransit<IBus8>(cfg => configure(cfg, serverName, serverData));
                    break;
                case 9:
                    collection.AddMassTransit<IBus9>(cfg => configure(cfg, serverName, serverData));
                    break;
            }
            return collection;
        }


        private static int GetServerNum(string serverName, bool makeNew = false)
        {
            serverName = serverName.ToUpperInvariant();
            if (BusNamesToNum.TryGetValue(serverName, out int serverNum))
                return serverNum;
            if (!makeNew)
                return -1;
            serverNum = Interlocked.Increment(ref _LastServerNum);
            if (serverNum >= _MaxBusses)
                return -1;
            BusNamesToNum[serverName] = serverNum;
            return serverNum;
        }

        private static ConcurrentDictionary<string, int> BusNamesToNum = new ConcurrentDictionary<string, int>();
        private static int _LastServerNum = -1;
        private const int _MaxBusses = 10;

    }

    public interface IBus1 : IBus { };
    public interface IBus2 : IBus { };
    public interface IBus3 : IBus { };
    public interface IBus4 : IBus { };
    public interface IBus5 : IBus { };
    public interface IBus6 : IBus { };
    public interface IBus7 : IBus { };
    public interface IBus8 : IBus { };
    public interface IBus9 : IBus { };
}
