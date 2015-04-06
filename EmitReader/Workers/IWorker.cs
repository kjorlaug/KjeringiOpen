using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib
{
    public interface IWorker
    {
        void ProcessData(EmitData data);
    }
}
