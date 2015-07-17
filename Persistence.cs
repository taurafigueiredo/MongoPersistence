using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace MongoPersistence
{
    public class Persistence<T>
    {
        [JsonIgnore]
        public ObjectId Id { get; set; }

        [BsonIgnore]
        public string _id
        {
            get
            {
                return this.Id.ToString().Replace("0", "").Length > 0 ? this.Id.ToString() : null;
            }
            set
            {
                Id = new ObjectId(value);
            }
        }

        [BsonIgnore]
        [JsonIgnore]
        public FilterDefinitionBuilder<T> filterBuilder { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public FilterDefinition<T> filter { get; set; }

        [BsonIgnore]
        [JsonIgnore]
        public string Database { get; set; }

        public Persistence()
        {
            if (filterBuilder == null) filterBuilder = Builders<T>.Filter;
        }

        public async Task InsertOrUpdate()
        {
            var data = GetDataAccess();
            if (_id != null)
            {
                filter = filterBuilder.Eq("_id", Id);
                await data.Update(filter,this.Database);
            }
            else
            {
                await data.Insert(this.Database);
            }
        }

        private Data<T> GetDataAccess()
        {
            var data = new Data<T>();
            data.Records = new List<T>();
            data.Records.Add((T)Convert.ChangeType(this, typeof(T)));
            return data;
        }

        public async Task<List<T>> Get(string id)
        {
            _id = id;
            filter = filterBuilder.Eq("_id", Id);
            var result = await Get(filter);
            return result;
        }

        public async Task<List<T>> Get(ObjectId _id)
        {
            filter = filterBuilder.Eq("_id", _id);
            var result = await Get(filter);
            return result;
        }

        public async Task<List<T>> Get()
        {
            filter = new BsonDocument();
            var result = await Get(filter);
            return result;
        }

        public async Task<List<T>> Get(FilterDefinition<T> _filter)
        {
            var result = await new Data<T>().Get(_filter, this.Database);
            return result;
        }

        public async Task<List<T>> Get(Dictionary<string, string> parameters)
        {
            List<FilterDefinition<T>> filters = new List<FilterDefinition<T>>();

            foreach (var item in parameters)
            {
                if (item.Key.Contains("_id"))
                {
                    FilterDefinition<T> filterItem = filterBuilder.Eq(item.Key, new ObjectId(item.Value));
                    filters.Add(filterItem);
                }
                else
                {
                    var nestedLevels = item.Key.Split('.').ToList();
                    var deeperLevel = typeof(T);

                    do
                    {
                        deeperLevel = deeperLevel.GetProperty(nestedLevels[0]).PropertyType;
                        nestedLevels.RemoveAt(0);
                    } while (nestedLevels.Count > 0);

                    dynamic changedValue = Convert.ChangeType(item.Value, deeperLevel);

                    FilterDefinition<T> filterItem = filterBuilder.Eq(item.Key, changedValue);
                    filters.Add(filterItem);
                }
            }

            filter = filterBuilder.And(filters);
            var result = await Get(filter);
            return result;
        }

        public async Task<List<T>> Get(string key, string value)
        {
            filter = filterBuilder.Eq(key, value);
            var result = await Get(filter);
            return result;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LookupKey : System.Attribute
    {
        public LookupKey()
        {
        }

    }
}
