# MongoPersistence

To install MongoPersistence nuget package, run the following command in the Package Manager Console:
```console
Install-Package MongoPersistence
```

## 1 - Add keys on Web.config

Web.config
```xml
<appSettings>
  <add key="MongoConnectionString" value="mongodb://localhost:27017" />
  <add key="MongoDefaultDatabase" value="database" />
</appSettings>
```

If you need to connect through SSH, you need to also add 3 (four) other keys on appSettings sections:
```xml
<add key="MongoSSHHost" value="ssh_host_ip" />
<add key="MongoSSHUser" value="ssh_username" />
<add key="MongoSSHPassword" value="ssh_password" />
```

## 2 - Create your "entity"
You just have to inherit from Persistence<T>, where T is your entity.
You can use 'dynamic' type if you are make reference to a MongoDB field which doesn't have a simple structure.
If you want to map your .NET attribute to a MongoDB field that have a different name, you can use the BsonElement decoration property.

You can also set the database where this class should persist using the "Database" property of your class.
```csharp
public class Profile : Persistence<Profile>
{
    public string EmailAddress { get; set; }
    public string Password { get; set; }
    public float Weight { get; set; }
    public float Height { get; set; }
    public DateTime Birthdate { get; set; }
    public string Name { get; set; }
    
    [BsonElement("NameOnMongoDB")]
    public string NameOnDotNet { get; set; }
    
    public Profile()
    {
      this.Database = "DatabaseIWantToUse";
    }
}
```

## 3 - Create your controller
If you want to just expose your entity, you just have to create a controller class that inherites from PersistenceController<T>, where T is your entity. By default, you have Get and Post methods pre-defined.

```csharp
public class ProfileController : PersistenceController<Profile>
{
    
}
```

To simple "equals" query, you can use the default "Get" method.
For example, to get all the people that have a weight of 60 kg and a height of 170 cm, you can do a request like this:
```
/api/Profile?Weight=60&Height=170
```

If you have any problems or want to suggest another feature, please open an issue.
