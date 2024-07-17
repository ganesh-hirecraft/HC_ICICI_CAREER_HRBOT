using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace beHC_HR_BOT
{
    public class CareerClass
    {
        private HCCommon hcCommon = null;
        public CareerClass(HCCommon objCommon = null)
        {
            if (objCommon == null)
                hcCommon = new HCCommon();
            else
                hcCommon = objCommon;
        }

        private CareerClass()
        {
        }
        private static readonly Lazy<CareerClass> Lazy = new Lazy<CareerClass>(() => new CareerClass());
        public static CareerClass Instance
        {
            get { return Lazy.Value; }
        }
        apiResponse res = null;


        #region [Login Mobile/Email]
        public apiResponse beCandidateLogin(CareerLoginDetails opCareerLoginDetails)
        {
            apiResponse Response = new apiResponse();
            try
            {
                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    if (opCareerLoginDetails.type.ToLower() == "otp")
                    {
                        Random r = new Random();
                        long rn = r.Next(1000, 9999);

                        using (SqlDataAdapter objDataAdapter = new SqlDataAdapter("Select top 0 SenderUserID,MobileNumber,MobileNo,Message,MessageDate,ScheduleDate,Priority,MessageStatus,MessageResponse,extSMSText from sms_outbox with(nolock)", objCon))
                        {
                            using (SqlCommandBuilder objCB = new SqlCommandBuilder(objDataAdapter))
                            {
                                DataSet objDataSet = new DataSet();
                                DataRow dr = null;
                                objDataAdapter.Fill(objDataSet, "sms_outbox");
                                if (objDataSet.Tables["sms_outbox"].Rows.Count == 0)
                                {
                                    dr = objDataSet.Tables["sms_outbox"].NewRow();
                                    dr["SenderUserID"] = 0;
                                    dr["MobileNumber"] = opCareerLoginDetails.value;
                                    dr["MobileNo"] = opCareerLoginDetails.value;
                                    dr["Message"] = Convert.ToString(ConfigurationManager.AppSettings["OTPMessage"]).Replace("[OTP]", Convert.ToString(rn));
                                    dr["MessageDate"] = System.DateTime.Now.ToUniversalTime();
                                    dr["ScheduleDate"] = System.DateTime.Now.ToUniversalTime();
                                    dr["Priority"] = 1;
                                    dr["MessageStatus"] = 1;
                                    dr["extSMSText"] = "";//"Career_OTP_EGES";
                                    objDataSet.Tables["sms_outbox"].Rows.Add(dr);
                                    objDataAdapter.Update(objDataSet, "sms_outbox");
                                }
                            }
                        }

                        OtpDetails otpDetails = new OtpDetails();
                        otpDetails.MobileNo = opCareerLoginDetails.value;
                        otpDetails.OTP = rn;
                        return Response = Common.SuccessResponse(res, otpDetails, 1);
                    }
                    else
                    {
                        using (SqlCommand oCmd = objCon.CreateCommand())
                        {
                            oCmd.CommandText = "usp_getBasicLoginData_Career";
                            oCmd.CommandType = CommandType.StoredProcedure;
                            oCmd.Parameters.AddWithValue("@value", opCareerLoginDetails.value);
                            SqlDataAdapter objda = new SqlDataAdapter(oCmd);

                            DataSet objds = new DataSet();
                            objda.Fill(objds);
                            if (objds.Tables[0].Rows.Count > 0)
                            {
                                object objCandidate = Common.DataTableToJSONObject(objds.Tables[0]);
                                return Response = Common.SuccessResponse(res, objCandidate, 1);
                            }
                            else
                                return Response = (Common.NotFoundResponse(res));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");
            }
            finally
            {

            }
            return Response;
        }
        #endregion

        #region[ Resume Saving ]

        void GetResumeData_BOT(byte[] bFileData, string sFileName, ref string resumeContent, ref string resumeText, ref string resumeHTML)
        {
            try
            {
                #region getting resume data
                byte[] temp = FileCompress.opDataZip(Convert.ToBase64String(bFileData));
                resumeContent = Convert.ToBase64String(temp);
                #endregion getting resume data
                string parseUrl = ConfigurationManager.AppSettings["ParsingService"];
                #region getting resume text
                var webClient = new WebClient();
                string boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
                webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);
                var fileData = webClient.Encoding.GetString(bFileData);
                var package = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}\r\n--{0}--\r\n", boundary, sFileName, "application/octet-stream", fileData);
                var nfile = webClient.Encoding.GetBytes(package);
                webClient.UseDefaultCredentials = true;
                if (ConfigurationManager.AppSettings["UseProxy"] == "true")
                {
                    IWebProxy proxy = new WebProxy(ConfigurationManager.AppSettings["ProxyAddress"]);
                    proxy.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ProxyUserName"], ConfigurationManager.AppSettings["ProxyPassword"]);
                    webClient.Proxy = proxy;
                }
                byte[] resp = webClient.UploadData(parseUrl + "convertedtext", "POST", nfile);
                string tData = System.Text.Encoding.UTF8.GetString(resp);
                tData = tData.Substring(1, (tData.Length - 2));
                resumeText = tData;
                #endregion getting resume text
                #region getting resume html
                string apiUrl = null, oDoc = null, sData = null;
                var oHtml = new GetHtmlText();
                oHtml.FileData = bFileData;
                oHtml.FileName = sFileName;
                oHtml.ResponseType = ".html";
                oDoc = Newtonsoft.Json.JsonConvert.SerializeObject(oHtml, Newtonsoft.Json.Formatting.None);
                apiUrl = parseUrl + "htmltext";
                Uri address = new Uri(apiUrl);
                HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
                request.Method = "Post";
                request.ContentType = "application/json";
                request.UseDefaultCredentials = true;
                var formatData = Encoding.ASCII.GetBytes(oDoc);
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(formatData, 0, formatData.Length);
                    stream.Flush();
                    stream.Close();
                }
                using (Stream responseStream = request.GetResponse().GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.ASCII);
                    sData = reader.ReadToEnd();
                    sData = sData.Substring(1, (sData.Length - 2));
                    byte[] bHtmlDoc = Convert.FromBase64String(sData);
                    resumeHTML = Encoding.ASCII.GetString(bHtmlDoc);
                }
                #endregion getting resume html
            }
            catch { }
        }
        public Int64 SaveResumeDocument_BOT(CreateResumeDocument doc, SqlConnection con)
        {
            Int64 code = 0;
            try
            {
                //using (SqlConnection con1 = new SqlConnection(Common.Connection.ConStr))
                //{
                using (SqlCommand cmd1 = con.CreateCommand())
                {
                    //cmd1.Transaction = oTrans;
                    cmd1.CommandText = "usp_CreateResumeDocument";
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@RID", 0);
                    cmd1.Parameters.AddWithValue("@TID", 0);
                    cmd1.Parameters.AddWithValue("@UserID", 0);
                    cmd1.Parameters.AddWithValue("@ResID", doc.ResID);
                    cmd1.Parameters.AddWithValue("@FileData", doc.PDFFileData);//
                                                                               //CMD.Parameters.AddWithValue("@FileData", Convert.ToBase64String(objCND.ResumeDocument));//
                    cmd1.Parameters.AddWithValue("@FileType", doc.FileName == "" ? "" : Path.GetExtension(doc.FileName));
                    cmd1.Parameters.AddWithValue("@FileName", doc.FileName);
                    cmd1.Parameters.AddWithValue("@Description", "");
                    cmd1.Parameters.AddWithValue("@ResHTMLText", doc.ResHTMLText);
                    cmd1.Parameters.AddWithValue("@ResConvertedText", doc.ResConvertedText);
                    cmd1.Parameters.AddWithValue("@ReqResID", 0);
                    var opCode = cmd1.Parameters.Add("@OPcode", SqlDbType.BigInt);
                    cmd1.Parameters.AddWithValue("@FileSize", doc.PDFFileData.Length);

                    opCode.Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    code = Convert.ToInt64(opCode.Value);
                }
                //}
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
            }
            return code;
        }
        public Int64 beSaveCandidateDocument_Career_BOT(ParamUpdateDoc_Career objParamUpdateDoc, SqlConnection con)
        {
            #region Member           
            byte[] FileData = null;
            int Filelen = 0;
            Int32 tFileType = 0;
            Int64 code = 0;
            #endregion
            try
            {
                #region Get FileType/FileData/Length
                string[] sFileExt = objParamUpdateDoc.FileName.Split('.');

                tFileType = Common.GetFileType(sFileExt[1]);

                FileData = FileCompress.opDataZip(Common.bStr(objParamUpdateDoc.FileData));
                Filelen = FileData.Length;
                #endregion

                using (SqlCommand cmd = con.CreateCommand())
                {
                    //cmd.Transaction = oTrans;
                    cmd.CommandText = "usp_UpdateCandidateProfilePic_Career";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ResID", objParamUpdateDoc.ResID);
                    cmd.Parameters.AddWithValue("@CandidateNo", objParamUpdateDoc.CandidateNo);
                    cmd.Parameters.AddWithValue("@FileName", objParamUpdateDoc.FileName);
                    cmd.Parameters.AddWithValue("@FileData", FileData);
                    cmd.Parameters.AddWithValue("@FileSize", Filelen);
                    cmd.Parameters.AddWithValue("@FileType", tFileType);
                    cmd.Parameters.AddWithValue("@UserID", objParamUpdateDoc.UserID);
                    cmd.Parameters.AddWithValue("@DocumentID", objParamUpdateDoc.DocumentID);
                    cmd.Parameters.AddWithValue("@Notes", objParamUpdateDoc.Notes == null ? "" : objParamUpdateDoc.Notes);

                    var opCode = cmd.Parameters.Add("@OpCode", SqlDbType.BigInt);
                    opCode.Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    code = Convert.ToInt64(opCode.Value);
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, "upload Document api: Career : " + ex);
            }
            finally
            {

            }
            return code;
        }
        public apiResponse beSaveResume_Career_BOT(JObject oData)
        {
            apiResponse oResponse = new apiResponse();
            Int64 iResID = 0, code = 0, iDocSussess = 0; //iReqID = 0, iUserID = 0,  iSourceID = 0, iDupRID = 0, iDupResumeSourceID = 0, iCreatedUserID = 0, istateid = 0, 
            Int16 iSourceTRype = 0; //iGender = 0,
            string Msg = "", fData = "", FileName = "", SourceEmailID = "", opCandidateNo = "", ProfilePic = "", ProfilePicFileName = "";
            //SqlTransaction oTransact = null;

            // CandidateData_save resumeDAta1 = new CandidateData_save();

            duplicateCheck dupCheck = new duplicateCheck();

            dupCheck = beCheckResumeDuplicate_BOT(oData);

            if (!dupCheck.IsResDuplicate)
            {
                #region[ Data table preperation]
                DataTable dtMapping = new DataTable();
                DataRow dr;
                dtMapping.Columns.Add("Fieldname", typeof(string));
                dtMapping.Columns.Add("Value", typeof(string));

                foreach (JProperty prop in oData.Properties())
                {
                    if (prop.Name.ToLower() == "resumedocument")
                    {
                        fData = Convert.ToString(prop.Value);

                    }
                    else if (prop.Name.ToLower() == "filename")
                    {
                        FileName = Convert.ToString(prop.Value);

                    }
                    else if (prop.Name.ToLower() == "sourcetype")
                    {
                        iSourceTRype = Convert.ToInt16(prop.Value);
                    }
                    else if (prop.Name.ToLower() == "empmailIid")
                    {
                        SourceEmailID = Convert.ToString(prop.Value);
                    }
                    else if (prop.Name.ToLower() == "profilepic")
                    {
                        ProfilePic = Convert.ToString(prop.Value);
                    }
                    else if (prop.Name.ToLower() == "profilepicfilename")
                    {
                        ProfilePicFileName = Convert.ToString(prop.Value);
                    }
                    else
                    {
                        dr = dtMapping.NewRow();
                        dr["Fieldname"] = prop.Name;
                        dr["Value"] = prop.Value;
                        dtMapping.Rows.Add(dr);
                    }
                }
                #endregion

                try
                {
                    using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                    {
                        con.Open();
                        //oTransact = con.BeginTransaction();
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            // cmd.Transaction = oTransact;
                            cmd.CommandText = "usp_saveCandidateResume";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ReqID", 0);
                            cmd.Parameters.AddWithValue("@SourceType", iSourceTRype);
                            cmd.Parameters.AddWithValue("@UDTT", dtMapping);

                            var opResID = cmd.Parameters.Add("@opResID", SqlDbType.BigInt);
                            opResID.Direction = ParameterDirection.Output;

                            var opCode = cmd.Parameters.Add("@opCode", SqlDbType.BigInt);
                            opCode.Direction = ParameterDirection.Output;

                            var opMsg = cmd.Parameters.Add("@opMsg", SqlDbType.NVarChar, -1);
                            opMsg.Direction = ParameterDirection.Output;

                            var opApplicantNo = cmd.Parameters.Add("@CandidateNo", SqlDbType.NVarChar, -1);
                            opApplicantNo.Direction = ParameterDirection.Output;

                            var opUserID = cmd.Parameters.Add("@opUserID", SqlDbType.BigInt);
                            opUserID.Direction = ParameterDirection.Output;

                            cmd.ExecuteNonQuery();

                            code = Convert.ToInt64(opCode.Value);
                            Msg = Convert.ToString(opMsg.Value);
                            iResID = Convert.ToInt64(opResID.Value);
                            opCandidateNo = Convert.ToString(opApplicantNo.Value);

                            if (code == 1)
                            {
                                /*Commented due to more time taking while converting file to html and raw text*/
                                //byte[] iDataFile = System.Convert.FromBase64String(fData.ToString());
                                //string resumeContent = "", resumeText = "", resumeHTML = "";
                                //GetResumeData(iDataFile, FileName, ref resumeContent, ref resumeText, ref resumeHTML);

                                //Save Candidate Resume
                                if (fData != "" && FileName != "")
                                {
                                    try
                                    {
                                        CreateResumeDocument oDoc = new CreateResumeDocument();
                                        oDoc.ResID = iResID;
                                        oDoc.PDFFileData = fData;
                                        oDoc.ResHTMLText = ""; //resumeHTML;
                                        oDoc.ResConvertedText = ""; //resumeText;
                                        oDoc.FileName = FileName;

                                        // iDocSussess = SaveResumeDocument(oDoc, oTransact, con);
                                        iDocSussess = SaveResumeDocument_BOT(oDoc, con);
                                    }
                                    catch (Exception ex)
                                    {
                                        Common.Logs("Resume:SaveResumeDocument()  CandidateNo " + opCandidateNo + " " + ex.ToString());
                                    }
                                }

                                //Save Candidate Profile Pic
                                if (ProfilePicFileName != "" && ProfilePic != "")
                                {
                                    try
                                    {
                                        ParamUpdateDoc_Career oDocUpdate = new ParamUpdateDoc_Career();
                                        oDocUpdate.ResID = iResID;
                                        oDocUpdate.CandidateNo = opCandidateNo;
                                        oDocUpdate.FileName = ProfilePicFileName;
                                        oDocUpdate.FileData = ProfilePic;
                                        oDocUpdate.UserID = Convert.ToInt64(opUserID.Value);

                                        // Int64 iDoc = beSaveCandidateDocument_Career(oDocUpdate, oTransact, con);
                                        Int64 iDoc = beSaveCandidateDocument_Career_BOT(oDocUpdate, con);
                                    }
                                    catch (Exception ex)
                                    {
                                        Common.Logs("Resume:beSaveCandidateDocument_Career(): CandidateNo " + opCandidateNo + " " + ex.ToString());
                                    }
                                }

                                oResponse.ResponseCode = 100;
                                oResponse.ResponseStatus = "success";
                                oResponse.ResponseMessage = Msg;
                                oResponse.NoOfRecord = 1;
                                oResponse.Data = opCandidateNo;
                            }
                            else if (code == -1)
                            {
                                oResponse.ResponseCode = 101;
                                oResponse.ResponseStatus = "Fail";
                                oResponse.ResponseMessage = Msg;
                                oResponse.NoOfRecord = 1;
                                oResponse.Data = "";
                            }

                            //if (oTransact != null)
                            //    oTransact.Commit();

                            if (code == 1 && SourceEmailID != "")
                            {
                                opCreateFavorite_BOT(iResID.ToString(), SourceEmailID, 100);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //if (oTransact != null)
                    //    oTransact.Rollback();
                    Common.ErrorRes(res, ex);
                    oResponse = Common.SomethingWentWrongResponse(oResponse, "Something went wrong.");
                }
            }
            else
            {
                oResponse.ResponseCode = 101;
                oResponse.ResponseStatus = "Fail";
                oResponse.ResponseMessage = "Application is already present in the database with applicant ID :" + Convert.ToString(dupCheck.CandidateNo);
                oResponse.NoOfRecord = 1;
                oResponse.Data = Convert.ToString(dupCheck.CandidateNo);
            }
            return oResponse;
        }
        public string opCreateFavorite_BOT(string ResID, string EmpEmailID, int ResponseNumber)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand oSql = con.CreateCommand())
                    {
                        oSql.CommandText = "CreateReferralFavouriteTag";
                        oSql.CommandType = CommandType.StoredProcedure;
                        oSql.Parameters.AddWithValue("@resid", ResID);
                        oSql.Parameters.AddWithValue("@empEmailID", EmpEmailID);
                        oSql.Parameters.AddWithValue("@responseNo", ResponseNumber);
                        var TemplateName = oSql.Parameters.Add("@TemplateName", SqlDbType.NVarChar, 500);
                        var MailSubject = oSql.Parameters.Add("@mailSubject", SqlDbType.NVarChar, 500);
                        TemplateName.Direction = ParameterDirection.Output;
                        MailSubject.Direction = ParameterDirection.Output;
                        if (con.State == ConnectionState.Closed)
                            con.Open();
                        oSql.ExecuteNonQuery();

                        //templateName = Convert.ToString(oSql.Parameters["@TemplateName"].Value);
                        //mailSubject = Convert.ToString(oSql.Parameters["@mailSubject"].Value);

                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
            }

            return "1";
        }
        public weightageanddupCriteria beGetweightageanddupCriteria_BOT(JObject oResData)
        {
            weightageanddupCriteria oRes = new weightageanddupCriteria();
            DataSet objDs = new DataSet();
            try
            {
                using (SqlConnection sqlCon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand sqlCmd = sqlCon.CreateCommand())
                    {
                        sqlCmd.CommandText = "usp_GetWeightageAndCupCriteria";
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@EmployeeID", Convert.ToString(oResData.GetValue("EmpID")));

                        Common.ErrorRes(res, "Step6");
                        SqlDataAdapter objda = new SqlDataAdapter(sqlCmd);
                        objda.Fill(objDs);
                        objda.Dispose();

                        if (objDs.Tables[0].Rows.Count > 0)
                        {
                            oRes.Criteria = objDs.Tables[0].Rows[0]["Criteria"].ToString();
                            oRes.FinalWeightage = Convert.ToDecimal(objDs.Tables[0].Rows[0]["FINALWEIGHTAGE"].ToString());
                            oRes.COOLINGPERIOD = Convert.ToInt16(objDs.Tables[0].Rows[0]["COOLINGPERIOD"].ToString());
                            if (objDs.Tables[1].Rows.Count > 0)
                            {
                                oRes.SourceEmailID = objDs.Tables[1].Rows[0]["SourceEmailID"].ToString() == null ? "" : objDs.Tables[1].Rows[0]["SourceEmailID"].ToString();
                                oRes.SourceCoolingPeriod = objDs.Tables[1].Rows[0]["SourceCoolingPeriod"].ToString() == "" ? "45" : objDs.Tables[1].Rows[0]["SourceCoolingPeriod"].ToString();
                            }
                            else
                            {
                                oRes.SourceEmailID = "";
                                oRes.SourceCoolingPeriod = "45";
                            }
                        }
                        else
                        {
                            oRes.Criteria = "";
                            oRes.FinalWeightage = 0;
                            oRes.COOLINGPERIOD = 0;
                            oRes.SourceEmailID = "";
                            oRes.SourceCoolingPeriod = "";
                        }
                        oRes.FinalWeightage = 12; // as per v3 
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
            }
            return oRes;
        }
        public duplicateCheck beCheckResumeDuplicate_BOT(JObject _ResData)
        {
            duplicateCheck oResponse = new duplicateCheck();
            apiResponse res1 = new apiResponse();
            DataSet objDs = new DataSet();
            decimal tTotalWeightage = 0, iFinalWeightage = 0;
            Int64 iDupRID = 0, iDupResumeSourceID = 0;
            DateTime dtDocMod = DateTime.Now;
            string dtSeniorReferDate = "", tSourceCoolingPeriod = "", CandidateID = "", dupSourceEmailID = "", dupSeniorSourceEmailID = "", EmpID = "";
            bool IsDuplicate = true, IsOverWrite = false, IsGradeOverwrite = false, IsGradeDuplicate = true;
            int TotalDays = 0, iCOOLINGPERIOD = 0;

            EmpID = Convert.ToString(_ResData.GetValue("EmpID"));
            weightageanddupCriteria oWeight = new weightageanddupCriteria();

            oWeight = beGetweightageanddupCriteria_BOT(_ResData);
            iFinalWeightage = oWeight.FinalWeightage;
            tSourceCoolingPeriod = oWeight.SourceCoolingPeriod;
            iCOOLINGPERIOD = oWeight.COOLINGPERIOD;
            try
            {
                using (SqlConnection oConn = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand oCmdd = oConn.CreateCommand())
                    {
                        oCmdd.CommandText = "usp_CheckResumeDuplicate_Career";
                        oCmdd.CommandType = CommandType.StoredProcedure;
                        oCmdd.Parameters.AddWithValue("@FName", Convert.ToString(_ResData.GetValue("Firstname")));
                        oCmdd.Parameters.AddWithValue("@LName", Convert.ToString(_ResData.GetValue("LastName")));
                        oCmdd.Parameters.AddWithValue("@EmailID", Convert.ToString(_ResData.GetValue("EmailID")));
                        oCmdd.Parameters.AddWithValue("@MobileNo", Convert.ToString(_ResData.GetValue("MobileNo")));
                        oCmdd.Parameters.AddWithValue("@DOB", Convert.ToString(_ResData.GetValue("DOB")));

                        SqlDataAdapter adata = new SqlDataAdapter(oCmdd);
                        adata.Fill(objDs, "HC_USER_MAIN");
                        adata.Dispose();

                        if (objDs.Tables["HC_USER_MAIN"].Rows.Count > 0)
                        {
                            iDupRID = Convert.ToInt64(objDs.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                            iDupResumeSourceID = Convert.ToInt64(objDs.Tables["HC_USER_MAIN"].Rows[0]["SourceID"]);//OriginalSourceID
                            dtDocMod = Convert.ToDateTime(objDs.Tables["HC_USER_MAIN"].Rows[0]["ModifiedDate"]);//Last Modified
                            if (objDs.Tables["HC_USER_MAIN"].Rows[0]["SeniorReferDate"] != null)
                                dtSeniorReferDate = Convert.ToString(objDs.Tables["HC_USER_MAIN"].Rows[0]["SeniorReferDate"]);//Last Modified
                            tTotalWeightage = Convert.ToDecimal(objDs.Tables["HC_USER_MAIN"].Rows[0]["Weightage"].ToString());
                            CandidateID = Convert.ToString(objDs.Tables["HC_USER_MAIN"].Rows[0]["UniqueNo"].ToString());
                            dupSourceEmailID = Convert.ToString(objDs.Tables["HC_USER_MAIN"].Rows[0]["SourceEmailID"].ToString());
                            dupSeniorSourceEmailID = Convert.ToString(objDs.Tables["HC_USER_MAIN"].Rows[0]["SeniorSourceEmailID"].ToString());
                        }
                        oResponse.CandidateNo = CandidateID;
                        int iSourceCoolingPeriod;
                        int.TryParse(tSourceCoolingPeriod, out iSourceCoolingPeriod);
                        if (tTotalWeightage < iFinalWeightage || tTotalWeightage == 0)
                        {
                            IsDuplicate = false;//New Resume 
                            IsGradeDuplicate = false;
                            iDupRID = 0;
                        }

                        #region Checking Cooling period
                        if (IsDuplicate)
                        {
                            int dupCoolingPeriod = 0;
                            string dupResumeID = "";
                            TotalDays = (DateTime.Now - dtDocMod).Days;

                            if ((dupSeniorSourceEmailID != "" && EmpID != "") || (dupSourceEmailID != "" && EmpID != ""))
                            {
                                using (SqlCommand _cmd = oConn.CreateCommand())
                                {
                                    _cmd.CommandText = "usp_getDuplicateResID_Career";
                                    _cmd.CommandType = CommandType.StoredProcedure;
                                    _cmd.Parameters.AddWithValue("@dupSeniorSourceEmailID", dupSeniorSourceEmailID);
                                    _cmd.Parameters.AddWithValue("@dupSourceEmailID", dupSeniorSourceEmailID);
                                    _cmd.Parameters.AddWithValue("@iDupRID", iDupRID);

                                    DataSet oDataSet = new DataSet();
                                    SqlDataAdapter _oData = new SqlDataAdapter(_cmd);
                                    _oData.Fill(oDataSet);

                                    if (dupSeniorSourceEmailID != "")
                                    {
                                        dupCoolingPeriod = Convert.ToInt16(oDataSet.Tables[0].Rows[0]["SourceCoolingPeriod"].ToString());
                                    }
                                    else if (dupSourceEmailID != "")
                                    {
                                        dupCoolingPeriod = Convert.ToInt16(oDataSet.Tables[0].Rows[0]["SourceCoolingPeriod"].ToString());
                                        IsGradeDuplicate = TotalDays <= iSourceCoolingPeriod;
                                    }
                                    dupResumeID = oDataSet.Tables[1].Rows[0]["RID"].ToString();
                                }

                                if (TotalDays >= iSourceCoolingPeriod && TotalDays != dupCoolingPeriod)
                                {
                                    IsGradeOverwrite = iSourceCoolingPeriod <= TotalDays && iCOOLINGPERIOD > TotalDays;
                                    IsDuplicate = false;
                                    if (!IsGradeOverwrite)
                                        IsOverWrite = true;

                                    #region Save HC_RESUME_BANK_DUPLICATE

                                    if (iDupRID > 0)
                                    {
                                        using (SqlCommand _cmd1 = oConn.CreateCommand())
                                        {
                                            _cmd1.CommandText = "usp_updateResumeBankDuplicate_Career";
                                            _cmd1.CommandType = CommandType.StoredProcedure;
                                            _cmd1.Parameters.AddWithValue("@DupResId", dupResumeID);
                                            _cmd1.Parameters.AddWithValue("@SourceID", 0);
                                            _cmd1.Parameters.AddWithValue("@FirstName", Convert.ToString(_ResData.GetValue("Firstname")));
                                            _cmd1.Parameters.AddWithValue("@LastName", Convert.ToString(_ResData.GetValue("LastName")));
                                            _cmd1.Parameters.AddWithValue("@MiddleName", "");
                                            _cmd1.Parameters.AddWithValue("@EmailID", Convert.ToString(_ResData.GetValue("EmailID")));
                                            _cmd1.Parameters.AddWithValue("@PhoneNo", "");
                                            _cmd1.Parameters.AddWithValue("@MobileNo", Convert.ToString(_ResData.GetValue("MobileNo")));
                                            _cmd1.Parameters.AddWithValue("@DOB", Convert.ToString(_ResData.GetValue("DOB")));
                                            _cmd1.Parameters.AddWithValue("@UserID", 0);
                                            _cmd1.Parameters.AddWithValue("@SourceEmailID", Convert.ToString(_ResData.GetValue("EmpMailID")));

                                            var opCode = _cmd1.Parameters.Add("@OpCode", SqlDbType.BigInt);
                                            opCode.Direction = ParameterDirection.Output;
                                        }
                                    }
                                    #endregion   Save HC_RESUME_BANK_DUPLICATE
                                }
                            }

                            if (!IsGradeOverwrite && IsDuplicate)
                            {
                                res1.Data = CandidateID;
                                res1.NoOfRecord = 1;
                                res1.ResponseCode = 101;
                                res1.ResponseMessage = "Application is already present in the database with applicant ID :" + CandidateID;

                                //opCreateFavorite(dupResumeID, Convert.ToString(_ResData.GetValue("EmpMailID")), 101);
                                //return res1;
                            }

                        }
                        #endregion

                        oResponse.IsResDuplicate = IsDuplicate;
                        oResponse.IsResGradeDuplicat = IsGradeDuplicate;
                        oResponse.IsResGradeOverwrite = IsGradeOverwrite;
                        oResponse.IsResOverWrite = IsOverWrite;

                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
            }

            return oResponse;
        }

        #endregion

        #region[ Candidate Basic Info ]
        public apiResponse beGetBasicDetails(string CandidateNo)
        {
            DataSet objDataSet = new DataSet();
            apiResponse Response = new apiResponse();
            SqlDataAdapter objda = null;

            try
            {
                using (SqlConnection objcon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand oCmd = objcon.CreateCommand())
                    {
                        oCmd.CommandText = "usp_GetBasicDetails_Career";
                        oCmd.CommandType = CommandType.StoredProcedure;
                        oCmd.Parameters.AddWithValue("@CandidateNo", CandidateNo);
                        oCmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);
                        objda = new SqlDataAdapter(oCmd);
                        objda.Fill(objDataSet);
                    }
                }
                BasicDetails_career BasicInfo = null;

                if (objDataSet == null)
                    Response = (Common.NotFoundResponse(res));
                else if (objDataSet.Tables[0].Rows.Count == 0)
                    Response = (Common.NotFoundResponse(res));
                else
                {
                    DataRow dr = objDataSet.Tables[0].Rows[0];
                    BasicInfo = new BasicDetails_career();
                    BasicInfo.ResumeID = Common.bInt64(dr["RID"]);
                    BasicInfo.CandidateNo = Common.bStr(dr["CandidateNo"]);
                    BasicInfo.Salutation = Common.bStr(dr["Salutation"]);
                    BasicInfo.FirstName = Common.bStr(dr["FirstName"]);
                    BasicInfo.MiddleName = Common.bStr(dr["MiddleName"]);
                    BasicInfo.LastName = Common.bStr(dr["LastName"]);
                    BasicInfo.Mobile = Common.bStr(dr["Mobile"]);
                    BasicInfo.DOB = Common.bStr(dr["DOB"]);
                    BasicInfo.Gender = Common.bInt64(dr["Gender"]);
                    BasicInfo.Email = Common.bStr(dr["Email"]);
                    BasicInfo.PAN = Common.bStr(dr["PAN"]);
                    BasicInfo.Aadhaar = Common.bStr(dr["Aadhaar"]);
                    BasicInfo.TotalExp = Common.bStr(dr["TotalExP"]);
                    BasicInfo.Source = Common.bStr(dr["Source"]);
                    //BasicInfo.Photo = Common.bStr(dr["Photo"]);
                    if (!Convert.IsDBNull(dr["Photo"]))
                    {
                        BasicInfo.Photo = Common.bStr(FileCompress.opDataUnzip((byte[])dr["Photo"]));
                    }
                    else
                    {
                        BasicInfo.Photo = "";
                    }
                    BasicInfo.isPhotoVerified = Convert.ToInt64(dr["isPhotoVerified"]);
                    BasicInfo.ApplicanteKYC = Convert.ToInt32(dr["ApplicanteKYC"]);
                    BasicInfo.RMeKYC = Convert.ToInt32(dr["RMeKYC"]);
                    BasicInfo.IsGraduation = Convert.ToBoolean(dr["IsGraduation"]);
                    BasicInfo.GraduationPercentage = Convert.ToInt64(dr["GraduationPercentage"]);
                    BasicInfo.GraduationPassYear = Convert.ToInt64(dr["GraduationPassYear"]);

                    Response = (Common.SuccessResponse(res, BasicInfo, objDataSet.Tables[0].Rows.Count));
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");

            }
            finally
            {
                if (objda != null)
                {
                    objda.Dispose();
                    objda = null;
                }

                if (objDataSet != null)
                {
                    objDataSet.Dispose();
                    objDataSet = null;
                }

            }
            return Response;
            //return RecruiterClass.Instance.beBasicInfo(CandidateNo);
        }
        #endregion

        #region[ Carrer Candidate Progress Status ]
        public apiResponse beGetCandidateProgressStatus_Career(inputParams oParam)
        {
            apiResponse Response = new apiResponse();
            DataSet objDs = new DataSet();
            DataTable objdt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetCandidateProgressStatus_Career";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CandidateNo", oParam.CandidateNo);
                        cmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);

                        SqlDataAdapter objda = new SqlDataAdapter(cmd);
                        objda.Fill(objdt);

                        if (objdt == null)
                            Response = (Common.NotFoundResponse(res));
                        else if (objdt.Rows.Count == 0)
                            return Response = (Common.NotFoundResponse(res));
                        else
                        {
                            var getCanStatus = Common.DataTableToJSONObject(objdt);

                            Response = (Common.SuccessResponse(res, getCanStatus, objdt.Rows.Count));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                //Response = Common.ErrorRes(res, ex);
                Common.ErrorRes(res, ex);
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");
            }
            return Response;
        }

        #endregion

        #region[ Candidate Quick Status ]
        public apiResponse beQuickCandiatestatus(string candidateNo)
        {
            apiResponse Response = new apiResponse();

            DataSet objds = null;
            try
            {
                objds = new DataSet();
                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    objCon.Open();
                    using (SqlCommand oCmd = objCon.CreateCommand())
                    {
                        oCmd.CommandText = "usp_getQuickCandiatestatus";
                        oCmd.CommandType = CommandType.StoredProcedure;
                        oCmd.Parameters.AddWithValue("@candidateNo", candidateNo);
                        oCmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);
                        SqlDataAdapter objda = new SqlDataAdapter(oCmd);
                        objda.Fill(objds);

                        if (objds == null)
                            Response = (Common.NotFoundResponse(res));
                        else if (objds.Tables[0].Rows.Count == 0)
                            return Response = Common.ErrorResponse(res, 0, "Invalid Request! kindly validate the Candidate No.");
                        else if (objds.Tables[0].Rows.Count == 0)
                            return Response = (Common.NotFoundResponse(res));
                        else
                        {
                            QuickCanStatus objQuickCanStatus = new QuickCanStatus();
                            objQuickCanStatus.ReqResID = Common.bInt64(objds.Tables[0].Rows[0]["ReqResID"]);
                            objQuickCanStatus.JobID = Common.bInt64(objds.Tables[0].Rows[0]["JobID"]);
                            objQuickCanStatus.StageTitle = Common.bStr(objds.Tables[0].Rows[0]["StageTitle"]);
                            objQuickCanStatus.StatusTitle = Common.bStr(objds.Tables[0].Rows[0]["StatusTitle"]);
                            objQuickCanStatus.IsOPQApplicable = Common.bBoolean(objds.Tables[0].Rows[0]["IsOPQApplicable"]);
                            objQuickCanStatus.OPQStatus = Common.bStr(objds.Tables[0].Rows[0]["OPQStatus"]);
                            objQuickCanStatus.OPQLink = Common.bStr(objds.Tables[0].Rows[0]["OPQLink"]);
                            objQuickCanStatus.IsSPTApplicable = Common.bBoolean(objds.Tables[0].Rows[0]["IsSPTApplicable"]);
                            objQuickCanStatus.SPTStatus = Common.bStr(objds.Tables[0].Rows[0]["SPTStatus"]);
                            objQuickCanStatus.IsDeclarationSubmitted = Common.bBoolean(objds.Tables[0].Rows[0]["IsDeclarationSubmitted"]);
                            objQuickCanStatus.IsPANUplooded = Common.bBoolean(objds.Tables[0].Rows[0]["IsPANUplooded"]);
                            objQuickCanStatus.IsPhotoUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsPhotoUploaded"]);
                            objQuickCanStatus.IsAadharUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsAadharUploaded"]);
                            objQuickCanStatus.IsLicenseUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsLicenseUploaded"]);
                            objQuickCanStatus.IsPassportUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsPassportUploaded"]);
                            objQuickCanStatus.IsVoterIdUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsVoterIdUploaded"]);
                            objQuickCanStatus.Is10thMarksUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["Is10thMarksUploaded"]);
                            objQuickCanStatus.Is12thMaksUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["Is12thMaksUploaded"]);
                            objQuickCanStatus.IsCAUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsCAUploaded"]);
                            objQuickCanStatus.IsOtherCareerUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsOtherCareerUploaded"]);
                            objQuickCanStatus.IsPostGradUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsPostGradUploaded"]);
                            objQuickCanStatus.IsGradUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsGradUploaded"]);
                            objQuickCanStatus.IsPaySlipUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsPaySlipUploaded"]);
                            objQuickCanStatus.IsAddressProofUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsAddressProofUploaded"]);
                            objQuickCanStatus.IsRelievingletterUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsRelievingletterUploaded"]);
                            objQuickCanStatus.IsResignationDocUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsResignationDocUploaded"]);
                            objQuickCanStatus.IsLatestCvUploaded = Common.bBoolean(objds.Tables[0].Rows[0]["IsLatestCvUploaded"]);
                            objQuickCanStatus.IsResume = Common.bBoolean(objds.Tables[0].Rows[0]["IsResume"]);
                            objQuickCanStatus.GradeID = Common.bInt64(objds.Tables[0].Rows[0]["GradeID"]);
                            objQuickCanStatus.Grade = Common.bStr(objds.Tables[0].Rows[0]["Grade"]);
                            objQuickCanStatus.JoingingConfirmDate = Common.bStr(objds.Tables[0].Rows[0]["JoingingConfirmDate"]);
                            objQuickCanStatus.Reason = Common.bStr(objds.Tables[0].Rows[0]["Reason"]);
                            objQuickCanStatus.Notes = Common.bStr(objds.Tables[0].Rows[0]["Notes"]);
                            objQuickCanStatus.JoinigDateCount = Common.bInt64(objds.Tables[0].Rows[0]["JoinigDateCount"]);
                            objQuickCanStatus.OfferAcceptanceDate = Common.bStr(objds.Tables[0].Rows[0]["OfferAcceptanceDate"]);
                            objQuickCanStatus.InductionMeetingURL = Common.bStr(objds.Tables[0].Rows[0]["InductionMeetingURL"]);
                            objQuickCanStatus.InductionMeetingPassword = Common.bStr(objds.Tables[0].Rows[0]["InductionMeetingPassword"]);
                            objQuickCanStatus.iseKYCCompleted = Common.bInt64(objds.Tables[0].Rows[0]["iseKYCCompleted"]);
                            objQuickCanStatus.isBookSlot = Common.bInt32(objds.Tables[0].Rows[0]["isBookSlot"]);
                            objQuickCanStatus.IndentNo = Common.bStr(objds.Tables[0].Rows[0]["IndentNo"]);
                            objQuickCanStatus.ResumeID = Common.bInt64(objds.Tables[0].Rows[0]["ResumeID"]);
                            objQuickCanStatus.isPOjob = Common.bBoolean(objds.Tables[0].Rows[0]["isPOjob"]);
                            objQuickCanStatus.SelfLoanFund = Common.bInt32(objds.Tables[0].Rows[0]["SelfLoanFund"]);
                            objQuickCanStatus.PlannedJoiningDate = Common.bStr(objds.Tables[0].Rows[0]["PlannedJoiningDate"]);
                            objQuickCanStatus.IsReScheduled = Common.bBoolean(objds.Tables[0].Rows[0]["IsReScheduled"]);
                            objQuickCanStatus.isEnableDeclaration = Common.bBoolean(objds.Tables[0].Rows[0]["isEnableDeclaration"]);
                            objQuickCanStatus.isQuestionaireeSubmitted = Common.bBoolean(objds.Tables[0].Rows[0]["isQuestionaireeSubmitted"]);
                            objQuickCanStatus.isJoiningInfo = Common.bBoolean(objds.Tables[0].Rows[0]["isJoiningInfo"]);
                            objQuickCanStatus.OfferType = Convert.ToString(objds.Tables[0].Rows[0]["OfferType"]);
                            //objQuickCanStatus.TestPassedDate = Convert.ToDateTime(objds.Tables[0].Rows[0]["TestPassedDate"]);
                            objQuickCanStatus.OfferSentCount = Convert.ToInt32(objds.Tables[0].Rows[0]["OfferSentCount"]);
                            objQuickCanStatus.IsProfilerApplicable = Convert.ToBoolean(objds.Tables[0].Rows[0]["IsProfilerApplicable"]);
                            objQuickCanStatus.IsOfferAccepted = Convert.ToInt32(objds.Tables[0].Rows[0]["IsOfferAccepted"]);
                            Response = (Common.SuccessResponse(res, objQuickCanStatus, 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");
            }

            return Response;
        }
        public apiResponse beGetQuickCandidateStatus_Career(string CandidateNo, bool isOffer = false)
        {
            apiResponse Response = new apiResponse();
            DataSet objDs = new DataSet();
            DataTable objdt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "usp_getQuickCandiatestatus_Career";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CandidateNo", CandidateNo);
                        cmd.Parameters.AddWithValue("@isOffer", isOffer);
                        cmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);

                        SqlDataAdapter objda = new SqlDataAdapter(cmd);
                        objda.Fill(objdt);

                        if (objdt == null)
                            Response = (Common.NotFoundResponse(res));
                        else if (objdt.Rows.Count == 0)
                            return Response = (Common.NotFoundResponse(res));
                        else
                        {
                            var getCanStatus = Common.DataTableToJSONObject(objdt);

                            Response = (Common.SuccessResponse(res, getCanStatus, 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");
            }
            return Response;
        }

        #endregion

        #region[ Timeline Journey ]
        public apiResponse beGetgetTimelineDetails(Int64 ReqResID, string mobileNo = "")
        {
            apiResponse Response = new apiResponse();
            DataSet objDs = new DataSet();
            DataTable objdt = new DataTable();
            mobileNo = mobileNo == null ? "" : mobileNo;
            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "usp_getApplicantTimeLineDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReqResID", ReqResID);
                        cmd.Parameters.AddWithValue("@mobileNo", mobileNo);

                        SqlDataAdapter objda = new SqlDataAdapter(cmd);
                        objda.Fill(objdt);

                        if (objdt == null)
                            Response = (Common.NotFoundResponse(res));
                        else if (objdt.Rows.Count == 0)
                            return Response = (Common.NotFoundResponse(res));
                        else
                        {

                            var getCanStatus = Common.DataTableToJSONObject(objdt);
                            string data = JsonConvert.SerializeObject(getCanStatus);
                            data = ClsCrypto.EncryptUsingAES(data);

                            Response = (Common.SuccessResponse(res, data, 1));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, "beGetgetTimelineDetails(): ReqResID: " + ReqResID.ToString() + ": " + ex.ToString());
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");
            }
            return Response;
        }

        #endregion

        #region[ Candidate Offer Accept or Reject ]

        public apiResponse beSaveCandidateOfferAcceptReject(OfferAcceptReject oOffer)
        {
            apiResponse oResponse = new apiResponse();
            Int64 code = 0;
            string msg = "";
            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "udp_SaveCandidateOfferDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReqResID", oOffer.ReqResID);
                        cmd.Parameters.AddWithValue("@ActionType", oOffer.ActionType);
                        cmd.Parameters.AddWithValue("@Type", oOffer.Type);
                        if (oOffer.ConfirmedJoiningDate != DateTime.MinValue)
                            cmd.Parameters.AddWithValue("@ConfirmedJoiningDate", oOffer.ConfirmedJoiningDate);
                        cmd.Parameters.AddWithValue("@Notes", oOffer.Notes);
                        cmd.Parameters.AddWithValue("@ReasonID", oOffer.ReasonID);
                        cmd.Parameters.AddWithValue("@UserID", oOffer.UserID);
                        cmd.Parameters.AddWithValue("@InductionMeetingURL", oOffer.InductionMeetingURL == "" ? "" : ClsCrypto.DecryptUsingAES(oOffer.InductionMeetingURL));
                        cmd.Parameters.AddWithValue("@InductionMeetingPassword", oOffer.InductionMeetingPassword == "" ? "" : ClsCrypto.DecryptUsingAES(oOffer.InductionMeetingPassword));

                        var opCode = cmd.Parameters.Add("@opCode", SqlDbType.BigInt);
                        opCode.Direction = ParameterDirection.Output;

                        var opMsg = cmd.Parameters.Add("@opMsg", SqlDbType.NVarChar, -1);
                        opMsg.Direction = ParameterDirection.Output;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        code = Convert.ToInt64(opCode.Value);
                        msg = Convert.ToString(opMsg.Value);

                        if (code > 0)
                        {
                            oResponse.NoOfRecord = 1;
                            oResponse.ResponseCode = 100;
                            oResponse.ResponseMessage = msg;
                            oResponse.Data = "";
                        }
                        else if (code == -2)
                        {
                            oResponse.NoOfRecord = 1;
                            oResponse.ResponseCode = 101;
                            oResponse.ResponseMessage = msg;
                            oResponse.Data = "";
                        }
                        else if (code == -3)
                        {
                            oResponse.NoOfRecord = 1;
                            oResponse.ResponseCode = 101;
                            oResponse.ResponseMessage = msg;
                            oResponse.Data = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, "beSaveCandidateOfferAcceptReject(): ReqResID: " + oOffer.ReqResID.ToString() + ": " + ex.ToString());
                oResponse = Common.SomethingWentWrongResponse(oResponse, "Something went wrong.");
            }
            return oResponse;
        }

        #endregion

        #region[ Education CRUD ]
        public apiResponse beGetAcademicDetails(string CandidateNo)
        {
            apiResponse response = new apiResponse();
            DataSet objDs = new DataSet();
            DataTable objdt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetResumeEducationDetails_career";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CandidateNo", CandidateNo);
                        cmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);

                        SqlDataAdapter objda = new SqlDataAdapter(cmd);
                        objda.Fill(objdt);

                        if (objdt == null)
                            response = (Common.NotFoundResponse(res));
                        else if (objdt.Rows.Count == 0)
                            return response = (Common.NotFoundResponse(res));
                        else
                        {
                            var getCanStatus = Common.DataTableToJSONObject(objdt);
                            response = (Common.SuccessResponse(res, getCanStatus, objdt.Rows.Count));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
            }
            return response;
        }

        #endregion

        #region [Expereince Details] 

        public apiResponse beGetEmploymentDetails(string CandidateNo)
        {
            DataSet objDataSet = new DataSet();
            apiResponse Response = new apiResponse();
            try
            {
                using (SqlConnection objcon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand oCmd = objcon.CreateCommand())
                    {
                        oCmd.CommandText = "usp_getResumeEmployeeData_Career";
                        oCmd.CommandType = CommandType.StoredProcedure;
                        oCmd.Parameters.AddWithValue("@PramCandidateNo", CandidateNo);
                        oCmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);
                        SqlDataAdapter objda = new SqlDataAdapter(oCmd);
                        objda.Fill(objDataSet);
                    }
                }

                EmploymentResponse EmploymentResponse = new EmploymentResponse();
                List<EmploymentDetails> EmploymentDetails = new List<EmploymentDetails>();
                List<CompaniesRoleInfoDetail> CompaniesRoleInfoDetail = new List<CompaniesRoleInfoDetail>();
                if (objDataSet == null)
                    Response = (Common.NotFoundResponse(res));
                //else if (objDataSet.Tables[1].Rows.Count == 0 && objDataSet.Tables[2].Rows.Count == 0)
                //    Response = (Common.NotFoundResponse(res));
                else
                {
                    if (objDataSet.Tables[0].Rows[0]["TotalExP"] != null && Convert.ToString(objDataSet.Tables[0].Rows[0]["TotalExP"]) != "")
                    {
                        string[] totalexp = (objDataSet.Tables[0].Rows[0]["TotalExP"].ToString()).Split('.');
                        EmploymentResponse.MonthOfExp = Common.bInt32(totalexp[1]);
                        EmploymentResponse.YearOfExp = Common.bInt32(totalexp[0]);
                    }
                    else
                    {
                        EmploymentResponse.MonthOfExp = Common.bInt32(0);
                        EmploymentResponse.YearOfExp = Common.bInt32(0);
                    }
                    EmploymentResponse.ExpectedCTC = objDataSet.Tables[0].Rows[0]["ExpectedCTC"].ToString();
                    EmploymentResponse.ExpectedCurrency = Common.bInt32(objDataSet.Tables[0].Rows[0]["ExpectedCurrency"].ToString());
                    EmploymentResponse.CurrentCTC = objDataSet.Tables[0].Rows[0]["PresentCTC"].ToString();
                    EmploymentResponse.CurrentCurrency = Common.bInt32(objDataSet.Tables[0].Rows[0]["PresentCurrency"].ToString());
                    EmploymentResponse.IsFresher = Convert.ToBoolean(objDataSet.Tables[0].Rows[0]["IsFresher"].ToString());
                    EmploymentResponse.IsCurrentEmpStatus = Convert.ToBoolean(objDataSet.Tables[0].Rows[0]["IsCurrentEmpStatus"].ToString());

                    EmploymentDetails = (from DataRow dr in objDataSet.Tables[1].Rows
                                         select new EmploymentDetails()
                                         {
                                             EmployeeRID = Common.bInt64(dr["EmployeeRID"]),
                                             EmploymentStatus = Common.bStr(dr["EmploymentStatus"]),
                                             EmploymentType = Common.bStr(dr["EmploymentType"]),
                                             EmployeeCode = Common.bStr(dr["EmployeeCode"]),
                                             Organization = Common.bStr(dr["Organization"]),
                                             Designation = Common.bStr(dr["Designation"]),
                                             Industry = Common.bStr(dr["Industry"]),
                                             Department = Common.bStr(dr["Department"]),
                                             Currency = Common.bStr(dr["Currency"]),
                                             CTCPerMonth = Common.bStr(dr["CTCPerMonth"]),
                                             ReasonforLeaving = Common.bStr(dr["ReasonforLeaving"]),
                                             EmploymetAddress = Common.bStr(dr["EmploymetAddress"]),
                                             CurrencyID = Common.bInt64(dr["CurrencyID"]),
                                             EmploymentTypeID = Common.bInt64(dr["EmploymentTypeID"]),
                                             CountryID = Common.bInt64(dr["CountryID"]),
                                             Country = Common.bStr(dr["Country"]),
                                             IndustryID = Common.bInt64(dr["IndustryID"]),
                                             MainIndustryID = Convert.ToInt64(dr["MainIndustryID"]),
                                             MainIndustry = Convert.ToString(dr["MainIndustry"]),
                                             BFSIBankID = Convert.ToInt64(dr["BFSIBankID"]),
                                             BFSIBank = Convert.ToString(dr["BFSIBank"]),
                                             IsCampus = Convert.ToInt32(dr["IsCampus"]),
                                             CampusExp = Convert.ToDecimal(dr["CampusExp"]),
                                             IsInternship = Convert.ToInt32(dr["IsInternship"]),
                                             FromDate = Convert.ToString(dr["FromDate"]),
                                             ToDate = Convert.ToString(dr["ToDate"]),
                                             CompaniesRoleInfoDetail = (from DataRow r in objDataSet.Tables[2].Rows
                                                                        where Convert.ToInt64(r["ResEmpid"]) == Convert.ToInt64(dr["RID"])
                                                                        select new CompaniesRoleInfoDetail
                                                                        {
                                                                            DesignationID = Common.bInt64(r["DesignationId"]),
                                                                            Designation = Convert.ToString(r["Designation"]),
                                                                            FromDate = Convert.ToString(r["FromDate"]),
                                                                            ToDate = Convert.ToString(r["ToDate"]),
                                                                            isCurrentEmp = Convert.ToInt32(r["isCurrentEmp"])
                                                                        }).ToList()
                                         }).ToList();

                    EmploymentResponse.tEmploymentDetails = EmploymentDetails;
                    Response = (Common.SuccessResponse(res, EmploymentResponse, objDataSet.Tables[0].Rows.Count));
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
                Response = Common.SomethingWentWrongResponse(Response, "Something went wrong.");
            }
            finally
            {

            }
            return Response;
        }

        #endregion
    }
}
