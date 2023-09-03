using BasketApi;
using BasketApi.Areas.SubAdmin.Models;
using BasketApi.CustomAuthorization;
using BasketApi.Models;
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
using System.Data.Entity;
using Z.EntityFramework.Plus;
using ExcelDataReader;
using System.Data;
using System.Text.RegularExpressions;
using RestSharp;

namespace BasketApi.Controllers
{
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/Whatsapp")]
    public class WhatsappController : ApiController
    {

        [Route("UploadWhatsappFile")]
        [Authorize]
        public async Task<IHttpActionResult> UploadWhatsappFile()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
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
                        Result = new Error { ErrorMessage = "File not found, please upload a file first." }
                    });
                }
                else if (httpRequest.Files.Count > 1)
                {
                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
                    {
                        Message = "UnsupportedMediaType",
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Result = new Error { ErrorMessage = "Multiple excel file are not allowed. Please upload 1 excel file." }
                    });
                }
                #endregion

                var postedFile = httpRequest.Files[0];

                if (postedFile != null && postedFile.ContentLength > 0)
                {

                    int MaxContentLength = 1024 * 1024 * 10; //Size = 1 MB  

                    IList<string> AllowedFileExtensions = new List<string> { ".xls", ".xlsx", ".xlsm" };
                    var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                    var extension = ext.ToLower();
                    if (!AllowedFileExtensions.Contains(extension))
                    {
                        return Content(HttpStatusCode.OK, new CustomResponse<Error>
                        {
                            Message = "UnsupportedMediaType",
                            StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                            Result = new Error { ErrorMessage = "Please Upload file of type .xls, .xlsx, .xlsm." }
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
                        newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ExcelFileFolderPath"] + postedFile.FileName);

                        while (File.Exists(newFullPath))
                        {
                            string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ExcelFileFolderPath"] + tempFileName + extension);
                        }
                        //postedFile.SaveAs(newFullPath);
                    }
                }

                MessageViewModel successResponse = new MessageViewModel { StatusCode = "200 OK", Details = "File Updated Successfully." };
                //var filePath = Utility.BaseUrl + ConfigurationManager.AppSettings["UserImageFolderPath"] + Path.GetFileName(newFullPath);
                var filePath = ConfigurationManager.AppSettings["ExcelFileFolderPath"] + Path.GetFileName(newFullPath);
                ImagePathViewModel model = new ImagePathViewModel { Path = filePath };

                using (RiscoContext ctx = new RiscoContext())
                {
                    var user = ctx.Users.FirstOrDefault(x => x.Email == userEmail);

                    if (user != null)
                    {
                        ctx.ExcelFile.Add(new ExcelFile
                        {
                            FileUrl = filePath,
                            UserId = user.Id
                        });
                        //ctx.SaveChanges();
                    }
                }




                using (var stream = File.Open(newFullPath, FileMode.Open, FileAccess.Read))
                {
                    // Auto-detect format, supports:
                    //  - Binary Excel files (2.0-2003 format; *.xls)
                    //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        // Choose one of either 1 or 2:
                        Regex r = new Regex(@"^\+?\d{0,2}\-?\d{4,5}\-?\d{5,6}");
                        var url = "https://api.ultramsg.com/instance45364/messages/chat";

                        // 1. Use the reader methods
                        do
                        {
                            var headers = new List<string>();
                            var Values = new List<string>();
                            string Message = String.Empty;
                            while (reader.Read())
                            {
                                // reader.GetDouble(0);
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    if (headers.Count < 4)
                                        headers.Add(Convert.ToString(reader[i]));
                                    else
                                    {
                                        Values.Add(Convert.ToString(reader[i]));
                                        if (r.Match(reader[2].ToString()).Success && i==0)
                                        {
                                            var client = new RestClient(url);

                                            var request = new RestRequest(url, Method.POST);
                                            request.AddHeader("content-type", "application/x-www-form-urlencoded");
                                            request.AddParameter("token", "8kwvzi7b6gog1gp4");
                                            request.AddParameter("to", reader[2].ToString());
                                            if(Message==String.Empty)
                                                request.AddParameter("body", "Hi There.");
                                            else
                                                request.AddParameter("to", reader[3].ToString());

                                            var responsee = await client.ExecuteAsync(request);
                                            var output = responsee.Content;
                                        }


                                    }
                                }


                            }
                        } while (reader.NextResult());

                        // 2. Use the AsDataSet extension method
                        //var result = reader.AsDataSet();

                   //     var tables = result.Tables
                   //.Cast<DataTable>()
                   //.Select(t => new
                   //{
                   //    TableName = t.TableName,
                   //    Columns = t.Columns
                   //                             .Cast<DataColumn>()
                   //                             .Select(x => x.ColumnName)
                   //                             .ToList()
                   //});

                        // The result of each spreadsheet is in result.Tables
                    }
                }






                CustomResponse<ImagePathViewModel> response = new CustomResponse<ImagePathViewModel> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = model };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }



        [Authorize]
        [HttpGet]
        [Route("GetUploadedFiles")]
        public async Task<IHttpActionResult> GetUploadedFiles(int UserId)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    var fileModel = ctx.ExcelFile.Include(x => x.User).Where(x => x.UserId == UserId && x.IsDeleted == false).ToList();

                    CustomResponse<List<ExcelFile>> response = new CustomResponse<List<ExcelFile>> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = fileModel };
                    return Ok(response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin")]
        [HttpPost]
        [Route("ChangeFileStatus")]
        public async Task<IHttpActionResult> ChangeFileStatus(ChangeFileStatus model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (RiscoContext ctx = new RiscoContext())
                {


                    var file = ctx.ExcelFile.FirstOrDefault(x => x.Id == model.FileId);

                    if (file != null)
                        file.Status = model.Status;

                    ctx.SaveChanges();
                    return Ok(new CustomResponse<string> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK });

                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

    }
}