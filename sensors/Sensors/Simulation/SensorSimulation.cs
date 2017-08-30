using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Text;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Protocol;
using Sensors.Utils;

namespace Sensors.Simulation
{
    public class SensorSimulation
    {
        public IMqttClient Mqtt { get; protected set; }

        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        public void Watch(Sensor sensor, TimeSpan interval)
        {

            _subscriptions.Add(Observable.Interval(interval)
                .Subscribe(x =>
                {
                    var value = sensor.Read();
                    var topic = $"sensor/{sensor.GetType().Name}".ToLower();
                    Mqtt.PublishAsync(
                        new MqttApplicationMessage(
                            retain: true,
                            topic: topic,
                            payload: Encoding.ASCII.GetBytes(value.ToString(CultureInfo.InvariantCulture)),
                            qualityOfServiceLevel: MqttQualityOfServiceLevel.ExactlyOnce
                        ));
                }));
        }

        public SensorSimulation()
        {
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