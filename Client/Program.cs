namespace Client
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using log4net.Config;
    using MassTransit;
    using MassTransit.Log4NetIntegration.Logging;
    using MassTransit.AzureServiceBusTransport;
    using Microsoft.ServiceBus;
    using Sample.MessageTypes;


    class Program
    {
        static void Main()
        {
            ConfigureLogger();

            // MassTransit to use Log4Net
            Log4NetLogger.Use();

            IBusControl busControl = CreateBus();

            busControl.Start();

            try
            {
                IRequestClient<ISimpleRequest, ISimpleResponse> client = CreateRequestClient(busControl);

                for (;;)
                {
                    Console.Write("Enter customer id (quit exits): ");
                    string customerId = Console.ReadLine();
                    if (customerId == "quit")
                        break;

                    // this is run as a Task to avoid weird console application issues
                    Task.Run(async () =>
                    {
                        ISimpleResponse response = await client.Request(new SimpleRequest(customerId));

                        Console.WriteLine("Customer Name: {0}", response.CusomerName);
                    }).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!!! OMG!!! {0}", ex);
            }
            finally
            {
                busControl.Stop();
            }
        }


        static IRequestClient<ISimpleRequest, ISimpleResponse> CreateRequestClient(IBusControl busControl)
        {
            var serviceAddress = new Uri(ConfigurationManager.AppSettings["ServiceAddress"]);
            IRequestClient<ISimpleRequest, ISimpleResponse> client =
                busControl.CreateRequestClient<ISimpleRequest, ISimpleResponse>(serviceAddress, TimeSpan.FromSeconds(10));

            return client;
        }

        static IBusControl CreateBus()
        {
            var serviceUri = ServiceBusEnvironment.CreateServiceUri("sb",
                    ConfigurationManager.AppSettings["ServiceBusNamespace"], "request_client");

            return Bus.Factory.CreateUsingAzureServiceBus(x => x.Host(serviceUri, h =>
            {
                h.SharedAccessSignature(s =>
                {
                    s.KeyName = ConfigurationManager.AppSettings["ServiceBusKeyName"];
                    s.SharedAccessKey = ConfigurationManager.AppSettings["ServiceBusSharedAccessKey"];
                });
            }));
        }

        static void ConfigureLogger()
        {
            const string logConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<log4net>
  <root>
    <level value=""DEBUG"" />
    <appender-ref ref=""console"" />
  </root>
  <appender name=""console"" type=""log4net.Appender.ColoredConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""%m%n"" />
    </layout>
  </appender>
</log4net>";

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logConfig)))
            {
                XmlConfigurator.Configure(stream);
            }
        }


        class SimpleRequest :
            ISimpleRequest
        {
            readonly string _customerId;
            readonly DateTime _timestamp;

            public SimpleRequest(string customerId)
            {
                _customerId = customerId;
                _timestamp = DateTime.UtcNow;
            }

            public DateTime Timestamp
            {
                get { return _timestamp; }
            }

            public string CustomerId
            {
                get { return _customerId; }
            }
        }
    }
}