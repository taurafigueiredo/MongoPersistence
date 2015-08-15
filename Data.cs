using MongoDB.Bson;
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
            await DoOperation(DataOperation.Insert, database);
        }

        public async Task Update(FilterDefinition<T> filter, string database = null)
        {
            await DoOperation(DataOperation.Update, filter, database);
        }

        public async Task Delete(FilterDefinition<T> filter, string database = null)
        {
            await DoOperation(DataOperation.Delete, filter, database);
        }

        public async Task<List<T>> Get(FilterDefinition<T> filter, string database = null)
        {
            await DoOperation(DataOperation.Get, filter, database);
            return Records;
        }

        //public async Task RunCommand(string query, string database = null)
        //{
        //    var db = GetDatabase(database);
        //    var command = new JsonCommand<BsonDocument>(query);
        //    var result = await db.RunCommandAsync(command);
        //    var teste = "a";
        //}

        private async Task DoOperation(DataOperation operation, FilterDefinition<T> filter = null, string database = null)
        {
            var collection = GetCollection(database);

            switch (operation)
            {
                case DataOperation.Insert:
                    foreach (var item in Records)
                    {
                        await collection.InsertOneAsync(item);
                    }
                    break;
                case DataOperation.Update:
                    foreach (var item in Records)
                    {
                        await collection.ReplaceOneAsync(filter, item);
                    }
                    break;
                case DataOperation.Delete:
                    await collection.DeleteOneAsync(filter);
                    break;
                case DataOperation.Get:
                    Records = await collection.Find(filter).ToListAsync<T>();
                    break;
                default:
                    break;
            }
        }

        private IMongoCollection<T> GetCollection(string database = null)
        {
            var collectionName = typeof(T).Name;
            var db = GetDatabase(database);

            var collection = db.GetCollection<T>(collectionName);
            return collection;
        }

        private IMongoDatabase GetDatabase(string database = null)
        {
            if (!String.IsNullOrEmpty(database))
                _database = database;

            var connectionString = String.Empty;

            if (PersistenceGlobal.SshClient != null && PersistenceGlobal.SshClient.IsConnected)
                connectionString = "mongodb://127.0.0.1:27017";
            else
                connectionString = ConfigurationManager.AppSettings["MongoConnectionString"];

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(_database);
            return db;
        }
    }
}