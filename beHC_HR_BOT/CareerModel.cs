using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class CareerModel
    {
    }

    #region [Login Mobile/Email]
    public class CareerLoginDetails
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class OtpDetails
    {
        public string MobileNo { get; set; }
        public long OTP { get; set; }
    }
    #endregion

    #region[ Resume Saving ]
    public class CandidateData_save
    {
        public string RequestType { get; set; }
        public string IndentID { get; set; }
        public string Firstname { get; set; }
        //public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public string DOB { get; set; }
        public string ResumeDocument { get; set; }
        public string FileName { get; set; }
        public string EmpID { get; set; }
        public string EmpMailID { get; set; }
        public Int32 SourceType { get; set; }
        public string IndentNo { get; set; }

        //public string Experience { get; set; }
    }
    public class CreateResumeDocument
    {
        public Int64 RID { get; set; }
        public Int64 ResID { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
        public string Description { get; set; }
        public string ResHTMLText { get; set; }
        public string ResConvertedText { get; set; }
        public string ResSkillText { get; set; }
        public string PDFFileData { get; set; }
    }

    #region weightage and dupCriteria

    public class weightageanddupCriteria
    {
        public string Criteria { get; set; }
        public decimal FinalWeightage { get; set; }
        public int COOLINGPERIOD { get; set; }
        public string SourceEmailID { get; set; }
        public string SourceCoolingPeriod { get; set; }
    }

    public class duplicateCheck
    {
        public bool IsResDuplicate { get; set; }
        public bool IsResOverWrite { get; set; }
        public bool IsResGradeOverwrite { get; set; }
        public bool IsResGradeDuplicat { get; set; }
        public string CandidateNo { get; set; }
    }

    #endregion

    public class ParamUpdateDoc_Career
    {
        public string CandidateNo { get; set; }
        public Int64 UserID { get; set; }
        public Int64 DocumentID { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
        public string Notes { get; set; }
        public Int64 ResID { get; set; }
    }

    public class GetHtmlText
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string ResponseType { get; set; }
    }

    #endregion

    #region [ Candidate Bsic Info ]

    public class BasicDetails_career
    {
        public long ResumeID { get; set; }
        public string CandidateNo { get; set; }
        public Int64 Gender { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string DOB { get; set; }
        public string PAN { get; set; }
        public string Aadhaar { get; set; }
        public string TotalExp { get; set; }
        public string Source { get; set; }
        public string Photo { get; set; }
        public Int64 isPhotoVerified { get; set; }
        public Int32 ApplicanteKYC { get; set; }
        public Int32 RMeKYC { get; set; }
        public bool IsGraduation { get; set; }
        public Int64 GraduationPercentage { get; set; }
        public Int64 GraduationPassYear { get; set; }

    }

    #endregion

    #region [ Input Parameters ]

    public class inputParams
    {
        public string CandidateNo { get; set; }
    }

    #endregion

    #region [ Candidate Quick Status ]
    public class QuickCanStatus
    {
        public Int64 ReqResID { get; set; }
        public Int64 JobID { get; set; }
        public string StageTitle { get; set; }
        public string StatusTitle { get; set; }
        public bool IsOPQApplicable { get; set; }
        public string OPQStatus { get; set; }
        public string OPQLink { get; set; }
        public bool IsSPTApplicable { get; set; }
        public string SPTStatus { get; set; }
        public bool IsDeclarationSubmitted { get; set; }
        public bool IsPANUplooded { get; set; }
        public bool IsPhotoUploaded { get; set; }
        public bool IsAadharUploaded { get; set; }
        public bool IsLicenseUploaded { get; set; }
        public bool IsPassportUploaded { get; set; }
        public bool IsVoterIdUploaded { get; set; }
        public bool Is10thMarksUploaded { get; set; }
        public bool Is12thMaksUploaded { get; set; }
        public bool IsGradUploaded { get; set; }
        public bool IsPostGradUploaded { get; set; }
        public bool IsCAUploaded { get; set; }
        public bool IsOtherCareerUploaded { get; set; }
        public bool IsPaySlipUploaded { get; set; }
        public bool IsAddressProofUploaded { get; set; }
        public bool IsRelievingletterUploaded { get; set; }
        public bool IsResignationDocUploaded { get; set; }
        public bool IsLatestCvUploaded { get; set; }
        public bool IsResume { get; set; }
        public Int64 GradeID { get; set; }
        public string Grade { get; set; }
        public string JoingingConfirmDate { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public Int64 JoinigDateCount { get; set; }
        public string OfferAcceptanceDate { get; set; }
        public string InductionMeetingURL { get; set; }
        public string InductionMeetingPassword { get; set; }
        public Int64 iseKYCCompleted { get; set; }
        public Int32 isBookSlot { get; set; }
        public string IndentNo { get; set; }
        public Int64 ResumeID { get; set; }
        public bool isPOjob { get; set; }
        public Int32 SelfLoanFund { get; set; }
        public string PlannedJoiningDate { get; set; }
        public bool IsReScheduled { get; set; }
        public bool isEnableDeclaration { get; set; }
        public bool isQuestionaireeSubmitted { get; set; }
        public bool isJoiningInfo { get; set; }
        public string OfferType { get; set; }
        public DateTime TestPassedDate { get; set; }
        public Int32 OfferSentCount { get; set; }
        public bool IsProfilerApplicable { get; set; }
        public Int32 IsOfferAccepted { get; set; }
    }
    public class quickStatus
    {
        public string CandidateNo { get; set; }
        public bool isOffer { get; set; }
    }

    #endregion

    #region[ Candidate Offer Accept or Reject ]
    public class OfferAcceptReject
    {
        public Int64 ReqResID { get; set; }
        public Int32 ActionType { get; set; }
        public Int32 Type { get; set; }
        public DateTime ConfirmedJoiningDate { get; set; }
        public string Notes { get; set; }
        public Int64 ReasonID { get; set; }
        public Int64 UserID { get; set; }
        public string InductionMeetingURL { get; set; }
        public string InductionMeetingPassword { get; set; }
    }

    #endregion

    public class DateRange
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }

    #region[ MasterAPI ]
    public class PanelSearchByDate : DateRange
    {
        public Int64 UserID { get; set; }

    }
    #endregion
}
