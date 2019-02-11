using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalRSelfHost
{
    public class Check
    {
        private readonly static Lazy<Check> _instance = new Lazy<Check>(() => new Check(GlobalHost.ConnectionManager.GetHubContext<CheckHub>().Clients));
        string weigth;
        private Check(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;

        }

        public static Check Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public string GetTemp()
        {
            return weigth;
        }

        public void SetTemp(string p)
        {
            weigth = p;
            BroadcastWeigth(weigth);
        }

        private void BroadcastWeigth(string temp)
        {
            Clients.All.addNewMessageToPage(temp);
        }


    }
}
