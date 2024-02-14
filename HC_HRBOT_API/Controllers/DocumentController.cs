using beHC_HR_BOT;
using HC_HRBOT_API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HC_HRBOT_API.Controllers
{
    [Authorize]
    [RoutePrefix("Document")]
    public class DocumentController : ApiController
    {
        private HCCommon objCommon = null;
        DocumentClass docCls = null;
        apiResponse response = null;
        APIPayload responsePayload = null;

        #region [ Upload Document ]
        /// <summary>
        /// Save and Update Candidate to Requiremnt
        /// </summary>
        [HttpPost]
        [Route("Applicants/UploadDocument")]
        public HttpResponseMessage SaveCandidateDocument([FromBody] APIPayload oData)//(ParamUpdateDoc obj)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                docCls = new DocumentClass(objCommon);
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
                    //var objContactInfo = objBe.beSaveCandidateDocument(obj);
                    //return Request.CreateResponse(HttpStatusCode.OK, objContactInfo);

                    string decryptPayload = ClsCrypto.DecryptUsingAES(oData.Data);

                    ParamUpdateDoc obj = JsonConvert.DeserializeObject<ParamUpdateDoc>(decryptPayload);

                    apiResponse res = new apiResponse();

                    var objUploadDoc = docCls.beSaveCandidateDocument(obj);

                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objUploadDoc));
                    // return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objUploadDoc));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);

                }
            }
            catch (Exception e)
            {
                Common.Logs("SaveCandidateDocument() :" + e.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
               // return Request.CreateResponse(HttpStatusCode.OK, response);
            }          

        }
        #endregion

        #region [ Offer Letter Document ]
        /// <summary>
        /// Offer Letter Document
        /// </summary>
        [HttpPost]
        [Route("Applicants/OfferLetterDocument")]
        public HttpResponseMessage getOfferLetterDocument([FromBody] APIPayload oData)//(CommonReqObj obj)
        {
            responsePayload = new APIPayload();
            try
            {
                objCommon = HCClaims.opGetClaimValues(Request);
                docCls = new DocumentClass(objCommon);
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
                    string decryptPayload = ClsCrypto.DecryptUsingAES(oData.Data);

                    CommonReqObj obj = JsonConvert.DeserializeObject<CommonReqObj>(decryptPayload);

                    var objOfferLetter = docCls.beGetOfferLetterDocument(obj);
                    responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objOfferLetter));
                    return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                }
            }
            catch (Exception ex)
            {
                Common.Logs("getOfferLetterDocument() : " + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //return Request.CreateResponse(HttpStatusCode.OK, response);
            }
        }
        #endregion
    }
}
