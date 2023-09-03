using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class OutBoundCalls
    {

        public int Id { get; set; }

        public string PhoneNumber { get; set; }

        public bool Status { get; set; }

        public DateTime UploadedDate { get; set; }

        public int State { get; set; }

        public bool IsDeleted { get; set; }


        [ForeignKey("UploadedFiles")]
        public int UploadedFiles_Id { get; set; }

        public UploadedFiles UploadedFiles { get; set; }


        public int? Franchise_Id { get; set; }

        public Franchise Franchise { get; set; }

        [ForeignKey("Admin")]
        public int Admin_Id { get; set; }

        public Admin Admin { get; set; }

        public string Source { get; set; }

        public string Name { get; set; }

        public string Gender { get; set; }

        public string Company { get; set; }

        public string Classification { get; set; }
        public string Address { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
        public string Note4 { get; set; }
        public string Note5 { get; set; }

        public int AnsweringStatus { get; set; }

        public TimeSpan CallTime { get; set; }

        public TimeSpan SessionDuration { get; set; }

        public int? OutBoundCallDialPlan_Id { get; set; }


        //public string CallStartTime { get; set; }

        //public string CallEndTime { get; set; }

        //public string TotalCallDuration { get; set; }

        public OutBoundCallDialPlan OutBoundCallDialPlan { get; set; }

    }
}
