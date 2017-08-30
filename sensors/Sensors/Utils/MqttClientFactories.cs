using System;
using MQTTnet;
using MQTTnet.Core.Client;

namespace Sensors.Utils
{
    public class MqttClientFactories
    {
        public IMqttClient CloudMqtt ( )
        {
            return new MqttClientFactory ( ).CreateMqttClient (new MqttClientOptions
            {
                ClientId = Guid.NewGuid ( ).ToString ( ),
                Port = 12662,
                UserName = "vihvzpqe",
                Password = "MJN_Kj7uz1lB",
                Server = "m20.cloudmqtt.com",
            });
        }

        public IMqttClient Eclipse ( )
        {
            return new MqttClientFactory ( ).CreateMqttClient (new MqttClientOptions
            {
                ClientId = Guid.NewGuid ( ).ToString ( ),
                Port = 1883,
                Server = "iot.eclipse.org",
            });
        }
    }
}