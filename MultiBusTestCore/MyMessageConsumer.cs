using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiBusTest
{
    public class MyMessageConsumer : IConsumer<MyMessage>
    {
        async Task IConsumer<MyMessage>.Consume(ConsumeContext<MyMessage> context)
        {
            Console.WriteLine($"Received Message from server {context.Message.Server}, msg: {context.Message.MsgData}");
            await Task.CompletedTask;
        }
    }
}
