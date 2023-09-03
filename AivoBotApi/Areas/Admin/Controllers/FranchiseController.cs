using BasketApi;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApplication1.Areas.Admin.Models;
using WebApplication1.BindingModels;
using static BasketApi.Global;

namespace WebApplication1.Areas.Admin.Controllers
{
    [RoutePrefix("api/Franchise")]
    public class FranchiseController : ApiController
    {
        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin", "GeneralUser")]
        [HttpPost]
        [Route("AddEditFranchise")]
        public async Task<IHttpActionResult> AddEditFranchise(FranchiseBindingModel model)
        {
            try
            {
                Validate(model);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Franchise franchise = new Franchise();

                using (RiscoContext ctx = new RiscoContext())
                {
                    if (model.Id == 0)
                    {

                        if (ctx.Franchise.Any(x => x.Name == model.Name && x.IsDeleted == false))
                        {
                            return Content(HttpStatusCode.OK, new CustomResponse<Error>
                            {
                                Message = "Conflict",
                                StatusCode = (int)HttpStatusCode.Conflict,
                                Result = new Error { ErrorMessage = "Franchise with same name already exists" }
                            });
                        }
                    }
                    else
                    {
                        franchise = ctx.Franchise.FirstOrDefault(x => x.Id == model.Id);

                        if (franchise.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) == false)
                        {
                            if (ctx.Franchise.Any(x => x.IsDeleted == false && x.Name.Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "Conflict",
                                    StatusCode = (int)HttpStatusCode.Conflict,
                                    Result = new Error { ErrorMessage = "Franchise with same name already exists" }
                                });
                            }
                        }
                    }

                    if (model.Id == 0)
                    {
                        franchise = ctx.Franchise.Add(new DAL.Franchise
                        {
                           Name=model.Name,
                           Address=model.Address,
                           State=model.State,
                           IsDeleted=false,
                           ZipCode=model.ZipCode,
                           NumberOfEmployees=model.NumberOfEmployees
                        });
                    }
                    else
                    {
                        ctx.Entry(franchise).CurrentValues.SetValues(model);
                    }
                    ctx.SaveChanges();

                    CustomResponse<DAL.Franchise> response = new CustomResponse<DAL.Franchise>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = franchise
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
        [Route("GetAllFranchises")]
        public async Task<IHttpActionResult> GetAllFranchises()
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {

                    CustomResponse<List<DAL.Franchise>> response = new CustomResponse<List<DAL.Franchise>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = ctx.Franchise.Where(x => !x.IsDeleted).ToList()
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
