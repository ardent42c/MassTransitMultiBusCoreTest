using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBusTest
{
    public class BusEnvironmentNameFormatter : IEntityNameFormatter
    {
        private readonly string _prefix;

        public BusEnvironmentNameFormatter(IEntityNameFormatter original, string server)
        {
            _prefix = $"{server}-topic-";
        }

        public string FormatEntityName<T>()
        {
            Console.WriteLine($"NameFormatter {_prefix}{typeof(T).Name}");
            return _prefix + typeof(T).Name;
        }

    }
}
