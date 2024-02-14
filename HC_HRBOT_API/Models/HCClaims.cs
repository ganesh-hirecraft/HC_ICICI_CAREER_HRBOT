using beHC_HR_BOT;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace HC_HRBOT_API.Models
{
    public class HCClaims
    {
        /// <summary>
        /// Get User claim values
        /// </summary>
        /// <param name="httpRequest"></param>
        public static HCCommon opGetClaimValues(HttpRequestMessage httpRequest)
        {
            HCCommon objCommon = new HCCommon();

            string token = httpRequest.Headers.GetValues("Authorization").First();

            string[] tokenArray = token.Split(' ');

            #region [Read user claim values]

            try
            {
                ClaimsPrincipal claimPrincipal = httpRequest.GetRequestContext().Principal as ClaimsPrincipal;

                var claimValues = claimPrincipal.Claims.AsEnumerable().Select(s => new
                {
                    Type = s.Type,
                    Value = s.Value
                }).ToList();

                foreach (var claimValue in claimValues)
                {
                    switch (claimValue.Type)
                    {
                        case HCClaimTypes.UID:
                            try
                            {
                                objCommon.UID = Convert.ToInt64(claimValue.Value.Trim());
                            }
                            catch (Exception ex)
                            {
                                Common.Logs("HCClaims_UID: " + ex.ToString());
                            }
                            break;
                        case HCClaimTypes.ResumeID:
                            try
                            {
                                objCommon.ResID = Convert.ToInt64(claimValue.Value.Trim());
                            }
                            catch (Exception ex)
                            {
                                Common.Logs("HCClaims_resID: " + ex.ToString());
                            }
                            break;

                        default:
                            break;
                    }
                }



            }
            catch (Exception ex)
            {
                Common.Logs("HCClaims: " + ex.ToString());
            }

            #endregion

            #region [Set up the IP Address]
            try
            {
                objCommon.IPAddress = HttpContext.Current != null ? HttpContext.Current.Request.UserHostAddress : "";
            }
            catch (Exception ex)
            {
                Common.Logs("HCClaims_IP: " + ex.ToString());
            }

            #endregion

            return objCommon;
        }

        public static bool opValidateAccessToken(HttpRequestMessage httpRequest, HCCommon ObjCommon)
        {
            bool validToken = true;

            string token = httpRequest.Headers.GetValues("Authorization").First();
            string[] tokenArray = token.Split(' ');

            using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "usp_ValidateAccessToken_Career";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", ObjCommon.UID);
                    cmd.Parameters.AddWithValue("@ResID", ObjCommon.ResID);
                    cmd.Parameters.AddWithValue("@AccessToken", tokenArray.Length > 1 ? tokenArray[1] : "");
                    var vOpcode = cmd.Parameters.Add("@isValidToken", SqlDbType.Bit);
                    vOpcode.Direction = ParameterDirection.Output;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    validToken = Convert.ToBoolean(vOpcode.Value);
                }
            }


            return validToken;
        }

        public static bool opValidateAccessUser(HttpRequestMessage httpRequest, HCCommon ObjCommon)
        {
            //7897_hgdy_323379_hkjhfb
            bool validUser = true;

            string key = httpRequest.Headers.GetValues("Guhya").First();

            if (key == "")
            {
                validUser = false;
            }
            else
            {
                var decryptedGuyha = ClsCrypto.DecryptUsingAES(key);
                var decryptedData = decryptedGuyha.Split('_');
                string guhyaUserID = decryptedData[2];

                Int64 a = Convert.ToInt64(guhyaUserID);

                if (a == ObjCommon.UID)
                {
                    validUser = true;
                }
                else
                {
                    validUser = false;
                }
            }
            return validUser;
        }

        #region check script tags
        /// <summary>
        /// Cross site script
        /// </summary>
        /// <param name="jObj"></param>
        /// <param name="jArr"></param>
        /// <param name="name"></param>
        /// <param name="ID"></param>
        public static bool ValidateCrossScript(JObject jObj, JArray jArr = null, string name = "", long ID = 0)
        {

            try
            {

                var IsRegEx = @"(<script>\w.*</script>)|(alert\()|(<script src=)|(javascript:alert)|(<span>\w.*</span>)|(<p>\w.*</p>)|(<iframe src=)|(&lt;)|(a href=)|(<img src=)|(<section>)|(<!doctype>)|(<!doctype)|(<a)|(<abbr)|(<acronym)|(<address)|(<applet)|(<area)|(<article)|(<aside)|(<audio)|(<b)|(<base)|(<basefont)|(<bb)|(<bdo)|(<big)|(<blockquote)|(<body)|(<button)|(<canvas)|(<caption)|(<center)|(<cite)|(<code)|(<col)|(<colgroup)|(<command)|(<datagrid)|(<datalist)|(<dd)|(<del)|(<details)|(<dfn)|(<dialog)|(<dir)|(<div)|(<dl)|(<dt)|(<em)|(<embed)|(<eventsource)|(<fieldset)|(<figcaption)|(<figure)|(<font)|(<footer)|(<form)|(<frame)|(<frameset)|(<head)|(<header)|(<hgroup)|(<hr/)|(<html)|(<i)|(<iframe)|(<img)|(<input)|(<ins)|(<isindex)|(<kbd)|(<keygen)|(<label)|(<legend)|(<li)|(<link)|(<map)|(<mark)|(<menu)|(<meta)|(<meter)|(<nav)|(<noframes)|(<noscript)|(<object)|(<ol)|(<optgroup)|(<option)|(<output)|(<param)|(<pre)|(<progress)|(<q)|(<rp)|(<rt)|(<ruby)|(<s)|(<samp)|(<section)|(<select)|(<small)|(<source)|(<span)|(<strike)|(<strong)|(<style)|(<sub)|(<sup)|(<table)|(<tbody)|(<td)|(<textarea)|(<tfoot)|(<th)|(<thead)|(<time)|(<title)|(<tr)|(<track)|(<tt)|(<u)|(<ul)|(<var)|(<video)|(<wbr)|(<)";

                if (IsRegEx != "")
                {
                    // string regexPattern = @"" + IsRegEx + "";
                    Regex r = new Regex(IsRegEx, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (jObj != null)
                    {
                        if (ID == 1)
                        {
                            foreach (JProperty data in jObj["Values"])
                            {
                                if (data.Name != "JobDesc")
                                {
                                    if (data.Value.Contains("(") || data.Value.Contains(")"))
                                    {
                                        return true;
                                    }
                                    if (r.IsMatch(System.Convert.ToString(data.Value)))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (JProperty data in jObj.Properties())
                            {

                                if (data.Name == "Answer" || data.Name == "EducationDetails")
                                {
                                    JArray Arr = null;
                                    Arr = (JArray)jObj["Answer"];

                                    if (Arr != null)
                                    {
                                        foreach (JToken value in Arr)
                                        {
                                            foreach (JProperty item in value)
                                            {
                                                if (r.IsMatch(System.Convert.ToString(item.Value)))
                                                {
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                 if (r.IsMatch(System.Convert.ToString(data.Value)))
                                {
                                    return true;
                                }
                            }

                        }
                    }
                    else if (jArr != null)
                    {
                        string[] tReqResID = jArr.ToObject<string[]>();
                        foreach (string ReqREsID in tReqResID)
                        {
                            if (r.IsMatch(System.Convert.ToString(ReqREsID)))
                            {
                                return true;
                            }
                        }
                    }
                    else if (name != "")
                    {
                        if (r.IsMatch(System.Convert.ToString(name)))
                        {
                            return true;
                        }
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                Common.Logs("ValidateCrossScript() :" + ex.ToString());
                return true;
            }
        }
        #endregion


    }
}