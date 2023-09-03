using BasketApi;
using BasketApi.Areas.SubAdmin.Models;
using BasketApi.CustomAuthorization;
using BasketApi.Models;
using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using WebApplication1.Areas.Admin.ViewModels;
using System.Data.Entity;
using static BasketApi.Utility;
using Newtonsoft.Json;
using static BasketApi.Global;
using System.Globalization;
using System.Data.Entity.Core.Objects;
using System.Web.Hosting;
using BasketApi.Components.Helpers;
using WebApplication1.ViewModels;
using WebApplication1.BindingModels;
using Z.EntityFramework.Plus;

namespace WebApplication1.Areas.Admin.Controllers
{
    [RoutePrefix("api/Admin")]
    public class AdminController : ApiController
    {
        //[BasketApi.Authorize/*("SubAdmin", "SuperAdmin", "ApplicationAdmin")*/]
        /// <summary>
        /// Add admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddUser")]
        public async Task<IHttpActionResult> AddUser(AdminBindingModel model)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                DAL.Admin admin = new DAL.Admin();
                DAL.Admin existingAdmin = new DAL.Admin();


                Validate(model);



                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (RiscoContext ctx = new RiscoContext())
                {
                    if (model.Id == 0)
                    {

                        if (ctx.Admins.Any(x => x.Username == model.Username && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "User with same username already exists" }
                            });
                        }
                    }
                    else
                    {
                        existingAdmin = ctx.Admins.FirstOrDefault(x => x.Id == model.Id);
                        model.Password = existingAdmin.Password;
                        if (existingAdmin.Username.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            if (ctx.Admins.Any(x => x.IsDeleted == false && x.Username.Equals(model.Username.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "User with same email already exists" }
                                });
                            }
                        }
                    }

                    if (model.Id == 0)
                    {
                        admin = ctx.Admins.Add(new DAL.Admin
                        {
                            Username = model.Username,
                            CommunicationPool = model.CommunicationPool,
                            CommunicationPoolName = model.CommunicationPoolName,
                            CreatedDate = DateTime.UtcNow,
                            DeActive = false,
                            Email = model.Email,
                            EmailConfirmed = false,
                            ExpirationTime = model.ExpirationTime,
                            IsVoiceGatewayRegistration = model.IsVoiceGatewayRegistration,
                            IsLineDocking = model.IsLineDocking,
                            IsDeleted = false,
                            Role = model.UserType,
                            UserType = model.UserType,
                            Password = CryptoHelper.Hash(model.Password),
                            JoinedOn = DateTime.UtcNow,
                            NoOfAIAgents = model.NoOfAIAgents,
                            DialInterval = model.DialInterval,
                            Status = Convert.ToInt32(UserStatus.Active),
                            Docking_Id = model.Docking_Id
                        });

                        if (model.IsVoiceGatewayRegistration)
                        {
                            admin.GatewayAccount = model.GatewayAccount;
                        }

                        if (model.IsLineDocking)
                        {
                            admin.SIPProxy = model.SIPProxy;
                            admin.CallPrefix = model.CallPrefix;
                        }

                        if (model.CommunicationPool)
                        {
                            admin.CommunicationPoolName = model.CommunicationPoolName;
                        }

                        if (model.Franchise_Id.HasValue)
                            admin.Franchise_Id = model.Franchise_Id.Value;


                        ctx.SaveChanges();

                    }
                    else
                    {
                        ctx.Entry(existingAdmin).CurrentValues.SetValues(model);
                        ctx.SaveChanges();

                        admin = existingAdmin;
                    }

                    await admin.GenerateToken(Request);

                    CustomResponse<DAL.Admin> response = new CustomResponse<DAL.Admin>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = admin
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize]
        /// <summary>
        /// Get Dashboard Stats
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IHttpActionResult> GetAllUsers()
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {

                    CustomResponse<List<DAL.Admin>> response = new CustomResponse<List<DAL.Admin>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = ctx.Admins.Include(x => x.Docking).Where(x => !x.IsDeleted).ToList()
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        /// <summary>
        /// Get Dashboard Stats
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAdminDashboardStats")]
        public async Task<IHttpActionResult> GetAdminDashboardStats()
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    DateTime TodayDate = DateTime.UtcNow.Date;

                    WebDashboardStatsViewModel model = new WebDashboardStatsViewModel
                    {

                        TotalUsers = ctx.Users.Count(),
                        DeviceUsage = ctx.Database.SqlQuery<DeviceStats>("select Count(Platform) as Count, (Count(Platform) * 100)/(select COUNT(Id) from UserDevices) as Percentage from UserDevices group by Platform order by Platform").ToList()
                    };

                    CustomResponse<WebDashboardStatsViewModel> response = new CustomResponse<WebDashboardStatsViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = model
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        //        [HttpGet]
        //        [Route("SearchAdmins")]
        //        public async Task<IHttpActionResult> SearchAdmins(string FirstName, string LastName, string Email, string Phone, int? StoreId)
        //        {
        //            try
        //            {
        //                using (RiscoContext ctx = new RiscoContext())
        //                {
        //                    string conditions = string.Empty;

        //                    if (!String.IsNullOrEmpty(FirstName))
        //                        conditions += " And Admins.FirstName Like '%" + FirstName.Trim() + "%'";

        //                    if (!String.IsNullOrEmpty(LastName))
        //                        conditions += " And Admins.LastName Like '%" + LastName.Trim() + "%'";

        //                    if (!String.IsNullOrEmpty(Email))
        //                        conditions += " And Admins.Email Like '%" + Email.Trim() + "%'";

        //                    if (!String.IsNullOrEmpty(Phone))
        //                        conditions += " And Admins.Phone Like '%" + Phone.Trim() + "%'";

        //                    if (StoreId.HasValue && StoreId.Value != 0)
        //                        conditions += " And Admins.Store_Id = " + StoreId;

        //                    #region query
        //                    var query = @"SELECT
        //  Admins.Id,
        //  Admins.FirstName,
        //  Admins.LastName,
        //  Admins.Email,
        //  Admins.Phone,
        //  Admins.Role,
        //  Admins.ImageUrl,
        //  Stores.Name AS StoreName
        //FROM Admins
        //LEFT OUTER JOIN Stores
        //  ON Stores.Id = Admins.Store_Id
        //WHERE Admins.IsDeleted = 0
        //AND Stores.IsDeleted = 0 " + conditions + @" UNION
        //SELECT
        //  Admins.Id,
        //  Admins.FirstName,
        //  Admins.LastName,
        //  Admins.Email,
        //  Admins.Phone,
        //  Admins.Role,
        //  Admins.ImageUrl,
        //  '' AS StoreName
        //FROM Admins
        //WHERE Admins.IsDeleted = 0
        //AND ISNULL(Admins.Store_Id, 0) = 0 " + conditions;

        //                    #endregion


        //                    var admins = ctx.Database.SqlQuery<SearchAdminViewModel>(query).ToList();

        //                    return Ok(new CustomResponse<SearchAdminListViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new SearchAdminListViewModel { Admins = admins } });
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return StatusCode(Utility.LogError(ex));
        //            }
        //        }



        //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("DeleteEntity")]

        public async Task<IHttpActionResult> DeleteEntity(int EntityType, int Id, int? User_Id)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    switch (EntityType)
                    {

                        case (int)BasketEntityTypes.Admin:
                            ctx.Admins.FirstOrDefault(x => x.Id == Id).IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.User:
                            var User = ctx.Users.FirstOrDefault(x => x.Id == Id);
                            if (User != null)
                                User.IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.Context:
                            var Context = ctx.Contexts.FirstOrDefault(x => x.Id == Id && x.User_Id == User_Id.Value);
                            if (Context != null)
                                Context.IsDeleted = true;
                            break;
                        case (int)BasketEntityTypes.ExcelFile:
                            var file = ctx.ExcelFile.FirstOrDefault(x => x.Id == Id);
                            if (file != null)
                                file.IsDeleted = true;
                            break;
                        default:
                            break;
                    }
                    ctx.SaveChanges();
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }





        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(AdminSetPasswordBindingModel model)
        {
            try
            {
                var userEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    throw new Exception("User Email is empty in user.identity.name.");
                }
                else if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (RiscoContext ctx = new RiscoContext())
                {
                    var hashedPassword = CryptoHelper.Hash(model.OldPassword);
                    var user = ctx.Admins.FirstOrDefault(x => x.Email == userEmail && x.Password == hashedPassword);
                    if (user != null)
                    {
                        user.Password = CryptoHelper.Hash(model.NewPassword);
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = "Forbidden", StatusCode = (int)HttpStatusCode.Forbidden, Result = new Error { ErrorMessage = "Invalid old password." } });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("ChangeUserStatuses")]
        public async Task<IHttpActionResult> ChangeUserStatuses(List<ChangeUserStatusModel> model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (RiscoContext ctx = new RiscoContext())
                {

                    foreach (var user in model)
                    {
                        var usr = ctx.Admins.FirstOrDefault(x => x.Id == user.Id);

                        if (usr != null)
                            usr.Status = user.Status;

                        ctx.SaveChanges();
                    }
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(SetForgotPasswordBindingModel model)
        {
            try
            {
                var userEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    throw new Exception("Unauthorized.");
                }
                else if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    using (RiscoContext ctx = new RiscoContext())
                    {
                        if (model.ConfirmNewPassword == model.NewPassword)
                        {
                            var hashedPassword = CryptoHelper.Hash(model.NewPassword);
                            var user = ctx.Admins.FirstOrDefault(x => x.Username == model.UserName);
                            if (user != null)
                            {
                                user.Password = CryptoHelper.Hash(model.NewPassword);
                                ctx.SaveChanges();
                                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                            }
                        }
                        else
                            return Ok(new CustomResponse<Error> { Message = "MisMatched", StatusCode = (int)HttpStatusCode.Forbidden, Result = new Error { ErrorMessage = "Password & Confirm Password Doesnt Match." } });

                    }
                    return Ok(new CustomResponse<Error> { Message = "Failure", StatusCode = (int)HttpStatusCode.InternalServerError, Result = new Error { ErrorMessage = "Internal Server Error." } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }





        //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        //[HttpPost]
        //[Route("ChangeUserStatuses")]
        //public async Task<IHttpActionResult> ChangeUserStatuses(ChangeUserStatusListBindingModel model)
        //{
        //    try
        //    {
        //        using (RiscoContext ctx = new RiscoContext())
        //        {
        //            foreach (var user in model.Users)
        //                ctx.Users.FirstOrDefault(x => x.Id == user.UserId).IsDeleted = user.Status;

        //            ctx.SaveChanges();
        //        }

        //        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}




        //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpGet]
        [Route("GetUsers")]
        public async Task<IHttpActionResult> GetUsers()
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    return Ok(new CustomResponse<SearchUsersViewModel>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = new SearchUsersViewModel
                        {
                            Users = ctx.Users.ToList()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]

        [HttpGet]
        [Route("GetUser")]
        public async Task<IHttpActionResult> GetUser(int UserId, int SignInType)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    BasketSettings.LoadSettings();

                    if (SignInType == (int)RoleTypes.User)
                    {
                        var user = ctx.Users.Include(x => x.Notifications.Select(x1 => x1.AdminNotification)).FirstOrDefault(x => x.Id == UserId);
                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = user });
                    }
                    else
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = "error" });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetAllSipAccounts")]
        public async Task<IHttpActionResult> GetAllSipAccounts()
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var docking = ctx.Docking.Where(x => !x.IsDeleted).ToList();
                    return Ok(new CustomResponse<List<Docking>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = docking });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("SendNotificationToUser")]
        public async Task<IHttpActionResult> SendNotificationToUser(SendNotificationToUserBindingModel model)
        {
            try
            {
                TimeZoneInfo UAETimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time"); DateTime utc = DateTime.UtcNow;
                DateTime UAE = TimeZoneInfo.ConvertTimeFromUtc(utc, UAETimeZone);

                AdminNotifications adminNotification = new AdminNotifications
                {
                    CreatedDate = UAE,
                    TargetAudienceType = (int)NotificationTargetAudienceTypes.IndividualUser,
                    Title = model.Title,
                    Description = model.Text
                };

                using (RiscoContext ctx = new RiscoContext())
                {
                    ctx.AdminNotifications.Add(adminNotification);
                    ctx.SaveChanges();

                    Notification Notification = new Notification
                    {
                        AdminNotification_Id = adminNotification.Id,
                        Title = model.Title,
                        Text = model.Text,
                        User_ID = model.User_Id
                    };

                    ctx.Notifications.Add(Notification);
                    ctx.SaveChanges();

                    var users = ctx.Users.Include(x => x.UserDevices).Where(x => x.IsNotificationsOn && x.IsDeleted == false && x.Id == model.User_Id).ToList();

                    var androidDevices = users.Where(x => x.IsNotificationsOn).SelectMany(x => x.UserDevices.Where(x1 => x1.Platform == true)).ToList();
                    var iosDevices = users.Where(x => x.IsNotificationsOn).SelectMany(x => x.UserDevices.Where(x1 => x1.Platform == false)).ToList();
                    if (androidDevices.Count > 0 || iosDevices.Count > 0)
                    {
                        HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                        {
                            Global.objPushNotifications.SendAndroidPushNotification(androidDevices, adminNotification);
                            Global.objPushNotifications.SendIOSPushNotification(iosDevices, adminNotification);
                        });
                    }
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("AddUpdateFCMToken")]
        public async Task<IHttpActionResult> AddUpdateFCMToken(FCMTokenModel model)
        {
            try
            {
                var userId = Convert.ToInt32(User.GetClaimValue("userid"));
                using (RiscoContext ctx = new RiscoContext())
                {
                    var adminToken = ctx.AdminTokens.FirstOrDefault(x => x.Admin_Id == userId && x.Token == model.Token);

                    if (adminToken != null)
                        adminToken.IsActive = true;
                    else
                        ctx.AdminTokens.Add(new AdminTokens { Admin_Id = userId, Token = model.Token, IsActive = true });

                    ctx.SaveChanges();

                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("TestNotificationToAdmin")]
        public async Task<IHttpActionResult> TestNotificationToAdmin(string Text)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var adminsToPush = ctx.AdminTokens.ToList();
                    var orderUrl = ConfigurationManager.AppSettings["WebsiteBaseUrl"] + "Dashboard/Orders?OrderId=2204";

                    HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                    {
                        Global.objPushNotifications.SendWebGCMPushNotification(adminsToPush, "New Order Received! #2204", "Brush Type A, WaterColor, BrushType D", orderUrl);
                    });
                }

                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize/*("SuperAdmin", "ApplicationAdmin")*/]
        [HttpPost]
        [Route("DeleteUsers")]
        public async Task<IHttpActionResult> DeleteUsers(List<DeleteUserBM> model)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {

                    foreach (var UserModel in model)
                    {
                        var Admin = ctx.Admins.FirstOrDefault(x => x.Id == UserModel.Id && !x.IsDeleted);

                        if (Admin != null)
                        {
                            Admin.IsDeleted = true;
                        }
                        ctx.SaveChanges();
                    }

                }
                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("DeleteFranchises")]
        public async Task<IHttpActionResult> DeleteFranchises(DeleteSelectedBindingModel model)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {

                    foreach (var UserModel in model.SelectedUsers)
                    {
                        var Franchise = ctx.Franchise.FirstOrDefault(x => x.Id == UserModel.Id && !x.IsDeleted);

                        if (Franchise != null)
                        {
                            Franchise.IsDeleted = true;
                        }
                        ctx.SaveChanges();
                    }

                }
                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize("SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("DeleteDockings")]
        public async Task<IHttpActionResult> DeleteDockings(DeleteSelectedBindingModel model)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {

                    foreach (var UserModel in model.SelectedUsers)
                    {
                        var Franchise = ctx.Docking.FirstOrDefault(x => x.Id == UserModel.Id && !x.IsDeleted);

                        if (Franchise != null)
                        {
                            Franchise.IsDeleted = true;
                        }
                        ctx.SaveChanges();
                    }

                }
                return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }




        [HttpGet]
        [Route("GetContextByUserId")]
        public async Task<IHttpActionResult> GetContextByUserId(int UserId)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var context = ctx.Contexts.FirstOrDefault(x => x.User_Id == UserId && !x.IsDeleted);

                    return Ok(new CustomResponse<Contexts> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = context });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetContextById")]
        public async Task<IHttpActionResult> GetContextById(int ContextId)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var context = ctx.Contexts.FirstOrDefault(x => x.Id == ContextId && !x.IsDeleted);

                    return Ok(new CustomResponse<Contexts> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = context });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


    }
}
