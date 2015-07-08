using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MongoPersistence
{
    public class Default
    {
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}