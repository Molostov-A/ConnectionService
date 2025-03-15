using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerModelsLibrary.Models;

// Класс для передачи данных о сообщении
public class MessageEventArgs : EventArgs
{
    public string Body { get; set; }
    public string CorrelationId { get; set; }
}
