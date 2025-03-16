using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerModelsLibrary.Models;

public class SearchConnectionsMessage
{
    public long UserId { get; set; }
    public OrderBy OrderBy { get; set; }
    public Direction Direction { get; set; }

}
