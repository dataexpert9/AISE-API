using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ExcelFile
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public string FileUrl { get; set; }

        public int Status { get; set; }

        public bool IsDeleted { get; set; }

        public User User { get; set; }
    }
}
