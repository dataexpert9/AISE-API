namespace DAL
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Admin
    {
        public Admin()
        {
            AdminTokens = new HashSet<AdminTokens>();
        }
        public int Id { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        public int Role { get; set; }

        [JsonIgnore]
        public string Password { get; set; }
        
        public string AccountNo { get; set; }

        [ForeignKey("Franchise")]
        public int? Franchise_Id { get; set; }
        
        
        [ForeignKey("Docking")]
        public int? Docking_Id { get; set; }

        public int Status { get; set; }


        public bool IsDeleted { get; set; }

        [NotMapped]
        public Token Token { get; set; }

        [NotMapped]
        public bool ImageDeletedOnEdit { get; set; }
        public string ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string FullName { get; set; }

        public string ProfilePictureUrl { get; set; }


        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        public int UserType { get; set; }

        public string SIPProxy { get; set; }

        public string CommunicationPoolName { get; set; }

        public string ExpirationTime { get; set; }

        #region RegistrationMode_Parameters
        public bool IsVoiceGatewayRegistration { get; set; }

        public bool IsLineDocking { get; set; }

        public bool CommunicationPool { get; set; }
        #endregion

        //registration mode : voice gateway registration options start
        public string GatewayAccount { get; set; }

        public string NoOfAIAgents { get; set; }

        public string DialInterval { get; set; }

        //registration mode : voice gateway registration options end 


        public string CallPrefix { get; set; }


        public short? SignInType { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool DeActive { get; set; }

        public DateTime JoinedOn { get; set; }

        public virtual Franchise Franchise { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AdminTokens> AdminTokens { get; set; }

        public virtual ICollection<Docking> Docking { get; set; }

    }
}
