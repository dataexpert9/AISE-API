using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BasketApi.CustomAuthorization
{
    //public class RoleTypes
    //{
    //    //User,
    //    //Deliverer,
    //    //SubAdmin,
    //    //SuperAdmin,
    //    //ApplicationAdmin
    //    public const string User = "User";
    //    public const string Deliverer = "Deliverer";
    //    public const string SubAdmin = "SubAdmin";
    //    public const string SuperAdmin = "SuperAdmin";
    //    public const string ApplicationAdmin = "ApplicationAdmin";
    //}

    public enum RoleTypes
    {
        User = 0,
        SuperAdmin = 1
    }
}