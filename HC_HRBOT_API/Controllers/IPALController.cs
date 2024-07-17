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
    [RoutePrefix("IPAL")]
    public class IPALController : ApiController
    {
        ImpIPAL clsIPAL = ImpIPAL.Instance;
        apiResponse response = null;
        APIPayload responsePayload = null;

        [HttpPost]
        [Route("getStageStatus")]
        [AllowAnonymous]
        public HttpResponseMessage opStagestatus([FromBody] APIPayload requestPayload)
        {
            responsePayload = new APIPayload();
            try
            {                 
                string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                dynamic objBasic = JsonConvert.DeserializeObject<dynamic>(decRequestPayload);
                string MobileNo = objBasic["MobileNo"];

                var objResponse = clsIPAL.begetstagestatus(MobileNo);

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objResponse));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
            catch(Exception ex)
            {
                Common.Logs("opStagestatus() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }

        [HttpPost]
        [Route("SaveDocumentDetails")]
        [AllowAnonymous]
        public HttpResponseMessage opSaveDocDetails([FromBody] APIPayload requestPayload)
        {
            responsePayload = new APIPayload();
            try
            {
                string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                FileDetails objFile = JsonConvert.DeserializeObject<FileDetails>(decRequestPayload);                

                var objResponse = clsIPAL.besavefiledata(objFile);

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objResponse));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
            catch (Exception ex)
            {
                Common.Logs("opSaveDocDetails() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }

        [HttpPost]
        [Route("IPALResumeSaving")]
        [AllowAnonymous]
        public HttpResponseMessage PostResume([FromBody] APIPayload requestPayload)//(CandidateData objCand)
        {            
            apiResponse objRP = new apiResponse();
            responsePayload = new APIPayload();
            string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);
            CandidateData objCand = JsonConvert.DeserializeObject<CandidateData>(decRequestPayload);

            if (objCand == null)
            {
                objRP.ResponseStatus = "Invalid Data. Please check the Input..";
                objRP.Data = "";
                objRP.ResponseCode=101;

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objRP));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
            else if (objCand.RequestType == "Duplicate Check")
            {
                var objDuplicate= clsIPAL.beDuplicateCheck(objCand);
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objDuplicate));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                
            }
            else if (objCand.RequestType == "Save Resume")
            {
                var objResume = clsIPAL.SaveResume(objCand);
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objResume));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
                //return clsIPAL.SaveResume(objCand);
            }
            else
            {
                objRP.ResponseStatus = "Failure";
                objRP.Data = "";
                objRP.ResponseCode = 101;
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objRP));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }

        [HttpPost]
        [Route("RequirementList")]
        [AllowAnonymous]
        public HttpResponseMessage GetRequirementDetails([FromBody] APIPayload requestPayload)
        {
            responsePayload = new APIPayload();
            try
            {
                string decRequestPayload = ClsCrypto.DecryptUsingAES(requestPayload.Data);

                dynamic objBasic = JsonConvert.DeserializeObject<dynamic>(decRequestPayload);
               // string MobileNo = objBasic["MobileNo"];
                string EmpID = objBasic["EmpID"];
                string ReqTitle= objBasic["ReqTitle"];
                string ReqNumber= objBasic["ReqNumber"];
                var objResponse = clsIPAL.opGetRequirementList(EmpID, ReqTitle, ReqNumber);

                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(objResponse));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
            catch (Exception ex)
            {
                Common.Logs("GetRequirementDetails() :" + ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
                responsePayload.Data = ClsCrypto.EncryptUsingAES(JsonConvert.SerializeObject(response));
                return Request.CreateResponse(HttpStatusCode.OK, responsePayload);
            }
        }
    }
}
