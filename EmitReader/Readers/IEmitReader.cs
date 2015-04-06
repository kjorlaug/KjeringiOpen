using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib
{
    public class EmitDataRecievedEventArgs
    {

        public EmitDataRecievedEventArgs(EmitData data)
        {
            this.Data = data;
        }

        public EmitData Data { get; set; }
    }

    public interface IEmitReader
    {
        event EventHandler<EmitDataRecievedEventArgs> DataReceived;
        void Start();
        void Stop();
    }

}
