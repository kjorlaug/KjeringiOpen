using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;

namespace EmitReaderLib.Writers
{
    public interface IRaceWriter
    {
        void PersistPass(Pass pass);
    }
}
