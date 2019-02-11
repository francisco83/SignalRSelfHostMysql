using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRSelfHost
{
    [HubName("stockTickerMini")]
    public class CheckHub : Hub
    {
        private readonly Check _check;

        public CheckHub() : this(Check.Instance) { }

        public CheckHub(Check checkWeight)
        {
            _check = checkWeight;
        }

        public void SetTemp(string p)
        {
            _check.SetTemp(p);
        }
        public string GetTemp()
        {
            return _check.GetTemp();
        }

    }
}
