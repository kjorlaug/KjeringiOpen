using System;
using System.Collections.Generic;
using System.Drawing;

using EmitReaderLib.Model;

namespace EmitReaderLib
{
    public interface IWorker
    {
        void ProcessData(EmitData data);
        void StartWork();

        String Name { get; set; }
        String BoxId { get; set; }
        String Hub { get; set; }

        event EventHandler<LogEventArgs> LogEntry;
        event EventHandler<KeyValuePair<Color, String>> StatusChange;
    }
}
