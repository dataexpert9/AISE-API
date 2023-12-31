﻿using BasketApi;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using static BasketApi.Utility;
using System.Data.Entity;
using System.Web;
using System.IO;
using System.Configuration;

namespace WebApplication1.Areas.Admin.Controllers
{
    [BasketApi.Authorize("SubAdmin", "SuperAdmin", "ApplicationAdmin", "User")]
    [RoutePrefix("api")]
    public class CommonController : ApiController
    {
        [HttpGet]
        [Route("GetEntityById")]
        public async Task<IHttpActionResult> GetEntityById(int EntityType, int Id)
        {
            try
            {
                using (RiscoContext ctx = new RiscoContext())
                {
                    switch (EntityType)
                    {
                        case (int)AivoBotEntityTypes.Admin:
                            return Ok(new CustomResponse<DAL.Admin> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Admins.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });
                        case (int)AivoBotEntityTypes.Franchise:
                            return Ok(new CustomResponse<DAL.Franchise> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Franchise.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });
                        case (int)AivoBotEntityTypes.Docking:
                            return Ok(new CustomResponse<DAL.Docking> { Message = Global.ResponseMessages.Success, StatusCode = (int)HttpStatusCode.OK, Result = ctx.Docking.FirstOrDefault(x => x.Id == Id && x.IsDeleted == false) });
                        default:
                            return Ok(new CustomResponse<Error> { Message = Global.ResponseMessages.BadRequest, StatusCode = (int)HttpStatusCode.BadRequest, Result = new Error { ErrorMessage = "Invalid entity type" } });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }

        //[BasketApi.Authorize]
        //[HttpPost]
        //[Route("UploadProductImages")]
        //public async Task<IHttpActionResult> UploadProductImages()
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        string newFullPath = string.Empty;
        //        string fileNameOnly = string.Empty;
                
              
        //        if (httpRequest.Files.Count > 0)
        //        {
        //            if (!Request.Content.IsMimeMultipartContent())
        //            {
        //                return Content(HttpStatusCode.OK, new CustomResponse<Error>
        //                {
        //                    Message = "UnsupportedMediaType",
        //                    StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
        //                    Result = new Error { ErrorMessage = "Multipart data is not included in request" }
        //                });
        //            }
        //        }
                
        //            HttpPostedFile postedFile = null;
        //            string fileExtension = string.Empty;
        //            #region ImageSaving
        //            List<Product_Images> productImages = new List<Product_Images>();
        //        foreach (var file in httpRequest.Files)
        //        {
        //            postedFile = httpRequest.Files[file.ToString()];
        //            if (postedFile != null && postedFile.ContentLength > 0)
        //            {
        //                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
        //                var ext = Path.GetExtension(postedFile.FileName);
        //                fileExtension = ext.ToLower();
        //                if (!AllowedFileExtensions.Contains(fileExtension))
        //                {
        //                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
        //                    {
        //                        Message = "UnsupportedMediaType",
        //                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
        //                        Result = new Error { ErrorMessage = "Please Upload image of type .jpg,.gif,.png" }
        //                    });
        //                }
        //                else if (postedFile.ContentLength > Global.MaximumImageSize)
        //                {
        //                    return Content(HttpStatusCode.OK, new CustomResponse<Error>
        //                    {
        //                        Message = "UnsupportedMediaType",
        //                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
        //                        Result = new Error { ErrorMessage = "Please Upload a file upto " + Global.ImageSize }
        //                    });
        //                }
        //            }
        //            var guid = Guid.NewGuid();
        //            newFullPath = HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"] + "_" + guid + fileExtension);
        //            Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/" + ConfigurationManager.AppSettings["ProductImageFolderPath"]));
        //            postedFile.SaveAs(newFullPath);
        //            productImages.Add(new Product_Images { Url = ConfigurationManager.AppSettings["ProductImageFolderPath"] + "_" + guid + fileExtension });
        //        }
        //        return Content(HttpStatusCode.OK, new CustomResponse<List<Product_Images>>
        //        {
        //            Message = "success",
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Result = productImages
        //        });
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(Utility.LogError(ex));
        //    }
        //}
    }
}
