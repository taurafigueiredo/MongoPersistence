using MongoDB.Driver;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoPersistence
{
    public class Data<T>
    {
        public Data()
        {
            PersistenceGlobal.SSH_Start();
        }

        public List<T> Records { get; set; }

        public async Task Insert()
        {
            var collection = GetCollection();
            foreach (var item in Records)
            {
                await collection.InsertOneAsync(item);
            }
        }

        public async Task Update(FilterDefinition<T> filter)
        {
            var collection = GetCollection();
            foreach (var item in Records)
            {
                await collection.ReplaceOneAsync(filter, item);
            }
        }

        public async Task<List<T>> Get(FilterDefinition<T> filter)
        {

            var collection = GetCollection();
            var cursor = collection.Find(filter);

            List<T> list = await cursor.ToListAsync<T>();


            return list;
        }

        private IMongoCollection<T> GetCollection()
        {
            var collectionName = typeof(T).Name;
            var connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];
            var defaultDatabase = ConfigurationManager.AppSettings["MongoDefaultDatabase"];
            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(defaultDatabase);

            var collection = db.GetCollection<T>(collectionName);

            return collection;
        }
    }
}