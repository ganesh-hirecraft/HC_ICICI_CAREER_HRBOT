using beHC_HR_BOT;
using HC_HRBOT_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HC_HRBOT_API.Controllers
{
    [Authorize]
    [RoutePrefix("Career")]
    public class CareerController : ApiController
    {
        private HCCommon objCommon = null;
        CareerClass careerCls = null;
        apiResponse response = null;
        APIPayload responsePayload = null;

        #region [ Login Mobile/Email ]
        /// <summary>
        /// Candidate Login By Mobile-No or Email-ID
        /// </summary>
        /// <param name="requestPayload"></param>
        /// <param name="Type"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public HttpResponseMessage Login(APIPayload requestPayload)
        {
            CryptLib lib = new CryptLib();
            responsePayload = new APIPayload();
            try
            {
                careerCls = new CareerClass(objCommon);

                string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);
                if (!decRequestPayload.StartsWith("-"))
                {
                    CareerLoginDetails candidatelogin = JsonConvert.DeserializeObject<CareerLoginDetails>(decRequestPayload);
                    var objLogin = careerCls.beCandidateLogin(candidatelogin);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objLogin));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);

                }
                else
                {
                    response = Common.ErrorResponse(response,0, "Invalid Request");
                   // return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request");
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs(ex.Message + Environment.NewLine + ex.StackTrace);
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
                // return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid Request");
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }

        #endregion

        #region[ Save Resume ]
        /// <summary>
        /// 
        /// Save Resume API for Career
        /// </summary>
        /// <param name="oResume"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveResume")]
        [AllowAnonymous]
        public HttpResponseMessage SaveResume([FromBody] APIPayload requestPayload)//([FromBody] JObject oResume)
        {
            //APIPayload _requestPayload = new APIPayload();
            responsePayload = new APIPayload();
            try
            {
                careerCls = new CareerClass();
                response = new apiResponse();
                JObject oResume = null;
                string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);
                oResume = JObject.Parse(decRequestPayload);

                var objresume = careerCls.beSaveResume_Career_BOT(oResume);

                // return Request.CreateResponse(HttpStatusCode.OK, objresume);

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objresume));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
            catch (Exception ex)
            {
                Common.Logs("SaveResume() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                //responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                //return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

        #endregion

        #region [ Candidate Basic Info ]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestPayload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("BasicDetails")]
        public HttpResponseMessage GetBasicDetails([FromBody] APIPayload requestPayload)//(string candidateNo)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);
                response = new apiResponse();

                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");
                   // return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                    dynamic objBasic = JsonConvert.DeserializeObject<dynamic>(decRequestPayload);
                    string CandidateNo = objBasic["CandidateNo"];

                    var objCanBasic = careerCls.beGetBasicDetails(CandidateNo);
                    //  return Request.CreateResponse(HttpStatusCode.OK, objCanBasic);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objCanBasic));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("GetBasicDetails() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }

        #endregion

        #region[ Carrer Candidate Progress Status ]
        /// <summary>
        /// Get Candidate Progress Status 
        /// </summary>
        [HttpPost]
        [Route("progressStatus")]
        public HttpResponseMessage GetCandidateProgressStatus_Career([FromBody] APIPayload requestPayload)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);
                response = new apiResponse();
                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                    //return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                    inputParams obj = JsonConvert.DeserializeObject<inputParams>(decRequestPayload);

                    var objProgress = careerCls.beGetCandidateProgressStatus_Career(obj);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objProgress));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                    //return Request.CreateResponse(HttpStatusCode.OK, objProgress);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("GetCandidateProgressStatus_Career() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                // return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }
        #endregion

        #region[ Quick Candidate Status ]
        /// <summary>
        /// Candidate Quick Status API
        /// </summary>
        [HttpPost]
        [Route("QuickCandiatestatus")]
        //[AllowAnonymous]
        public HttpResponseMessage QuickCandiatestatus([FromBody] APIPayload requestPayload)//(string candidateNo)
        {
            responsePayload = new APIPayload();
            response = new apiResponse();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);

                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                    //return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                    inputParams obj = JsonConvert.DeserializeObject<inputParams>(decRequestPayload);
                    var CandidateNo = obj.CandidateNo;

                    var objQuickStatus = careerCls.beQuickCandiatestatus(CandidateNo);
                    //return Request.CreateResponse(HttpStatusCode.OK, objQuickStatus);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objQuickStatus));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("QuickCandiatestatus() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                // return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

        /// <summary>
        /// Quick Candidate Status Career
        /// </summary>
        [HttpPost]
        [Route("QuickCandidateStatusCareer")]
        //[AllowAnonymous]
        public HttpResponseMessage GetQuickCandidateStatus_Career([FromBody] APIPayload requestPayload)//(string candidateNo)
        {
            responsePayload = new APIPayload();
            response = new apiResponse();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);

                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                    //return Request.CreateResponse(HttpStatusCode.Unauthorized, response);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                    quickStatus objQuick = JsonConvert.DeserializeObject<quickStatus>(decRequestPayload);
                    string CandidateNo = objQuick.CandidateNo;
                    bool isOffer = objQuick.isOffer;

                    var objQuickStatus = careerCls.beGetQuickCandidateStatus_Career(CandidateNo, isOffer);
                    //return Request.CreateResponse(HttpStatusCode.OK, objQuickStatus);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objQuickStatus));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("GetQuickCandidateStatus_Career() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //return Request.CreateResponse(HttpStatusCode.OK, response);
            }

        }

        #endregion

        #region [ timeline journey ]
        /// <summary>
        /// Get Candidate Timeline Journey
        /// </summary>
        [HttpPost]
        [Route("getTimelineDetails")]
        [AllowAnonymous]
        public HttpResponseMessage beGetgetTimelineDetails([FromBody] APIPayload requestPayload)
        {
            // objCommon = HCClaims.opGetClaimValues(Request);
            careerCls = new CareerClass();
            response = new apiResponse();
            string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);
            dynamic od = JsonConvert.DeserializeObject<dynamic>(decRequestPayload);
            Int32 ReqResID = od["ReqResID"];
            string mobileNo = "";
            try
            {
                mobileNo = od["MobileNo"];
            }
            catch
            {

            }
            var objEmail = careerCls.beGetgetTimelineDetails(ReqResID, mobileNo);
            return Request.CreateResponse(HttpStatusCode.OK, objEmail);
        }

        #endregion

        #region [ Offer Accept/Reject ]
        /// <summary>
        /// Offer Accept/Reject
        /// </summary>
        [HttpPost]
        [Route("offeraccepctreject")]
        public HttpResponseMessage SaveCandidateOfferAcceptReject([FromBody] APIPayload requestPayload)//([FromBody] OfferAcceptReject oOffer)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);
                response = new apiResponse();
                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");

                    //return Request.CreateResponse(HttpStatusCode.Unauthorized, response);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                    OfferAcceptReject oOffer = JsonConvert.DeserializeObject<OfferAcceptReject>(decRequestPayload);
                    var objAcceptOffer = careerCls.beSaveCandidateOfferAcceptReject(oOffer);
                   

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objAcceptOffer));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("SaveCandidateOfferAcceptReject() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }
        #endregion

        #region [ Education Details ]
        /// <summary>
        /// Get Education Data
        /// </summary>
        /// <param name="requestPayload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AcademicDetails")]
        public HttpResponseMessage GetAcademicDetails([FromBody] APIPayload requestPayload)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);
                response = new apiResponse();
                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);
                    inputParams obj = JsonConvert.DeserializeObject<inputParams>(decRequestPayload);
                    var CandidateNo = obj.CandidateNo;

                    var objCanAcademic = careerCls.beGetAcademicDetails(CandidateNo);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objCanAcademic));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("SaveCandidateAcademicInfo() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }

        #endregion

        #region [ Experience Details ]
        /// <summary>
        /// Get Experience Details
        /// </summary>
        /// <param name="requestPayload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("EmploymentDetails")]
        public HttpResponseMessage GetEmploymentDetails([FromBody] APIPayload requestPayload)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                careerCls = new CareerClass(objCommon);
                response = new apiResponse();
                bool isValidToken = HCClaims.opValidateAccessToken(Request, objCommon);

                if (!isValidToken)
                {
                    response = Common.UnauthorizedResponse(response, "Authorization has been denied for this request.");
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
                else
                {
                    string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);
                    inputParams obj = JsonConvert.DeserializeObject<inputParams>(decRequestPayload);

                    string CandidateNo = obj.CandidateNo;
                    var objemp = careerCls.beGetEmploymentDetails(CandidateNo);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objemp));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("GetEmploymentDetails() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }

        #endregion
    }
}
