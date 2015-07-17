using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MongoPersistence
{
    public static class PersistenceGlobal
    {
        public static SshClient SshClient { get; set; }

        public static void SSH_Start()
        {
            string host = ConfigurationManager.AppSettings["MongoSSHHost"];
            if (!String.IsNullOrEmpty(host) && PersistenceGlobal.SshClient == null)
            {
                string user = ConfigurationManager.AppSettings["MongoSSHUser"];
                string pwd = ConfigurationManager.AppSettings["MongoSSHPassword"];

                PersistenceGlobal.SshClient = new SshClient(host, user, pwd);

                PersistenceGlobal.SshClient.Connect();

                var connData = ConfigurationManager.AppSettings["MongoConnectionString"].Replace("mongodb://", "").Split(':');
                var port = new ForwardedPortLocal("127.0.0.1", Convert.ToUInt32("27017"), connData[0], Convert.ToUInt32(connData[1]));
                PersistenceGlobal.SshClient.AddForwardedPort(port);
                port.Start();

                AppDomain.CurrentDomain.DomainUnload += new EventHandler(SSH_Stop);
            }
        }

        public static void SSH_Stop(object sender, EventArgs e)
        {
            if (PersistenceGlobal.SshClient != null && PersistenceGlobal.SshClient.IsConnected)
            {
                foreach (var item in PersistenceGlobal.SshClient.ForwardedPorts)
                {
                    item.Stop();
                }
                PersistenceGlobal.SshClient.Disconnect();
            }
        }
    }
}