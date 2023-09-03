using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public partial class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public virtual User User { get; set; }
        public int User_Id { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Contexts> Contexts { get; set; }
    }
}
