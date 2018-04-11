using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib.Model;

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

    public class LogEventArgs
    {

        public LogEventArgs(String data)
        {
            this.Data = data;
        }

        public String Data { get; set; }
    }

    public interface IEmitReader
    {
        event EventHandler<EmitDataRecievedEventArgs> DataReceived;

        String Port { get; set; }
        String BoxId { get; set; }
        void Start();
        void Stop();
    }

}
