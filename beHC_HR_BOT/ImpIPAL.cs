using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.OleDb;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using System.Xml;

namespace beHC_HR_BOT
{
    public class ImpIPAL
    {
        private ImpIPAL()
        {
        }
        private static readonly Lazy<ImpIPAL> Lazy = new Lazy<ImpIPAL>(() => new ImpIPAL());
        public static ImpIPAL Instance
        {
            get { return Lazy.Value; }
        }
        apiResponse res = null; 

        public apiResponse begetstagestatus(string mobileno)
        {            
            apiResponse response = new apiResponse();
            try
            {
                using (SqlConnection conn = new SqlConnection(Common.Connection.ConStr))
                {
                    conn.Open();
                    using (SqlCommand oCmd = conn.CreateCommand())
                    {
                        oCmd.CommandText = "usp_getcandidatestagedetails";
                        oCmd.CommandType = CommandType.StoredProcedure;
                        oCmd.Parameters.AddWithValue("@phone", mobileno);
                        var code = oCmd.Parameters.Add("@opcode", SqlDbType.BigInt);
                        code.Direction = ParameterDirection.Output;
                        SqlDataAdapter da = new SqlDataAdapter(oCmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        oCmd.ExecuteNonQuery();

                        Int64 res1 = Convert.ToInt64(code.Value);
                        if (res1 == 200)
                        {

                            var getCanStatus = Common.DataTableToJSONObject(dt);
                            response.ResponseCode = 100;
                            response.ResponseMessage = "SuccesFully Completed";
                            response.ResponseStatus = "Success";
                            response.Data = getCanStatus;
                        }
                        else
                        {
                            response.ResponseCode = 101;
                            response.ResponseMessage = "";
                            response.ResponseStatus = "Failed";
                            response.NoOfRecord = 0;
                            response.Data = "";
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
                response = Common.SomethingWentWrongResponse(response, "Something went wrong.");
            }
            return response;
        }

        public apiResponse besavefiledata(FileDetails file)
        {
            string fType = "";

            apiResponse response = new apiResponse();
            byte[] bytes = FileCompress.opDataZip(file.FileData);
            try
            {
                String[] op = file.FileName.Split('.');
                if (op.Length > 1)
                {
                    fType = '.' + op[op.Length - 1];
                }
                else
                    fType = '.' + op[0];
                switch (fType)
                {
                    case ".xls":
                    case ".xlsx":
                        file.FileType = 1;
                        break;
                    case ".pdf":
                        file.FileType = 2;
                        break;
                    case ".rtf":
                        file.FileType = 3;
                        break;
                    case ".txt":
                        file.FileType = 4;
                        break;
                    case ".doc":
                    case ".docx":
                        file.FileType = 5;
                        break;
                    case ".png":
                        file.FileType = 6;
                        break;
                    case ".jpeg":
                    case ".jpg":
                        file.FileType = 7;
                        break;
                    case ".html":
                        file.FileType = 8;
                        break;
                    case ".htm":
                        file.FileType = 9;
                        break;
                    default:
                        file.FileType = 0;
                        break;
                }
                // byte[] bytes = Convert.FromBase64String(file.FileData);

                using (SqlConnection conn = new SqlConnection(Common.Connection.ConStr))
                {
                    conn.Open();
                    using (SqlCommand oCmd = conn.CreateCommand())
                    {
                        oCmd.CommandText = "usp_saveresumedocumentdetails";
                        oCmd.CommandType = CommandType.StoredProcedure;
                        oCmd.Parameters.AddWithValue("@mobileno", file.MobileNo);
                        oCmd.Parameters.AddWithValue("@filename", file.FileName);
                        oCmd.Parameters.AddWithValue("@filedata", bytes);
                        oCmd.Parameters.AddWithValue("@docid", file.DocId);
                        oCmd.Parameters.AddWithValue("@filesize", bytes.Length);
                        oCmd.Parameters.AddWithValue("@filetype", file.FileType);
                        oCmd.Parameters.AddWithValue("@candidateno", file.candidateno);
                        var code = oCmd.Parameters.Add("@opcode", SqlDbType.BigInt);
                        code.Direction = ParameterDirection.Output;
                        var message = oCmd.Parameters.Add("@message", SqlDbType.NVarChar, -1);
                        message.Direction = ParameterDirection.Output;
                        oCmd.ExecuteNonQuery();
                        string result = Convert.ToString(message.Value);
                        Int64 res = Convert.ToInt64(code.Value);
                        if (res == 100 || res == 300)
                            response.ResponseStatus = "Success";

                        else
                            response.ResponseStatus = "Failed";

                        response.ResponseCode = res;
                        response.ResponseMessage = result;

                    }

                }
            }
            catch (Exception ex)
            {
                Common.Logs(ex.ToString());
                response.ResponseCode = 104;
                response.ResponseMessage = "Fail002";
                response.ResponseStatus = "Fail002";
            }
            return response;
        }


        #region [ Save Resume ]
        public apiResponse SaveResume(CandidateData objCND)
        {

            #region Resume
            //public string beSaveResumeV5(string data, byte[] fileContent)
            //   {
            #region Variable Part
            string ResumeTicketNumber = "", ResumeNumber = "", tReqResumeID = "";  //, tEmployeeID = "", Location
            Int64 iReqID = 0, ResumeID = 0, iResID = 0, iSourceID = 0, iDupRID = 0, iDupResumeSourceID = 0, iCreatedUserID = 0, istateid = 0; //, iSContactID = 0
            Int16 iGender = 0;
            apiResponse objRes = null;
            string tFirstName = "", tLastName = "", tFatherName = "", tEMailID = "", tPhone = "", tMobile = "", tDateofBirth = "", tDateofBirth1 = "", tResumeSourceRef = "", tNotes = "", tRecieptDate = "", tPincode = "", tFileName = ""; //, tFileSize = "", tPassport = "", tDocument = ""
            // this is for resume log
            //string tLogFirstName = "", tLogLastName = "", tLogEmailID = "", tLogPhoneH = "", tLogMobile = "", tLogPassportNo = "", tLogDocModifiedDate = "", tLogCreatedID = "";
            String tStageTitle = "", tStatusTitle = ""; //, tCurrentCTC = "", tPresentEmployer = ""
            decimal iTotalExp = 0.0m, iRelExp = 0.0m;
            bool bCheckDuplicateStatus = true;
          //  string iDupUniqueNo = "";
            string CandidateID = "";
            string tCriteria = "";
            string tpreferedlocation = ""; // tpreferedlocationtext = "",

            System.Xml.XmlNodeList xnlEducationTypes;
            decimal iFinalWeightage = 0, iCOOLINGPERIOD = 0;
            System.Xml.XmlDocument objdoc;
            objdoc = new System.Xml.XmlDocument();
            System.Xml.XmlNodeList xnlWorkExperience;
            SqlTransaction objTrans = null;
            DateTime dtDocMod = DateTime.Now;
            bool temp = false;
            string[] indArray = new string[100];
            Boolean bFlgAdded = false;
            string  tCVData = ""; //tIsApplyedAadharNo = "", tAadharNo = "",
            #endregion
            try
            {
                String str = "";
                str = CreateCandidateXML(objCND);
                objdoc.LoadXml(str);
                System.Xml.XmlNode objnode;
                #region "retrieve data from xml"
                objnode = objdoc.GetElementsByTagName("IndentID")[0];
                if (objnode.InnerText != "")
                    iReqID = Convert.ToInt64(objnode.InnerText);

                objnode = objdoc.GetElementsByTagName("EmpMailID")[0];
                tResumeSourceRef = objnode.InnerText;

                //objnode = objdoc.GetElementsByTagName("EmployeeID")[0];
                //tEmployeeID = objnode.InnerText;
                //if (tEmployeeID.Length > 50)
                //    tEmployeeID = tEmployeeID.Substring(0, 49);

                //objnode = objdoc.GetElementsByTagName("SourceID")[0];
                //if (objnode.InnerText != "")
                iSourceID = Convert.ToInt64(ConfigurationManager.AppSettings["BotSourceID"]);

                //objnode = objdoc.GetElementsByTagName("CreatedUserID")[0];
                //if (objnode.InnerText != "")
                iCreatedUserID = Convert.ToInt64(ConfigurationManager.AppSettings["CreatedUserID"]);

                //objnode = objdoc.GetElementsByTagName("SContactID")[0];
                //if (objnode.InnerText != "")
                //    iSContactID = Convert.ToInt64(objnode.InnerText);

                objnode = objdoc.GetElementsByTagName("Firstname")[0];
                tFirstName = objnode.InnerText;
                tFirstName = opValidatequotes(tFirstName);
                if (tFirstName.Length > 25)
                    tFirstName = tFirstName.Substring(0, 24);

                objnode = objdoc.GetElementsByTagName("LastName")[0];
                tLastName = objnode.InnerText;
                tLastName = opValidatequotes(tLastName);
                if (tLastName.Length > 25)
                    tLastName = tLastName.Substring(0, 24);

                try
                {
                    if (objdoc.GetElementsByTagName("DOB")[0].InnerText.IndexOf("0001") == -1)
                        tDateofBirth = objdoc.GetElementsByTagName("DOB")[0].InnerText;
                    else
                        tDateofBirth = "";
                    if (tDateofBirth != "" && tDateofBirth.StartsWith("-") == false)
                        tDateofBirth = Convert.ToDateTime(tDateofBirth).ToString("dd-MMM-yyyy hh:mm:ss tt");
                    else
                        tDateofBirth = "";
                }
                catch { }
                try
                {
                    if (objdoc.GetElementsByTagName("DOB")[0].InnerText.IndexOf("0001") == -1)
                        tDateofBirth1 = objdoc.GetElementsByTagName("DOB")[0].InnerText;
                    else
                        tDateofBirth1 = "";
                    if (tDateofBirth1 != "" && tDateofBirth1.StartsWith("-") == false)
                    {
                        tDateofBirth1 = Convert.ToDateTime(tDateofBirth1).ToString("MM-dd-yyyy");

                        tDateofBirth1 = tDateofBirth1.Replace("-", "/");
                    }
                    else
                        tDateofBirth1 = "";
                }
                catch { }
                try
                {
                    objnode = objdoc.GetElementsByTagName("EmailID")[0];
                    tEMailID = opValidatequotes(objnode.InnerText);
                    if (tEMailID.Length > 100)
                        tEMailID = tEMailID.Substring(0, 99);
                }
                catch { }
                try
                {
                    objnode = objdoc.GetElementsByTagName("MobileNo")[0];
                    tPhone = objnode.InnerText;
                    tPhone = opValidatequotes(tPhone);
                    if (tPhone.Length > 50)
                        tPhone = tPhone.Substring(0, 49);
                }
                catch { }
                objnode = objdoc.GetElementsByTagName("MobileNo")[0];
                tMobile = objnode.InnerText;
                tMobile = opValidatequotes(tMobile);
                if (tMobile.Length > 50)
                    tMobile = tMobile.Substring(0, 49);

                //objnode = objdoc.GetElementsByTagName("PassportNo")[0];
                //tPassport = objnode.InnerText;
                //tPassport = opValidatequotes(tPassport);



                //objnode = objdoc.GetElementsByTagName("TotalExperience")[0];
                //if (objnode.InnerText != "")
                //    iTotalExp = Convert.ToDecimal(objnode.InnerText);
                //else
                //    iTotalExp = 0.0m;

                //objnode = objdoc.GetElementsByTagName("ReleventExperience")[0];
                //if (objnode.InnerText != "")
                //    iRelExp = Convert.ToDecimal(objnode.InnerText);
                //else
                //    iRelExp = 0.0m;

                //objnode = objdoc.GetElementsByTagName("FatherName")[0];
                //tFatherName = opValidatequotes(objnode.InnerText);
                //if (tFatherName.Length > 50)
                //    tFatherName = tLastName.Substring(0, 49);
                //Location = objdoc.GetElementsByTagName("LocationID")[0].InnerText;
                //objnode = objdoc.GetElementsByTagName("Document")[0];
                //tDocument = opValidatequotes(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("Gender")[0];
                //if (objnode.InnerText != "")
                //    iGender = Convert.ToInt16(opValidatequotes(objnode.InnerText));

                //objnode = objdoc.GetElementsByTagName("CurrentCTC")[0];
                //tCurrentCTC = opValidatequotes(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("PresentEmployer")[0];
                //tPresentEmployer = opValidatequotes(objnode.InnerText);
                //if (tPresentEmployer.Length > 100)
                //    tPresentEmployer = tPresentEmployer.Substring(0, 99);

                //objnode = objdoc.GetElementsByTagName("Notes")[0];
                //tNotes = opValidatequotes(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("RecieptDate")[0];
                //tRecieptDate = opValidatequotes(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("Pincode")[0];
                //tPincode = opValidatequotes(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("state")[0];
                //if (objnode.InnerText != "")
                //    istateid = Convert.ToInt64(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("Prefereedlocationtext")[0];
                //if (Convert.ToString(objnode.InnerText).Trim() != "")
                //    tpreferedlocationtext = objnode.InnerText;

                //objnode = objdoc.GetElementsByTagName("Preferredlocation")[0];
                //if (Convert.ToString(objnode.InnerText).Trim() != "")
                //    tpreferedlocation = objnode.InnerText;

                objnode = objdoc.GetElementsByTagName("FileName")[0];
                if (Convert.ToString(objnode.InnerText).Trim() != "")
                    tFileName = objnode.InnerText;

                //objnode = objdoc.GetElementsByTagName("FileSize")[0];
                //if (Convert.ToString(objnode.InnerText).Trim() != "")
                //    tFileSize = objnode.InnerText;


                //objnode = objdoc.GetElementsByTagName("IsApplyedAadharNo")[0];
                //tIsApplyedAadharNo = opValidatequotes(objnode.InnerText);

                //objnode = objdoc.GetElementsByTagName("AadharNo")[0];
                //tAadharNo = opValidatequotes(objnode.InnerText);

                objnode = objdoc.GetElementsByTagName("ResumeDocument")[0];
                tCVData = opValidatequotes(objnode.InnerText);



                #endregion
            }
            catch (Exception ex)
            {

                objRes = new apiResponse();
                objRes.Data = "";
                objRes.ResponseCode = 101;
                objRes.ResponseStatus = "Error2 : " + ex.Message;
                return objRes;
                //Debug.Assert(false, ex.Message, ex.StackTrace);
                //return "-2 :" + ex.Message;
            }


            bool flagTrue = false;

            DateTime todaydate = System.DateTime.Today.ToUniversalTime();


            DataSet objDataSet = new DataSet();
            SqlDataAdapter objDataAdapter = null;
            SqlCommandBuilder objCB = null;
            SqlConnection myConn = new SqlConnection(Common.Connection.ConStr);
            try
            {
                myConn.Open();
                #region weightage and dupCriteria

                objDataAdapter = new SqlDataAdapter("select RID,Title,CutOffWeightage as FINALWEIGHTAGE,CoolingPeriod as COOLINGPERIOD,'' as Criteria " +
                    " from HCM_DUPLICATE_CHECK_TEMPLATE with(nolock)", myConn);
                objCB = new SqlCommandBuilder(objDataAdapter);
                objDataAdapter.Fill(objDataSet, "HC_DUPLICATECHECK_TEMPLATE");
                objDataAdapter.Dispose();
                objCB.Dispose();

                if (objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows.Count > 0)
                {
                    tCriteria = objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["Criteria"].ToString();
                    iFinalWeightage = Convert.ToDecimal(objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["FINALWEIGHTAGE"].ToString());
                    iCOOLINGPERIOD = Convert.ToDecimal(objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["COOLINGPERIOD"].ToString());
                }
                else
                {
                    tCriteria = "";
                    iFinalWeightage = 0;
                    iCOOLINGPERIOD = 0;
                }
                iFinalWeightage = 12;//as per v3

                #endregion
                #region Stage Title
                using (SqlCommand objcommand = new SqlCommand("select Title from hcm_stage s with(nolock) where StageLevel = 1", myConn))
                {
                    tStageTitle = Convert.ToString(objcommand.ExecuteScalar());
                }
                #endregion
                #region Status Title
                string tStatusID = "0";
                objDataAdapter = new SqlDataAdapter("select RID,Title as Status from HCM_STATUS with(nolock) " +
                    " where RID in (select StatusID from hcm_applicant_workflow_template_status_detail with(nolock) " +
                    " where TmplDetailId = (select RID from hcm_applicant_workflow_template_detail with(nolock) " +
                    " where TemplateId= (select WFTemplateID from hc_requisitions with(nolock) where RID = " + iReqID + ") and StageLevel = 1) and StatusLevel = 1)", myConn);
                objDataAdapter.Fill(objDataSet, "HCM_STATUS");
                if (objDataSet.Tables["HCM_STATUS"].Rows.Count > 0)
                {
                    tStatusID = Convert.ToString(objDataSet.Tables["HCM_STATUS"].Rows[0]["RID"]);
                    tStatusTitle = Convert.ToString(objDataSet.Tables["HCM_STATUS"].Rows[0]["Status"]);
                }

                #endregion
                #region Password
                Random random = new Random();
                string tPassword = string.Empty;
                if (tDateofBirth != "")
                    tPassword = Convert.ToDateTime(tDateofBirth).ToString("dd/MM/yyyy");
                else tPassword = random.Next(10000, 99999).ToString();
                using (SqlCommand objcmd = new SqlCommand("select dbo.ufn_UserPasswordEncryptionAndDecryption('" + tPassword + "',1)", myConn))
                {
                    tPassword = Convert.ToString(objcmd.ExecuteScalar());
                }
                #endregion
                DataRow dr = null;
                flagTrue = false;
                #region Duplicate Check in HC_USER_MAIN
                if (bCheckDuplicateStatus == true)
                {
                    string tDuplicateCheckQuery = "SELECT top 1 '0' as  SourceID,RID,ISNULL(ModifiedDate,'') as ModifiedDate , isnull((select top 1 CandidateNo from hc_resume_bank b with(nolock) where b.UserID = HC_USER_MAIN.RID),'') as UniqueNo, ";
                    if (tFirstName.Trim().Length > 0)
                        tDuplicateCheckQuery += "(CASE ISNULL(FirstName,'') WHEN '" + tFirstName.Trim() + "' THEN 4 ELSE 0 END) + (CASE ISNULL(LastName,'') WHEN '" + tFirstName.Trim() + "' THEN 4 ELSE 0 END) ";
                    if (tLastName.Trim().Length > 0)
                        tDuplicateCheckQuery += "+ (CASE ISNULL(FirstName,'') WHEN '" + tLastName.Trim() + "' THEN 4 ELSE 0 END) + (CASE ISNULL(LastName,'') WHEN '" + tLastName.Trim() + "' THEN 4 ELSE 0 END)";
                    if (tEMailID.Trim().Length > 0)
                        tDuplicateCheckQuery += "+ (CASE When (ISNULL(EmailID,'') like '" + tEMailID.Trim() + "%' OR ISNULL(EmailID,'') like ';" + tEMailID.Trim() + "%' OR ISNULL(EmailID,'') like '," + tEMailID.Trim() + "%' OR ISNULL(EmailID,'') like '%[,;]" + tEMailID.Trim() +
                            "' OR ISNULL(EmailID,'') like '%[,;]" + tEMailID.Trim() + "[,;]%') THEN 4 ELSE 0 END)  ";

                    if (tDateofBirth.Trim().Length > 0)
                        tDuplicateCheckQuery += "+ (CASE cast(ISNULL(DOB,'') as date) WHEN '" + tDateofBirth + "' THEN 4 ELSE 0 END)";
                    if (tDuplicateCheckQuery.Trim().StartsWith("+") == true)
                        tDuplicateCheckQuery = tDuplicateCheckQuery.Substring(1, tDuplicateCheckQuery.Length - 1);

                    tDuplicateCheckQuery += "  AS Weightage FROM HC_USER_MAIN With(Nolock) where ISNULL(FirstName,'') = '" + tFirstName.Trim() + "' or ISNULL(LastName,'') ='" + tFirstName.Trim() +
                        "' or ISNULL(FirstName,'') = '" + tLastName.Trim() + "' or ISNULL(LastName,'') ='" + tLastName.Trim() + "'  or  cast(ISNULL(DOB,'') as date) = '" + tDateofBirth + "' or  ISNULL(EmailID,'') = '" + tEMailID.Trim() + "' ORDER BY Weightage DESC";

                    objDataAdapter = new SqlDataAdapter(tDuplicateCheckQuery, myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.Fill(objDataSet, "HC_USER_MAIN");
                    objDataAdapter.Dispose();
                    objCB.Dispose();

                    decimal tTotalWeightage = 0;
                    if (objDataSet.Tables["HC_USER_MAIN"].Rows.Count > 0)
                    {
                        iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                        iDupResumeSourceID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["SourceID"]);//OriginalSourceID
                        dtDocMod = Convert.ToDateTime(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["ModifiedDate"]);//Last Modified
                        tTotalWeightage = Convert.ToDecimal(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["Weightage"].ToString());
                        CandidateID = Convert.ToString(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["UniqueNo"].ToString());
                    }
                    if (tTotalWeightage < iFinalWeightage || tTotalWeightage == 0)
                    {
                        flagTrue = true;//New Resume 
                        iDupRID = 0;
                    }
                    else if (tTotalWeightage > 0)
                    {
                        flagTrue = false;

                        objRes = new apiResponse();
                        objRes.Data = CandidateID;
                        objRes.ResponseCode = 101;
                        objRes.ResponseStatus = "Application is already present in the database with applicant ID :" + CandidateID;
                        return objRes;
                    }
                }
                else
                    flagTrue = true;
                #endregion
                #region Checking Cooling period if flagTrue is false
                if (!flagTrue)
                {
                    if (DateTime.Compare(DateTime.Now, dtDocMod) > 0)
                    {
                        if (iCOOLINGPERIOD != 0)
                        {
                            DateTime dtOldDocModifiedDate = Convert.ToDateTime(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["ModifiedDate"]);
                            TimeSpan diffDays = DateTime.Now - dtOldDocModifiedDate;
                            int TotalDays = diffDays.Days;
                            if (TotalDays > iCOOLINGPERIOD)
                            {
                                flagTrue = true;//overwrite
                                ResumeID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                                temp = true;
                            }
                            else
                            {   //put it into Duplicate manager
                                flagTrue = false;
                                iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                            }
                        }
                        else
                        {   //overwrite
                            ResumeID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                            iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"].ToString());
                            flagTrue = true;
                        }
                    }
                    else
                    {   //put it into Duplicate manager
                        flagTrue = false;
                        iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                    }
                }
                #endregion
                #region Remove HC_USER_MAIN From Dataset
                if (objDataSet.Tables.Contains("HC_USER_MAIN"))
                    objDataSet.Tables.Remove("HC_USER_MAIN");
                #endregion
                objTrans = myConn.BeginTransaction();
                if (flagTrue)
                {
                    #region HC_USER_MAIN
                    objDataAdapter = new SqlDataAdapter("select top 1 RID,FirstName,LastName,MiddleName,EmailID,MobileNo," +
                        " UserName,[Password],ReportingToID,Status,Notes,CreatedUserID,ModifiedUserID,CreatedDate,ModifiedDate," +
                        " TID,IsFirstTimeLogin,PhoneNo,DOB,GenderID,ConfigTemplateID,DesignationID,IsAdmin,IsPrimary,IsDataSync from HC_USER_MAIN where RID = " + ResumeID, myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    //objCB.GetInsertCommand().Transaction = objTrans;
                    //objCB.GetUpdateCommand().Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "HC_USER_MAIN");
                    if (objDataSet.Tables["HC_USER_MAIN"].Rows.Count == 0)
                        dr = objDataSet.Tables["HC_USER_MAIN"].NewRow();
                    else
                        dr = objDataSet.Tables["HC_USER_MAIN"].Rows[0];
                    if (dr == null)
                    {
                        dr["FirstName"] = tFirstName;
                        dr["LastName"] = tLastName;
                        dr["MiddleName"] = tFatherName;
                        dr["EmailID"] = tEMailID;
                        dr["MobileNo"] = tMobile;
                        dr["UserName"] = ResumeNumber;
                        dr["Password"] = tPassword;
                        dr["UserName"] = tEMailID;
                        dr["ReportingToID"] = 0;
                        dr["Status"] = 1;
                        dr["Notes"] = tNotes;
                        dr["CreatedUserID"] = iCreatedUserID;
                        dr["ModifiedUserID"] = iCreatedUserID;
                        dr["CreatedDate"] = todaydate.ToString("yyyy-MM-dd hh:mm tt");
                        dr["IsFirstTimeLogin"] = 1;
                        #region Not Nullable value so inserting after discussion with Mr.Ganesh
                        dr["TID"] = 0;
                        dr["ConfigTemplateID"] = 0;
                        dr["DesignationID"] = 0;
                        dr["IsAdmin"] = 0;
                        dr["IsPrimary"] = 0;
                        dr["IsDataSync"] = 1;
                        #endregion nullable ends here
                        dr["PhoneNo"] = tPhone;
                        if (tDateofBirth == "")
                            dr["DOB"] = System.DBNull.Value;
                        else
                            dr["DOB"] = tDateofBirth;
                        dr["GenderID"] = iGender;
                        dr["ModifiedDate"] = todaydate.ToString("yyyy-MM-dd hh:mm tt");
                    }
                    else
                    {

                        dr["FirstName"] = tFirstName;
                        dr["LastName"] = tLastName;
                        dr["MiddleName"] = tFatherName;
                        dr["EmailID"] = tEMailID;
                        dr["MobileNo"] = tMobile;
                        dr["UserName"] = ResumeNumber;
                        dr["Password"] = tPassword;
                        dr["UserName"] = tEMailID;
                        dr["ReportingToID"] = 0;
                        dr["Status"] = 1;
                        dr["Notes"] = tNotes;
                        dr["CreatedUserID"] = iCreatedUserID;
                        dr["ModifiedUserID"] = iCreatedUserID;
                        //  dr["CreatedDate"] = todaydate.ToString("yyyy-MM-dd hh:mm tt");
                        dr["ModifiedDate"] = todaydate.ToString("yyyy-MM-dd hh:mm tt");

                        dr["IsFirstTimeLogin"] = 1;
                        #region Not Nullable value so inserting after discussion with Mr.Ganesh
                        dr["TID"] = 0;
                        dr["ConfigTemplateID"] = 0;
                        dr["DesignationID"] = 0;
                        dr["IsAdmin"] = 0;
                        dr["IsPrimary"] = 0;
                        dr["IsDataSync"] = 1;
                        #endregion nullable ends here
                        dr["PhoneNo"] = tPhone;
                        if (tDateofBirth == "")
                            dr["DOB"] = System.DBNull.Value;
                        else
                            dr["DOB"] = tDateofBirth;
                        dr["GenderID"] = iGender;
                    }
                    if (objDataSet.Tables["HC_USER_MAIN"].Rows.Count == 0)
                        objDataSet.Tables["HC_USER_MAIN"].Rows.Add(dr);

                    objDataAdapter.Update(objDataSet, "HC_USER_MAIN");
                    objDataAdapter.Dispose();
                    objCB.Dispose();
                    #endregion

                    //#region GETMAX RID FROM HC_USER_MAIN
                    //using (SqlCommand objcmd = new SqlCommand("select  Max(RID) from hc_user_main with(nolock) " + " where DOB='" + tDateofBirth + "' and Emailid = '" + tEMailID + "' and FirstName like '" + tFirstName, myConn, objTrans))
                    #region GETMAX RID FROM HC_USER_MAIN
                    using (SqlCommand objcmd = new SqlCommand("select Max(RID) from hc_user_main with(nolock) " +
                        " where FirstName like '" + tFirstName + "'and DOB='" + tDateofBirth + "' and Emailid = '" + tEMailID + "'", myConn, objTrans))
                    {
                        ResumeID = Convert.ToInt64(objcmd.ExecuteScalar());
                    }
                    #endregion
                    #region NEW ROW IN HC_USER
                    objDataAdapter = new SqlDataAdapter("select RID,UserID,RoleID,[Status],TID from hc_user where UserID=" + ResumeID, myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    //objCB.GetInsertCommand().Transaction = objTrans;
                    //objCB.GetUpdateCommand().Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "HC_USER");
                    if (objDataSet.Tables["HC_USER"].Rows.Count == 0)
                        dr = objDataSet.Tables["HC_USER"].NewRow();
                    else
                        dr = objDataSet.Tables["HC_USER"].Rows[0];
                    dr["UserID"] = ResumeID;
                    dr["RoleID"] = 4;//for candidate
                    dr["Status"] = 1;
                    dr["TID"] = 0;
                    if (objDataSet.Tables["HC_USER"].Rows.Count == 0)
                        objDataSet.Tables["HC_USER"].Rows.Add(dr);
                    objDataAdapter.Update(objDataSet, "HC_USER");
                    objDataAdapter.Dispose();
                    objCB.Dispose();
                    #endregion
                    #region NEW ROW IN HC_RESUME_BANK
                    objDataAdapter = new SqlDataAdapter("Select RID,UserID,CandidateNo,AppAadharNo,SourceID,CreatedUserID,ModifiedDate,SourceEmailID," +
                        " PresentCTC,TotalExp,RelevantExp,ResStatus,ExtCVReceipt  " +
                        " from HC_RESUME_BANK where UserID=" + ResumeID, myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    objCB.GetUpdateCommand().Transaction = objTrans;
                    objCB.GetInsertCommand().Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "HC_RESUME_BANK");
                    #region HC_RESUME_BANK NEW RESUME
                    if (objDataSet.Tables["HC_RESUME_BANK"].Rows.Count == 0)
                    {
                        dr = objDataSet.Tables["HC_RESUME_BANK"].NewRow();
                        dr["CreatedUserID"] = iCreatedUserID;
                    }
                    else
                        dr = objDataSet.Tables["HC_RESUME_BANK"].Rows[0];
                    dr["UserID"] = ResumeID;
                    dr["CandidateNo"] = CandidateID;
                    //if (tIsApplyedAadharNo != "")
                    //    dr["AppAadharNo"] = tIsApplyedAadharNo.ToLower() == "yes" ? true : false;
                    //if (tAadharNo != "")
                    //    dr["ExtAadharNo"] = tAadharNo;
                    dr["TotalExp"] = iTotalExp;
                    dr["RelevantExp"] = iRelExp;
                    //     dr["PresentCTC"] = tCurrentCTC;
                    dr["SourceID"] = iSourceID;
                    dr["SourceEmailID"] = tResumeSourceRef;
                    //     dr["ExtPresentEmployer"] = tPresentEmployer;
                    dr["ModifiedDate"] = todaydate.ToString("yyyy-MM-dd hh:mm tt");
                    if (iReqID != 0)
                        dr["ResStatus"] = 1; //Lokesh and Ganesh Discussed and suggested
                    else
                        dr["ResStatus"] = 0; //Lokesh and Ganesh Discussed and suggested

                    if (!string.IsNullOrEmpty(tRecieptDate))
                        dr["ExtCVReceipt"] = tRecieptDate;
                    else
                        dr["ExtCVReceipt"] = DBNull.Value;
                    //dr["ExtPrefLocation"] = tpreferedlocation;

                    if (objDataSet.Tables["HC_RESUME_BANK"].Rows.Count == 0)
                        objDataSet.Tables["HC_RESUME_BANK"].Rows.Add(dr);

                    objDataAdapter.Update(objDataSet, "HC_RESUME_BANK");
                    objDataAdapter.Dispose();
                    objCB.Dispose();
                    if (objDataSet.Tables.Contains("HC_RESUME_BANK"))
                    {
                        objDataSet.Tables["HC_RESUME_BANK"].Rows.Clear();
                        objDataSet.Tables["HC_RESUME_BANK"].Columns.Clear();
                        objDataSet.Tables["HC_RESUME_BANK"].Dispose();
                    }
                    #endregion
                    #endregion
                    if (iResID == 0)
                    {
                        #region GET MAX RID HC_RESUME_BANK
                        objDataAdapter = new SqlDataAdapter("select MAX(RID) as RID from HC_RESUME_BANK WITH (NOLOCK) " +
                        " Where UserID=" + ResumeID + " and ModifiedDate= '" + todaydate.ToString("yyyy-MM-dd hh:mm tt") + "'", myConn);
                        objDataAdapter.SelectCommand.Transaction = objTrans;
                        objCB = new SqlCommandBuilder(objDataAdapter);
                        objDataAdapter.Fill(objDataSet, "HC_RESUME_BANK");
                        objDataAdapter.Dispose();
                        objCB.Dispose();
                        iResID = Convert.ToInt64(objDataSet.Tables["HC_RESUME_BANK"].Rows[0]["RID"]);
                        #endregion
                    }
                    //Inserting every new resume
                    #region HC_RESUME_BANK_HISTORY
                    objDataAdapter = new SqlDataAdapter("select top 0 RID,ResID,UserID,SourceID,SaveType,UpdatedDate from HC_RESUME_BANK_HISTORY", myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "RESUME_BANK_HISTORY");
                    if (objDataSet.Tables["RESUME_BANK_HISTORY"].Rows.Count == 0)
                    {
                        dr = objDataSet.Tables["RESUME_BANK_HISTORY"].NewRow();
                        dr["ResID"] = iResID;
                        dr["UserID"] = iCreatedUserID;
                        dr["UpdatedDate"] = System.DateTime.Now.ToUniversalTime();
                        dr["SourceID"] = iSourceID;
                        dr["SaveType"] = temp ? 4 : 1;//based on 1-New,2-Direct, 3-Career,4-Overwrite
                        objDataSet.Tables["RESUME_BANK_HISTORY"].Rows.Add(dr);

                        objDataAdapter.Update(objDataSet, "RESUME_BANK_HISTORY");
                        objDataAdapter.Dispose();
                        if (objDataSet.Tables.Contains("RESUME_BANK_HISTORY"))
                        {
                            objDataSet.Tables["RESUME_BANK_HISTORY"].Rows.Clear();
                            objDataSet.Tables["RESUME_BANK_HISTORY"].Columns.Clear();
                            objDataSet.Tables["RESUME_BANK_HISTORY"].Dispose();
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Save HC_RESUME_BANK_DUPLICATE
                    using (SqlCommand objcmd = new SqlCommand("select RID from hc_resume_bank with(nolock) where UserID = " + iDupRID, myConn, objTrans))
                    {
                        string dupResumeID = Convert.ToString(objcmd.ExecuteScalar());
                        iDupRID = dupResumeID == "" ? 0 : Convert.ToInt64(dupResumeID);
                    }
                    if (iDupRID > 0)
                    {
                        objDataAdapter = new SqlDataAdapter("Select TOP 0 RID,DupResID,FirstName,LastName,EmailID,PhoneNo, " +
                                                                " MobileNo,DOB,SourceID,CreatedOn,CreatedBy " +
                                                                " from HC_RESUME_BANK_DUPLICATE ", myConn);
                        objCB = new SqlCommandBuilder(objDataAdapter);
                        objDataAdapter.SelectCommand.Transaction = objTrans;
                        objDataAdapter.Fill(objDataSet, "DUPLICATE");
                        if (objDataSet.Tables["DUPLICATE"].Rows.Count == 0)
                        {
                            dr = objDataSet.Tables["DUPLICATE"].NewRow();
                            dr["DupResID"] = iDupRID;
                            dr["FirstName"] = tFirstName;
                            dr["LastName"] = tLastName;
                            dr["EmailID"] = tEMailID;
                            dr["PhoneNo"] = tPhone;
                            dr["MobileNo"] = tMobile;
                            if (tDateofBirth == "")
                                dr["DOB"] = System.DBNull.Value;
                            else
                                dr["DOB"] = Convert.ToDateTime(tDateofBirth).ToUniversalTime();
                            dr["SourceID"] = iSourceID;
                            dr["CreatedOn"] = todaydate.ToString("yyyy-MM-dd hh:mm tt");
                            dr["CreatedBy"] = iCreatedUserID;
                            objDataSet.Tables["DUPLICATE"].Rows.Add(dr);
                            objDataAdapter.Update(objDataSet, "DUPLICATE");
                            objDataAdapter.Dispose();
                            objCB.Dispose();
                            if (objDataSet.Tables.Contains("DUPLICATE"))
                            {
                                objDataSet.Tables["DUPLICATE"].Rows.Clear();
                                objDataSet.Tables["DUPLICATE"].Columns.Clear();
                                objDataSet.Tables["DUPLICATE"].Dispose();
                            }
                        }
                        if (objTrans != null)
                            objTrans.Commit();
                        objTrans = null;
                        //  return "2:" + iDupUniqueNo;
                        objRes = new apiResponse();
                        objRes.Data = CandidateID;
                        objRes.ResponseCode = 101;
                        objRes.ResponseStatus = "Application is already present in the database with applicant ID :" + CandidateID;
                        return objRes;
                    }
                    #endregion
                }
                #region HC_RESUME_MAPPING I,E PREFERED LOCATION
                if (!string.IsNullOrEmpty(tpreferedlocation))
                {
                    using (SqlCommand objcmd = new SqlCommand("delete from hc_resume_mapping where Type = 4 and ResID = " + iResID, myConn, objTrans))
                    {
                        objcmd.ExecuteNonQuery();
                    }


                    objDataAdapter = new SqlDataAdapter("select top 0 RID,ResID,Type,MappingID from hc_resume_mapping", myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "HC_RESUME_MAPPING");
                    string[] PrefArray = tpreferedlocation.Split(',');
                    for (int i = 0; i < PrefArray.Length; i++)
                    {
                        dr = objDataSet.Tables["HC_RESUME_MAPPING"].NewRow();
                        dr["ResID"] = iResID;
                        dr["Type"] = 4;//for preferred location
                        dr["MappingID"] = PrefArray[i];

                        objDataSet.Tables["HC_RESUME_MAPPING"].Rows.Add(dr);

                    }
                    objDataAdapter.Update(objDataSet, "HC_RESUME_MAPPING");
                    objDataAdapter.Dispose();
                    objCB.Dispose();
                }
                #endregion
                #region HC_ADDRESS ACCORDING TO V5
                #region GET COUNTRY ID BASED ON STATE ID
                string CoutryID = "0";
                using (SqlCommand objcmd = new SqlCommand("select CountryID  from HCM_STATES where RID = " + istateid + "", myConn, objTrans))
                {
                    CoutryID = Convert.ToString(objcmd.ExecuteScalar()) == "" ? "0" : Convert.ToString(objcmd.ExecuteScalar());
                }
                #endregion
                objDataAdapter = new SqlDataAdapter("select RID,UserID,AddressTypeID,City,StateID," +
                    " CountryID,Pincode from HC_ADDRESS " +
                    " where AddressTypeID = 1 and UserID =" + ResumeID, myConn);
                objCB = new SqlCommandBuilder(objDataAdapter);
                objDataAdapter.SelectCommand.Transaction = objTrans;
                objCB.GetInsertCommand().Transaction = objTrans;
                objCB.GetUpdateCommand().Transaction = objTrans;
                objDataAdapter.Fill(objDataSet, "HC_ADDRESS");
                if (objDataSet.Tables["HC_ADDRESS"].Rows.Count == 0)
                    dr = objDataSet.Tables["HC_ADDRESS"].NewRow();
                else
                    dr = objDataSet.Tables["HC_ADDRESS"].Rows[0];
                dr["UserID"] = ResumeID;
                dr["AddressTypeID"] = "1";
                //       dr["City"] = Location;
                dr["StateID"] = istateid;
                dr["CountryID"] = CoutryID;
                dr["Pincode"] = tPincode;

                if (objDataSet.Tables["HC_ADDRESS"].Rows.Count == 0)
                    objDataSet.Tables["HC_ADDRESS"].Rows.Add(dr);

                objDataAdapter.Update(objDataSet, "HC_ADDRESS");
                objDataAdapter.Dispose();
                objCB.Dispose();
                #endregion
                #region "EDUCATION"
                //Insertion into the Education
                objDataAdapter = new SqlDataAdapter("Select Top 0 RID, ResID, EduID,InstituteName,Year,Percentage from HC_RESUME_EDUCATION", myConn);
                objCB = new SqlCommandBuilder(objDataAdapter);
                objDataAdapter.SelectCommand.Transaction = objTrans;
                objCB.GetInsertCommand().Transaction = objTrans;
                objCB.GetUpdateCommand().Transaction = objTrans;
                objCB.GetDeleteCommand().Transaction = objTrans;
                objDataAdapter.Fill(objDataSet, "HC_RESUME_EDUCATION");
                dr = null;

                xnlEducationTypes = objdoc.GetElementsByTagName("Education");
                if (xnlEducationTypes.Count > 0)
                {
                    for (int iCount = 0; iCount <= xnlEducationTypes.Item(0).ChildNodes.Count - 1; iCount++)
                    {
                        if (xnlEducationTypes.Item(0).ChildNodes.Count > 0)
                        {
                            try
                            {
                                dr = objDataSet.Tables["HC_RESUME_EDUCATION"].NewRow();
                                dr["ResID"] = iResID;
                                dr["EduID"] = xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[0].InnerText;
                                dr["InstituteName"] = ((xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[1].InnerText.Trim() == "") ? DBNull.Value.ToString() : xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[1].InnerText.Trim());
                                dr["Year"] = (xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[2].InnerText == "") ? 0 : Convert.ToInt32(xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[2].InnerText);
                                dr["Percentage"] = ((xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[3].InnerText.Trim() == "") ? DBNull.Value.ToString() : xnlEducationTypes.Item(0).ChildNodes[iCount].ChildNodes[3].InnerText.Trim());
                                objDataSet.Tables["HC_RESUME_EDUCATION"].Rows.Add(dr);
                                objDataAdapter.Update(objDataSet, "HC_RESUME_EDUCATION");
                            }
                            catch { }
                        }
                    }
                }
                if (objDataAdapter != null)
                    objDataAdapter.Dispose();
                if (objCB != null)
                    objCB.Dispose();
                #endregion
                #region Insertion into Work Experience

                string strQuery = "Select Top 0 RID,ResID,Employer,DesignationID,DesignationText,StartDate,EndDate from HC_RESUME_EMPLOYER";
                objDataAdapter = new SqlDataAdapter(strQuery, myConn);
                objCB = new SqlCommandBuilder(objDataAdapter);
                objDataAdapter.SelectCommand.Transaction = objTrans;
                objCB.GetInsertCommand().Transaction = objTrans;
                objCB.GetUpdateCommand().Transaction = objTrans;
                objCB.GetDeleteCommand().Transaction = objTrans;
                objDataAdapter.Fill(objDataSet, "HC_RESUME_EMPLOYER");
                dr = null;

                xnlWorkExperience = objdoc.GetElementsByTagName("Experience");
                if (xnlWorkExperience.Count > 0)
                {
                    for (int iCount = 0; iCount <= xnlWorkExperience.Item(0).ChildNodes.Count - 1; iCount++)
                    {
                        if (xnlWorkExperience.Item(0).ChildNodes.Count > 0)
                        {
                            dr = objDataSet.Tables["HC_RESUME_EMPLOYER"].NewRow();
                            dr["ResID"] = iResID;
                            dr["Employer"] = xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[0].InnerText;
                            dr["DesignationID"] = (xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[1].InnerText == "") ? 0 : Convert.ToInt64(xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[1].InnerText);
                            dr["DesignationText"] = xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[4].InnerText.Trim();

                            //if (iCount == 0)
                            //{
                            //    dr["Particular"] = "0";
                            //}
                            //else
                            //{
                            //    dr["Particular"] = "1";
                            //}
                            if (xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[2].InnerText != "")
                            {
                                DateTime FromDate = Convert.ToDateTime(xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[2].InnerText);
                                dr["StartDate"] = FromDate.Date;
                                //dr["FromMonth"] = (Int16)FromDate.Month;
                                //dr["FromYear"] = FromDate.Year;
                            }
                            if (xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[3].InnerText.Trim() != "")
                            {
                                DateTime ToDate = Convert.ToDateTime(xnlWorkExperience.Item(0).ChildNodes[iCount].ChildNodes[3].InnerText.Trim());
                                dr["EndDate"] = ToDate.Date;
                                //dr["ToMonth"] = (Int16)ToDate.Month;
                                //dr["ToYear"] = ToDate.Year;
                            }
                            objDataSet.Tables["HC_RESUME_EMPLOYER"].Rows.Add(dr);
                            objDataAdapter.Update(objDataSet, "HC_RESUME_EMPLOYER");
                        }
                    }
                }
                if (objDataAdapter != null)
                    objDataAdapter.Dispose();
                if (objCB != null)
                    objCB.Dispose();
                #endregion
                #region HC_REQ_RESUME
                if (iReqID != 0)
                {
                    #region GET WFTemplateID
                    string WFTemplateID = "0";
                    using (SqlCommand objcommand = new SqlCommand("SELECT WFTemplateID FROM hc_requisitions q with(nolock) WHERE RID = " + iReqID, myConn, objTrans))
                    {
                        WFTemplateID = Convert.ToString(objcommand.ExecuteScalar());
                    }
                    #endregion
                    DataTable objDTEMP = new DataTable("HC_REQ_RESUME");
                    #region INSERT INTO HC_REQ_RESUME
                    objDataAdapter = new SqlDataAdapter("SELECT RID,ReqID,ResID,StatusLevel,StageLevel,StageTitle,StatusTitle, " +
                        "StageID,StatusID,SourceID,CreatedUserID,ModifiedUserID,CreatedDate,ModifiedDate,TicketNo,AddedType,SourceEmailID,WFTemplateID " +
                        "FROM HC_REQ_RESUME WHERE ResID = " + iResID + " AND ReqID = " + iReqID, myConn);

                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    objCB.GetInsertCommand().Transaction = objTrans;
                    objCB.GetUpdateCommand().Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "HC_REQ_RESUME");
                    if (objDataSet.Tables["HC_REQ_RESUME"].Rows.Count == 0)
                    {
                        dr = objDataSet.Tables["HC_REQ_RESUME"].NewRow();
                        bFlgAdded = true;

                        dr["ReqID"] = iReqID;
                        dr["ResID"] = iResID;
                        dr["StatusLevel"] = "1";
                        dr["StageLevel"] = "1";
                        dr["StageID"] = "1";
                        dr["StatusID"] = tStatusID == "" ? "0" : tStatusID;//Get hcm_status
                        dr["StageTitle"] = tStageTitle;
                        dr["StatusTitle"] = tStatusTitle;
                        dr["SourceID"] = iSourceID;
                        dr["CreatedUserID"] = iCreatedUserID;
                        dr["ModifiedUserID"] = iCreatedUserID;
                        dr["CreatedDate"] = DateTime.Now.ToUniversalTime();
                        dr["ModifiedDate"] = DateTime.Now.ToUniversalTime();
                        //dr["Probablity"] = 2;
                        dr["TicketNo"] = ResumeTicketNumber;
                        //dr["ISAuto"] = 1;
                        dr["AddedType"] = "10";
                        dr["WFTemplateID"] = WFTemplateID == "" ? "0" : WFTemplateID;
                        dr["SourceEmailID"] = tResumeSourceRef;
                    }
                    else
                    {
                        bFlgAdded = false;
                        dr = objDataSet.Tables["HC_REQ_RESUME"].Rows[0];
                        tReqResumeID = dr["RID"].ToString();
                        ResumeTicketNumber = dr["TicketNo"].ToString();
                        dr["ModifiedUserID"] = iCreatedUserID;
                        dr["ModifiedDate"] = DateTime.Now.ToUniversalTime();
                    }

                    if (objDataSet.Tables["HC_REQ_RESUME"].Rows.Count == 0)
                        objDataSet.Tables["HC_REQ_RESUME"].Rows.Add(dr);
                    objDataAdapter.Update(objDataSet, "HC_REQ_RESUME");

                    if (objDataAdapter != null)
                        objDataAdapter.Dispose();
                    if (objCB != null)
                        objCB.Dispose();
                    #endregion
                    if (tReqResumeID == "")
                    {
                        #region Get Max RID FROM HC_REQ_RESUME
                        objDataAdapter = new SqlDataAdapter("SELECT RID,TicketNo FROM HC_REQ_RESUME Where ReqID = " + iReqID + " And ResID =" + iResID + " order by RID desc", myConn);
                        objCB = new SqlCommandBuilder(objDataAdapter);
                        objDataAdapter.SelectCommand.Transaction = objTrans;
                        objDataAdapter.Fill(objDataSet, "HC_REQ_RESUME_MAX");
                        tReqResumeID = objDataSet.Tables["HC_REQ_RESUME_MAX"].Rows[0]["RID"].ToString();
                        ResumeTicketNumber = objDataSet.Tables["HC_REQ_RESUME_MAX"].Rows[0]["TicketNo"].ToString();

                        if (objDataAdapter != null)
                            objDataAdapter.Dispose();
                        if (objCB != null)
                            objCB.Dispose();
                        #endregion
                        #region HC_REQ_RESUME_STAGE_STATUS
                        objDataAdapter = new SqlDataAdapter("select top 0 RID,ReqResID,StageLevel,StageTitle,StatusLevel,StatusTitle,StatusDate,UpdatedUserID," +
                            " UpdatedDate,InvRoundID,StageID,StatusID,ReasonGroupID from HC_REQ_RESUME_STAGE_STATUS", myConn);
                        objCB = new SqlCommandBuilder(objDataAdapter);
                        objDataAdapter.SelectCommand.Transaction = objTrans;
                        objDataAdapter.Fill(objDataSet, "HC_REQ_RESUME_STATUS");

                        dr = objDataSet.Tables["HC_REQ_RESUME_STATUS"].NewRow();
                        dr["ReqResID"] = tReqResumeID;
                        dr["StageLevel"] = "1";
                        dr["StageID"] = "1";
                        dr["StatusID"] = tStatusID == "" ? "0" : tStatusID;
                        dr["StatusLevel"] = "1";
                        dr["StageTitle"] = tStageTitle;
                        dr["StatusTitle"] = tStatusTitle;
                        dr["StatusDate"] = DateTime.Now.ToUniversalTime();
                        dr["UpdatedDate"] = DateTime.Now.ToUniversalTime();
                        dr["UpdatedUserID"] = iCreatedUserID;
                        dr["InvRoundID"] = 0;
                        dr["ReasonGroupID"] = 0;

                        objDataSet.Tables["HC_REQ_RESUME_STATUS"].Rows.Add(dr);
                        objDataAdapter.Update(objDataSet, "HC_REQ_RESUME_STATUS");

                        if (objDataAdapter != null)
                            objDataAdapter.Dispose();
                        if (objCB != null)
                            objCB.Dispose();
                        #endregion
                    }
                    #region CHANGING TO PLANNED TO INPROCESS
                    // changing to planned to inprocess
                    objDataAdapter = new SqlDataAdapter("SELECT RID,ReqStatus, ModifiedUserID FROM HC_REQUISITIONS where RID=" + iReqID, myConn);
                    objCB = new SqlCommandBuilder(objDataAdapter);
                    objDataAdapter.SelectCommand.Transaction = objTrans;
                    objDataAdapter.Fill(objDataSet, "HC_REQUISITIONS");
                    if (objDataSet.Tables["HC_REQUISITIONS"].Rows.Count > 0)
                    {
                        if (objDataSet.Tables["HC_REQUISITIONS"].Rows[0]["ReqStatus"].ToString() == "0")
                        {
                            objDataSet.Tables["HC_REQUISITIONS"].Rows[0]["ReqStatus"] = "1";
                            objDataSet.Tables["HC_REQUISITIONS"].Rows[0]["ModifiedUserID"] = iCreatedUserID;
                            objDataAdapter.Update(objDataSet, "HC_REQUISITIONS");
                        }
                    }
                    if (objDataAdapter != null)
                        objDataAdapter.Dispose();
                    if (objCB != null)
                        objCB.Dispose();
                    #endregion
                }
                #endregion
                #region "HC_RESUME_DOCUMENTS"
                strQuery = "delete from HC_RESUME_DOCUMENTS where ResID = " + iResID.ToString();
                using (SqlCommand cmd = new SqlCommand(strQuery, myConn, objTrans))
                    cmd.ExecuteNonQuery();

                byte[] iDataFile = System.Convert.FromBase64String(objCND.ResumeDocument.ToString());
                string resumeContent = "", resumeText = "", resumeHTML = "";
                GetResumeData(iDataFile, tFileName, ref resumeContent, ref resumeText, ref resumeHTML);

                SqlCommand CMD = new SqlCommand("usp_CreateResumeDocument", myConn, objTrans);
                CMD.CommandType = CommandType.StoredProcedure;
                CMD.Parameters.AddWithValue("@RID", 0);
                CMD.Parameters.AddWithValue("@TID", 0);
                CMD.Parameters.AddWithValue("@UserID", 0);
                CMD.Parameters.AddWithValue("@ResID", iResID);
                CMD.Parameters.AddWithValue("@FileData", objCND.ResumeDocument);//
                                                                                //CMD.Parameters.AddWithValue("@FileData", Convert.ToBase64String(objCND.ResumeDocument));//
                CMD.Parameters.AddWithValue("@FileType", tFileName == "" ? "" : Path.GetExtension(tFileName));
                CMD.Parameters.AddWithValue("@FileName", tFileName);
                CMD.Parameters.AddWithValue("@Description", "");
                CMD.Parameters.AddWithValue("@ResHTMLText", resumeHTML);
                CMD.Parameters.AddWithValue("@ResConvertedText", resumeText);
                CMD.Parameters.AddWithValue("@ReqResID", 0);
                CMD.Parameters.Add("@OPcode", OleDbType.BigInt);
                CMD.Parameters.AddWithValue("@FileSize", objCND.ResumeDocument.Length);

                CMD.Parameters["@OPcode"].Direction = ParameterDirection.Output;
                CMD.ExecuteNonQuery();
                CMD.Dispose();

                #region Removed
                /*
                     objDataAdapter = new SqlDataAdapter("select RID,ResID,Description,TID,FileData,FileType,FileName,FileSize,convertdocmigration,ReqResID,CreatedOn,ResHTMLText,ResConvertedText from HC_RESUME_DOCUMENTS where RESID =" + iResID, myConn);
                     SqlCommandBuilder cmb1 = new SqlCommandBuilder(objDataAdapter);
                     objDataAdapter.SelectCommand.Transaction = objTrans;
                     objDataAdapter.Fill(objDataSet, "HC_RESUME_DOCUMENTS");

                     dr = objDataSet.Tables["HC_RESUME_DOCUMENTS"].NewRow();
                     dr["ResID"] = iResID;
                     dr["Description"] = "";
                     dr["TID"] = "0";
                     dr["ReqResID"] = "0";
                     if (fileContent != null)
                         dr["FileData"] = resumeContent;
                     else
                         dr["FileData"] = DBNull.Value;
                     dr["FileType"] = tFileName == "" ? "" : Path.GetExtension(tFileName);
                     dr["FileName"] = tFileName;
                     //dr["FileSize"] = tFileSize == "" ? "0" : tFileSize;
                     dr["FileSize"] = fileContent.Length;
                     dr["convertdocmigration"] = "0";
                     dr["ResHTMLText"] = resumeHTML;
                     dr["ResConvertedText"] = resumeText;
                     dr["CreatedOn"] = DateTime.Now.ToUniversalTime();

                     objDataSet.Tables["HC_RESUME_DOCUMENTS"].Rows.Add(dr);
                     objDataAdapter.Update(objDataSet, "HC_RESUME_DOCUMENTS");*/
                #endregion

                #endregion

                #region GenerateReqTicketNo
                if (!string.IsNullOrEmpty(tReqResumeID) && tReqResumeID != "0")
                {
                    using (SqlCommand objcmd = new SqlCommand("usp_PrepareJobCode", myConn, objTrans))
                    {
                        objcmd.Parameters.AddWithValue("@ReqID", 0);
                        objcmd.Parameters.AddWithValue("@LanguageType", 0);
                        objcmd.Parameters.Add("@FinalJC", SqlDbType.VarChar, 100);
                        objcmd.Parameters["@FinalJC"].Direction = ParameterDirection.Output;
                        objcmd.Parameters.AddWithValue("@Type", 2);
                        objcmd.CommandType = CommandType.StoredProcedure;
                        if (myConn.State == ConnectionState.Closed)
                            myConn.Open();
                        objcmd.ExecuteNonQuery();

                        ResumeTicketNumber = Convert.ToString(objcmd.Parameters["@FinalJC"].Value);
                    }
                    using (SqlCommand objcmd = new SqlCommand("update HC_Req_Resume set TicketNo='" + ResumeTicketNumber + "' where Rid=" + tReqResumeID, myConn, objTrans))
                    {
                        if (myConn.State == ConnectionState.Closed)
                            myConn.Open();
                        objcmd.ExecuteNonQuery();
                    }
                }
                #endregion

                if (!temp)
                {
                    #region GenerateCandidateNo
                    string tCandidateNo = "0";
                    if (!string.IsNullOrEmpty(Convert.ToString(iResID)) && iResID != 0)
                    {
                        using (SqlCommand objcmd = new SqlCommand("usp_PrepareJobCode", myConn, objTrans))
                        {
                            objcmd.Parameters.AddWithValue("@ReqID", 0);
                            objcmd.Parameters.AddWithValue("@LanguageType", 0);
                            objcmd.Parameters.Add("@FinalJC", SqlDbType.VarChar, 100);
                            objcmd.Parameters["@FinalJC"].Direction = ParameterDirection.Output;
                            objcmd.CommandType = CommandType.StoredProcedure;
                            objcmd.Parameters.AddWithValue("@Type", 1);
                            if (myConn.State == ConnectionState.Closed)
                                myConn.Open();
                            objcmd.ExecuteNonQuery();

                            tCandidateNo = Convert.ToString(objcmd.Parameters["@FinalJC"].Value);
                            CandidateID = tCandidateNo;
                        }
                        using (SqlCommand objcmd = new SqlCommand("update HC_Resume_bank set CandidateNo=" + tCandidateNo + " where Rid=" + iResID, myConn, objTrans))
                        {
                            if (myConn.State == ConnectionState.Closed)
                                myConn.Open();
                            objcmd.ExecuteNonQuery();
                        }
                    }
                    #endregion
                }

                if (objTrans != null)
                    objTrans.Commit();
                objTrans = null;
                objDataAdapter = null;
                objDataSet.Dispose();

                if (temp)
                {

                    if (bFlgAdded)
                    {
                        //return "4:" + ResumeTicketNumber + ":" + tReqResumeID;//overwrite and Added
                        objRes = new apiResponse();
                        objRes.Data = CandidateID;
                        objRes.ResponseCode = 101;
                        objRes.ResponseStatus = CandidateID + "  overwrite and Added";
                        return objRes;
                    }

                    else
                    {
                        //return "3:" + ResumeTicketNumber + ":" + tReqResumeID;//overwrite

                        objRes = new apiResponse();
                        objRes.Data = CandidateID;
                        objRes.ResponseCode = 100;
                        objRes.ResponseStatus = CandidateID + "  overwrite";
                        return objRes;
                    }
                }
                else
                {// return "1:" + ResumeTicketNumber + ":" + tReqResumeID; 
                    objRes = new apiResponse();
                    objRes.Data = CandidateID;
                    objRes.ResponseCode = 100;
                    objRes.ResponseStatus = "Congratulations! - Resume is Submitted Successfully.ApplicantID :" + "" + CandidateID;
                    return objRes;
                }





            }
            catch (Exception ex)
            {
                //Debug.Assert(false, ex.Message, ex.StackTrace);
                // return "-4 :" + ex.Message + ex.StackTrace;
                Common.Logs(ex.Message);
                objRes = new apiResponse();
                objRes.Data = CandidateID;
                objRes.ResponseCode = 101;
                objRes.ResponseStatus = "Error Occured : " + ex.Message;
                return objRes;
            }
            finally
            {
                if (objTrans != null)
                    objTrans.Rollback();
                objTrans = null;
                if (myConn != null && myConn.State == ConnectionState.Open)
                    myConn.Close();
                if (objDataSet != null)
                    objDataSet.Dispose();
                objDataSet = null;
                if (objDataAdapter != null)
                    objDataAdapter.Dispose();
                objDataAdapter = null;
                if (objCB != null)
                    objCB.Dispose();
                objCB = null;

            }

            #endregion


            //ResumeResponse obj = null;
            //obj.ResponseNumber = 100;
            //obj.Status = "Sucess";
            //obj.CandidateID = "ENI-09";
            //return obj;
        }

        void GetResumeData(byte[] bFileData, string sFileName, ref string resumeContent, ref string resumeText, ref string resumeHTML)
        {
            try
            {
                #region getting resume data
                byte[] temp =FileCompress.opDataZip(Convert.ToBase64String(bFileData));
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

        public string CreateCandidateXML(Object objCandidate)
        {

            XmlDocument xmlDoc = new XmlDocument();   //Represents an XML document, 
                                                      // Initializes a new instance of the XmlDocument class.          
            XmlSerializer xmlSerializer = new XmlSerializer(objCandidate.GetType());
            // Creates a stream whose backing store is memory. 
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, objCandidate);
                xmlStream.Position = 0;
                //Loads the XML document from the specified string.
                xmlDoc.Load(xmlStream);
                return xmlDoc.InnerXml;
            }
        }

        public string opValidatequotes(string sParaValue)
        {
            sParaValue = sParaValue.Replace("'", "").Replace(";", "");
            return sParaValue;
        }

        public apiResponse beDuplicateCheck(CandidateData data)
        {
            DateTime dtDocMod = DateTime.Today;
            Int64 iDupRID = 0, iDupCheck = 0, iDupResumeSourceID = 0;
            string tCriteria = "";
            bool bCheckDuplicateStatus = true;
            decimal iFinalWeightage = 0, iCOOLINGPERIOD = 0;
           // string iDupUniqueNo = "";
            string CandidateID = "";
            string iExitedEmployee = "";
            System.Xml.XmlDocument objdoc;
            objdoc = new System.Xml.XmlDocument();
            bool flagTrue = false;
            string tFirstName = "", tLastName = "", tEMailID = "", tMobile = "", tDateofBirth = ""; //, tPassport = ""
            apiResponse objRes = null;

            try
            {


                String str = "";
                str = CreateCandidateXML(data);
                //str = serializer.Serialize(;
                objdoc.LoadXml(str);
                System.Xml.XmlNode objnode;

                objnode = objdoc.GetElementsByTagName("Firstname")[0];
                tFirstName = opValidatequotes(objnode.InnerText);
                if (tFirstName.Length > 25)
                    tFirstName = tFirstName.Substring(0, 24);

                objnode = objdoc.GetElementsByTagName("LastName")[0];
                tLastName = opValidatequotes(objnode.InnerText);
                if (tLastName.Length > 25)
                    tLastName = tLastName.Substring(0, 24);


                if (objdoc.GetElementsByTagName("DOB")[0].InnerText.IndexOf("0001") == -1)
                    tDateofBirth = objdoc.GetElementsByTagName("DOB")[0].InnerText;
                else
                    tDateofBirth = "";
                if (tDateofBirth != "" && tDateofBirth.StartsWith("-") == false)
                    tDateofBirth = Convert.ToDateTime(tDateofBirth).ToString("dd-MMM-yyyy hh:mm:ss tt");
                else
                    tDateofBirth = "";


                objnode = objdoc.GetElementsByTagName("EmailID")[0];
                tEMailID = opValidatequotes(objnode.InnerText);
                if (tEMailID.Length > 100)
                    tEMailID = tEMailID.Substring(0, 99);


                //objnode = objdoc.GetElementsByTagName("PassportNo")[0];
                //tPassport = opValidatequotes(objnode.InnerText);
                //if (tPassport.Length > 10)
                //    tPassport = tPassport.Substring(0, 9);

                objnode = objdoc.GetElementsByTagName("MobileNo")[0];
                tMobile = opValidatequotes(objnode.InnerText);
            }
            catch (Exception ex)
            {
                Common.Logs(ex.Message);
                objRes.Data = "";
                objRes.ResponseCode = 101;
                objRes.ResponseStatus = "Error1: " + ex.Message;
                return objRes;
                //return "-2 :" + ex.Message;
            }

            DataSet objDataSet = new DataSet();
            SqlDataAdapter myDataAdapter = null;
            SqlCommandBuilder cmb = null;
            SqlConnection myConn = new SqlConnection(Common.Connection.ConStr);
            myConn.Open();
            try
            {
                #region Weightage  weightage and dupCriteria

                myDataAdapter = new SqlDataAdapter("Select RID,Title,CutOffweightage as FINALWEIGHTAGE,CoolingPeriod as COOLINGPERIOD,'' as Criteria from HCM_DUPLICATE_CHECK_TEMPLATE WHERE rid= " + iDupCheck + "  ", myConn);
                cmb = new SqlCommandBuilder(myDataAdapter);
                myDataAdapter.Fill(objDataSet, "HC_DUPLICATECHECK_TEMPLATE");
                myDataAdapter.Dispose();
                if (objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows.Count > 0)
                {
                    tCriteria = objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["Criteria"].ToString();
                    iFinalWeightage = Convert.ToDecimal(objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["FINALWEIGHTAGE"].ToString());
                    iCOOLINGPERIOD = Convert.ToDecimal(objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["COOLINGPERIOD"].ToString());
                }
                else
                {
                    objDataSet.Tables.Remove("HC_DUPLICATECHECK_TEMPLATE");
                    //myDataAdapter = new SqlDataAdapter("Select  * from HC_DUPLICATECHECK_TEMPLATE WHERE rid in (select DuplicateCheckTempID from HC_SYSCONFIG )", myConn);
                    myDataAdapter = new SqlDataAdapter("Select top 1 RID,Title,CutOffweightage as FINALWEIGHTAGE,CoolingPeriod as COOLINGPERIOD,'' as Criteria from HCM_DUPLICATE_CHECK_TEMPLATE", myConn);

                    cmb = new SqlCommandBuilder(myDataAdapter);
                    myDataAdapter.Fill(objDataSet, "HC_DUPLICATECHECK_TEMPLATE");
                    myDataAdapter.Dispose();
                    if (objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows.Count > 0)
                    {
                        tCriteria = objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["Criteria"].ToString();
                        iFinalWeightage = Convert.ToDecimal(objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["FINALWEIGHTAGE"].ToString());
                        iCOOLINGPERIOD = Convert.ToDecimal(objDataSet.Tables["HC_DUPLICATECHECK_TEMPLATE"].Rows[0]["COOLINGPERIOD"].ToString());
                    }
                    else
                    {
                        tCriteria = "";
                        iFinalWeightage = 0;
                        iCOOLINGPERIOD = 0;
                    }
                }
                iFinalWeightage = 12;
                #endregion


                #region Check Duplicate Resume in Hc_Resume_Bank

                if (bCheckDuplicateStatus == true)
                {

                    #region Duplicate Check in HC_RESUME_BANK_DUPLICATE_CHECK

                    string tDuplicateCheckQuery = " SELECT top 1 '0' as ResumeSourceID,RID,ModifiedDate as DocModifiedDate," +
                        " isnull((select top 1 CandidateNo from hc_resume_bank b with(nolock) where b.UserID = m.RID),'') as UniqueNo, " +
                                                  " /*Isnull(ExitedEmployee,'') */ '' as ExitedEmployee, ";
                    //if (bCheckDuplicatebyEmailID == false)
                    //{

                    if (tFirstName.Trim().Length > 0)
                        tDuplicateCheckQuery += " (CASE  WHEN (FirstName LIKE '" + tFirstName.Trim() + "%' OR FirstName Like '" + tFirstName.Trim() + "%') THEN 4 ELSE 0 END) ";

                    if (tLastName.Trim().Length > 0)
                        tDuplicateCheckQuery += " + (CASE  WHEN (LastName LIKE '" + tLastName.Trim() + "%' OR LastName Like '" + tLastName.Trim() + "%') THEN 4 ELSE 0 END) ";

                    if (tEMailID.Trim().Length > 0)
                        tDuplicateCheckQuery += "+ (CASE When (EmailID like '" + tEMailID.Trim() + "%' OR EmailID like ';" + tEMailID.Trim() + "%' OR EmailID like '," + tEMailID.Trim() + "%' OR EmailID like '%[,;]" + tEMailID.Trim() + "' OR EmailID like '%[,;]" + tEMailID.Trim() + "[,;]%') THEN 4 ELSE 0 END)  ";

                    if (tDateofBirth.Trim().Length > 0)
                        tDuplicateCheckQuery += "+ (CASE DOB WHEN '" + tDateofBirth + "' THEN 2 ELSE 0 END)";
                    if (tDuplicateCheckQuery.Trim().StartsWith("+") == true)
                        tDuplicateCheckQuery = tDuplicateCheckQuery.Substring(1, tDuplicateCheckQuery.Length - 1);

                    tDuplicateCheckQuery += "  AS Weightage FROM HC_USER_MAIN m With (Nolock) ORDER BY Weightage DESC";

                    myDataAdapter = new SqlDataAdapter(tDuplicateCheckQuery, myConn);
                    cmb = new SqlCommandBuilder(myDataAdapter);
                    myDataAdapter.Fill(objDataSet, "HC_USER_MAIN");
                    myDataAdapter.Dispose();
                    cmb.Dispose();

                    decimal tTotalWeightage = 0;
                    if (objDataSet.Tables["HC_USER_MAIN"].Rows.Count > 0)
                    {
                        iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                        iDupResumeSourceID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["ResumeSourceID"]);//OriginalSourceID
                        dtDocMod = Convert.ToDateTime(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["DocModifiedDate"]);//DocModifiedDate
                        tTotalWeightage = Convert.ToDecimal(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["Weightage"].ToString());
                        CandidateID = Convert.ToString(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["UniqueNo"].ToString());
                        // CandidateID=Convert.ToString(objDataSet.Tables[""] )
                        iExitedEmployee = Convert.ToString(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["ExitedEmployee"].ToString());
                    }
                    if (tTotalWeightage < iFinalWeightage || tTotalWeightage == 0)
                    {
                        flagTrue = true;//New Resume 
                        iDupRID = 0;
                    }
                    else if (tTotalWeightage > 0)
                        flagTrue = false;
                    #region COMMENTED CODE
                    // }
                    //else
                    //{
                    //    //flagTrue = true;
                    //    decimal tTotalWeightage1 = 0;
                    //    if (objDataSet.Tables["HC_RESUME_BANK"].Rows.Count > 0)
                    //    {
                    //        tTotalWeightage1 = Convert.ToDecimal(objDataSet.Tables["HC_RESUME_BANK"].Rows[0]["Weightage"].ToString());
                    //        iDupRID = Convert.ToInt64(objDataSet.Tables["HC_RESUME_BANK"].Rows[0]["RID"]);
                    //    }
                    //    if ((tTotalWeightage1 < iFinalWeightage) || (tTotalWeightage1 == 0))
                    //    {
                    //        flagTrue = true;//New Resume 
                    //        iDupRID = 0;
                    //    }
                    //    else if (tTotalWeightage1 > 0)
                    //        flagTrue = false;
                    //}
                    #endregion

                    #endregion
                }
                else
                    flagTrue = true;

                #region Checking Cooling period if flagTrue is false
                if (flagTrue == false)
                {

                    if (DateTime.Compare(DateTime.Now, dtDocMod) > 0)
                    {
                        if (iCOOLINGPERIOD != 0)
                        {
                            DateTime dtOldDocModifiedDate = Convert.ToDateTime(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["DocModifiedDate"]);
                            TimeSpan diffDays = DateTime.Now - dtOldDocModifiedDate;
                            int TotalDays = diffDays.Days;
                            if (TotalDays > iCOOLINGPERIOD)
                            {
                                flagTrue = true;//overwrite
                                                //  ResumeID = Convert.ToInt64(objDataSet.Tables["HC_RESUME_BANK"].Rows[0]["RID"]);
                                                // temp = true;
                            }
                            else
                            {   //put it into Duplicate manager
                                flagTrue = false;
                                iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                            }
                        }
                        else
                        {   //overwrite
                            // ResumeID = Convert.ToInt64(objDataSet.Tables["HC_RESUME_BANK"].Rows[0]["RID"]);
                            iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"].ToString());
                            flagTrue = true;
                        }
                    }
                    else
                    {   //put it into Duplicate manager
                        flagTrue = false;
                        iDupRID = Convert.ToInt64(objDataSet.Tables["HC_USER_MAIN"].Rows[0]["RID"]);
                    }
                }

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                Common.Logs(ex.Message);
                objRes = new apiResponse();
                objRes.Data = "";
                objRes.ResponseCode = 101;
                objRes.ResponseStatus = "Error2 : " + ex.Message;
                return objRes;
            }
            finally
            {

                if (myConn != null && myConn.State == ConnectionState.Open)
                    myConn.Close();
                if (myDataAdapter != null)
                    myDataAdapter.Dispose();
            }

            if (flagTrue)
            {
                //return "1:";
                objRes = new apiResponse();
                objRes.Data = "";
                objRes.ResponseCode = 100;
                objRes.ResponseStatus = "New Resume";
                return objRes;
            }

            else
            { //return "0:" + iDupUniqueNo + ":" + iExitedEmployee;


                objRes = new apiResponse();
                objRes.Data = CandidateID;
                objRes.ResponseCode = 101;
                objRes.ResponseStatus = "Application is already present in the database with applicant ID : " + CandidateID;
                return objRes;
            }

        }
        #endregion

        public void Response(string Msg)
        {

            string LogPath = System.Configuration.ConfigurationManager.AppSettings["LogPath"];

            if (!Directory.Exists(LogPath + @"\Response"))
                Directory.CreateDirectory(LogPath + @"\Response");

            File.AppendAllText(LogPath + @"\Response\Response_" + DateTime.Now.ToString("dd_MMM_yyyy_hh_mm_ss") + ".xml", Msg);

            // File.AppendAllText(LogPath + @"\Response\ResponseXML_" + DateTime.Now.ToString("dd_MMM_yyyy") + ".txt", repeatChar + Environment.NewLine + "Error Date and time: " + DateTime.Now.ToString() + Environment.NewLine + errMsg + Environment.NewLine + repeatChar + Environment.NewLine);

        }

        public string EncryptDataFromDB(string v)
        {
            
            try
            {
                using (SqlConnection conn = new SqlConnection(Common.Connection.ConStr))
                {
                    conn.Open();
                    using (SqlCommand oCmd = conn.CreateCommand())
                    {
                        oCmd.CommandText = "usp_encyptFromDB";
                        oCmd.CommandType = CommandType.StoredProcedure;
                        oCmd.Parameters.AddWithValue("@rid", v);
                        var code = oCmd.Parameters.Add("@opcode", SqlDbType.NVarChar, -1);
                        code.Direction = ParameterDirection.Output;
                        oCmd.ExecuteNonQuery();
                        v = Convert.ToString(code.Value);

                    }
                }
            }
            catch (Exception ex)
            {
                Common.Logs(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            return v;
        }

        public List<RequirementList> opGetRequirementList(string EmpId, string ReqTitle, string ReqNumber)
        {
            List<RequirementList> ApplicantList = new List<RequirementList>();
            string sqlQuery = "", SourceID = "";
            SourceID = ConfigurationManager.AppSettings["SourceID"];
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            apiResponse response = new apiResponse();
            try
            {
                RequirementList objApplicant = null;

                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    if (con != null && con.State == ConnectionState.Closed)
                    {
                        con.Open();
                    }
                    try
                    {

                        if (con != null && con.State == ConnectionState.Closed)
                        {
                            con.Open();
                        }
                        sqlQuery = " select q.RID as RID,q.ReqTitle,q.ReqNumber as JobCode, " +
                                           " isnull((select Name from hc_entity e with(nolock) where rid = (select top 1 OrgID from hc_req_org o with(nolock) " +
                                           " where o.ReqID = q.RID and OrgLevel = 1)),'') as ClientName," +
                                           "isnull((select Name from hc_entity e with(nolock) where rid = (select top 1 OrgID from hc_req_org o with(nolock) " +
                                           " where o.ReqID = q.RID and OrgLevel = 2)),'') as BranchName," +
                                           " /*isnull((select DeptCode from HC_CLIENT_BRANCH with (Nolock) where HC_CLIENT_BRANCH.RID=q.BranchID),'')*/ '' as BranchCode, " +
                                           " (cast(MinExperience as nvarchar(max) ))  +' to '+ (cast(MaxExperience as nvarchar(max))) as Experience," +
                                           " isnull(Keyword,'') as SkillsText, " +
                                           " STUFF((select ',' + Title from HCM_LOCATIONS with(nolock) where RID in (select MappingID FROM HC_REQUISITION_MAPPING with(nolock) " +
                                           " where ReqID = q.RID and Type= 9 ) for xml path('')),1,1,'') as LocationText ";
                        if (SourceID != "0")
                        {
                            sqlQuery = sqlQuery + " from HC_REQUISITIONS q with (Nolock)  where q.RID in (select ReqID from HC_REQ_ALLOCATION a with (nolock) where MemberID in (select Rid from hcm_resume_source with (nolock) where Type in (1)) and  a.Enddate >='" + today + "' " +
                                "AND a.IsActive in (1) and a.MemberID in (" + SourceID + "))  and q.ReqStatus in (0,1,7,8)  and q.RID not in (0) and   q.EndDate >='" + today + "' ";


                        }

                        if (ReqTitle != "" && ReqTitle != null)
                        {
                            sqlQuery = sqlQuery + " and q.reqtitle = '" + ReqTitle + "'";
                        }

                        if (ReqNumber != "" && ReqNumber != null)
                        {
                            sqlQuery = sqlQuery + " and q.ReqNumber = '" + ReqNumber + "'";
                        }

                        SqlCommand PersonalCmd = new SqlCommand(sqlQuery, con);
                        //PersonalCmd.CommandType = CommandType.StoredProcedure;                            
                        //PersonalCmd.Parameters.AddWithValue("@EmpId", EmpId.ToString());                    
                        PersonalCmd.CommandTimeout = 0;
                        PersonalCmd.ExecuteNonQuery();

                        //Get RequirementDetails
                        SqlDataAdapter sqlDA = new SqlDataAdapter(PersonalCmd);
                        DataTable tmpDt = new DataTable("PersonalInfo");
                        sqlDA.Fill(tmpDt);

                        if (tmpDt.Rows.Count > 0)
                        {
                            foreach (DataRow drow in tmpDt.Rows)
                            {
                                objApplicant = new RequirementList();
                                objApplicant.IndentID = drow["RID"].ToString();
                                objApplicant.JobCode = drow["JobCode"].ToString();
                                objApplicant.JobTitle = drow["ReqTitle"].ToString();
                                objApplicant.Location = drow["LocationText"].ToString();
                                objApplicant.ExperienceInYears = drow["Experience"].ToString();
                                objApplicant.Skills = drow["SkillsText"].ToString();
                                objApplicant.IndentURL = System.Configuration.ConfigurationManager.AppSettings["IndentURL"] + EncryptDataFromDB(drow["RID"].ToString());

                                ApplicantList.Add(objApplicant);
                            }

                        }
                        else
                        {
                            response=Common.NotFoundResponse(res, 0);
                            Common.Logs("Requirement Not Found!!!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.Logs(ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                    con.Close();
                }

            }


            catch (SqlException ex)
            {
                Common.Logs(ex.Message + Environment.NewLine + ex.StackTrace);
                
            }

            catch (Exception ex)
            {
                //ApplicantIntegration AI = new ApplicantIntegration();
                Common.Logs(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            string GetCandidateXMLData = CreateCandidateXML(ApplicantList);
            Response(GetCandidateXMLData);
            return ApplicantList;

        }
    }
}
