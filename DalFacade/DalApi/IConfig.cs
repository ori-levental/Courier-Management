using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalApi;
public interface IConfig
{
    DateTime Clock { get; set; }
    int MaxRange { get; set; }
    void Reset();
}

