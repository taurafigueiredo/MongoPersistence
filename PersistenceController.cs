using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MongoPersistence
{
    public class PersistenceController<T> : ApiController
    {
        public Persistence<T> GenericPersistence { get; set; }

        public PersistenceController()
        {
            GenericPersistence = new Persistence<T>();
        }

        // GET api/values
        public virtual async Task<List<T>> Get()
        {
            var querystring = Request.GetQueryNameValuePairs();
            if (querystring.Count() > 0)
            {
                var parameters = new Dictionary<string, string>();
                foreach (var item in querystring)
                {
                    parameters.Add(item.Key, item.Value);
                }
                return await GenericPersistence.Get(parameters);
            }

            return await GenericPersistence.Get();

        }

        // GET api/values/5
        public async Task<List<T>> Get(string id)
        {
            //id = Base64.Decode(id);
            return await GenericPersistence.Get(id);
        }


        // POST api/values
        public virtual async Task<List<T>> Post([FromBody]T value)
        {
            GenericPersistence = (Persistence<T>)Convert.ChangeType(value, typeof(T));

            if (Request.GetQueryNameValuePairs().Where(c => c.Key == "Action" & c.Value == "Get").Count() > 0)
            {
                var parameters = new Dictionary<string, string>();
                AppendProperties(value, parameters);
                return await GenericPersistence.Get(parameters);
            }
            else
            {
                await FindIdByLookupKey(value);
                await GenericPersistence.InsertOrUpdate();
            }

            return new List<T>();
        }

        public virtual async Task<List<T>> Put([FromBody]T value)
        {
            GenericPersistence = (Persistence<T>)Convert.ChangeType(value, typeof(T));

            await FindIdByLookupKey(value);

            await GenericPersistence.Update();

            return new List<T>();
        }

        public virtual async Task<List<T>> Put(string id, [FromBody]T value)
        {
            GenericPersistence = (Persistence<T>)Convert.ChangeType(value, typeof(T));
            GenericPersistence._id = id;

            await GenericPersistence.Update();

            return new List<T>();
        }

        public virtual async Task<List<T>> Delete(string id)
        {
            //id = Base64.Decode(id);
            var list = await GenericPersistence.Get(id);
            var item = list.First();
            GenericPersistence = (Persistence<T>)Convert.ChangeType(item, typeof(T));
            await GenericPersistence.Delete();

            return new List<T>();
        }

        public virtual async Task<List<T>> Delete([FromBody]T value)
        {
            GenericPersistence = (Persistence<T>)Convert.ChangeType(value, typeof(T));

            await FindIdByLookupKey(value);

            await GenericPersistence.Delete();

            return new List<T>();
        }

        private async Task FindIdByLookupKey(T value)
        {
            if (GenericPersistence._id == null)
            {
                var pk = value.GetType().GetProperties().Where(c => c.CustomAttributes.Where(x => x.AttributeType.Name == "LookupKey").Count() > 0).Select(c => c).FirstOrDefault();

                if (pk != null)
                {
                    List<T> actual = new List<T>();
                    actual = await GenericPersistence.Get(pk.Name, (string)pk.GetValue(value));

                    if (actual.Count > 0)
                        GenericPersistence._id = ((Persistence<T>)Convert.ChangeType(actual[0], typeof(T)))._id;
                }
            }
        }

        private static void AppendProperties(object value, Dictionary<string, string> parameters, string baseType = "")
        {
            var properties = value.GetType().GetProperties();
            foreach (var item in properties)
            {
                dynamic def = Default.GetDefault(item.PropertyType);
                var itemValue = item.GetValue(value);

                var _baseType = String.IsNullOrEmpty(baseType) ? item.Name : (baseType + "." + item.Name);

                if (!item.DeclaringType.FullName.Contains("MongoPersistence"))
                {
                    if (!item.PropertyType.Namespace.Equals("System") && itemValue != null)
                        AppendProperties(itemValue, parameters, _baseType);
                    else
                    {
                        if (!object.Equals(itemValue, def))
                            parameters.Add(_baseType, itemValue.ToString());
                    }
                }
            }
        }
    }
}
