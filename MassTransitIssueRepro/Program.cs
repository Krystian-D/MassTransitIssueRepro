using Consumer;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using Messages;
using System;
using System.Threading.Tasks;

namespace MassTransitIssueRepro
{
    class Program
    {
        const string ConnectionString = ""; //Set connection string

        static void Main(string[] args)
        {
            try
            {
                var bus = CreateBus();
                bus.Start();

                //for (int i = 0; i < 10; i++)
                //{
                //    bus.Publish(new EventMessage { SessionId = 1, Order = i }).Wait();
                //}

                Console.WriteLine("Done");
                Console.ReadLine();

                bus.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


        }

        static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingAzureServiceBus(configurator =>
            {
                var host = configurator.Host(ConnectionString, hostConfigurator =>
                {

                });

                configurator.SubscriptionEndpoint<EventMessage>(host, $"sessionsubscription", endpoint =>
                {
                    endpoint.RequiresSession = true;
                    endpoint.Consumer<EventMessageConsumer>();
                });


                configurator.Send<EventMessage>(config =>
                {
                    config.UseSessionIdFormatter(x =>
                    {
                        return x.Message.SessionId.ToString();
                    });
                });

            });
        }
    }
}

namespace Messages
{
    public class EventMessage
    {
        public int SessionId { get; set; }

        public int Order { get; set; }
    }

}

namespace Consumer
{

    public class EventMessageConsumer : IConsumer<EventMessage>
    {
        public Task Consume(ConsumeContext<EventMessage> context)
        {
            var message = context.Message;
            Console.WriteLine($"Received {message} Order:{message.SessionId} {message.Order}");
            return Task.CompletedTask;
        }
    }

}
