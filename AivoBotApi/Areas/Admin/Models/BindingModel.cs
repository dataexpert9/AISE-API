using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace BasketApi.Areas.SubAdmin.Models
{
    public class CategoryBindingModel
    {
        [Required]
        public string Name { get; set; }

        //[Required]
        public string Description { get; set; }

        [Required]
        public short StoreId { get; set; }

        public short Status { get; set; }
        
        public int ParentCategoryId { get; set; }
    }

    public class SubCategoryBindingModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public short CatId { get; set; }

        [Required]
        public short StoreId { get; set; }

        public short Status { get; set; }
    }

    public class StoreBindingModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public string Description { get; set; }

        [Required]
        public string Address { get; set; }

        public string ImageUrl { get; set; }

        public TimeSpan Open_From { get; set; }

        public TimeSpan Open_To { get; set; }
        //public DbGeography Location { get; set; }
       
        public bool ImageDeletedOnEdit { get; set; }
    }

    public class StoreImageBindingModel
    {
        [Required]
        public int storeId { get; set; }
        
    }

    public class AdminBindingModel
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string ProfilePictureUrl { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [StringLength(10, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password")]
        public string ConfirmPassword { get; set; }

        public int UserType { get; set; }

        public string SIPProxy { get; set; }

        public string CommunicationPoolName { get; set; }

        [Required(ErrorMessage = "Expiration Time for account is required.")]
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

        public short? Status { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool DeActive { get; set; }

        public int? Docking_Id { get; set; }

        public DateTime JoinedOn { get; set; }

        public int? Franchise_Id { get; set; }
    }

    public class SearchProductModel
    {
        public string ProductName { get; set; }
        public string ProductPrice { get; set; }
        public string CategoryName { get; set; }
    }

    public class DockingBindingModel
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

        public int User_Id { get; set; }
        public int Admin_Id { get; set; }

    }


    public class OutBoundFileImportBindingModel
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "User Id Is Required.")]
        public int Admin_Id { get; set; }

        [Required(ErrorMessage = "UserName Is Required.")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Franchise Identity Is Required.")]
        public int Franchise_Id { get; set; }

    }

    public class DocumentBindingModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Document Title/Name Is Required.")]
        public string Title { get; set; }

        public int User_Id { get; set; }
    }

    public class ContextBindingModel
    {
        public int Id { get; set; }

        public string ContextText { get; set; }

        public int User_Id { get; set; }

    }


}