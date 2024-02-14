using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class DocumentClass
    {
        private HCCommon hcCommon = null;
        public DocumentClass(HCCommon objCommon = null)
        {
            if (objCommon == null)
                hcCommon = new HCCommon();
            else
                hcCommon = objCommon;
        }

        private DocumentClass()
        {
        }
        private static readonly Lazy<CareerClass> Lazy = new Lazy<CareerClass>(() => new CareerClass());
        public static CareerClass Instance
        {
            get { return Lazy.Value; }
        }
        apiResponse res = null;


        #region [ Upload Docuemnt]

        public apiResponse beSaveCandidateDocument(ParamUpdateDoc objParamUpdateDoc)
        {
            #region Member
            apiResponse Response = new apiResponse();
            byte[] FileData = null;
            int Filelen = 0;
            Int32 tFileType = 0;
            #endregion
            try
            {
                #region Get FileType/FileData/Length
                string[] sFileExt = objParamUpdateDoc.FileName.Split('.');

                tFileType = Common.GetFileType(sFileExt[1]);

                FileData = FileCompress.opDataZip(Common.bStr(objParamUpdateDoc.FileData));
                Filelen = FileData.Length;
                #endregion

                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = objCon.CreateCommand())
                    {
                        cmd.CommandText = "usp_UploadCandidateDocument_Career";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@RID", 0);
                        cmd.Parameters.AddWithValue("@ResID", hcCommon.ResID);
                        cmd.Parameters.AddWithValue("@CandidateNo", objParamUpdateDoc.CandidateNo);
                        cmd.Parameters.AddWithValue("@FileName", objParamUpdateDoc.FileName);
                        cmd.Parameters.AddWithValue("@FileData", FileData);
                        cmd.Parameters.AddWithValue("@FileSize", Filelen);
                        cmd.Parameters.AddWithValue("@FileType", tFileType);
                        cmd.Parameters.AddWithValue("@UserID", objParamUpdateDoc.UserID == 0 ? hcCommon.UID : objParamUpdateDoc.UserID);
                        cmd.Parameters.AddWithValue("@DocumentID", objParamUpdateDoc.DocumentID);
                        cmd.Parameters.AddWithValue("@Notes", objParamUpdateDoc.Notes);
                        cmd.Parameters.AddWithValue("@IsProfilePic", objParamUpdateDoc.IsProfilePic);

                        var opCode = cmd.Parameters.Add("@OpCode", SqlDbType.BigInt);
                        opCode.Direction = ParameterDirection.Output;

                        var opUniuqDocID = cmd.Parameters.Add("@UniqueDocID", SqlDbType.BigInt);
                        opUniuqDocID.Direction = ParameterDirection.Output;

                        objCon.Open();
                        cmd.ExecuteNonQuery();
                        objCon.Close();
                        Int64 code = Convert.ToInt64(opCode.Value);
                        Int64 UniuqDocID = Convert.ToInt64(opUniuqDocID.Value);
                        if (code > 0)
                        {
                            Response.NoOfRecord = 1;
                            Response.ResponseCode = 100;
                            Response.ResponseMessage = "Candidate Document Saved successfully";
                            Response.Data = Convert.ToString(UniuqDocID);
                        }
                        else
                        {
                            Response.NoOfRecord = 1;
                            Response.ResponseCode = 101;
                            Response.ResponseMessage = "No Records Found.";
                            Response.Data = "";
                        }
                    }
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

        #region [ Offfer Letter Document ]
        public apiResponse beGetOfferLetterDocument(CommonReqObj obj)
        {
            apiResponse Response = new apiResponse();
            DataTable dt = new DataTable();
            GetOfferLetterDoc GetOfferLetterDoc = new GetOfferLetterDoc();
            try
            {
                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = objCon.CreateCommand())
                    {
                        cmd.CommandText = "usp_GetOfferLetterDocument_Career";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ReqResID", obj.ReqResID);

                        SqlDataAdapter objda = new SqlDataAdapter(cmd);
                        objda.Fill(dt);

                        if (dt == null)
                            Response = (Common.NotFoundResponse(res));
                        else if (dt.Rows.Count == 0)
                            Response = (Common.NotFoundResponse(res));
                        else
                        {
                            GetOfferLetterDoc.CandidateStatus = Common.bStr(dt.Rows[0]["CandidateStatus"]);
                            GetOfferLetterDoc.DateTime = Common.bStr(dt.Rows[0]["MDateTime"]);
                            GetOfferLetterDoc.FileName = Common.bStr(dt.Rows[0]["docName"]);
                            GetOfferLetterDoc.FileData = Common.bStr(FileCompress.opDataUnzip((byte[])dt.Rows[0]["DocData"]));
                            Response = Common.SuccessResponse(res, GetOfferLetterDoc, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ErrorRes(res, ex);
            }
            finally
            {

            }
            return Response;
        }
        #endregion
    }
}
