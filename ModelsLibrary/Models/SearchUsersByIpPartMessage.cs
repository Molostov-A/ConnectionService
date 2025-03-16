using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerModelsLibrary.Models;

public class SearchUsersByIpPartMessage
{
    public string Ip {  get; set; }
    public string Protocol { get; set; }
}
