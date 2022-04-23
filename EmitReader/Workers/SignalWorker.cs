using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using System.Drawing;

using EmitReaderLib.Model;
using System.Net.Http;
using System.Threading.Tasks;

namespace EmitReaderLib
{
    public class Http
    {
        static HttpClient _httpClient = new HttpClient();

        public static async Task<string> Get(string url)
        {
            // The actual Get method
            using (var result = await _httpClient.GetAsync(url))
            {
                string content = await result.Content.ReadAsStringAsync();
                return content;
            }
        }
    }


    public class SignalWorker : IWorker
    {
        public event EventHandler<LogEventArgs> LogEntry;
        public event EventHandler<KeyValuePair<Color, string>> StatusChange;
        public event EventHandler<Boolean> Ready;


        private static Queue<EmitData> ToSend = new Queue<EmitData>();
        private static Boolean sending = false;
        private static Object lockObj = new Object();

        public string Name { get; set; }
        public string BoxId { get; set; }
        public string Hub { get; set; }

        public void StartWork()
        {
            Ready(this, true);

            _ = Http.Get("https://kjeringiopen.azurewebsites.net/2022");

            if (StatusChange != null)
                StatusChange(this, new KeyValuePair<Color, string>(Color.Green, "Ready "));

            while (true)
            {
                if (sending)
                {
                    return;
                }

                lock (lockObj)
                {
                    sending = true;
                }

                while (ToSend.Count > 0)
                {
                    EmitData dataToSend = ToSend.Peek();
                    try
                    {

                        _ = Http.Get($"https://kjeringiopen.azurewebsites.net/api/AddPass?card={dataToSend.Chip}&location={dataToSend.BoxId}&time={dataToSend.Time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'")}");

                        if (LogEntry != null)
                            LogEntry(this, new LogEventArgs($"{dataToSend.Chip} - {dataToSend.Time}"));

                        lock (lockObj)
                            ToSend.Dequeue();
                        Console.WriteLine("Successfully " + dataToSend.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (StatusChange != null)
                            StatusChange(this, new KeyValuePair<Color, string>(Color.Red, "Error sending, retry in 3 sec : " + ex.Message));
                        System.Threading.Thread.Sleep(3000);
                    }
                }
                lock (lockObj)
                {
                    sending = false;
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void ProcessData(EmitData data)
        {
            lock (lockObj)
            {
                ToSend.Enqueue(data);
            }
        }

    }
}
