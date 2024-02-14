using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class LoginCheck
    {
        public LoginCheck()
        {
        }
        private static readonly Lazy<LoginCheck> lazy = new Lazy<LoginCheck>(() => new LoginCheck());
        public static LoginCheck Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        #region [ Login Check ]

        public LoginResponse beLoginCheck(string MobileNo, string sPassword = "", string CandidateNo = "", string ipAddress = "", string loginType = "")
        {
            LoginResponse Res = new LoginResponse();
            DataSet objDataSet = new DataSet();
            SqlDataAdapter objda = null;

            try
            {
                using (SqlConnection con = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "usp_CareerLoginCheck";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@MobileNo", MobileNo);
                        cmd.Parameters.AddWithValue("@CandidateNo", CandidateNo);
                        cmd.Parameters.AddWithValue("@ipAddress", ipAddress);

                        objda = new SqlDataAdapter(cmd);
                        objda.Fill(objDataSet);

                        if (objDataSet == null)
                        {
                            // Res = null; 
                            Res.ID = -1;
                            Res.Code = "Failed";//HCErrorCodes.Sucess;
                            Res.UserID = 0;
                            Res.ResumeID = 0;
                            Res.Message = "Invalid Credentials";
                        }

                        else if (objDataSet.Tables[0].Rows.Count == 0)
                        {
                            // Res = null; 
                            Res.ID = -1;
                            Res.Code = "Failed";//HCErrorCodes.Sucess;
                            Res.UserID = 0;
                            Res.ResumeID = 0;
                            Res.Message = "Invalid Credentials";
                        }
                        //Res = null;
                        else
                        {
                            DataRow dr = objDataSet.Tables[0].Rows[0];

                            Res.ID = 0;
                            Res.Code = "Success";//HCErrorCodes.Sucess;
                            Res.UserID = Convert.ToInt64(dr["UserID"]);
                            Res.ResumeID = Convert.ToInt64(dr["ResID"]);
                            Res.Message = "Login Successfull";
                            Res.LastLoginDate = Convert.ToDateTime(dr["LastLoginDate"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Res = null;
                Common.Logs("beLoginCheck() MobileNo: " + MobileNo.ToString() + " CandidateNo: " + CandidateNo.ToString() + "  " + ex.ToString());
            }

            return Res;
        }

        #endregion
    }
}
