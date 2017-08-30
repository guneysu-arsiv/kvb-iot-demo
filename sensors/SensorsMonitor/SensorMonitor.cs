using System;
using System.Collections.Generic;
using MQTTnet.Core.Client;
using SensorsMonitor.DataAccess;
using SensorsMonitor.Models;
using SensorsMonitor.Utils;

namespace SensorsMonitor
{
    public class SensorMonitor
    {
        public IMqttClient Mqtt { get; protected set; }
        public ElasticSearch<SensorData> ElasticSearch { get; protected set; }

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public SensorMonitor()
        {
            /*
            string conn = Environment.GetEnvironmentVariable("ELASTICSEARCH_URI");
            string index = Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX");
            ElasticSearch = new ElasticSearch<SensorData>(conn, index);
            */
            
            Mqtt = new MqttClientFactories().CloudMqtt();
            Mqtt.ConnectAsync().Wait(1000);
        }

        public void StopAll()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }
}