using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EmitReaderLib;
using EmitReaderLib.Model;
using System.Net;
using System.IO;

namespace SmsSendMessage
{
    class Program
    {
        static void Main(string[] args)
        {

            Race race = Race.LoadYear(2018, @"c:\temp\2018.json");

            int i = 0;
            foreach (Participant p in race.Participants.Where(p => !p.Classes.Contains(race.Classes.Where(c => c.Id.Equals("TEST")).First())))
            {
                if (p.Startnumber > 0)
                {
                    foreach (String mobil in p.Telephone)
                    {
                        String s = "Oppgje nr. " + p.Startnumber.ToString() + " ved henting i Saften fredag 13.april kl.14.30-21.00, eller laurdag 14.april kl. 08.00-09.30.%0A%0ALykke til i KjeringiOpen 2018 :)%0ASMS-tenesta er levert av Difi i samarbeid med Linkmobility";
                        try
                        {
                            String url = String.Format(@"http://simple.pswin.com/?USER=kjeringiopen&{0}&RCV=47{1}&TXT={2}&snd=Kjeringi18&ENC=utf-8", "PW=0DgFPq2k3", mobil, s);
                            WebClient webClient = new WebClient();
                            Stream stream = webClient.OpenRead(url);
                            StreamReader reader = new StreamReader(stream);
                            String request = reader.ReadToEnd();
                            Console.WriteLine("Success: " + url);
                            i++;
                        }
                        catch (WebException ex)
                        {
                        }

                    }                    
                }
                else
                {
                    Console.WriteLine("No startnumber " + p.Name);
                }
            }
            Console.Write(i);
            Console.ReadKey();
        }
    }
}
