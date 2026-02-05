using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.EasyLog.Interfaces
{
    public interface ILogSerializer
    {
        string Serialize(object entry);
        string FileExtension { get; }

    }
}
