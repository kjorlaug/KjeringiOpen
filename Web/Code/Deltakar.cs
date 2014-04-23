using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KjeringiData
{
    public class Klasse
    {
        public String Id { get; set; }
        public String Namn { get; set; }
    }

    public class Deltakar
    {
        public int Startnummer { get; set; }
        public int EmitID { get; set; }

        public String Namn { get; set; }

        public Klasse Klasse { get; set; }
        public Boolean Bedrift { get; set; }
        public Boolean Super { get; set; }

        //public List<String> Medlemmer { get; }
        protected List<Passering> Passeringar { get; set; }
    }

    public class Passering
    {
        public Plassering Location { get; set; }
        public int EmitID { get; set; }
        public DateTime Tid { get; set; }
    }

    public class Plassering
    {
        public int BoksId { get; set; }
        public Boolean Offisiell { get; set; }
        public String Namn { get; set; }
        public int Sekvens { get; set; }
    }

    public class Konkurranse
    {
        //public List<Klasse> Klassar { get; }
        //public List<Deltakar> Deltagarar { get; }
        //public List<Plassering> Plasseringar { get; }

        //public List<Passering> Passeringar { get; }

        //public Dictionary<int, Deltakar> DeltagarEtterEmitId { get; }
    }

}
