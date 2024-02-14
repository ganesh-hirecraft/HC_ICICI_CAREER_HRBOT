using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class DocumentModel
    {
    }

    #region[ Upload Document  ]
    public class ParamUpdateDoc
    {
        public string CandidateNo { get; set; }
        public Int64 UserID { get; set; }
        public Int64 DocumentID { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
        public string Notes { get; set; }
        public Int32 IsProfilePic { get; set; }
    }
    #endregion

    #region[Offer Letter Doc]
    public class CommonReqObj
    {
        public string CandidateNo { get; set; }
        public Int64 UserID { get; set; }
        public long JobID { get; set; }
        public long ReqResID { get; set; }
    }
    public class GetFiles
    {
        public string FileName { get; set; }
        public string FileData { get; set; }
    }

    public class GetOfferLetterDoc : GetFiles
    {
        public string DateTime { get; set; }
        public string CandidateStatus { get; set; }
    }
    #endregion
}
