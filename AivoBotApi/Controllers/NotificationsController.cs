using BasketApi.CustomAuthorization;
using BasketApi.Models;
using BasketApi.ViewModels;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using WebApplication1.BindingModels;
using static BasketApi.Global;

namespace BasketApi.Controllers
{
    [BasketApi.Authorize("User", "Guest", "Deliverer")]
    [RoutePrefix("api/User")]
    public class NotificationsController : ApiController
    {
       
        /// <summary>
        /// Mark notification as read.
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MarkNotificationAsRead")]
        public async Task<IHttpActionResult> MarkNotificationAsRead(int NotificationId)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var notification = ctx.Notifications.FirstOrDefault(x => x.Id == NotificationId);

                    if (notification != null)
                    {
                        notification.Status = (int)Global.NotificationStatus.Read;
                        ctx.SaveChanges();
                        CustomResponse<string> response = new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK };
                        return Ok(response);
                    }
                    else
                    {
                        CustomResponse<Error> response = new CustomResponse<Error> { Message = Global.ResponseMessages.NotFound, StatusCode = (int)HttpStatusCode.NotFound, Result = new Error { ErrorMessage = "Invalid notificationid" } };
                        return Ok(response);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

      

    }
}
