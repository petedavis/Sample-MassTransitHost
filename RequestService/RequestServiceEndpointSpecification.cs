namespace RequestService
{
    using System;
    using MassTransit;
    using MassTransit.Hosting;

    /// <summary>
    /// Configures an endpoint for the assembly
    /// </summary>
    public class RequestServiceEndpointSpecification :
        IEndpointSpecification
    {
        /// <summary>
        /// The default queue name for the endpoint, which can be overridden in the .config
        /// file for the assembly
        /// </summary>
        public string QueueName
        {
            get { return "input_queue"; }
        }

        /// <summary>
        /// The default concurrent consumer limit for the endpoint, which can be overridden in the .config
        /// file for the assembly
        /// </summary>
        public int ConsumerLimit
        {
            get { return Environment.ProcessorCount; }
        }

        /// <summary>
        /// Configures the endpoint, with consumers, handlers, sagas, etc.
        /// </summary>
        public void Configure(IReceiveEndpointConfigurator configurator)
        {
            // message consumers, middleware, etc. are configured here
            configurator.Consumer<RequestConsumer>();
        }
    }
}
