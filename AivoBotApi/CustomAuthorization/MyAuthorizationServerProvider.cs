using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using DAL;
using System.Net.Http;
using System.Web.Http;
using BasketApi.CustomAuthorization;
using System.IO;
using System.Text;
using BasketApi;
using Newtonsoft.Json;
using System.Net;
using BasketApi.Components.Helpers;

namespace BasketApi
{
    public class MyAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public static OAuthGrantResourceOwnerCredentialsContext AuthorizeContext;
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            try
            {
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);

                var form = await context.Request.ReadFormAsync();

                var SignInType = form["signintype"];
                var UserId = form["identity"];

                using (RiscoContext ctx = new RiscoContext())
                {
                    identity.AddClaim(new Claim("Email", form["Email"]));
                    identity.AddClaim(new Claim(ClaimTypes.Name, form["Email"]));
                    identity.AddClaim(new Claim("identity", UserId));
                    switch ((RoleTypes)Enum.Parse(typeof(RoleTypes), SignInType))
                    {
                        case RoleTypes.User:
                            string EmailAddress = form["Email"].ToString();

                            if (form["NewRegister"] != null && form["NewRegister"] == "1")
                            {

                            }
                            else
                            {
                                User userModel = ctx.Users.FirstOrDefault(x => x.Email == EmailAddress && x.Password == context.Password);

                                if (userModel == null)
                                {
                                    break;
                                }

                            }
                            identity.AddClaim(new Claim(ClaimTypes.Role, RoleTypes.User.ToString()));
                            context.Validated(identity);
                            break;
                        case RoleTypes.SuperAdmin:

                            Admin adminModel = ctx.Admins.FirstOrDefault(x => x.Username == context.UserName && x.Password == context.Password);
                            identity.AddClaim(new Claim(ClaimTypes.Role, RoleTypes.SuperAdmin.ToString()));
                            context.Validated(identity);
                            break;
                        default:
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(ctx.Users.FirstOrDefault());
                            context.SetError("invalid username or password!", json);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            #region commented
            //if (context.UserName == "admin" && context.Password == "admin")
            //{
            //    //identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            //    identity.AddClaim(new Claim("username", "admin"));
            //    identity.AddClaim(new Claim(ClaimTypes.Name, "admin"));
            //    context.Validated(identity);
            //}
            //else if (context.UserName == "user" && context.Password == "user")
            //{
            //    //identity.AddClaim(new Claim(ClaimTypes.Role, "user"));
            //    identity.AddClaim(new Claim("username", "user"));
            //    identity.AddClaim(new Claim(ClaimTypes.Name, "Mohsin"));
            //    context.Validated(identity);
            //} 
            #endregion
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }
    }
}