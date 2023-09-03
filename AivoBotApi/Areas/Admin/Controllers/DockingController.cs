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
    [RoutePrefix("api/Docking")]
    public class DockingController : ApiController
    {

        //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("AddEditDock")]
        public async Task<IHttpActionResult> AddEditDock(DockingBindingModel model)
        {
            try
            {
                Validate(model);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingDock = new Docking();
                var dock = new Docking();

                using (RiscoContext ctx = new RiscoContext())
                {

                    if (model.Admin_Id == 0)
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "Conflict",
                            StatusCode = (int)HttpStatusCode.Conflict,
                            Result = new Error { ErrorMessage = "Select Valid User From DropDown" }
                        });
                    }

                    if (model.Id == 0)
                    {

                        if (ctx.Docking.Any(x => x.ServerIP == model.ServerIP && x.IsDeleted == false))
                        {

                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Server With IP already exists" }
                            });
                        }
                    }
                    else
                    {
                        existingDock = ctx.Docking.FirstOrDefault(x => x.Id == model.Id);
                        model.Password = existingDock.Password;
                        if (existingDock.ServerIP.Equals(model.ServerIP, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            if (ctx.Docking.Any(x => x.IsDeleted == false && x.ServerIP.Equals(model.ServerIP.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Server With IP already exists" }
                                });
                            }
                        }
                    }

                    if (model.Id == 0)
                    {
                        dock = ctx.Docking.Add(new DAL.Docking
                        {
                            ServerIP = model.ServerIP,
                            Admin_Id = model.Admin_Id,
                            Account = model.Account,
                            CellNumber = model.CellNumber,
                            IsDeleted = false,
                            LineName = model.LineName,
                            LineNotes = model.LineNotes,
                            Password = model.Password,
                            Port = model.Port,
                            VoiceCoding = model.VoiceCoding,
                            Status = Convert.ToInt16(Statuses.Active),
                            IsEnabled=false                            
                        });

                        ctx.SaveChanges();

                    }
                    else
                    {
                        ctx.Entry(existingDock).CurrentValues.SetValues(model);
                        ctx.SaveChanges();
                        dock = existingDock;
                    }

                    CustomResponse<DAL.Docking> response = new CustomResponse<DAL.Docking>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = dock
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [BasketApi.Authorize/*("SubAdmin", "SuperAdmin", "ApplicationAdmin")*/]
        /// <summary>
        /// Get Dashboard Stats
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllDockers")]
        public async Task<IHttpActionResult> GetAllDockers()
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {

                    var dockings = ctx.Docking.Include(x => x.Admin).ToList();

                    foreach (var sip in dockings)
                    {
                        sip.AdminName = sip.Admin.Username;
                    }


                    CustomResponse<List<DAL.Docking>> response = new CustomResponse<List<DAL.Docking>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = ctx.Docking.Where(x => !x.IsDeleted).ToList()
                    };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


    }
}
