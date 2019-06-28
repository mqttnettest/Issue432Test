using System;
using System.Threading;
using log4net;
using MQTTnet.Diagnostics;

namespace ClassLibrary1
{
    public class CustomMqttNetLogger : IMqttNetLogger
    {
        private static readonly ILog Log = LogManager.GetLogger("MqttCommunicationLogger");

        public event EventHandler<MqttNetLogMessagePublishedEventArgs> LogMessagePublished;

        public string LogId { get; private set; }

        public CustomMqttNetLogger(string logId)
        {
            LogId = logId;
        }

        public IMqttNetChildLogger CreateChildLogger(string source = null)
        {
            return new MqttNetChildLogger(this, source);
        }

        public void Publish(MqttNetLogLevel logLevel, string source, string message, object[] parameters, Exception exception)
        {
            MqttNetLogMessage logMessage;
            switch (logLevel)
            {
                case MqttNetLogLevel.Verbose:
                    if (exception == null)
                    {
                        Log.DebugFormat("[{0}] - [{1}]: {2}", source, LogId, string.Format(message, parameters));
                    }
                    else
                    {
                        Log.DebugFormat("[{0}] - [{1}]: {2}\r\n{3}", source, LogId, string.Format(message, parameters), exception);
                    }
                    logMessage = new MqttNetLogMessage(LogId, DateTime.Now, Thread.CurrentThread.ManagedThreadId, source, MqttNetLogLevel.Verbose, string.Format(message, parameters), exception);
                    break;
                case MqttNetLogLevel.Info:
                    if (exception == null)
                    {
                        Log.InfoFormat("[{0}] - [{1}]: {2}", source, LogId, string.Format(message, parameters));
                    }
                    else
                    {
                        Log.InfoFormat("[{0}] - [{1}]: {2}\r\n{3}", source, LogId, string.Format(message, parameters), exception);
                    }
                    logMessage = new MqttNetLogMessage(LogId, DateTime.Now, Thread.CurrentThread.ManagedThreadId, source, MqttNetLogLevel.Info, string.Format(message, parameters), exception);
                    break;
                case MqttNetLogLevel.Warning:
                    if (exception == null)
                    {
                        Log.WarnFormat("[{0}] - [{1}]: {2}", source, LogId, string.Format(message, parameters));
                    }
                    else
                    {
                        Log.WarnFormat("[{0}] - [{1}]: {2}\r\n{3}", source, LogId, string.Format(message, parameters), exception);
                    }
                    logMessage = new MqttNetLogMessage(LogId, DateTime.Now, Thread.CurrentThread.ManagedThreadId, source, MqttNetLogLevel.Warning, string.Format(message, parameters), exception);
                    break;
                case MqttNetLogLevel.Error:
                    if (exception == null)
                    {
                        Log.ErrorFormat("[{0}] - [{1}]: {2}", source, LogId, string.Format(message, parameters));
                    }
                    else
                    {
                        Log.ErrorFormat("[{0}] - [{1}]: {2}\r\n{3}", source, LogId, string.Format(message, parameters), exception);
                    }
                    logMessage = new MqttNetLogMessage(LogId, DateTime.Now, Thread.CurrentThread.ManagedThreadId, source, MqttNetLogLevel.Error, string.Format(message, parameters), exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
            }
            if (LogMessagePublished != null) LogMessagePublished(this, new MqttNetLogMessagePublishedEventArgs(logMessage));
        }
    }
}
