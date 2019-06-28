using System;
using System.Collections.Generic;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;

namespace ClassLibrary1
{
    using System.Threading;
    using MQTTnet.Client.Options;
    using MQTTnet.Client.Receiving;
    using MQTTnet.Client.Unsubscribing;

    public class Client
    {
        private readonly MqttFactory _factory;
        private IMqttClient _mqttClient;
        private static readonly ILog Log = LogManager.GetLogger("MqttCommunicationLogger");
        private string _clientName;
        public event EventHandler MessageReceived;

        public Client()
        {
            _factory = new MqttFactory();
            _mqttClient = _factory.CreateMqttClient();
        }

        public void Connect(string clientName, string channel)
        {
            if (_mqttClient.IsConnected) return;

            _clientName = clientName;
            _mqttClient = _factory.CreateMqttClient(new CustomMqttNetLogger(_clientName));

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(new Action<MqttClientConnectedEventArgs>(OnConnected));
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(new Action<MqttClientDisconnectedEventArgs>(OnDisconnected));
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(OnMessageReceived));

            IMqttClientOptions options = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1", 1883)
                .WithClientId(clientName)
                /*.WithWillMessage(new MqttApplicationMessage
                {
                    Topic = "will/message/topic",
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce,
                    Payload = new byte[] { },
                    Retain = true
                })*/
                .WithKeepAlivePeriod(new TimeSpan(0, 0, 15))
                .WithCommunicationTimeout(new TimeSpan(0, 0, 10))
                .WithCleanSession()
                .Build();

            try
            {
                Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, _clientName, string.Format("Connecting to broker at {0}:{1}.", "127.0.0.1", "1883"));
                _mqttClient.ConnectAsync(options).Wait();
            }
            catch (Exception e)
            {
                _mqttClient.ConnectedHandler = null;
                _mqttClient.DisconnectedHandler = null;
                _mqttClient.ApplicationMessageReceivedHandler = null;
                Log.Error("Connection error:", e);
            }
        }

        public void Begin(string channel)
        {
            if (_mqttClient.IsConnected)
            {
                _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(channel).WithAtMostOnceQoS().Build()).Wait();
            }
        }

        public void Send(string channel, byte[] payload, bool retained)
        {
            if (_mqttClient.IsConnected)
            {
                MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                    .WithTopic(channel)
                    .WithPayload(payload)
                    .WithAtMostOnceQoS()
                    .WithRetainFlag(retained)
                    .Build();

                _mqttClient.PublishAsync(message).Wait();
            }
        }

        public void End(string channel)
        {
            if (!_mqttClient.IsConnected) return;
            _mqttClient.UnsubscribeAsync(
                new MqttClientUnsubscribeOptions() { TopicFilters = new List<string> { channel } },
                CancellationToken.None).Wait();
        }

        public void Disconnect()
        {
            if (!_mqttClient.IsConnected) return;
            _mqttClient.DisconnectAsync().Wait();
        }

        private async void OnConnected(MqttClientConnectedEventArgs mqttClientConnectedEventArgs)
        {
            Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, _clientName, "Connected.");
        }

        private async  void OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            Log.DebugFormat("[{0}] - [{1}]: {2}", GetType().Name, _clientName, string.Format("Message received on topic {0} with payload {1}.", e.ApplicationMessage.Topic, BitConverter.ToString(e.ApplicationMessage.Payload)));
            if (MessageReceived != null) MessageReceived(this, null);
        }

        private async void OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            _mqttClient.ConnectedHandler = null;
            _mqttClient.DisconnectedHandler = null;
            _mqttClient.ApplicationMessageReceivedHandler = null;

            if (e.ClientWasConnected == false || e.Exception != null)
            {
                Log.InfoFormat("[{0}] - [{1}]: {2}\r\n{3}", GetType().Name, _clientName, "Disconnected.", e.Exception);
            }
            else
            {
                Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, _clientName, "Disconnected.");
            }
        }
    }
}
