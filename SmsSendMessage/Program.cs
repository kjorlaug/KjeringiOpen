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

            Race race = Race.LoadYear(2017, @"c:\temp\2017.json");

            foreach (Participant p in race.Participants.Where(p => !p.Classes.Contains(race.Classes.Where(c => c.Id.Equals("TEST")).First())))
            {
                if (p.Startnumber > 0)
                {
                    foreach (String mobil in p.Telephone)
                    {
                        String s = "Oppgje startnummer " + p.Startnumber.ToString() + " ved henting. Lykke til i KjeringiOpen 2017 :)  SMS-tjenestene levert av Difi i samarbeid med Linkmobility";
                        try
                        {
                            String url = String.Format(@"http://sms.pswin.com/http4sms/send.asp?USER=kjeringiopen&{0}&RCV=47{1}&TXT={2}&snd=Kjeringi", "PW=0DgFPq2k3", mobil, s);
                            WebClient webClient = new WebClient();
                            Stream stream = webClient.OpenRead(url);
                            StreamReader reader = new StreamReader(stream);
                            String request = reader.ReadToEnd();
                            Console.WriteLine("Success: " + url);
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
            Console.ReadKey();
        }
    }
}
