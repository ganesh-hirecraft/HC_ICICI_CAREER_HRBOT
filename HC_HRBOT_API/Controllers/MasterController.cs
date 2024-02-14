using beHC_HR_BOT;
using HC_HRBOT_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace HC_HRBOT_API.Controllers
{
    [Authorize]
    [RoutePrefix("hc/master")]
    public class MasterController : ApiController
    {
        //private HCCommon objCommon = null;
        apiResponse response = null;
        MasterClass objBe = null;

        /// <summary>
        ///  Master {name} :
        ///  Country; State; City; University; College; Degree; EducationGroup; Specialization ; Designation; EmploymentType; Currency; NameTitle; Gender ;  Relation; Occupation; Language; BloodGroup; Category; DocumentType; SearchFunction; SearchLocation; MaritalStatus; Reference; AddressType; AllStatus; InterviewMode; InterviewPanel; interviewstatus,offergrade,salary 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{name}")]
        [ActionName("GetMaster")]
        [AllowAnonymous]
        public HttpResponseMessage LoadMaster(string name)
        {
            APIPayload responsePayload = new APIPayload();
            try
            {
                // objCommon = HCClaims.opGetClaimValues(Request);
                objBe = new MasterClass();
                response = new apiResponse();

                //bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                //if (!isValidToken)
                //{
                //    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                //    return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                //    //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objCanBasic));
                //    //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //}
                //else
                //{
                var masterData = objBe.GetMaster(name);
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(masterData));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(masterData));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                // }
                //var masterData = objBe.GetMaster(name);

                //return Request.CreateResponse(HttpStatusCode.OK, masterData);

            }
            catch (Exception ex)
            {
                Common.Logs("LoadMaster() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

        /// <summary>
        ///  Master {name}/followed by {refId} if required. Ex: Load State based on CountryID, Load City Based On StateID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{name}/{refId:int}")]
        [ActionName("GetMasterByID")]
        [AllowAnonymous]
        //[NonAction]
        public HttpResponseMessage LoadMaster(string name, Int64 refId)
        {
            // return objBe.GetMaster(name, refId);

            APIPayload responsePayload = new APIPayload();
            try
            {
                // objCommon = HCClaims.opGetClaimValues(Request);
                objBe = new MasterClass();
                response = new apiResponse();

                //bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                //if (!isValidToken)
                //{
                //    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                //    return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                //    //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objCanBasic));
                //    //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //}
                //else
                //{
                var masterData = objBe.GetMaster(name, refId);

                return Request.CreateResponse(HttpStatusCode.OK, masterData);

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(masterData));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //}
                //var masterData = objBe.GetMaster(name, refId);

                //return Request.CreateResponse(HttpStatusCode.OK, masterData);

            }
            catch (Exception ex)
            {
                Common.Logs("LoadMaster() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

        /// <summary>
        ///  Search by  : type{ role; panel} : title {user input}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("NameSearch")]
        [ActionName("NameSearch")]
        //[NonAction]
        [AllowAnonymous]
        public HttpResponseMessage MasterSearchByTitle(MasterNameSearch obj)
        {
            APIPayload responsePayload = new APIPayload();
            try
            {
                //objCommon = HCClaims.opGetClaimValues(Request);
                objBe = new MasterClass();
                response = new apiResponse();

                //bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                //if (!isValidToken)
                //{
                //    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                //    return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                //    //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objCanBasic));
                //    //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //}
                //else
                //{
                var masterData = objBe.NameSearch(obj);

                return Request.CreateResponse(HttpStatusCode.OK, masterData);
                //}

                //  return objBe.NameSearch(obj);
            }
            catch (Exception ex)
            {
                Common.Logs("MasterSearchByTitle() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        /// <summary>
        ///  Panel search By Date Range{From-Date,To-Date}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("GetPanelList")]
        [ActionName("NameSearch")]
        //[NonAction]
        [AllowAnonymous]
        public HttpResponseMessage PanelByDateRange(PanelSearchByDate obj)
        {
            //return objBe.bePanelByDateRange(obj);

            APIPayload responsePayload = new APIPayload();
            try
            {
                //objCommon = HCClaims.opGetClaimValues(Request);
                objBe = new MasterClass();
                response = new apiResponse();

                //bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                //if (!isValidToken)
                //{
                //    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                //    return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                //    //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objCanBasic));
                //    //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //}
                //else
                //{
                var masterData = objBe.bePanelByDateRange(obj);

                return Request.CreateResponse(HttpStatusCode.OK, masterData);
                //}

                //  return objBe.NameSearch(obj);
            }
            catch (Exception ex)
            {
                Common.Logs("MasterSearchByTitle() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

    }
}