using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonData.Services;

public interface IDataService
{
    Task SaveConnectionAsync(long userId, string address, string protocol);

    Task 
}
