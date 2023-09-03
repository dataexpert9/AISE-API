using BasketApi;
using BasketApi.Areas.SubAdmin.Models;
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
using System.Data.Entity;

namespace WebApplication1.Areas.Admin.Controllers
{
    [RoutePrefix("api/Documents")]
    public class DocumentsController : ApiController
    {



        [BasketApi.Authorize/*("SubAdmin", "SuperAdmin", "ApplicationAdmin")*/]

        [HttpPost]
        [Route("AddEditContext")]
        public async Task<IHttpActionResult> AddEditContext(ContextBindingModel model)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Contexts existingContexts = new Contexts();
                Contexts NewContexts = new Contexts();

                using (RiscoContext ctx = new RiscoContext())
                {

                    existingContexts = ctx.Contexts.FirstOrDefault(x => x.User_Id == model.User_Id && !x.IsDeleted);

                    if (existingContexts != null)
                    {
                        existingContexts.ContextText = model.ContextText;
                        existingContexts.ModifiedOn = DateTime.Now;
                    }
                    else
                    {
                        ctx.Contexts.Add(new Contexts
                        {
                            ContextText = model.ContextText,
                            User_Id = model.User_Id,
                            IsDeleted = false,
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now
                        });
                    }


                    ctx.SaveChanges();

                    CustomResponse<string> response = new CustomResponse<string>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = "Record Updated Successfully."
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
        [HttpGet]
        [Route("GetContexts")]
        public async Task<IHttpActionResult> GetContexts(int UserId = 0)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    List<Contexts> contexts = new List<Contexts>();

                    if (UserId == 0)
                    {
                        contexts = ctx.Contexts.Where(x => !x.IsDeleted).ToList();
                    }
                    else
                    {
                        contexts = ctx.Contexts.Where(x => x.User_Id == UserId && !x.IsDeleted).ToList();
                    }


                    CustomResponse<List<DAL.Contexts>> response = new CustomResponse<List<DAL.Contexts>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = contexts
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