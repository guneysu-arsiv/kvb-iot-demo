using System;
using System.Text;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Packets;
using MQTTnet.Core.Protocol;

namespace Sensors
{
    public class Program
    {
        /*
         * http://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices
         */
        public static void Main(string[] args)
        {
            var manager = new SensorManager();

            var topics = new TopicFilter[]
            {
                new TopicFilter("sensor/#", MqttQualityOfServiceLevel.AtMostOnce),
                new TopicFilter("alarm/#", MqttQualityOfServiceLevel.AtLeastOnce),
            };

            manager.Mqtt.SubscribeAsync(topics).Wait(1000);

            manager.Mqtt.ApplicationMessageReceived += async (object sender, MqttApplicationMessageReceivedEventArgs e) =>
            {
                // TODO Sensor values can be implemented as IObservable and processed with Rx.Linq

                var topic = e.ApplicationMessage.Topic;
                var sensorType = topic.Split('/')[1];

                float value = float.MinValue;
                
                if (topic.StartsWith("sensor/"))
                   value = float.Parse(Encoding.ASCII.GetString(e.ApplicationMessage.Payload));

                if (value > 0.8)
                {
                    var payload = Encoding.ASCII.GetBytes(DateTime.Now.ToShortTimeString());
                    
                    var alarm = new MqttApplicationMessage(
                        retain: true,
                        topic: $"alarm/{sensorType}",
                        payload: payload,
                        qualityOfServiceLevel: MqttQualityOfServiceLevel.AtLeastOnce
                    );

                    await manager.Mqtt.PublishAsync(alarm);
                }

                switch (topic)
                {
                    case "sensor/gas":
                    {
                        var data = new GasSensorData()
                        {
                            DateTime = DateTime.Now,
                            Value = value,
                            Type = sensorType.ToUpper(),
                        };

                        manager.ElasticSearch.Index(data);

                        break;
                    }
                    case "sensor/smoke":
                    {
                        var data = new SmokeSensorData()
                        {
                            DateTime = DateTime.Now,
                            Value = value,
                            Type = sensorType.ToUpper(),
                        };

                        manager.ElasticSearch.Index(data);

                        break;
                    }
                    default:
                        break;
                }

                Console.WriteLine($"{e.ApplicationMessage.Topic}: {Encoding.ASCII.GetString(e.ApplicationMessage.Payload)}");
            };

            manager.Watch(new Gas(min: 0.05, max: 0.95), TimeSpan.FromMilliseconds(1000));
            manager.Watch(new Smoke(min: 0.05, max: 0.95), TimeSpan.FromMilliseconds(1000));

            Console.ReadLine();

            //            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
            //            {
            //                manager.StopAll();
            //            });
            //            Thread.Sleep(60);
        }
    }
}