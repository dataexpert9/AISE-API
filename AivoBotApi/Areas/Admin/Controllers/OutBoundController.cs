using BasketApi;
using BasketApi.Areas.SubAdmin.Models;
using DAL;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using static BasketApi.Global;
using WebApplication1.Areas.Admin.Models;
using BasketApi.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [RoutePrefix("api/OutBound")]
    public class OutBoundController : ApiController
    {

        [HttpPost]
        [Route("AddEditUploadFile")]
        public async Task<IHttpActionResult> AddEditUploadFile()
        {
            try
            {

                OutBoundFileImportBindingModel model = new OutBoundFileImportBindingModel();

                var httpRequest = HttpContext.Current.Request;
                string newFullPath = string.Empty;
                string FilePath = string.Empty;
                string Extenion = string.Empty;
                string UniqueFileName = string.Empty;


                List<OutBoundCalls> outBounds = new List<OutBoundCalls>();
                OutBoundCalls outBoundObj = new OutBoundCalls();

                UploadedFiles uploadedFiles = new UploadedFiles();

                if (httpRequest.Params["Id"] != null)
                    model.Admin_Id = Convert.ToInt32(httpRequest.Params["Id"]);

                if (httpRequest.Params["UserName"] != null)
                    model.UserName = Convert.ToString(httpRequest.Params["UserName"]);

                if (httpRequest.Params["Franchise_Id"] != null)
                    model.Franchise_Id = Convert.ToInt32(httpRequest.Params["Franchise_Id"]);

                Validate(model);

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
                        Result = new Error { ErrorMessage = "Multipart data is not included in request" }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple files are not supported, please upload one file" }
                    });
                }
                #endregion

                using (RiscoContext ctx = new RiscoContext())
                {

                    HttpPostedFile postedFile = null;
                    #region ImageSaving
                    if (httpRequest.Files.Count > 0)
                    {
                        postedFile = httpRequest.Files[0];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {
                            IList<string> AllowedFileExtensions = new List<string> { ".xlsx", ".csv", ".xls" };
                            var ext = Path.GetExtension(postedFile.FileName);
                            ext = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(ext))
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload file of type .xlsx,.csv,.xls" }
                                });
                            }
                            else if (postedFile.ContentLength > Global.MaximumImageSize)
                            {
                                return Content(HttpStatusCode.OK, new CustomResponse<Error>
                                {
                                    Message = "UnsupportedMediaType",
                                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                                    Result = new Error { ErrorMessage = "Please Upload a file upto " + Global.ImageSize }
                                });
                            }
                            else
                            {
                                int count = 1;

                                Extenion = Path.GetExtension(postedFile.FileName);
                                FilePath = "~/" + ConfigurationManager.AppSettings["OutBoundCallUploadPath"] + postedFile.FileName;
                                UniqueFileName = Guid.NewGuid().ToString("N");
                                newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["OutBoundCallUploadPath"] + UniqueFileName + Extenion);


                                while (File.Exists(newFullPath))
                                {
                                    string tempFileName = string.Format("{0}({1})", UniqueFileName, count++);
                                    newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["OutBoundCallUploadPath"] + UniqueFileName + Extenion);
                                }
                                postedFile.SaveAs(newFullPath);
                            }
                        }
                        //model.ImageUrl = ConfigurationManager.AppSettings["AdminImageFolderPath"] + Path.GetFileName(newFullPath);
                    }
                    #endregion

                    if (model.Id == 0)
                    {
                        uploadedFiles = ctx.UploadedFiles.Add(new UploadedFiles
                        {
                            FilePath = FilePath,
                        });
                        ctx.SaveChanges();


                        using (var stream = File.Open(newFullPath, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = ExcelReaderFactory.CreateReader(stream))
                            {

                                DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                                    {
                                        UseHeaderRow = true
                                    }
                                });

                                DataTable dataTable = result.Tables[0];


                                foreach (DataRow row in dataTable.Rows)
                                {
                                    outBoundObj = new OutBoundCalls();
                                    foreach (DataColumn column in dataTable.Columns)
                                    {
                                        if (column.ToString().Contains("Number"))
                                            outBoundObj.PhoneNumber = row[column].ToString();

                                        if (column.ToString().Contains("Source"))
                                            outBoundObj.Source = row[column].ToString();

                                        if (column.ToString().Contains("Name"))
                                            outBoundObj.Name = row[column].ToString();

                                        if (column.ToString().Contains("Gender"))
                                            outBoundObj.Gender = row[column].ToString();

                                        if (column.ToString().Contains("Company"))
                                            outBoundObj.Company = row[column].ToString();

                                        if (column.ToString().Contains("Classification"))
                                            outBoundObj.Classification = row[column].ToString();

                                        if (column.ToString().Contains("Address"))
                                            outBoundObj.Address = row[column].ToString();

                                        if (column.ToString().Contains("Note 1"))
                                            outBoundObj.Note1 = row[column].ToString();

                                        if (column.ToString().Contains("Note 2"))
                                            outBoundObj.Note2 = row[column].ToString();

                                        if (column.ToString().Contains("Note 3"))
                                            outBoundObj.Note3 = row[column].ToString();

                                        if (column.ToString().Contains("Note 4"))
                                            outBoundObj.Note4 = row[column].ToString();

                                        if (column.ToString().Contains("Note 5"))
                                            outBoundObj.Note5 = row[column].ToString();


                                        outBoundObj.IsDeleted = false;
                                        outBoundObj.State = Convert.ToInt32(CallStatuses.NotRegistered);
                                        outBoundObj.UploadedDate = DateTime.UtcNow;
                                        outBoundObj.Status = true;
                                        outBoundObj.UploadedFiles_Id = uploadedFiles.Id;

                                        if (model.Franchise_Id != 0)
                                            outBoundObj.Franchise_Id = model.Franchise_Id;

                                        outBoundObj.Admin_Id = model.Admin_Id;

                                    }
                                    outBounds.Add(outBoundObj);
                                    outBoundObj = null;
                                }

                                ctx.OutBoundCalls.AddRange(outBounds);
                                ctx.SaveChanges();
                            }
                        }
                    }

                    CustomResponse<string> response = new CustomResponse<string>
                    {
                        Message = ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = "File Uploaded Successfully."
                    };

                    return Ok(response);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetOutBoundCalls")]
        public async Task<IHttpActionResult> GetOutBoundCalls(int Franchise_Id)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var NotRegEnum = Convert.ToInt32(CallStatuses.NotRegistered);

                    if (Franchise_Id != 0)
                        return Ok(new CustomResponse<List<DAL.OutBoundCalls>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.OutBoundCalls.Where(x => x.Franchise_Id == Franchise_Id && x.State<= NotRegEnum && x.IsDeleted == false).ToList() });
                    else
                        return Ok(new CustomResponse<List<DAL.OutBoundCalls>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.OutBoundCalls.Where(x =>  x.State <= NotRegEnum && x.IsDeleted == false).ToList() });

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [HttpGet]
        [Route("GetDialPlans")]
        public async Task<IHttpActionResult> GetDialPlans(int? Franchise_Id)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    List<OutBoundCallDialPlan> outBoundCallsDialPlan = new List<OutBoundCallDialPlan>();
                    List<OutBoundCalls> outBoundCalls = new List<OutBoundCalls>();


                    if (Franchise_Id.Value != 0)
                        outBoundCalls = ctx.OutBoundCalls.Include(x => x.OutBoundCallDialPlan).Where(x => x.Franchise_Id == Franchise_Id.Value && !x.IsDeleted).ToList();
                    else
                        outBoundCalls = ctx.OutBoundCalls.Include(x => x.OutBoundCallDialPlan).Where(x => !x.IsDeleted).ToList();

                    if (outBoundCalls.Count > 0)
                    {
                        if (Franchise_Id.Value != 0)
                            outBoundCallsDialPlan = ctx.OutBoundCallDialPlan.Where(x => x.Franchise_Id == Franchise_Id.Value && !x.IsDeleted).ToList();

                        else
                            outBoundCallsDialPlan = ctx.OutBoundCallDialPlan.Where(x => !x.IsDeleted).ToList();


                    }

                    return Ok(new CustomResponse<List<DAL.OutBoundCallDialPlan>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = outBoundCallsDialPlan });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        
        [HttpGet]
        [Route("GetDialedList")]
        public async Task<IHttpActionResult> GetDialedList(int? Franchise_Id)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    List<OutBoundCallDialPlan> outBoundCallsDialPlan = new List<OutBoundCallDialPlan>();
                    List<OutBoundCalls> outBoundCalls = new List<OutBoundCalls>();
                    var NotRegEnum=Convert.ToInt32(CallStatuses.NotRegistered);

                    if (Franchise_Id.Value != 0)
                        outBoundCalls = ctx.OutBoundCalls.Include(x => x.OutBoundCallDialPlan).Where(x => x.Franchise_Id == Franchise_Id.Value && x.State > NotRegEnum && !x.IsDeleted).ToList();
                    else
                        outBoundCalls = ctx.OutBoundCalls.Include(x => x.OutBoundCallDialPlan).Where(x => !x.IsDeleted &&  x.State > NotRegEnum).ToList();

                    if (outBoundCalls.Count > 0)
                    {
                        if (Franchise_Id.Value != 0)
                            outBoundCallsDialPlan = ctx.OutBoundCallDialPlan.Where(x => x.Franchise_Id == Franchise_Id.Value && !x.IsDeleted).ToList();

                        else
                            outBoundCallsDialPlan = ctx.OutBoundCallDialPlan.Where(x => !x.IsDeleted).ToList();


                    }

                    return Ok(new CustomResponse<List<DAL.OutBoundCallDialPlan>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = outBoundCallsDialPlan });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [HttpPost]
        [Route("AddDialPlan")]
        public async Task<IHttpActionResult> AddDialPlan(DialPlanBindingModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                using (RiscoContext ctx = new RiscoContext())
                {
                    string StartTime = string.Empty;
                    string EndTime = string.Empty;

                    try
                    {
                        StartTime = DateTime.Parse(model.StartTime).ToString("hh:mm tt");
                        EndTime = DateTime.Parse(model.EndTime).ToString("hh:mm tt");
                    }
                    catch (Exception ex)
                    {
                        return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = "Invalid Time Format. Contact Administrator." });
                    }
                    var DialPlanBM = new OutBoundCallDialPlan
                    {
                        IsDeleted = false,
                        StartingTime = DateTime.Parse(model.StartTime).ToString("hh:mm tt"),
                        EndTime = DateTime.Parse(model.EndTime).ToString("hh:mm tt"),
                        State = Convert.ToInt32(Statuses.InActive)
                    };

                    if (model.Franchise_Id != 0)
                        DialPlanBM.Franchise_Id = model.Franchise_Id;


                    ctx.OutBoundCallDialPlan.Add(DialPlanBM);
                    ctx.SaveChanges();

                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = "Dial Plan Inserted Successfully." });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }


        [HttpPost]
        [Route("ChangedialPlanStatus")]
        public async Task<IHttpActionResult> ChangedialPlanStatus(ChangeDialPlanStatusModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                using (RiscoContext ctx = new RiscoContext())
                {
                    var DialPlan = ctx.OutBoundCallDialPlan.FirstOrDefault(x => x.Id == model.Id && !x.IsDeleted);

                    if (DialPlan != null)
                    {

                        if (model.Status == Convert.ToInt32(Statuses.Active))
                        {
                            var Status = Convert.ToInt32(Statuses.Active);
                            var ActiveDialPlan = ctx.OutBoundCallDialPlan.FirstOrDefault(x => x.Franchise_Id == DialPlan.Franchise_Id && x.State == Status);
                            if (ActiveDialPlan != null)
                                ActiveDialPlan.State = Convert.ToInt32(Statuses.InActive);
                        }

                        DialPlan.State = model.Status;

                    }


                    ctx.SaveChanges();

                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = "Dial Plan Inserted Successfully." });


                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

    }
}
