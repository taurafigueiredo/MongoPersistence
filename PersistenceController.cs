﻿using System;
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

            if (Request.GetQueryNameValuePairs().Where(c => c.Key == "Action" & c.Value == "Get").Count() == 0)
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

                await GenericPersistence.InsertOrUpdate();
            }
            else
            {
                var properties = value.GetType().GetProperties();
                var parameters = new Dictionary<string, string>();
                //TODO: analisar encadeamento de propriedades
                foreach (var item in properties)
                {
                    dynamic def = Default.GetDefault(item.PropertyType);
                    var itemValue = item.GetValue(value);
                    if (!object.Equals(itemValue, def))
                    {
                        parameters.Add(item.Name, itemValue.ToString());
                    }
                }
                return await GenericPersistence.Get(parameters);
            }

            return new List<T>();
        }
    }
}