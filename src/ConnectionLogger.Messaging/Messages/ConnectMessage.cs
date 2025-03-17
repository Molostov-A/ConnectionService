using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionLogger.Messaging.Messages
{
    public class ConnectMessage
    {
        public long UserId { get; init; }

        public string Ip { get; set; }

        public DateTime ConnectedAt { get; set; }
    }
}
