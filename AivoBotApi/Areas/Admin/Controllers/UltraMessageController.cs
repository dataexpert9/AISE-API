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

namespace BasketApi.Controllers
{
    public class UltraMessageController : ApiController
    {
        //[BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        //[HttpPost]
        //[Route("StartMessagi")]
        //public async Task<IHttpActionResult> ChangeFileStatus(ChangeFileStatus model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        using (RiscoContext ctx = new RiscoContext())
        //        {


        //            var file = ctx.ExcelFile.FirstOrDefault(x => x.Id == model.FileId);

        //            if (file != null)
        //                file.Status = model.Status;

        //            ctx.SaveChanges();
        //            return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}
    }
}