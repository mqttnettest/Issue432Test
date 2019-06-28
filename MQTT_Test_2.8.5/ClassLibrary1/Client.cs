using System;
using System.Collections.Generic;
using log4net;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace ClassLibrary1
{
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

            _mqttClient.Connected += OnConnected;
            _mqttClient.Disconnected += OnDisconnected;
            _mqttClient.ApplicationMessageReceived += OnMessageReceived;

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
                _mqttClient.Connected -= OnConnected;
                _mqttClient.Disconnected -= OnDisconnected;
                _mqttClient.ApplicationMessageReceived -= OnMessageReceived;
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
            _mqttClient.UnsubscribeAsync(new List<string> { channel }).Wait();
        }

        public void Disconnect()
        {
            if (!_mqttClient.IsConnected) return;
            _mqttClient.DisconnectAsync().Wait();
        }

        private void OnConnected(object sender, MqttClientConnectedEventArgs mqttClientConnectedEventArgs)
        {
            Log.InfoFormat("[{0}] - [{1}]: {2}", GetType().Name, _clientName, "Connected.");
        }

        private void OnMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            Log.DebugFormat("[{0}] - [{1}]: {2}", GetType().Name, _clientName, string.Format("Message received on topic {0} with payload {1}.", e.ApplicationMessage.Topic, BitConverter.ToString(e.ApplicationMessage.Payload)));
            if (MessageReceived != null) MessageReceived(this, null);
        }

        private void OnDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            _mqttClient.Connected -= OnConnected;
            _mqttClient.Disconnected -= OnDisconnected;
            _mqttClient.ApplicationMessageReceived -= OnMessageReceived;

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
