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
            _database = ConfigurationManager.AppSettings["MongoDefaultDatabase"];
        }

        public List<T> Records { get; set; }

        private string _database { get; set; }

        public async Task Insert(string database = null)
        {
            if (!String.IsNullOrEmpty(database))
                _database = database;

            var collection = GetCollection();
            foreach (var item in Records)
            {
                await collection.InsertOneAsync(item);
            }
        }

        public async Task Update(FilterDefinition<T> filter, string database = null)
        {
            if (!String.IsNullOrEmpty(database))
                _database = database;

            var collection = GetCollection();
            foreach (var item in Records)
            {
                await collection.ReplaceOneAsync(filter, item);
            }
        }

        public async Task<List<T>> Get(FilterDefinition<T> filter, string database = null)
        {
            if(!String.IsNullOrEmpty(database))
                _database = database;

            var collection = GetCollection();
            var cursor = collection.Find(filter);

            List<T> list = await cursor.ToListAsync<T>();


            return list;
        }

        private IMongoCollection<T> GetCollection()
        {
            var collectionName = typeof(T).Name;
            var connectionString = String.Empty;

            if(PersistenceGlobal.SshClient.IsConnected)
                connectionString = "mongodb://127.0.0.1:27017";
            else
                connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(_database);

            var collection = db.GetCollection<T>(collectionName);

            return collection;
        }
    }
}