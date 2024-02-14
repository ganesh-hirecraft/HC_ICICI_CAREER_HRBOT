using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace beHC_HR_BOT
{
    public class MasterClass
    {
        private HCCommon hcCommon = null;
        public MasterClass(HCCommon objCommon = null)
        {
            if (objCommon == null)
                hcCommon = new HCCommon();
            else
                hcCommon = objCommon;
        }

        private apiResponse res = null;

        #region Master
        public apiResponse GetMaster(string type, Int64 refID = 0)
        {
            string sQuery = "";
            switch (type.ToLower())
            {
                //hemant added industry for master


                #region  Master for  type


                case "educationtype":
                    if (refID == 0)
                        sQuery = " select RID as Value as Value,Title from hcm_education_group where rid in (1,2,7,13,14) ";
                    else
                        sQuery = "";
                    break;
                #endregion


                #region  masterpart and full time type


                case "fullparttime":
                    if (refID == 0)
                        sQuery = " select RID as Value , Title from HCM_EDUCATION with (nolock) where EduGroupID in (select RID  from hcm_education_group with (nolock) where rid=16)  ";
                    else
                        sQuery = "";
                    break;
                #endregion

                #region RTH status


                case "rthstatus":
                    if (refID != 0)
                        sQuery = "select RID as Value,Title from HCM_STATUS where statuslevel =" + refID + "' ) and status=1 order by Title ";
                    else
                        sQuery = "";
                    break;
                #endregion

                #region Industry
                case "industry":
                    sQuery = "select RID as Value,Title from hcm_industry with(nolock) where Status = 1 order by Title ";
                    break;
                #endregion
                #region revised reason

                case "revisedreason":
                    sQuery = "select r.RID as Value as Value,u.Title from hc_reason u inner join HC_REVISED_REASON_MAPPING r on u.rid = r.reasonid order by u.title asc";
                    break;
                #endregion
                #region
                case "intrejectreason":
                    //sQuery = "select RID as Value,Title from HCM_REASON_GROUP with(nolock)  where status=1   ";
                    sQuery = " select RID as Value, Title from HC_REASON where rid in (select ReasonId from hc_reason_stage_status where StatusID = " + refID + ")";

                    break;
                #endregion


                #region Country
                case "country":
                    sQuery = "select RID as Value,Title from hcm_country with(nolock) where Status = 1 order by Title ";
                    break;
                #endregion

                #region State
                case "state":
                    if (refID != 0)
                        sQuery = " select RID as Value ,Title from hcm_states s with(nolock)  where s.CountryID = " + refID + " and Status = 1 order by Title ";
                    else
                        sQuery = " select RID as Value ,Title FROM HCM_STATES WITH(NOLOCK) where Status=1  ORDER BY Title ";
                    break;
                #endregion

                #region City
                case "city":
                    if (refID != 0)
                        sQuery = "select RID as Value,Title from HCM_LOCATIONS with(Nolock) where StateID ='" + refID + "' and Status = 1 and IsIntegrated = 1 and ReqLocationStatus = 0 order by Title";
                    else
                        sQuery = " select RID as Value,Title from hcm_locations l with(nolock) where  Status = 1 and IsIntegrated = 1 and ReqLocationStatus = 0 order by Title ";
                    break;
                #endregion

                #region University
                case "university":
                    sQuery = " select RID as Value,isnull(Title,'') as 'Title' from HCM_UNIVERSITY_GROUP with (nolock) where Status = 1 order by Title ";
                    break;
                #endregion

                #region College
                case "college":
                    sQuery = " select RID as Value,isnull(Title,'')as Title FROM HCM_UNIVERSITY_TYPE WITH(NOLOCK)  where Status=1 ORDER BY Title ";
                    break;
                #endregion

                #region Degree
                case "degree":
                    //if (refID != 0)
                    //    sQuery = "select RID as Value,isnull(Title, '') as Title FROM HCM_EDUCATION WITH(NOLOCK) where Status = 1 and EduGroupID = " + refID + " ORDER BY Title ";
                    //else
                    //sQuery = " select RID as Value,isnull(Title,'')as Title,isnull(EduGroupID,0) as TypeIndex FROM HCM_EDUCATION WITH(NOLOCK) where Status = 1 and EduGroupID = " + refID + " ORDER BY Title ";
                    sQuery = "select RID as Value,isnull(Title, '') as Title FROM HCM_EDUCATION WITH(NOLOCK) where Status = 1  ORDER BY Title ";
                    break;
                #endregion

                #region EducationGroup
                case "educationgroup":
                    //sQuery = " select RID as Value,isnull(Title,'')as Title from hcm_education_group with(nolock) ORDER BY Title ";
                    sQuery = "select RID as Value,isnull(a.Title,'')as Title from hcm_education_group a with(nolock) where engkeyid in (1,8,7,11,13,14)  order by a.title";
                    break;
                #endregion

                #region Specialization
                case "specialization":
                    sQuery = " select RID as Value,isnull(Title,'') as Title FROM HCM_EDUCATION_SPECIALIZATION_TYPES WITH(NOLOCK) WHERE Status in (1)  order by Title ";
                    break;
                #endregion

                #region Designation
                case "designation":
                    sQuery = "select RID as Value,Title from hcm_designations with(nolock)  where Status = 1 order by Title";
                    break;
                #endregion

                #region EmploymentType
                case "employmenttype":
                    sQuery = " select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 49 and Status in (1) order by Title ";
                    break;
                #endregion

                #region Currency
                case "currency":
                    sQuery = " select ConstValue as Value,Title as SalaryAmount  from hcm_common_master with(nolock) where REfType = 53 and Status in (1) order by Title ";
                    break;
                #endregion

                #region NameTitle
                case "nametitle":
                    sQuery = "select RID as Value,Title from hcm_salutation with(nolock) where status = 1";
                    break;
                #endregion

                #region Gender
                case "gender":
                    sQuery = " select RID as Value,Title  from HCM_gender With(NOLOCK) where status=1  ";
                    break;
                #endregion

                #region Skill
                case "skill":
                    sQuery = " select RID as Value,Title  from hcm_skills With(NOLOCK) where status=1  ";
                    break;
                #endregion
                #region Rating
                case "rating":
                    sQuery = " select RID as Value,Title  from [HCM_SKILL_RATING] With(NOLOCK) where status=1  ";
                    break;
                #endregion

                #region Relation
                case "relation":
                    sQuery = " SELECT ConstValue as Value,isnull(Title,'')as Title FROM HCM_COMMON_MASTER WITH(NOLOCK) where RefType = 57 and status=1  ORDER BY Title ";
                    break;
                #endregion

                #region Occupation
                case "occupation":
                    sQuery = " select ConstValue as Value,isnull(Title,'') as Title from HCM_COMMON_MASTER with(nolock) where RefType = 79 order by  Title ";
                    break;
                #endregion

                #region Language
                case "language":
                    sQuery = "select RID as Value,isnull(Title,'') as 'Title' from HCM_RESUME_LANGUAGES with (nolock) where Status = 1 order by Title";
                    break;
                #endregion

                #region BloodGroup
                case "bloodgroup":
                    sQuery = " select RID as Value,Title from hcm_bloodgroup with(nolock) order by Title ";
                    break;
                #endregion

                #region Category
                case "category":
                    sQuery = " select ConstValue as Value,Title from HCM_COMMON_MASTER with(nolock) where RefType = 82 order by Title ";
                    break;
                #endregion

                #region DocumentType
                case "documenttype":
                    sQuery = " select RID as Value,Title from HCM_DOCUMENT_TYPE With(NOLOCK) where status=1 order by Title ";
                    break;
                #endregion

                #region  searchfunction
                case "searchfunction":
                    sQuery = "select RID as Value,Title from HCM_FUNCTIONS l with(nolock) where l.Status = 1 and l.RID in (select MappingID from HC_REQUISITION_MAPPING m with(nolock)  where m.[Type] = 0 and m.ReqID in(select RID  from HC_REQUISITIONS HRR with(nolock) where HRR.EndDate >= GETUTCDATE()AND HRR.ReqStatus IN (0,1) AND HRr.RID as Value IN (SELECT ReqID FROM hc_req_allocation with(nolock))))";
                    break;
                #endregion

                #region SearchLocation
                case "SearchLocation":
                    sQuery = "select RID as Value,Title  from HCM_LOCATIONS l with(nolock) where l.RID in (select MappingID from HC_REQUISITION_MAPPING m with(nolock)  where m.[Type] = 9 and m.ReqID in(select RID  from HC_REQUISITIONS HRR with(nolock) where HRR.EndDate >= GETUTCDATE() AND HRR.ReqStatus IN (0,1) AND HRr.RID as Value IN (SELECT ReqID FROM hc_req_allocation with(nolock) " +
                      " /*WHERE memberid in(" + ConfigurationManager.AppSettings["AllocationMemberid"] + ")*/" +
                      " ))) and l.[Status] = 1 order by Title";
                    break;
                #endregion

                #region MaritalStatus
                case "maritalstatus":
                    sQuery = "select ConstValue as Value,Title from HCM_COMMON_MASTER with(nolock) where RefType = 27 " +
                        "/* and RID in (" + ConfigurationManager.AppSettings["MaritalStatus"] + ") */" +
                        " and status=1 order by Title";
                    break;
                #endregion

                #region Reference
                case "reference":
                    sQuery = "select ConstValue as Value,Title from HCM_COMMON_MASTER with(nolock) where RefType = 102 order by Title ";
                    break;
                #endregion

                #region Address Type
                case "addresstype":
                    sQuery = " select RID as Value ,Title from HCM_Address_Type With(Nolock) where Status=1 ";
                    break;
                #endregion

                #region All Status
                case "allstatus":
                    sQuery = " select RID as Value,Title from HCM_Status With(NOLOCK) where RID in(Select distinct StatusID from HC_Req_Resume WITH(NOLOCK))  order by title asc";
                    break;
                #endregion

                #region Interview Status
                case "interviewstatus":
                    sQuery = "  select RID as Value,Title from HCM_Status With(NOLOCK) where RID in(Select distinct StatusID from HC_Req_Resume WITH(NOLOCK) where StageLevel=3)  ";
                    break;
                #endregion

                #region Interview Mode
                case "interviewmode":
                    sQuery = " select ConstValue as Value,Title  from HCM_Common_Master WITH(NOLOCK) where reftype=3  order by Title  ";
                    break;
                #endregion

                #region Interview Panel
                case "interviewpanel":
                    if (refID != 0)
                        sQuery = "  Select InvPanel.RID as Value,InvPanel.Title from HC_INTERVIEW_PANEL InvPanel WITH(NOLOCK),HC_INTERVIEW_PANEL_DATE InvPanelDate WITH(NOLOCK) where InvPanel.RID as Value=InvPanelDate.PanelId and InvPanel.rmid in(" + refID + ") ";
                    break;
                #endregion

                #region Grade
                case "grade":
                    sQuery = " select RID as Value, Title from HCM_GRADE With(NOLOCK) where Status = 1 order by title ";
                    break;
                #endregion

                #region Offer Grade
                case "offergrade":
                    sQuery = " select RID as Value, Title from HCM_GRADE With(NOLOCK) where Status = 1 order by title ";
                    break;
                #endregion

                #region Salary
                case "salary":
                    sQuery = " Select distinct Type as Title,0 as RID  from HC_Salary_Compensation with(nolock)";
                    break;
                #endregion
                #region Main Group
                case "maingroup":
                    sQuery = " select RID as Value, Name as Title from HC_Entity With(NOLOCK) where HC_Entity.RID in(Select OrgID from HC_req_Org with(Nolock)where HC_req_Org.orglevel = 1)  order by Name ";
                    break;
                #endregion

                #region Sub Group
                case "subgroup":
                    sQuery = " select RID as Value, Name as Title from HC_Entity With(NOLOCK) where HC_Entity.RID in(Select OrgID from HC_req_Org with(Nolock)where HC_req_Org.orglevel = 2)  order by Name";
                    break;
                #endregion

                #region Department
                case "department":
                    sQuery = " select RID as Value, Name as Title from HC_Entity With(NOLOCK) where HC_Entity.RID in(Select OrgID from HC_req_Org with(Nolock)where HC_req_Org.orglevel = 3)  order by Name";
                    break;
                #endregion

                #region TypeOfOffer
                case "typeofoffer":
                    sQuery = " Select ConstValue as Value,Title from HCM_Common_Master With(Nolock) where Reftype=83 and status=1 order by Title";
                    break;
                #endregion

                #region Recruiter List
                case "recruiter":
                    sQuery = " Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(1)) order by Title";
                    break;
                #endregion

                #region HiringManage List
                case "hiringmanager":
                    sQuery = " Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(2)) order by Title";
                    break;
                #endregion

                #region RecruiterHiringManager List
                case "recruiterhiringmanager":
                    sQuery = " Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(1,2)) order by Title";
                    break;
                #endregion

                #region Offer Tiering
                case "offertire":
                    sQuery = " Select ConstValue as Value, Title from HCM_Common_Master With(NOLOCK) where reftype=84 and status=1  order by Title";
                    break;
                #endregion

                #region Zone
                case "zone":
                    sQuery = " Select ConstValue as Value, Title from HCM_Common_Master With(NOLOCK) where reftype=8 and status=1 order by Title";
                    break;
                #endregion

                #region OfferRejectStatus
                case "offerrejectstatus":
                    //Select  RID, Title from HCM_Status With(NOLOCK) where RID in(71,10,11,6,9) and status=1 order by Title"; hemant added statuslevel for selecting reject reason
                    //sQuery = " Select  statuslevel as RID, Title from HCM_Status With(NOLOCK) where RID in(71,10) and status=1 order by Title";
                    sQuery = "Select statuslevel as Value, Title from HCM_Status With(NOLOCK) where RID in(71, 22, 9) and status = 1 order by Title";
                    break;
                #endregion


                #region PhotonDReviseStatus
                case "photondrevisestatus":
                    sQuery = "Select  statuslevel as Value, Title from HCM_Status With(NOLOCK) where RID in(22, 5, 75, 77) and status = 1 order by Title asc";
                    //sQuery = " Select  statuslevel as RID, Title from HCM_Status With(NOLOCK) where RID in(22,5,9,77) and status=1 order by Title";
                    break;
                #endregion


                #region docpending
                case "docpending":
                    // sQuery = "Select statuslevel as Value, Title from HCM_Status With(NOLOCK) where RID in(78) and status = 1 order by Title";

                    sQuery = " Select  statuslevel as Value, Title from HCM_Status With(NOLOCK) where RID in(79) and status=1 order by Title";
                    break;
                #endregion

                #region Photondrevisedstatus
                case "photonrevisereason":
                    if (refID != 0)
                        sQuery = "select RID as Value,Title from HC_Reason where RID in(Select ReasonId from HC_Reason_Stage_Status where StageLevel=4 and StatusLevel='" + refID + "' ) and IsActive=1 order by Title ";
                    else
                        sQuery = "";
                    break;
                #endregion


                #region InductionLocationMaster
                case "indlocation":
                    sQuery = "select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 186 and Status in (1) order by Title asc ";
                    break;
                #endregion

                #region InCaseOfCA
                case "incaseofca":
                    sQuery = "select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 12 and Status = 1 order by ConstValue asc ";
                    break;
                #endregion

                #region IsMBAGraduate
                case "ismbagraduate":
                    sQuery = "select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 154 and Status = 1 order by ConstValue asc ";
                    break;
                #endregion
                #region Location
                case "location":
                    sQuery = " select RID as Value,Title from hcm_locations l with(nolock) where  Status = 1 ";
                    break;
                #endregion

                #region not reachable reason
                case "notreachable":
                    if (refID != 0)
                        sQuery = "select RID as Value,Title from HC_Reason where RID in(Select ReasonId from HC_Reason_Stage_Status where StageLevel=4 and StatusLevel='" + refID + "' ) and IsActive=1 order by Title ";
                    else
                        sQuery = "";
                    break;
                #endregion

                #region Offer Reject Reason
                case "offerrejectreason":
                    if (refID != 0)
                        sQuery = "select RID as Value,Title from HC_Reason where RID in(Select ReasonId from HC_Reason_Stage_Status where StageLevel=4 and StatusLevel='" + refID + "' ) and IsActive=1 order by Title ";
                    else
                        sQuery = " select RID as Value,Title from HC_Reason where RID in(Select ReasonId from HC_Reason_Stage_Status where StageLevel=4) and IsActive=1 order by Title ";
                    break;
                #endregion

                #region Candidate Offer Reject Reason 
                case "candidateofferrejectreason":
                    if (refID != 0)
                        sQuery = "select RID as Value,Title from HC_Reason where RID in(Select ReasonId from HC_Reason_Stage_Status where StageLevel=4 and StatusLevel='" + refID + "' ) and IsActive=1 and IsCandidate=1  order by Title ";
                    else
                        sQuery = " select RID as Value,Title from HC_Reason where RID in(Select ReasonId from HC_Reason_Stage_Status where StageLevel=4 and StatusLevel=15 ) and IsActive=1 and IsCandidate=1 order by Title ";
                    break;
                #endregion

                #region checker
                case "checker":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='checker' and Status=1))";
                    break;
                #endregion
                #region approver
                case "approver":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='approver' and Status=1))";
                    break;
                #endregion

                #region edutype
                case "edutype":
                    sQuery = "select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 110 and Status = 1 order by ConstValue asc ";
                    break;
                #endregion

                #region EducationGroup BP and digitalVC
                case "educationgroupbp":
                    //sQuery = " select RID as Value,isnull(Title,'')as Title from hcm_education_group with(nolock) ORDER BY Title ";
                    sQuery = "select RID as Value,isnull(a.Title,'')as Title from hcm_education_group a with(nolock) where engkeyid in (1,8,7,11,10,12)  order by a.title";
                    break;
                #endregion

                #region stickyagent
                case "stickyagent":
                    sQuery = "select ConstValue as Value,concat(Title,' - (',(select top 1 EmployerID from hc_user_main with(nolock) where rid=refid),')') as Title from hcm_common_master with(nolock) where REfType = 244 and Status = 1 order by ConstValue asc ";
                    break;
                #endregion

                #region Bankdetails
                case "bankdetails":
                    sQuery = "select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 88 and Status = 1 order by ConstValue asc ";
                    break;
                #endregion

                #region onroll
                case "onroll":
                    sQuery = "select ConstValue as Value,Title  from hcm_common_master with(nolock) where REfType = 245 and Status = 1 order by ConstValue asc ";
                    break;
                #endregion

                #region bpuser
                case "bpuser":
                    sQuery = " SELECT    hum.RID as [Value], (Isnull(firstname, '') + ' ' + Isnull(lastname, '') + ISNULL((NULLIF((' (' + ISNULL(EmployerID, '') + ')'), ' ()')), '')) AS Title, isnull(hum.EmailID, '') AS EmailID ,Isnull(hum.MobileNo, '') AS MobileNo FROM HC_USER_MAIN hum WITH (NOLOCK) WHERE hum.RID IN (  SELECT UserID FROM HC_USER WITH (NOLOCK) WHERE RoleID = 2 and Status  = 1 )  AND hum.STATUS = 1  and hum.EmailID like '%@icicibank.com%' and exists (select top 1 1 from HC_CONTACTS_ROLE_MAPPING with(nolock) where userid=hum.rid and contactRoleID=11)  order by Title ";
                    break;
                #endregion

                #region entrance Exam
                case "entranceexam":
                    sQuery = " Select ConstValue as Value, Title from HCM_Common_Master With(NOLOCK) where reftype=253 and status=1  order by Title";
                    break;
                #endregion

                #region Campus name
                case "campusname":
                    sQuery = " Select ConstValue as Value, Title from HCM_Common_Master With(NOLOCK) where reftype=251 and status=1  order by Title";
                    break;
                #endregion

                #region Campus Degree
                case "campusdegree":
                    sQuery = " Select ConstValue as Value, Title from HCM_Common_Master With(NOLOCK) where reftype=249 and status=1  order by Title";
                    break;
                #endregion

                default:
                    sQuery = "";
                    break;

                    // select RID as Value,Title from HCM_Locations With(NOLOCK) where ISCompany=0 and rid in (select locationid from hcm_interview_venue With(NOLOCK) where rid in (select InvVenueId from HC_REQ_RES_INTERVIEW_STAGES With(NOLOCK)))order by Title

            }
            if (sQuery != "")
                return LoadMaster(sQuery);
            else
            {
                apiResponse Response = new apiResponse();
                Response = (Common.InvalidResponse(res));
                return Response;
            }
        }
        private apiResponse LoadMaster(string query)
        {
            apiResponse Response = new apiResponse();
            try
            {
                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlDataAdapter objDa = new SqlDataAdapter(query, objCon))
                    {
                        using (DataTable objdt = new DataTable())
                        {
                            objDa.Fill(objdt);

                            if (objdt == null)
                                Response = (Common.NotFoundResponse(res));

                            else if (objdt.Rows.Count > 0)
                            {
                                var LocMaster = Common.DataTableToJSONObject(objdt);
                                Response = (Common.SuccessResponse(res, LocMaster, objdt.Rows.Count));
                            }
                            else
                                Response = (Common.NotFoundResponse(res));

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response = Common.ErrorRes(res, ex);
            }
            finally
            {

            }

            return Response;
        }
        public apiResponse NameSearch(MasterNameSearch obj)
        {
            apiResponse Response = new apiResponse();
            string sQuery = "";
            sQuery = " select RID as Value,Title  from HCM_INTERVIEW_ROUND WITH(NOLOCK) order by Title ";
            switch (obj.type.ToLower())
            {
                #region Designation
                case "role":
                    sQuery = "select RID as Value,Title from hcm_designations with(nolock)  where Status = 1  and Title like @title order by Title";
                    break;
                #endregion
                #region Interview Panel
                case "panel":
                    sQuery = "  Select InvPanel.RID as Value,InvPanel.Title from HC_INTERVIEW_PANEL InvPanel WITH(NOLOCK),HC_INTERVIEW_PANEL_DATE InvPanelDate WITH(NOLOCK) where InvPanel.RID as Value=InvPanelDate.PanelId and InvPanel.rmid in(@UserId)  and InvPanel.Title like @title";
                    break;
                #endregion

                #region Location
                case "location":
                    //sQuery = " select RID as Value,Title  from HCM_LOCATIONS l with(nolock) where l.RID in (select MappingID from HC_REQUISITION_MAPPING m with(nolock)  where m.[Type] = 9 and m.ReqID in(select RID   from HC_REQUISITIONS HRR with(nolock) where HRR.EndDate >= GETUTCDATE() AND HRR.ReqStatus IN (0,1) AND HRr.RID as Value IN (SELECT ReqID FROM hc_req_allocation with(nolock) ))) and l.[Status] = 1  and Title like @title";

                    sQuery = " select RID as Value,Title  from HCM_LOCATIONS l with(nolock) Where l.[Status] = 1 and Title like @title ";
                    break;
                #endregion

                #region Main Group
                case "maingroup":
                    sQuery = "select RID as Value, Name as Title from HC_Entity With(NOLOCK) where HC_Entity.RID in(Select OrgID from HC_req_Org with(Nolock)where HC_req_Org.orglevel = 1)   and Name like @title order by Name";
                    break;
                #endregion

                #region Sub Group
                case "subgroup":
                    sQuery = "select RID as Value, Name as Title from HC_Entity With(NOLOCK) where HC_Entity.RID in(Select OrgID from HC_req_Org with(Nolock)where HC_req_Org.orglevel = 2)   and Name like @title order by Name";
                    break;
                #endregion

                #region Department
                case "department":
                    sQuery = "select RID as Value, Name as Title from HC_Entity With(NOLOCK) where HC_Entity.RID in(Select OrgID from HC_req_Org with(Nolock)where HC_req_Org.orglevel = 3)   and Name like @title order by Name";
                    break;
                #endregion

                #region Recruiter List
                case "recruiter":
                    //sQuery = " Select m.RID as Value ,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(1)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title";
                    sQuery = " Select m.RID as [Value], (Isnull(firstname, '') + ' ' + Isnull(lastname, '') + ISNULL((NULLIF((' (' + ISNULL(EmployerID, '') + ')'), ' ()')), '')) AS Title, isnull(m.EmailID, '') AS EmailID, Isnull(m.MobileNo, '') AS MobileNo from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(1)) AND (m.FirstName LIKE '%' + @title + '%' OR m.EmployerID LIKE  '%' + @title + '%')  order by Title ";
                    break;
                #endregion

                #region HiringManage List
                case "hiringmanager":
                    sQuery = " Select m.RID as Value ,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(2)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title";
                    break;
                #endregion

                #region RecruiterHiringManager List
                case "recruiterhiringmanager":
                    sQuery = " Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and m.rid in(Select USERid from HC_USER With(Nolock) where RoleID in(1,2)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title";
                    break;
                #endregion

                #region checker
                case "checker":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='checker' and Status=1)) and  ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title";
                    break;
                #endregion

                #region approver
                case "approver":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='approver' and Status=1)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title ";
                    break;
                #endregion
                #region photono
                case "phonetray":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='phonetray' and Status=1)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title ";
                    break;
                #endregion

                #region photond
                case "photond":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='photond' and Status=1)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title ";
                    break;
                #endregion

                #region maker
                case "maker":
                    sQuery = "  Select m.RID as Value,ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') as Title from HC_User_Main m With(NOLOCK) where m.status=1 and RID in(Select UserID from HC_Contacts_Role_Mapping With(Nolock) where ContactRoleID in( select RefID  from HC_Contacts_Role With(NOLOCK) where title='maker' and Status=1)) and ISNULL(FirstName,'') +' '+ISNULL(LASTname,'') like @title order by Title ";
                    break;
                #endregion

                #region bpuser
                case "bpuser":
                    sQuery = "  SELECT  hum.RID as [Value], (Isnull(firstname, '') + ' ' + Isnull(lastname, '') + ISNULL((NULLIF((' (' + ISNULL(EmployerID, '') + ')'), ' ()')), '')) AS Title, isnull(hum.EmailID, '') AS EmailID , Isnull(hum.MobileNo, '') AS MobileNo FROM HC_USER_MAIN hum WITH (NOLOCK) WHERE hum.RID IN (  SELECT UserID FROM HC_USER WITH (NOLOCK) WHERE RoleID = 2 and Status  = 1 )  AND hum.STATUS = 1AND (hum.FirstName LIKE '%' + @title + '%' OR hum.EmployerID LIKE  '%' + @title + '%'  OR ( Isnull(hum.firstname, '') + ' ' + Isnull(hum.lastname, '')) like '' + @title + '%' ) and hum.EmailID like '%@icicibank.com%' and exists (select top 1 1 from HC_CONTACTS_ROLE_MAPPING with(nolock) where userid=hum.rid and contactRoleID=11)  order by Title ";
                    break;
                #endregion

                default:
                    sQuery = "";
                    break;
            }

            if (sQuery == "")
            {
                Response = Common.InvalidResponse(res);
                return Response;
            }

            try
            {
                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlDataAdapter objda = new SqlDataAdapter(sQuery, objCon))
                    {
                        objda.SelectCommand.Parameters.AddWithValue("@UserId", obj.UserId);
                        objda.SelectCommand.Parameters.AddWithValue("@title", "%" + obj.searchByTitle + "%");
                        using (DataTable objdt = new DataTable())
                        {
                            objda.Fill(objdt);
                            if (objdt == null)
                                Response = Common.NotFoundResponse(res);
                            else if (objdt.Rows.Count == 0)
                                Response = Common.NotFoundResponse(res);
                            else
                            {
                                //List<HCMaster> Master = new List<HCMaster>();
                                //HCMaster objMaster = null;
                                //foreach (DataRow dr in objdt.Rows)
                                //{
                                //    objMaster = new HCMaster();
                                //    objMaster.Value = Common.bInt64(dr["Value"]);
                                //    objMaster.Title = Common.bStr(dr["Title"]);
                                //    Master.Add(objMaster);
                                //}

                                var Master = Common.DataTableToJSONObject(objdt);



                                Response = Common.SuccessResponse(res, Master, objdt.Rows.Count);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response = Common.ErrorRes(res, ex);
            }
            return Response;

        }
        public apiResponse bePanelByDateRange(PanelSearchByDate obj)
        {
            apiResponse Response = new apiResponse();
            string sQuery = "";
            sQuery = "Select InvPanel.RID ,InvPanel.Title as PanelTitle, ISNULL((Select Title FROM HCM_Interview_Venue WITH(NOLOCK) where RID=InvPanel.VenueId),'') as VanueTitle,REPLACE(CONVERT(NVARCHAR,CAST(InvPanelDate.InterviewDate AS DATETIME), 106), ' ', '-') As InterviewDate,InvPanelDate.StartTime,InvPanelDate.EndTime from HC_INTERVIEW_PANEL InvPanel WITH(NOLOCK),HC_INTERVIEW_PANEL_DATE InvPanelDate WITH(NOLOCK) where InvPanel.RID as Value=InvPanelDate.PanelId and InvPanel.rmid in(@UserId) ";


            try
            {
                using (SqlConnection objCon = new SqlConnection(Common.Connection.ConStr))
                {
                    using (SqlDataAdapter objda = new SqlDataAdapter(sQuery, objCon))
                    {
                        if (!string.IsNullOrEmpty(obj.FromDate) && !string.IsNullOrEmpty(obj.ToDate))
                        {
                            sQuery += " and InvPanelDate.InterviewDate between @FromDate and @ToDate)";

                            try
                            {
                                objda.SelectCommand.Parameters.AddWithValue("@FromDate", Convert.ToDateTime(obj.FromDate).ToString("dd-MMM-yyyy"));
                                objda.SelectCommand.Parameters.AddWithValue("@ToDate", Convert.ToDateTime(obj.ToDate).ToString("dd-MMM-yyyy"));
                            }
                            catch (Exception ex)
                            {
                                Response = Common.ErrorRes(res, ex);
                                return Response;
                            }

                        }
                        else
                        {//if no valildate date then error 
                            Response = Common.ErrorRes(res, "Select date");
                            return Response;

                        }

                        objda.SelectCommand.Parameters.AddWithValue("@UserId", obj.UserID);


                        using (DataTable objdt = new DataTable())
                        {
                            objda.Fill(objdt);
                            if (objdt == null)
                                Response = Common.NotFoundResponse(res);
                            else if (objdt.Rows.Count == 0)
                                Response = Common.NotFoundResponse(res);
                            else
                            {
                                List<GetPanelList> GetPanelList = new List<GetPanelList>();
                                GetPanelList objGetPanelList = null;
                                foreach (DataRow dr in objdt.Rows)
                                {
                                    objGetPanelList = new GetPanelList();
                                    objGetPanelList.PanelID = Common.bInt64(dr["RID"]);
                                    objGetPanelList.PanelTitle = Common.bStr(dr["PanelTitle"]);
                                    objGetPanelList.VanueTitle = Common.bStr(dr["VanueTitle"]);
                                    objGetPanelList.InterviewDate = Common.bStr(dr["InterviewDate"]);
                                    objGetPanelList.StartTime = Common.bStr(dr["StartTime"]);
                                    objGetPanelList.EndTime = Common.bStr(dr["EndTime"]);
                                    GetPanelList.Add(objGetPanelList);
                                }

                                Response = Common.SuccessResponse(res, GetPanelList, objdt.Rows.Count);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response = Common.ErrorRes(res, ex);
            }
            return Response;

        }
        #endregion
    }
}
