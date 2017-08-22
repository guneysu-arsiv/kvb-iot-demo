using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Text;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Protocol;

namespace Sensors
{
    public class SensorManager
    {
        public IMqttClient Mqtt { get; protected set; }
        public ElasticSearch<SensorData> ElasticSearch { get; protected set; }

        List<IDisposable> _subscriptions = new List<IDisposable>();
        List<Sensor> _sensors = new List<Sensor>();

        public void Watch(Sensor sensor, TimeSpan interval)
        {
            _sensors.Add(sensor);

            _subscriptions.Add(Observable.Interval(interval)
                .Subscribe(x =>
                {
                    var value = sensor.Read();
                    Mqtt.PublishAsync(
                        new MqttApplicationMessage(
                            retain: true,
                            topic: $"sensor/{sensor.GetType().Name}".ToLower(),
                            payload: Encoding.ASCII.GetBytes(value.ToString(CultureInfo.InvariantCulture)),
                            qualityOfServiceLevel: MqttQualityOfServiceLevel.ExactlyOnce
                        ));
                }));
        }

        public SensorManager()
        {
            Mqtt = new MqttClientFactories().CloudMqtt();
            string conn = Environment.GetEnvironmentVariable("ELASTICSEARCH_URI");
            string index = Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX");
            
            ElasticSearch = new ElasticSearch<SensorData>(conn, index);
            Mqtt.ConnectAsync().Wait(1000);
        }

        public void StopAll()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }
}