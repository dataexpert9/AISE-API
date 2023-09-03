using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Areas.Admin.Models
{
    public class FranchiseModels
    {

    }

    public class FranchiseBindingModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public int NumberOfEmployees { get; set; }

        public short Status { get; set; }
    }

    public class DialPlanBindingModel
    {
        public int Id { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public int? OutBoundCall_Id { get; set; }

        public int Franchise_Id { get; set; }
    }
}