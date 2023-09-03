using DAL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Nexmo.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using BasketApi.CustomAuthorization;
using BasketApi.Models;
using BasketApi.ViewModels;
using System.IO;
using System.Configuration;
using System.Data.Entity;
using System.Net.Mail;
using static BasketApi.Global;
using BasketApi.Components.Helpers;
using System.Web.Hosting;
using System.Web.Http.Cors;
using WebApplication1.BindingModels;
using static BasketApi.Utility;
using System.Text;
using System.Security.Cryptography;

namespace BasketApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private ApplicationUserManager _userManager;

        [Route("all")]
        public IHttpActionResult Getall()
        {
            try
            {
                //var nexmoVerifyResponse = NumberVerify.Verify(new NumberVerify.VerifyRequest { brand = "INGIC", number = "+923325345126" });
                //var nexmoCheckResponse = NumberVerify.Check(new NumberVerify.CheckRequest { request_id = nexmoVerifyResponse.request_id, code = "6310"});
                return Ok("Hello");
            }
            catch (Exception ex)
            {
                return Ok("Hello");
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("Login")]
        [HttpPost]
        public async Task<IHttpActionResult> Login(LoginBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                using (RiscoContext ctx = new RiscoContext())
                {
                    DAL.User userModel;

                    var hashedPassword = CryptoHelper.Hash(model.Password);
                    userModel = ctx.Users.Include(x => x.PaymentCards).FirstOrDefault(x => (x.UserName == model.Username) && x.Password == hashedPassword);

                    if (userModel != null)
                    {


                        await userModel.GenerateToken(Request);
                        BasketSettings.LoadSettings();
                        userModel.BasketSettings = BasketSettings.Settings;
                        if (!String.IsNullOrEmpty(userModel.Interests))
                        {
                            var lstUserIntrests = userModel.Interests.Split(',').Select(int.Parse).ToList();

                        }
                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userModel });
                    }

                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "Forbidden",
                        StatusCode = (int)HttpStatusCode.Forbidden,
                        Result = new Error { ErrorMessage = "Invalid email or password." }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        /// <summary>
        /// Login for web admin panel
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("WebPanelLogin")]
        [HttpPost]
        public async Task<IHttpActionResult> WebPanelLogin(WebLoginBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (RiscoContext ctx = new RiscoContext())
                {
                    DAL.Admin adminModel;
                    var hashedPassword = CryptoHelper.Hash(model.Password);
                    adminModel = ctx.Admins.FirstOrDefault(x => x.Username == model.UserName && x.Password == hashedPassword && x.IsDeleted == false);

                    if (adminModel != null)
                    {
                        await adminModel.GenerateToken(Request);
                        CustomResponse<Admin> response = new CustomResponse<Admin> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = adminModel };
                        return Ok(response);
                    }
                    else
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "Forbidden",
                            StatusCode = (int)HttpStatusCode.Forbidden,
                            Result = new Error { ErrorMessage = "Invalid Email or Password" }
                        });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [AllowAnonymous]
        [Route("Register")]
        [HttpPost]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string ApiToken = String.Empty;
                using (RiscoContext ctx = new RiscoContext())
                {
                    DAL.Admin adminModel;
                    var hashedPassword = CryptoHelper.Hash(model.Password);

                    var existingUser = ctx.Users.FirstOrDefault(x => (x.UserName == model.Username || x.Email==model.Email) && x.IsDeleted != true);


                    Dictionary<string, string> valuePairs = new Dictionary<string, string>();

                    if (existingUser != null)
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "Forbidden",
                            StatusCode = (int)HttpStatusCode.Forbidden,
                            Result = new Error { ErrorMessage = "User with Username or Email already exist." }
                        });
                    else
                    {
                        var user = ctx.Users.Add(new DAL.User
                        {
                            Email = model.Email,
                            UserName = model.Username,
                            EmailConfirmed = false,
                            FullName = model.FullName,
                            IsDeleted = false,
                            Password = hashedPassword,
                            SignInType = Convert.ToInt32(RoleTypes.User),
                            Status = Convert.ToInt16(UserStatus.Active)
                        });

                        await user.GenerateToken(Request,1);
                        if (user.Token != null || user.Token.access_token == null)
                        {
                            Random generator = new Random();
                            string code = generator.Next(0, 1000000).ToString("D6");

                            user.VerifyNumberCodes.Add(new VerifyNumberCodes
                            {
                                User_Id = user.Id,
                                CreatedAt = DateTime.UtcNow,
                                IsDeleted = false,
                                Code = code
                            });


                            user.UserSubscriptions.Add(new UserSubscriptions
                            {
                                ActivationCode = user.Token.access_token,
                                CreatedDate = DateTime.UtcNow,
                                ExpiryDate = DateTime.UtcNow.AddDays(30),
                                IsDeleted = false,
                                Status = (int)UserStatus.Active,
                                SubscriptionDate = DateTime.UtcNow,
                                Type = 0,
                                User_Id = user.Id
                            });

                            //ctx.SaveChanges();

                            valuePairs.Add("EMAIL", model.Email);
                            valuePairs.Add("CODE", code);

                            //sendJoiningEmailtoUser(model.Email, valuePairs);

                            return Ok(new CustomResponse<String> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = "Account has been created.Login to enjoy our services." });
                        }
                        else
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "InternalServerError",
                                StatusCode = (int)HttpStatusCode.InternalServerError,
                                Result = new Error { ErrorMessage = "Something Went Wrong! Try Again. Or Contact Administrator." }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        private int SavePicture(HttpRequestMessage request, out string PicturePath)
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                PicturePath = String.Empty;

                if (httpRequest.Files.Count > 1)
                    return 3;

                foreach (string file in httpRequest.Files)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                    var postedFile = httpRequest.Files[file];
                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                        IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                        var ext = Path.GetExtension(postedFile.FileName);
                        var extension = ext.ToLower();
                        if (!AllowedFileExtensions.Contains(extension))
                        {
                            var message = string.Format("Please Upload image of type .jpg,.gif,.png.");
                            return 1;
                        }
                        else if (postedFile.ContentLength > MaxContentLength)
                        {

                            var message = string.Format("Please Upload a file upto 1 mb.");
                            return 2;
                        }
                        else
                        {
                            int count = 1;
                            string fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                            string newFullPath = HttpContext.Current.Server.MapPath("~/App_Data/" + postedFile.FileName);

                            while (File.Exists(newFullPath))
                            {
                                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                                newFullPath = HttpContext.Current.Server.MapPath("~/App_Data/" + tempFileName + extension);
                            }

                            postedFile.SaveAs(newFullPath);
                            PicturePath = newFullPath;
                        }
                    }
                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut(OAuthDefaults.AuthenticationType);
            return Ok();
        }

        [Authorize]
        [Route("MarkVerified")]
        [HttpPost]
        public IHttpActionResult MarkUserAccountAsVerified(UserModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                using (RiscoContext ctx = new RiscoContext())
                {
                    var userModel = ctx.Users.FirstOrDefault(x => x.Email == model.Email);
                    if (userModel == null)
                        return BadRequest("User account doesn't exist");

                    userModel.Status = (int)Global.StatusCode.Verified;
                    ctx.SaveChanges();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        /// <summary>
        /// Send verification code to user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [Authorize]
        [HttpPost]
        [Route("SendVerificationSms")]
        public async Task<IHttpActionResult> SendVerificationSms(PhoneBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = Convert.ToInt32(User.GetClaimValue("userid"));

                using (RiscoContext ctx = new RiscoContext())
                {
                    var user = ctx.Users.FirstOrDefault(x => x.Id == userId);
                    //user.Phone = model.PhoneNumber;

                    if (ctx.Users.Any(x => x.Id != user.Id && x.Phone == model.PhoneNumber))
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "Conflict",
                            StatusCode = (int)HttpStatusCode.Conflict,
                            Result = new Error { ErrorMessage = "User with entered phone number already exists." }
                        });
                    }


                    if (user != null)
                    {
                        var codeInt = new Random().Next(111111, 999999);

                        await ctx.VerifyNumberCodes.Where(x => x.User_Id == user.Id).ForEachAsync(x => x.IsDeleted = true);
                        await ctx.SaveChangesAsync();
                        user.VerifyNumberCodes.Add(new VerifyNumberCodes { Phone = model.PhoneNumber, CreatedAt = DateTime.Now, IsDeleted = false, User_Id = user.Id, Code = codeInt.ToString() });
                        ctx.SaveChanges();

                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = user });

                        //var results = SMS.Send(new SMS.SMSRequest
                        //{
                        //    from = "Skribl",
                        //    title = "Skribl",
                        //    to = model.PhoneNumber,
                        //    text = "Verification Code : " + codeInt
                        //});

                        //if (results.messages.First().status == "0")
                        //{
                        //    return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = user });
                        //}
                        //else
                        //{
                        //    using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "/ErrorLog.txt"))
                        //    {
                        //        sw.WriteLine("neximo error : " + DateTime.Now + Environment.NewLine);

                        //        sw.WriteLine(Environment.NewLine + "Status" + results.messages.First().status);
                        //        sw.WriteLine(Environment.NewLine + "RemainingBalance" + results.messages.First().remaining_balance);
                        //        sw.WriteLine(Environment.NewLine + "MessagePrice" + results.messages.First().message_price);
                        //        sw.WriteLine(Environment.NewLine + "ErrorText" + results.messages.First().error_text);
                        //        sw.WriteLine(Environment.NewLine + "ClientRef" + results.messages.First().client_ref);
                        //    }
                        //    return Content(HttpStatusCode.OK, new CustomResponse<Error> { Message = "InternalServerError", StatusCode = (int)HttpStatusCode.InternalServerError, Result = new Error { ErrorMessage = "SMS failed due to some reason." } });
                        //}
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = "NotFound", StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = "User with entered phone number doesn’t exist." } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [Authorize]
        [Route("ChangeForgotPassword")]
        public async Task<IHttpActionResult> ChangeForgotPassword(SetForgotPasswordBindingModel model)
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
                    var user = ctx.Users.FirstOrDefault(x => x.Email == userEmail && x.IsDeleted == false);
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

        /// <summary>
        /// Verify code sent to user. 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        //[HttpPost]
        //[Route("VerifySmsCode")]
        //public IHttpActionResult VerifySmsCode(PhoneVerificationModel model)
        //{
        //	try
        //	{
        //		var userEmail = User.Identity.Name;

        //		if (string.IsNullOrEmpty(userEmail))
        //			throw new Exception("User Email is empty in user.identity.name.");
        //		if (!ModelState.IsValid)
        //			return BadRequest(ModelState);

        //		using (RiscoContext ctx = new RiscoContext())
        //		{
        //			var userModel = ctx.Users.Include(x => x.VerifyNumberCodes).FirstOrDefault(x => x.Id == model.UserId);
        //			BasketSettings.LoadSettings();
        //			userModel.BasketSettings = BasketSettings.Settings;
        //			userModel.GenerateToken(Request);

        //			var codeEntry = userModel.VerifyNumberCodes.FirstOrDefault(x => x.Code == model.Code && x.IsDeleted == false && DateTime.Now.Subtract(x.CreatedAt).Minutes < 60);
        //			if (codeEntry != null)
        //			{
        //				userModel.Phone = codeEntry.Phone;
        //				userModel.PhoneConfirmed = true;
        //				codeEntry.IsDeleted = true;
        //				ctx.SaveChanges();
        //				return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userModel });
        //			}
        //			else
        //				return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.Forbidden, Result = new Error { ErrorMessage = "Invalid code" } });
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		return StatusCode(Utility.LogError(ex));
        //	}
        //}

        [AllowAnonymous]
        [HttpGet]
        [Route("VerifyUserCode")]
        public async Task<IHttpActionResult> VerifyUserCode(int userId, int code)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var user = ctx.Users.Include(x => x.ForgotPasswordTokens).FirstOrDefault(x => x.Id == userId);

                    if (user == null)
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid UserId" } });

                    var token = user.ForgotPasswordTokens.FirstOrDefault(x => x.Code == Convert.ToString(code) && x.IsDeleted == false && DateTime.UtcNow.Subtract(x.CreatedAt).Minutes < 11);

                    if (token != null)
                    {
                        token.IsDeleted = true;
                        ctx.SaveChanges();
                        await user.GenerateToken(Request);
                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = user });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid code" } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [Route("UploadCoverImage")]
        [Authorize]
        public async Task<IHttpActionResult> UploadCoverImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                #region Validations
                var userEmail = User.Identity.Name;
                if (string.IsNullOrEmpty(userEmail))
                {
                    throw new Exception("User Email is empty in user.identity.name.");
                }
                else if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request." }
                    });
                }
                else if (httpRequest.Files.Count == 0)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "NotFound",
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Result = new Error { ErrorMessage = "Image not found, please upload an image." }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not allowed. Please upload 1 image." }
                    });
                }
                #endregion

                var postedFile = httpRequest.Files[0];

                if (postedFile != null && postedFile.ContentLength > 0)
                {

                    int MaxContentLength = 1024 * 1024 * 10; //Size = 1 MB  

                    IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    var extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "UnsupportedMediaType",
                            StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                            Result = new Error { ErrorMessage = "Please Upload image of type .jpg, .gif, .png." }
                        });
                    }
                    else if (postedFile.ContentLength > MaxContentLength)
                    {

                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "UnsupportedMediaType",
                            StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                            Result = new Error { ErrorMessage = "Please Upload a file upto 1 mb." }
                        });
                    }
                    else
                    {
                        int count = 1;
                        fileNameOnly = Path.GetFileNameWithoutExtension(postedFile.FileName);
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["UserImageFolderPath"] + postedFile.FileName);

                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["UserImageFolderPath"] + tempFileName + extension);
                        }
                        postedFile.SaveAs(newFullPath);
                    }
                }

                MessageViewModel successResponse = new MessageViewModel { StatusCode = "200 OK", Details = "Image Updated Successfully." };
                //var filePath = Utility.BaseUrl + ConfigurationManager.AppSettings["UserImageFolderPath"] + Path.GetFileName(newFullPath);
                var filePath = ConfigurationManager.AppSettings["UserImageFolderPath"] + Path.GetFileName(newFullPath);
                ImagePathViewModel model = new ImagePathViewModel { Path = filePath };

                using (RiscoContext ctx = new RiscoContext())
                {
                    ctx.Users.FirstOrDefault(x => x.Email == userEmail).CoverPictureUrl = filePath;
                    ctx.SaveChanges();
                }

                return Content(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [Route("AddExternalLogin")]
        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

                if (ticket == null || ticket.Identity == null || (ticket.Properties != null
                    && ticket.Properties.ExpiresUtc.HasValue
                    && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
                {
                    return BadRequest("External login failure.");
                }

                ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

                if (externalData == null)
                {
                    return BadRequest("The external login is already associated with an account.");
                }

                IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
                    new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

                if (!result.Succeeded)
                {
                    //return GetErrorResult(result);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        /// <summary>
        /// Update user profile with image. This is multipart request. SignInType 0 for user, 1 for deliverer
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("UpdateUserProfileImage")]
        public async Task<IHttpActionResult> UpdateUserProfileImage()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string fileNameOnly = string.Empty;

                var userId = Convert.ToInt32(User.GetClaimValue("userid"));

                #region Validations
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multipart data is not included in request." }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple images are not supported, please upload one image." }
                    });
                }
                #endregion

                using (RiscoContext ctx = new RiscoContext())
                {
                    User userModel;

                    HttpPostedFile postedFile = null;
                    string fileExtension = string.Empty;

                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {

                            IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                            //var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                            var ext = Path.GetExtension(postedFile.FileName);
                            fileExtension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(fileExtension))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload image of type .jpg,.gif,.png." }
                                });
                            }
                            else if (postedFile.ContentLength > Global.MaximumImageSize)
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload a file upto " + Global.ImageSize + "." }
                                });
                            }
                        }
                    }
                    #endregion

                    userModel = ctx.Users.FirstOrDefault(x => x.Id == userId);

                    if (userModel == null)
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "NotFound",
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Result = new Error { ErrorMessage = "UserId does not exist." }
                        });
                    }
                    else
                    {
                        if (httpRequest.Files.Count > 0)
                        {
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["UserImageFolderPath"] + userModel.Id + fileExtension);
                            postedFile.SaveAs(newFullPath);
                            userModel.ProfilePictureUrl = ConfigurationManager.AppSettings["UserImageFolderPath"] + userModel.Id + fileExtension;
                        }

                        ctx.SaveChanges();

                        await userModel.GenerateToken(Request);
                        BasketSettings.LoadSettings();
                        userModel.BasketSettings = BasketSettings.Settings;

                        CustomResponse<User> response = new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userModel };
                        return Ok(response);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [Authorize]
        [HttpPost]
        [Route("SetAccountSettings")]
        public async Task<IHttpActionResult> SetAccountSettings(AccountSettingsBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = Convert.ToInt32(User.GetClaimValue("userid"));

                using (RiscoContext ctx = new RiscoContext())
                {
                    User userModel = ctx.Users.FirstOrDefault(x => x.Id == userId);

                    if (userModel.FullName != model.FullName)
                    {
                        if (ctx.Users.Any(x => x.FullName == model.FullName))
                        {
                            return Ok(new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "User Name already exists." }
                            });
                        }
                    }
                    else
                    {
                        userModel.FullName = model.FullName;
                        userModel.Language = model.Language;
                        userModel.IsLoginVerification = model.IsLoginVerification;
                        userModel.CountryCode = model.CountryCode;
                        userModel.AboutMe = model.AboutMe;
                        userModel.IsVideoAutoPlay = model.IsVideoAutoPlay;
                        userModel.Interests = model.Interests;

                        ctx.SaveChanges();
                        CustomResponse<User> response = new CustomResponse<User>
                        {
                            Message = Global.ResponseMessages.Success,
                            StatusCode = (int)HttpStatusCode.OK,
                            Result = userModel
                        };
                        return Ok(response);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("ExternalLogin")]
        public async Task<IHttpActionResult> ExternalLogin(string accessToken, int? socialLoginType)
        {
            try
            {
                if (socialLoginType.HasValue && !string.IsNullOrEmpty(accessToken))
                {
                    SocialLogins socialLogin = new SocialLogins();
                    // send access token and social login type to GetSocialUserData in return it will give you full name email and profile picture of user 
                    var socialUser = await socialLogin.GetSocialUserData(accessToken, (SocialLogins.SocialLoginType)socialLoginType.Value);

                    if (socialUser != null)
                    {
                        using (RiscoContext ctx = new RiscoContext())
                        {

                            // if user have privacy on his / her email then we will create email address from his user Id which will be send by mobile developer 
                            if (string.IsNullOrEmpty(socialUser.email))
                            {
                                socialUser.email = socialUser.id + "@gmail.com";
                            }
                            var existingUser = ctx.Users.Include(x => x.UserSubscriptions).Include(x => x.PaymentCards).FirstOrDefault(x => x.Email == socialUser.email);

                            if (existingUser != null)
                            {
                                // if user already have registered through social login them wee will always check his picture and name just to get updated values of that user from facebook 
                                existingUser.ProfilePictureUrl = socialUser.picture;
                                existingUser.FullName = socialUser.name;
                                ctx.SaveChanges();
                                await existingUser.GenerateToken(Request);
                                BasketSettings.LoadSettings();
                                existingUser.BasketSettings = BasketSettings.Settings;
                                CustomResponse<User> response = new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = existingUser };
                                return Ok(response);
                            }
                            else
                            {
                                int SignInType = 0;
                                if (socialLoginType.Value == (int)BasketApi.SocialLogins.SocialLoginType.Google)
                                {
                                    SignInType = (int)Utility.SocialLoginType.Google;
                                }
                                else if (socialLoginType.Value == (int)BasketApi.SocialLogins.SocialLoginType.Facebook)
                                    SignInType = (int)Utility.SocialLoginType.Facebook;


                                var newUser = new User { FullName = socialUser.name, Email = socialUser.email, ProfilePictureUrl = socialUser.picture, SignInType = SignInType, Status = 1, IsNotificationsOn = true };
                                ctx.Users.Add(newUser);
                                ctx.SaveChanges();
                                await newUser.GenerateToken(Request);
                                BasketSettings.LoadSettings();
                                newUser.BasketSettings = BasketSettings.Settings;

                                HostingEnvironment.QueueBackgroundWorkItem(cancellationToken =>
                                {
                                    //sendJoiningEmail(socialUser.email);
                                });
                                return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = newUser });
                            }
                        }
                    }
                    else
                        return BadRequest("Unable to get user info");
                }
                else
                    return BadRequest("Please provide access token along with social login type");
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        /// <summary>
        /// Contact us
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ContactUs")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ContactUs(ContactUsBindingModel model)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    //var userSubscriptions = ctx.UserSubscriptions.Include(x => x.Box).Where(x => x.User_Id == model.UserId && x.Box.ReleaseDate.Month == model.Month && x.Box.ReleaseDate.Year == model.Year).ToList();

                    //if (userSubscriptions.Count == 0)
                    //{
                    //    return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.OK, Result = new Error { ErrorMessage = "You can't provide feedback for the selected box." } });
                    //}

                    if (!String.IsNullOrEmpty(model.Description))
                    {
                        if (model.UserId.HasValue)
                            ctx.ContactUs.Add(new ContactUs { UserId = model.UserId.Value, Description = model.Description });
                        else
                            ctx.ContactUs.Add(new ContactUs { Description = model.Description });
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = Global.ResponseMessages.CannotBeEmpty("Description") } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        /// <summary>
        /// An email will be sent to user containing password.
        /// </summary>
        /// <param name="Email">Email of user.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ResetPasswordThroughEmail")]
        public async Task<IHttpActionResult> ResetPasswordThroughEmail(string Email)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var user = ctx.Users.FirstOrDefault(x => x.Email == Email);

                    if (user != null)
                    {
                        var codeInt = new Random().Next(111111, 999999);

                        string subject = "Reset your password - " + EmailUtil.FromName;
                        const string body = "Use this code as current password";

                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(EmailUtil.FromMailAddress.Address, EmailUtil.FromPassword)
                        };

                        var message = new MailMessage(EmailUtil.FromMailAddress, new MailAddress(Email))
                        {
                            Subject = subject,
                            Body = body + " " + codeInt
                        };

                        smtp.Send(message);

                        user.Password = CryptoHelper.Hash(codeInt.ToString());
                        //user.ForgotPasswordTokens.Add(new ForgotPasswordToken { CreatedAt = DateTime.UtcNow, IsDeleted = false, User_ID = user.Id, Code = Convert.ToString(codeInt) });
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = user });
                    }
                    else
                    {
                        return Ok(new CustomResponse<Error> { Message = "NotFound", StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = "User with entered email doesn’t exist." } });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        /// <summary>
        /// Register for getting push notifications
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("RegisterPushNotification")]
        public async Task<IHttpActionResult> RegisterPushNotification(RegisterPushNotificationBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                using (RiscoContext ctx = new RiscoContext())
                {
                    var user = ctx.Users.Include(x => x.UserDevices).FirstOrDefault(x => x.Id == model.User_Id);
                    if (user != null)
                    {
                        var existingUserDevice = user.UserDevices.FirstOrDefault(x => x.UDID.Equals(model.UDID));
                        if (existingUserDevice == null)
                        {
                            //foreach (var userDevice in user.UserDevices)
                            //    userDevice.IsActive = false;

                            var userDeviceModel = new UserDevice
                            {
                                Platform = model.IsAndroidPlatform,
                                ApplicationType = model.IsPlayStore ? UserDevice.ApplicationTypes.PlayStore : UserDevice.ApplicationTypes.Enterprise,
                                EnvironmentType = model.IsProduction ? UserDevice.ApnsEnvironmentTypes.Production : UserDevice.ApnsEnvironmentTypes.Sandbox,
                                UDID = model.UDID,
                                AuthToken = model.AuthToken,
                                IsActive = true
                            };

                            PushNotificationsUtil.ConfigurePushNotifications(userDeviceModel);

                            user.UserDevices.Add(userDeviceModel);
                            ctx.SaveChanges();
                            return Ok(new CustomResponse<UserDevice> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userDeviceModel });
                        }
                        else
                        {
                            //foreach (var userDevice in user.UserDevices)
                            //    userDevice.IsActive = false;

                            existingUserDevice.Platform = model.IsAndroidPlatform;
                            existingUserDevice.ApplicationType = model.IsPlayStore ? UserDevice.ApplicationTypes.PlayStore : UserDevice.ApplicationTypes.Enterprise;
                            existingUserDevice.EnvironmentType = model.IsProduction ? UserDevice.ApnsEnvironmentTypes.Production : UserDevice.ApnsEnvironmentTypes.Sandbox;
                            existingUserDevice.UDID = model.UDID;
                            existingUserDevice.AuthToken = model.AuthToken;
                            existingUserDevice.IsActive = true;
                            ctx.SaveChanges();
                            PushNotificationsUtil.ConfigurePushNotifications(existingUserDevice);
                            return Ok(new CustomResponse<UserDevice> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = existingUserDevice });
                        }
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = Global.ResponseMessages.GenerateNotFound("User") } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public async Task<IHttpActionResult> GetUser(int UserId)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var userModel = ctx.Users.Include(x => x.PaymentCards).FirstOrDefault(x => x.Id == UserId && x.IsDeleted == false);

                    if (userModel != null)
                    {


                        BasketSettings.LoadSettings();
                        await userModel.GenerateToken(Request);
                        userModel.BasketSettings = BasketSettings.Settings;
                        if (!String.IsNullOrEmpty(userModel.Interests))
                        {
                            var lstUserIntrests = userModel.Interests.Split(',').Select(int.Parse).ToList();

                        }
                        return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userModel });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new Error { ErrorMessage = "Invalid UserId" } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("GetContext")]
        public async Task<IHttpActionResult> GetContext(GetContextBindingModel model)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var Context = ctx.Contexts.Include(x => x.User).FirstOrDefault(x => x.User.UserName == model.Token && !x.IsDeleted);

                    if (Context != null)
                    {
                        Context.User = null;
                        return Ok(new CustomResponse<Contexts> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = Context });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new Error { ErrorMessage = "Invalid UserId" } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        //[Authorize]
        //[HttpGet]
        //[Route("GetUserContext")]
        //public async Task<IHttpActionResult> GetUserContext(string UserKey)
        //{
        //	try
        //	{
        //		using (RiscoContext ctx = new RiscoContext())
        //		{
        //			var userModel = ctx.Users.Include(x => x.PaymentCards).FirstOrDefault(x => x.Id == UserId && x.IsDeleted == false);

        //			if (userModel != null)
        //			{


        //				BasketSettings.LoadSettings();
        //				await userModel.GenerateToken(Request);
        //				userModel.BasketSettings = BasketSettings.Settings;
        //				if (!String.IsNullOrEmpty(userModel.Interests))
        //				{
        //					var lstUserIntrests = userModel.Interests.Split(',').Select(int.Parse).ToList();

        //				}
        //				return Ok(new CustomResponse<User> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = userModel });
        //			}
        //			else
        //				return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new Error { ErrorMessage = "Invalid UserId" } });
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		return StatusCode(Utility.LogError(ex));
        //	}
        //}



        [HttpGet]
        [Route("MarkDeviceAsInActive")]
        public async Task<IHttpActionResult> MarkDeviceAsInActive(int UserId, int DeviceId)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var device = ctx.UserDevices.FirstOrDefault(x => x.Id == DeviceId && x.User_Id == UserId);
                    if (device != null)
                    {
                        device.IsActive = false;
                        ctx.SaveChanges();
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });
                    }
                    else
                        return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = new Error { ErrorMessage = "Invalid UserId or DeviceId." } });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //public async Task sendJoiningEmail(string joinerEmail)
        //{
        //	try
        //	{
        //		string subject = "New User Signed Up - " + EmailUtil.FromName;
        //		string body = "A new user " + joinerEmail + " has just signed up";

        //		var smtp = new SmtpClient
        //		{
        //			Host = "smtp.gmail.com",
        //			Port = 587,
        //			EnableSsl = true,
        //			DeliveryMethod = SmtpDeliveryMethod.Network,
        //			UseDefaultCredentials = false,
        //			Credentials = new NetworkCredential(EmailUtil.FromMailAddress.Address, EmailUtil.FromPassword)
        //		};

        //		var AdminEmail = BasketSettings.GetAdminEmailForOrders();

        //		var message = new MailMessage(EmailUtil.FromMailAddress, new MailAddress(AdminEmail))
        //		{
        //			Subject = subject,
        //			Body = body
        //		};

        //		smtp.Send(message);

        //		//sendJoiningEmailtoUser(joinerEmail);
        //	}
        //	catch (Exception ex)
        //	{
        //		Utility.LogError(ex);
        //	}
        //}

        public async Task sendJoiningEmailtoUser(string joinerEmail, Dictionary<string, string> valuePairs)
        {
            try
            {
                var body = GetEmailTemplate(EmailUtil.EmailTypes.Register, valuePairs);




                var smtpClient = new SmtpClient("aise.aivo.asia")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("support@aise.aivo.asia", "Aisesupport888#"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("support@aise.aivo.asia"),
                    Subject = "subject",
                    Body = "<h1>Hello</h1>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add("dunkeydelivery1@gmail.com");

                smtpClient.Send(mailMessage);


                //var smtp = new SmtpClient
                //{
                //    Host = "mail.aise.aivo.asia",
                //    Port = 26,
                //    EnableSsl = false,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //    UseDefaultCredentials = true,
                //    Credentials = new NetworkCredential(EmailUtil.FromMailAddress.Address, EmailUtil.FromPassword)
                //};

                ////var message = new MailMessage(EmailUtil.FromMailAddress, new MailAddress(joinerEmail))
                ////{
                ////    Subject = "Welcome to AISE",
                ////    Body = body
                ////};
                //using (MailMessage mail = new MailMessage())
                //{
                //    mail.From = new MailAddress(EmailUtil.FromMailAddress.ToString());
                //    mail.To.Add(valuePairs.First(x => x.Key == "EMAIL").Value);
                //    mail.Subject = "Welcome to AISE";
                //    mail.Body = body;
                //    mail.IsBodyHtml = true;
                //    //mail.Attachments.Add(new Attachment("C:\\file.zip"));


                //    smtp.Send(mail);
                //}


            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }
        public String GetEmailTemplate(EmailUtil.EmailTypes emailType, Dictionary<string, string> valuePairs)
        {

            string body = string.Empty;
            string FilePath = string.Empty;
            //using streamreader for reading my htmltemplate   

            switch (emailType)
            {
                case EmailUtil.EmailTypes.Register:
                    FilePath = Path.Combine(HttpRuntime.AppDomainAppPath, "EmailTemplates/RegisterEmailTemplate.html");
                    break;
                default:
                    break;
            }

            using (StreamReader reader = new StreamReader(FilePath))
            {
                body = reader.ReadToEnd();

                foreach (var item in valuePairs)
                {
                    body.Replace("{" + item.Key + "}", item.Value);
                }
            }

            return body;
        }
        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        [Authorize]
        [HttpPost]
        [Route("SetPrivacySettings")]
        public async Task<IHttpActionResult> SetPrivacySettings(PrivacySettingsBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                using (RiscoContext ctx = new RiscoContext())
                {
                    var userId = Convert.ToInt32(User.GetClaimValue("userid"));

                    User UserResult = ctx.Users.FirstOrDefault(x => x.Id == userId);

                    UserResult.IsPostLocation = model.IsPostLocation;
                    UserResult.TaggingPrivacy = model.TaggingPrivacy;
                    UserResult.FindByEmail = model.FindByEmail;
                    UserResult.FindByPhone = model.FindByPhone;
                    UserResult.MessagePrivacy = model.MessagePrivacy;
                    ctx.SaveChanges();

                    CustomResponse<User> response = new CustomResponse<User>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = UserResult
                    };

                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[Authorize]
        //[HttpPost]
        //[Route("SetNotificationSettings")]
        //public async Task<IHttpActionResult> SetNotificationSettings(NotificationSettingsBindingModel model)
        //{
        //	try
        //	{
        //		if (!ModelState.IsValid)
        //		{
        //			return BadRequest(ModelState);
        //		}
        //		using (RiscoContext ctx = new RiscoContext())
        //		{
        //			var userId = Convert.ToInt32(User.GetClaimValue("userid"));

        //			User UserResult = ctx.Users.FirstOrDefault(x => x.Id == userId);

        //			UserResult.MuteUnverifiedEmail = model.MuteUnverifiedEmail;
        //			UserResult.MuteUnverifiedPhone = model.MuteUnverifiedPhone;

        //			ctx.SaveChanges();

        //			CustomResponse<User> response = new CustomResponse<User>
        //			{
        //				Message = Global.ResponseMessages.Success,
        //				StatusCode = (int)HttpStatusCode.OK,
        //				Result = UserResult
        //			};

        //			return Ok(response);
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		return StatusCode(Utility.LogError(ex));
        //	}
        //}
    }
}
