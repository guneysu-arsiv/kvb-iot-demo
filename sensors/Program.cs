using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Elasticsearch.Net;
using MQTTnet;
using MQTTnet.Core;
using MQTTnet.Core.Client;
using MQTTnet.Core.Packets;
using MQTTnet.Core.Protocol;
using Nest;

namespace Sensors
{
    internal class Program
    {
        /*
         * http://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices
         */
        public static void Main (string [] args)
        {
            var manager = new SensorManager ( );

            manager.Mqtt.SubscribeAsync (new TopicFilter [] {
                //new TopicFilter ("$SYS/#", MqttQualityOfServiceLevel.AtMostOnce),
                new TopicFilter ("sensor/#", MqttQualityOfServiceLevel.AtMostOnce),
            }).Wait (1000);

            manager.Mqtt.ApplicationMessageReceived += Client_ApplicationMessageReceived;

            manager.Watch (new Gas (min: 0.05, max: 0.951), TimeSpan.FromMilliseconds (1000));
            manager.Watch (new Smoke (min: 0.05, max: 0.951), TimeSpan.FromMilliseconds (1000));

            Console.ReadLine ( );

            //            Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
            //            {
            //                manager.StopAll();
            //            });
            //            Thread.Sleep(60);
        }

        private static void Client_ApplicationMessageReceived (object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var msg = float.Parse(Encoding.ASCII.GetString (e.ApplicationMessage.Payload));

            Console.WriteLine ($"{e.ApplicationMessage.Topic}: {msg}");
        }
    }

    public class ExampleMqttBrokers
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

    public class SensorManager
    {
        public IMqttClient Mqtt { get; protected set; }
        public ElasticSearchClient<object> ElasticSearch { get; protected set; }

        List<IDisposable> _subscriptions = new List<IDisposable> ( );
        List<Sensor> _sensors = new List<Sensor> ( );

        public void Watch (Sensor sensor, TimeSpan interval)
        {
            _sensors.Add (sensor);

            this._subscriptions.Add (Observable.Interval (interval)
                .Subscribe (x =>
                {
                    var value = sensor.Read ( );
                    Mqtt.PublishAsync (
                        new MqttApplicationMessage (
                            topic: $"sensor/{sensor.GetType ( ).Name}".ToLower(),
                            payload: Encoding.ASCII.GetBytes (value.ToString ( )),
                            qualityOfServiceLevel: MqttQualityOfServiceLevel.ExactlyOnce,
                            retain: true));
                }));
        }
        public SensorManager ( )
        {
            Mqtt = new ExampleMqttBrokers ( ).CloudMqtt ( );
            this.ElasticSearch = new ElasticSearchClient<object> ("http://elastic:changeme@localhost:9200", "iot");
            Mqtt.ConnectAsync ( ).Wait (1000);

        }
        public void StopAll ( )
        {
            _subscriptions.ForEach (x => x.Dispose ( ));
            _subscriptions.Clear ( );
        }
    }

    public abstract class Sensor
    {
        public Sensor ( )
        {
            Guid = Guid.NewGuid ( );
        }

        protected Sensor (double min, double max)
        {
            _min = min;
            _max = max;
            _random = new Random ( );

            Observable.Interval (TimeSpan.FromMilliseconds (100)).Subscribe (x => { _current = NextValue ( ); });
        }

        protected double _max { get; }
        protected double _current { get; set; }
        protected double _min { get; }
        protected Random _random { get; set; }

        protected Guid Guid { get; set; }

        public string Id => Guid.ToString ( );

        public double Read ( )
        {
            return _current;
        }

        public double NextValue ( )
        {
            var value = _min + ( _max - _min ) * _random.NextDouble ( );
            return value;
        }
    }

    public class Gas : Sensor
    {
        public Gas (double min, double max) : base (min, max)
        {
        }
    }

    public class Smoke : Sensor
    {
        public Smoke (double min, double max) : base (min, max)
        {
        }
    }


    public class ElasticSearchClient<T> where T : class, new()
    {
        public ElasticSearchClient (string connString, string defaultIndex)
        {
            var node = new Uri (connString);
            var connectionPool = new SniffingConnectionPool (new [] { node });

            var config = new ConnectionConfiguration (connectionPool);

            var pool = new SingleNodeConnectionPool (node);

            var settings = new ConnectionSettings (pool)
                .DefaultIndex (defaultIndex);

            Client = new ElasticClient (settings);
        }

        public ElasticSearchClient (IConnectionSettingsValues settings)
        {
            Client = new ElasticClient (settings);
        }

        public static ConnectionSettings CreateSettings (Uri uri, string indexName)
        {
            return new ConnectionSettings (uri: uri)
                .DefaultIndex (defaultIndex: indexName)
                ;
        }

        public ElasticClient Client { get; set; }

        public void DeleteIndexIfExists (string index)
        {
            if ( Client.IndexExists (index).Exists )
                Client.DeleteIndex (index);
        }

        public IBulkResponse Index (IEnumerable<T> data, string pipeline)
        {
            IList<IBulkOperation> ops = data.Select (e => new BulkIndexOperation<T> (e)
            {
                Pipeline = pipeline
            }).Cast<IBulkOperation> ( ).ToList ( );

            var br = new BulkRequest ( )
            {
                Pipeline = pipeline,
                Operations = ops
            };

            return Client.Bulk (br);
        }

        public IBulkResponse Index (IEnumerable<T> data)
        {
            return Client.IndexMany (data);
        }

        public IIndexResponse Index (T data)
        {
            return Client.Index (data);
        }

        public void SwapAlias (string index)
        {
            throw new NotImplementedException ( );
        }

        public IBulkResponse Index (IQueryable<T> data)
        {
            return Client.IndexMany (objects: data);
        }
    }
}