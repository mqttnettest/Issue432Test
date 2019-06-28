using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using log4net;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Implementations;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace ClassLibrary1
{
    public class Broker
    {
        private readonly MqttFactory _factory;
        private IMqttServer _mqttServer;
        private static readonly ILog Log = LogManager.GetLogger("MqttCommunicationLogger");

        public bool BrokerIsLaunched { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Broker()
        {
            _factory = new MqttFactory();
            _mqttServer = _factory.CreateMqttServer();
            BrokerIsLaunched = false;
        }

        /// <summary>
        /// Start the broker
        /// </summary>
        /// <param name="ipAddress">IP address of the broker</param>
        /// <param name="port">Port for the broker</param>
        public async Task Start(string ipAddress, int port)
        {
            _mqttServer = _factory.CreateMqttServer(new List<IMqttServerAdapter> { new MqttTcpServerAdapter(new CustomMqttNetLogger("Broker").CreateChildLogger()) }, new CustomMqttNetLogger("Broker"));

            IMqttServerOptions options = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(port)
                .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(ipAddress))
                .WithDefaultCommunicationTimeout(new TimeSpan(0, 0, 15))
                .WithConnectionBacklog(100)
                .Build();

            Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, "Broker", "Starting MQTT broker.");

            await _mqttServer.StartAsync(options);
            BrokerIsLaunched = true;

            Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, "Broker", "MQTT broker started.");
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public async Task Stop()
        {
            Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, "Broker", "Stopping MQTT broker.");

            await _mqttServer.StopAsync();
            BrokerIsLaunched = false;

            Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, "Broker", "MQTT broker stopped.");
        }

        public async Task Send(string topic, string payload)
        {
            await _mqttServer.PublishAsync(topic, payload, MqttQualityOfServiceLevel.AtMostOnce);
        }
    }
}
