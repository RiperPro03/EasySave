using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.EasyLog.Interfaces
{
    public interface ILogWriter
    {
        bool Write(string filepath, string message);
    }
}
