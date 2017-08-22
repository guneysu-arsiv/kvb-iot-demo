using System;
using System.Text;
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
            };

            manager.Mqtt.SubscribeAsync(topics).Wait(1000);

            manager.Mqtt.ApplicationMessageReceived += (object sender, MqttApplicationMessageReceivedEventArgs e) =>
            {
                var sensorType = e.ApplicationMessage.Topic.Split('/')[1];
                var value = float.Parse(Encoding.ASCII.GetString(e.ApplicationMessage.Payload));
                
                switch (sensorType)
                {
                    case "gas":
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
                    case "smoke":
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
                
                var msg = float.Parse(Encoding.ASCII.GetString(e.ApplicationMessage.Payload));

                Console.WriteLine($"{e.ApplicationMessage.Topic}: {msg}");
//                manager.ElasticSearch.Index()
            };

            manager.Watch(new Gas(min: 0.05, max: 0.951), TimeSpan.FromMilliseconds(1000));
            manager.Watch(new Smoke(min: 0.05, max: 0.951), TimeSpan.FromMilliseconds(1000));

            Console.ReadLine();

            //            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
            //            {
            //                manager.StopAll();
            //            });
            //            Thread.Sleep(60);
        }
    }

    public class SensorData
    {
        public DateTime DateTime { get; set; }
        public float Value { get; set; }
        public string Type  { get; set; }
    }

    public class GasSensorData : SensorData
    {
    }
    
    public class SmokeSensorData : SensorData
    {
    }
}
