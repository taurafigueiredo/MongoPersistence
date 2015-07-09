# MongoPersistence

## 1 - Add keys on Web.config

Web.config
```
<appSettings>
  <add key="MongoConnectionString" value="mongodb://localhost:27017" />
  <add key="MongoDefaultDatabase" value="fitnesstracker" />
</appSettings>
```

If you need to connect through SSH, add these keys on appSettings sections (like above):
```
<add key="MongoSSHHost" value="ssh_host_ip" />
<add key="MongoSSHUser" value="ssh_username" />
<add key="MongoSSHPassword" value="ssh_password" />
<add key="MongoRemoteConnectionString" value="remote_ip:remote_port" />
```

## 2 - Create your "entity"
You just have to inherit from Persistence<T>, where T is your entity.
```
public class Profile : Persistence<Profile>
{
    public string EmailAddress { get; set; }
    public string Password { get; set; }
    public float Weight { get; set; }
    public float Height { get; set; }
    public DateTime Birthdate { get; set; }
    public string Name { get; set; }
}
```

## 3 - Create your controller
If you want to just expose your entity, you just have to create a controller class that inherites from PersistenceController<T>, where T is your entity. By default, you have Get and Post methods pre-defined. (I'm working on getting this part better)

```
public class ProfileController : PersistenceController<Profile>
{
    
}
```

If you have any problems or want to suggest another feature, please open an issue.
