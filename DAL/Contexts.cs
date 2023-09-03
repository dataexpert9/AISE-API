using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public partial class Contexts
    {
        public int Id { get; set; }

        public string ContextText { get; set; }

        [ForeignKey("User_Id")]
        public User User { get; set; }

        public int User_Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

    }
}
