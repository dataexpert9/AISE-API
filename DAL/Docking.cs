using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Docking
    {
        public int Id { get; set; }
        public string LineName { get; set; }

        public string LineNotes { get; set; }

        public string ServerIP { get; set; }

        public string Port { get; set; }

        public string Account { get; set; }

        public string Password { get; set; }

        public string CellNumber { get; set; }

        public string VoiceCoding { get; set; }

        public bool IsEnabled { get; set; }

        public short Status { get; set; }

        public bool IsDeleted { get; set; }

        public string AdminName { get; set; }

        public int Admin_Id { get; set; }

        public Admin Admin { get; set; }

    }
}
