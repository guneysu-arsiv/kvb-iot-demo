using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;

namespace Sensors
{
    public class ElasticSearch<T> where T : class, new()
    {
        public ElasticSearch(string connString, string defaultIndex)
        {
            this.DefaultIndex = defaultIndex;

            var node = new Uri(connString);
            var connectionPool = new SniffingConnectionPool(new[] {node});

            var config = new ConnectionConfiguration(connectionPool);

            var pool = new SingleNodeConnectionPool(node);

            var settings = new ConnectionSettings(pool)
                .DefaultIndex(defaultIndex);

            Client = new ElasticClient(settings);
        }

        public ElasticSearch(IConnectionSettingsValues settings)
        {
            Client = new ElasticClient(settings);
        }

        public static ConnectionSettings CreateSettings(Uri uri, string indexName)
        {
            return new ConnectionSettings(uri: uri)
                    .DefaultIndex(defaultIndex: indexName)
                ;
        }

        public ElasticClient Client { get; set; }

        public void DeleteIndexIfExists(string index)
        {
            if (Client.IndexExists(index).Exists)
                Client.DeleteIndex(index);
        }

        public IBulkResponse Index(IEnumerable<T> data, string pipeline)
        {
            IList<IBulkOperation> ops = data.Select(e => new BulkIndexOperation<T>(e)
            {
                Pipeline = pipeline
            }).Cast<IBulkOperation>().ToList();

            var br = new BulkRequest()
            {
                Pipeline = pipeline,
                Operations = ops
            };

            return Client.Bulk(br);
        }

        public string IndexName => $"{DefaultIndex}-{DateTime.Now:yyyy.MM.dd}";

        public string DefaultIndex { get; set; }

        public IBulkResponse Index(IEnumerable<T> data)
        {
            return Client.IndexMany(objects: data, index:IndexName);
        }

        public IIndexResponse Index(T data)
        {
            return Client.Index(document:data, selector: d => d.Index(this.IndexName));
            
        }

        public void SwapAlias(string index)
        {
            throw new NotImplementedException();
        }

        public IBulkResponse Index(IQueryable<T> data)
        {
            return Client.IndexMany(objects: data, index: IndexName);
        }
    }
}