using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class clsIPAL
    {
    }

    public class FileDetails
    {
        public string FileData { get; set; }
        public Int64 FileType { get; set; }
        public string FileName { get; set; }
        public string MobileNo { get; set; }
        public Int64 DocId { get; set; }
        public string candidateno { get; set; }
        public Int64 FileSize { get; set; }
    }

    public class ResumeResponse
    {
        public string Status { get; set; }
        public string CandidateID { get; set; }
        public Int32 ResponseNumber { get; set; }
    }

    public class CandidateData
    {
        public string RequestType { get; set; }
        public string IndentID { get; set; }
        public string Firstname { get; set; }
        //      public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string DOB { get; set; }
        public string ResumeDocument { get; set; }
        public string FileName { get; set; }
        public string EmpID { get; set; }
        public string EmpMailID { get; set; }
        public Int64 PresentIndustryID { get; set; }
        public Int64 RelevantIndustryID { get; set; }
        public string OtherIndustry { get; set; }
        public bool IsFresher { get; set; }

        //public string Experience { get; set; }
        public Int64 ExrGraduationPercentage { get; set; }
        public Int32 IsSelf { get; set; }
        public string EmployeeID { get; set; }
    }

    public class RequirementList
    {
        public string IndentID { get; set; }
        public string JobCode { get; set; }
        public string JobTitle { get; set; }
        public string MainGroup { get; set; }
        public string SubGroup { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string ExperienceInYears { get; set; }
        public string Skills { get; set; }
        public string IndentURL { get; set; }
    }
}

