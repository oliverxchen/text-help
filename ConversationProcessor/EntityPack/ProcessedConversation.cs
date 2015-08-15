using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityPack
{
    public class ProcessedConversation
    {
        //V.phone_number, vm, rm, U.location, U.nationality, U.age_group, U.gender
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public string Nationality { get; set; }
        public int AgeGroup { get; set; }
        public string Gender { get; set; }
        public double ReporterMsgSenti { get; set; }
        public double VolunteerMsgSenti { get; set; }
        public double OverallMsgSenti { get; set; }
        public int Category { get; set; }
        public int Days { get; set; }
        public int Exchanges { get; set; } 
    }
}
