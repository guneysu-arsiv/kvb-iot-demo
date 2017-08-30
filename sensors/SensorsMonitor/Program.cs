using System;
using System.Text;
using MQTTnet.Core.Client;
using MQTTnet.Core.Packets;
using MQTTnet.Core.Protocol;
using SensorsMonitor.Models;

namespace SensorsMonitor
{
    public class Program
    {
        /*
         * http://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices
         */
        public static void Main(string[] args)
        {
            var manager = new SensorMonitor();
            var controls = new SystemControls();
            
            var topics = new TopicFilter[]
            {
                new TopicFilter("sensor/#", MqttQualityOfServiceLevel.AtMostOnce),
            };

            manager.Mqtt.SubscribeAsync(topics).Wait(1000);

            manager.Mqtt.ApplicationMessageReceived += (object sender, MqttApplicationMessageReceivedEventArgs e) =>
            {
                // TODO Sensor values can be implemented as IObservable and processed with Rx.Linq

                var topic = e.ApplicationMessage.Topic;
                var sensorType = topic.Split('/')[1];
                SensorData data = null;
                
                float value = float.MinValue;
                
                if (topic.StartsWith("sensor/"))
                   value = float.Parse(Encoding.ASCII.GetString(e.ApplicationMessage.Payload));

                switch (topic)
                {
                    case "sensor/gas":
                    {
                        data = new GasSensorData()
                        {
                            DateTime = DateTime.Now,
                            Value = value,
                            Type = sensorType.ToUpper(),
                        };

//                        manager.ElasticSearch.Index(data);

                        break;
                    }
                    case "sensor/smoke":
                    {
                        data = new SmokeSensorData()
                        {
                            DateTime = DateTime.Now,
                            Value = value,
                            Type = sensorType.ToUpper(),
                        };

//                        manager.ElasticSearch.Index(data);

                        break;
                    }
                    default:
                        break;
                }
                
                if (value > 0.8)
                    controls.Alarm(data);

                Console.WriteLine(data);
            };

            Console.ReadLine();

            //            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
            //            {
            //                manager.StopAll();
            //            });
            //            Thread.Sleep(60);
        }
    }
}