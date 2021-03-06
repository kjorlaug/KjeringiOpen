﻿using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client;
using System.Drawing;

using EmitReaderLib.Model;

namespace EmitReaderLib
{
    public class SignalWorker : IWorker, IDisposable
    {
        public event EventHandler<LogEventArgs> LogEntry;
        public event EventHandler<KeyValuePair<Color, string>> StatusChange;
        public event EventHandler<Boolean> Ready;

        private HubConnection myHubConn;
        private IHubProxy myHub;

        private static Queue<EmitData> ToSend = new Queue<EmitData>();
        private static Boolean sending = false;
        private static Object lockObj = new Object();

        public string Name { get; set; }
        public string BoxId { get; set; }
        public string Hub { get; set; }

        public void StartWork()
        {
            if (StatusChange != null)
                StatusChange(this, new KeyValuePair<Color, string>(Color.Yellow, "Initiating SignalR - contacting hub " + Hub));

            myHubConn = new HubConnection(Hub);
            myHub = myHubConn.CreateHubProxy("resultatServiceHub");

            myHubConn.Reconnected += myHubConn_Reconnected;
            myHubConn.Reconnecting += myHubConn_Reconnecting;
            myHubConn.ConnectionSlow += myHubConn_ConnectionSlow;
            myHubConn.Closed += myHubConn_Closed;

            // Listen to newPass to give feedback on things registering.
            myHub.On<Participant>("newPass", (data) =>
            {
                try
                {
                    if (LogEntry != null)
                        LogEntry(this, new LogEventArgs(data.EmitID.ToString() + " - startnummer " + data.Startnumber.ToString() + " - " + data.TotalTime));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            myHubConn.Start()
                .ContinueWith((prevTask) =>  // Initialization
                {
                    if (StatusChange != null)
                        StatusChange(this, new KeyValuePair<Color, string>(Color.Green, "Connected: " + myHubConn.State));
                    myHub.Invoke("Join", "EmitReader - " + Name);
                    myHub.Invoke("AddtoGroup", BoxId.ToString());
                })
                .ContinueWith((prevTask) => // Loop forever to process data registered
                {
                    if (Ready != null)
                        Ready(this, true);

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
                                myHub.Invoke("SendPassering", new object[] { dataToSend });
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
                }).Wait();
        }

        public void ProcessData(EmitData data)
        {
            lock (lockObj)
            {
                ToSend.Enqueue(data);
            }
        }

        void myHubConn_Closed()
        {
            if (StatusChange != null)
                StatusChange(this, new KeyValuePair<Color, string>(Color.Red, "myHubConn_Closed New State - retrying in 5 sec: " + myHubConn.State));

            Console.WriteLine("myHubConn_Closed New State - retrying in 5 sec:" + myHubConn.State);
            System.Threading.Thread.Sleep(5000);
            myHubConn.Start().Wait();
        }

        void myHubConn_ConnectionSlow()
        {
            if (StatusChange != null)
                StatusChange(this, new KeyValuePair<Color, string>(Color.Yellow, "myHubConn_ConnectionSlow New State: " + myHubConn.State));
            Console.WriteLine("myHubConn_ConnectionSlow New State:" + myHubConn.State);
        }

        void myHubConn_Reconnecting()
        {
            if (StatusChange != null)
                StatusChange(this, new KeyValuePair<Color, string>(Color.Yellow, "myHubConn_Reconnecting New State: " + myHubConn.State));
            Console.WriteLine("myHubConn_Reconnecting New State:" + myHubConn.State);
        }

        void myHubConn_Reconnected()
        {
            if (StatusChange != null)
                StatusChange(this, new KeyValuePair<Color, string>(Color.Green, "myHubConn_Reconnected New State: " + myHubConn.State));
            Console.WriteLine("myHubConn_Reconnected New State:" + myHubConn.State);
        }

        public void Dispose()
        {
            myHubConn.Stop();
        }
    }
}
