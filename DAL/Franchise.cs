using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL
{
    public class Franchise
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Region { get; set; }

        public string ZipCode { get; set; }

        public string Address { get; set; }
        public string State { get; set; }

        public short Status { get; set; }

        public int NumberOfEmployees { get; set; }

        public bool IsDeleted { get; set; }

        public List<Admin> Admins { get; set; }

        public List<OutBoundCalls> OutBoundCalls { get; set; }
        public List<OutBoundCallDialPlan> OutBoundCallDialPlan { get; set; }
    }
}
