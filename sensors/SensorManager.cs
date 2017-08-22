using System;
using System.Collections.Generic;
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
        public ElasticSearchClient<SensorData> ElasticSearch { get; protected set; }

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
                            payload: Encoding.ASCII.GetBytes(value.ToString()),
                            qualityOfServiceLevel: MqttQualityOfServiceLevel.ExactlyOnce
                        ));
                }));
        }

        public SensorManager()
        {
            Mqtt = new MqttClientFactories().CloudMqtt();
            ElasticSearch = new ElasticSearchClient<SensorData>("http://elastic:changeme@192.168.1.20:9200", "iot");
            Mqtt.ConnectAsync().Wait(1000);
        }

        public void StopAll()
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }
}