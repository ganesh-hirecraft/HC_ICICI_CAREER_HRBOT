using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class LoginCheckModel
    {
    }

    public sealed class LoginResponse
    {
        public Int64 ID { get; set; }
        public Int64 UserID { get; set; }
        public Int64 ResumeID { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    public sealed class HCClaimTypes
    {
        public const string UID = "UID";
        public const string UserName = "UserName";
        public const string ResumeID = "ResumeID";
        public const string IP = "IP";
    }

    public class HCCommon
    {
        public long TID { get; set; }

        public long UID { get; set; }
        public long ResID { get; set; }

        public string IPAddress { get; set; }

        public string Timezone { get; set; }

        public HCCommon()
        {

            IPAddress = "";

        }
    }
}
