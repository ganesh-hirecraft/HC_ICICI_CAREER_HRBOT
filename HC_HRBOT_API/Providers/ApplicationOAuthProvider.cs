using beHC_HR_BOT;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace HC_HRBOT_API.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicClientId"></param>
        //public ApplicationOAuthProvider(string publicClientId)
        //{
        //    if (publicClientId == null)
        //    {
        //        throw new ArgumentNullException("publicClientId");
        //    }

        //    _publicClientId = publicClientId;
        //}


        public override async Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            //System.Diagnostics.Debugger.Break();
            string grantType = "", encrptMobileNo = "", encrptCandidateNo = "", dcrptMobileNo = "", dcrptCandidateNo = "";
            bool isValidUser = false;
            var formCollection = await context.Request.ReadFormAsync();
            grantType = formCollection.Get("grant_type").ToString();
            encrptMobileNo = formCollection.Get("key").ToString();
            encrptCandidateNo = formCollection.Get("data").ToString();

            encrptMobileNo = encrptMobileNo.Replace(" ", "+");
            encrptCandidateNo = encrptCandidateNo.Replace(" ", "+");

            dcrptMobileNo = ClsCrypto.DecryptUsingAES(encrptMobileNo);
            dcrptCandidateNo = ClsCrypto.DecryptUsingAES(encrptCandidateNo);

            string requestIPAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
            LoginCheck _loginCheck = new LoginCheck();
            LoginResponse oResponse = null;
            try
            {
                oResponse = _loginCheck.beLoginCheck(dcrptMobileNo, "", dcrptCandidateNo, requestIPAddress, "");

                if ((oResponse == null) || (oResponse.ID == -1))
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                else
                {
                    string sMessage = Newtonsoft.Json.JsonConvert.SerializeObject(oResponse, Newtonsoft.Json.Formatting.None);
                    context.SetError(sMessage);
                }

                if (oResponse != null && !Convert.ToString(oResponse.ID).Trim().StartsWith("-"))
                {
                    // If valid user then make flag as 'true'. based on this flag we are giving authentication response.
                    isValidUser = true;
                }

                if (!isValidUser)
                {
                    if (oResponse == null)
                    {
                        // Send unauthorized response.
                        context.Response.Headers.Add("HC-Challenge",
                                                                 new[] { ((int)HttpStatusCode.Unauthorized).ToString() });
                        return;
                    }
                    else
                    {
                        // Based on back end response ID. we are returning the values. 
                        // If It's: -1, -3, -5 means then return unAuthoriaed  status code
                        // -2: Forbidden status code we are returning.
                        switch (oResponse.ID)
                        {
                            case -1:
                                context.Response.Headers.Add("HC-Challenge",
                                                                   new[] { Convert.ToString((int)HttpStatusCode.Unauthorized) });

                                //var headerValues = context.Response.Headers.GetValues("HC-Challenge");
                                //// Assign our status code in to the header
                                //context.Response.StatusCode =Convert.ToInt16(headerValues.FirstOrDefault());
                                //context.Response.Headers.Remove("HC-Challenge");

                                return;
                            case -2:
                                context.Response.Headers.Add("HC-Challenge",
                                               new[] { ((int)HttpStatusCode.Forbidden).ToString() });
                                return;
                            case -3 - 5:
                                context.Response.Headers.Add("HC-Challenge",
                                               new[] { Convert.ToString((int)HttpStatusCode.Unauthorized) });
                                return;
                            case -4:
                                context.Response.Headers.Add("HC-Challenge",
                           new[] { Convert.ToString((int)HttpStatusCode.InternalServerError) });
                                return;
                            default:
                                context.Response.Headers.Add("HC-Challenge",
                                                new[] { Convert.ToString((int)HttpStatusCode.Unauthorized) });
                                return;
                        }
                    }
                }

                context.OwinContext.Set<Int64>("as:UserId", (oResponse.UserID.ToString().Trim().StartsWith("-")) ? 0 : oResponse.UserID);
                IList<Claim> ClimColl = new List<Claim>()
                    {
                        new Claim(HCClaimTypes.UID, (oResponse.UserID.ToString().Trim().StartsWith("-")) ? "0": oResponse.UserID.ToString()),
                            new Claim(HCClaimTypes.ResumeID,(oResponse.ResumeID.ToString().Trim().StartsWith("-")) ? "0": oResponse.ResumeID.ToString())
                    };

                var identity = new ClaimsIdentity(ClimColl, context.Options.AuthenticationType);
                ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                //  Thread.CurrentPrincipal = principal;
                if (HttpContext.Current != null && HttpContext.Current.User != null)
                    HttpContext.Current.User = principal;

                var props = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            {
                                "keyData", ClsCrypto.EncryptUsingAES( oResponse.UserID.ToString())
                            },
                            {
                                "keyValue", ClsCrypto.EncryptUsingAES(oResponse.ResumeID.ToString())
                            },
                            {
                                "LastLoginDate", ClsCrypto.EncryptUsingAES( Convert.ToString(oResponse.LastLoginDate))
                            }
                        });

                var ticket = new AuthenticationTicket(identity, props);
                //Alter tthe Replace ticket information and validating
                await Task.Run(() => context.Validated(ticket));

            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {


            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
            //return clsEncryptAndDecrypt.EncryptUsingAES(JsonConvert.SerializeObject(Task.FromResult<object>(null)));
        }

        public override Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context)
        {
            //System.Diagnostics.Debugger.Break();

            if (!String.IsNullOrEmpty(context.AccessToken))
            {
                var expiryTimeinHours = Convert.ToInt64(ConfigurationManager.AppSettings["TokenExpiryInMinutes"]);
                string UserID = context.AdditionalResponseParameters["keyData"].ToString();
                string ResID = context.AdditionalResponseParameters["keyValue"].ToString();
                string AccessToken = context.AccessToken.ToString();
                // string IPaddress = context.AdditionalResponseParameters["IP"].ToString();
                DateTime IssuedDate = Convert.ToDateTime(context.AdditionalResponseParameters[".issued"]);
                DateTime ExpireDate = Convert.ToDateTime(context.AdditionalResponseParameters[".expires"]);
                // long ExpiresIn = 0;
                string requestIPAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
                string Devicetype = ConfigurationManager.AppSettings["Devicetype"];

                try
                {
                    using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                    {
                        SqlCommand Cmd = new SqlCommand();
                        Cmd.Connection = con;
                        con.Open();
                        Cmd.CommandText = "usp_SaveCandidateTokenData";
                        Cmd.CommandType = CommandType.StoredProcedure;
                        Cmd.Parameters.AddWithValue("@UserID", ClsCrypto.DecryptUsingAES(UserID));
                        Cmd.Parameters.AddWithValue("@ResID", ClsCrypto.DecryptUsingAES(ResID));
                        Cmd.Parameters.AddWithValue("@AccessToken", AccessToken);
                        Cmd.Parameters.AddWithValue("@IssuedDate", IssuedDate);
                        Cmd.Parameters.AddWithValue("@ExpiresDate", ExpireDate);
                        Cmd.Parameters.AddWithValue("@IpAddress", requestIPAddress);
                        Cmd.Parameters.AddWithValue("@Devicetype", Devicetype);
                        var vResults = Cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    Common.ErrorRes(null, ex);
                }
            }

            return Task.FromResult<object>(null);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            // Resource owner password credentials does not provide a client ID.
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
            }

            return Task.FromResult<object>(null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        /// 

    }
}