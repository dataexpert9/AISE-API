using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class OutBoundCallDialPlan
    {
        public int Id { get; set; }

        public string StartingTime { get; set; }

        public string EndTime { get; set; }

        public int State { get; set; }

        public bool IsDeleted { get; set; }

        [ForeignKey("Franchise")]
        public int? Franchise_Id { get; set; }

        public Franchise Franchise { get; set; }


        public List<OutBoundCalls> OutBoundCalls { get; set; }

    }
}
