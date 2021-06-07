using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using ChangeControl.Helpers;

namespace ChangeControl.Controllers{
    public class DetailController : ChangeControlController{
        // GET: Detail
        private DetailModel M_Detail;
        private RequestModel M_Req;
        private HomeModel M_Home;
        private MailController C_Mail;
       
        public DetailController(){
            M_Detail = new DetailModel();
            M_Req = new RequestModel();
            M_Home = new HomeModel();
            C_Mail = new MailController();
            if(ViewBag.QCAudit == null) ViewBag.QCAudit = M_Home.GetQcAudit();
            if(ViewBag.PEAudit == null) ViewBag.PEAudit = M_Home.GetPEAudit();
        }
        public class Value{
            public List<string> value { get; set; }
        }

        public class RawFile{
            public HttpPostedFileBase file {get; set;}
            public string description {get; set;}
            public int id {get; set;}
            public string code {get; set;}
        }

        public bool isTrialable = false;
        private string date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        private string date = DateTime.Now.ToString("yyyyMMddHHmmss");
        public static TopicAlt Topic;
        public static long? file_id;

        public ActionResult Index(string id){
            if((string)(Session["User"]) == null || (string)(Session["Department"]) == null){
                Session["RedirectID"] = id ?? null;
                Session["RedirectMode"] = "Detail";
                return RedirectToAction("Index", "LogIn");
            }

            GenerateTopicList(Session["Department"].ToString(), Session["Position"].ToString());//noti top layout
            Session["RedirectID"] = null;

                Session["TopicCode"] = id ?? M_Detail.GetFirstTopic();

            Topic = M_Detail.GetTopicByCodeAndOwned(Session["TopicCode"].ToString(), Session["Department"].ToString());
            Topic.Profile = M_Detail.getUserByID(Topic.User_insert);
            Topic.Time_insert = Topic.Time_insert.StringToDateTime();
            Topic.Timing = Topic.Timing.StringToDateTime();
            
            Session["TopicCode"] = Topic.Code;
            Session["TopicID"] = Topic.ID;
            if(Topic.ApprovedDate != null) {Topic.ApprovedDate = Topic.ApprovedDate.StringToDateTime();}
            if(Topic.ApprovedBy != null) {Topic.ApproverProfile = M_Detail.getUserByID(Topic.ApprovedBy);}

            ViewBag.ResubmitList = this.GetResubmitListByTopicID();
            Session["ReviewList"]  = ViewBag.ReviewList = this.GetReviewListByTopicID();// Get element for department review
            Session["TrialList"]  = ViewBag.TrialList = this.GetTrialListByTopicCode();
            Session["ConfirmList"] = ViewBag.ConfirmList = this.GetConfirmListByTopicCode();

            Topic.RelatedListAlt = M_Detail.GetRelatedByID(Topic.Related);

            FilterReviewRelated(ViewBag.ReviewList);
            FilterTrialRelated(ViewBag.ReviewList, ViewBag.TrialList);
            FilterConfirmRelated(ViewBag.ConfirmList);

            var DepartmentGroup = M_Detail.GetDepartmentGroup(); //Get raw group of departments
            List<DepartmentList> departmentList = new List<DepartmentList>();
            
            foreach(string GroupName in DepartmentGroup){ //For edit related as PE_Process in Review phase
                List<Department> departments = new List<Department>();
                departments = M_Detail.GetDepartmentByGroup(GroupName);
                foreach(Department department in departments){
                    var rl = Topic.RelatedListAlt.Find(e => e.Department == department.Name);
                    if(rl != null){
                        department.Status = 1;
                    }
                }
                departmentList.Add(new DepartmentList(){Name = GroupName.Replace(" ", "_"), Department = departments}); //Convert raw group into department list for radio
            }
            ViewData["DepartmentList"] = departmentList;


            var Related = M_Detail.GetRelatedByID(Topic.Related);

            if(Topic.Status == 12){ //If topic rejected
                var RejectMessage = GetRejectMessageByTopicCode(Topic.Code);
                RejectMessage.Profile =  M_Detail.getUserByID(RejectMessage.User);
                RejectMessage.Date =  RejectMessage.Date.StringToDateTime();
                ViewBag.RejectMessage = RejectMessage;
            }
            Session["Topic"] = ViewData["Topic"] = Topic;
            return View();
        }

        public ActionResult InsertReview(string topic_id, string topic_code,string topic_status,Boolean isExternal)
        {
            var mail = "";
            var pos = "";
            //if((Topic.Status == 3 || Topic.Status == 7) && Topic.Type == "External"){
            //    M_Detail.UpdateTopicStatus(topic_code, 8);
            //}
            if ((topic_status == "3" || topic_status == "7") && isExternal)
            {
                M_Detail.UpdateTopicStatus(topic_code, 8);
            }
            mail = "EmailReviewed";
            pos = "Approver";
            Session["ReviewID"] = M_Detail.InsertReview(Convert.ToInt64(topic_id), topic_code, Session["User"].ToString(), Session["Department"].ToString());

            return Json(new {code=1,mail,dept=Session["Department"].ToString(), pos}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertReviewItem(string status, string description, int id){
            M_Detail.InsertReviewItem(status, description, id, (long) Session["ReviewID"]);
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InsertFile(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["ReviewID"], "Review", file_item.description.ReplaceSingleQuote(), Session["User"], file_item.code, Session["Department"].ToString(), date_ff);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateFileDesc(string desc){
            M_Detail.UpdateFileDesc(file_id,desc);
            return Json(true);
        }

        public ActionResult GetReviewByTopicID(string topic_code){
            var Review = M_Detail.GetReviewByTopicCode(topic_code);
            return Json(Review);
        }

        public ActionResult GetReviewItemByReviewID(long review_id){
            var ReviewItem = M_Detail.GetReviewItemByReviewID(review_id);
            return Json(ReviewItem);
        }

        public List<Review> GetReviewListByTopicID(){
            List<ReviewItem> rv_item_list = new List<ReviewItem>();
            List<Review> rv_list = new List<Review>();
            List<FileItem> file_list = new List<FileItem>();

            var tc_file_list = M_Detail.GetFileByID(Topic.ID, "Topic", Session["TopicCode"].ToString(), Topic.Department);
            Topic.FileList = tc_file_list;

            ViewData["FormReviewItem"] = null;
            var tmp_dept = (string) Session["Department"];
            var tmp_dept_id = (int)Session["DepartmentID"];

            // Set review item for each department
            if(!ViewBag.PEAudit.Contains(tmp_dept) && !ViewBag.QCAudit.Contains(tmp_dept)){
                var rv_form_item = M_Detail.GetReviewItemByDepartment(tmp_dept_id);
                ViewData["FormReviewItem"] = rv_form_item;
            }
            rv_list = M_Detail.GetReviewByTopicCode(Session["TopicCode"].ToString());
            
            isTrialable = M_Detail.GetTrialStatusByTopicAndDept(Session["TopicCode"].ToString(),tmp_dept_id);
            ViewData["isTrialable"] = isTrialable;

            foreach(Review rv_element in rv_list){
                rv_item_list = M_Detail.GetReviewItemByReviewID(rv_element.ID);
                rv_element.Item = rv_item_list;
                rv_element.Date = rv_element.Date.StringToDigitDate();
                if(rv_element.ApprovedDate.AsNullIfEmpty() != null) rv_element.ApprovedDate = rv_element.ApprovedDate.StringToDigitDate();
                rv_element.Profile = (rv_element.User.AsNullIfEmpty() != null) ? M_Detail.getUserByID(rv_element.User) : new Models.User();
                rv_element.Approver = (rv_element.ApprovedBy.AsNullIfEmpty() != null) ? M_Detail.getUserByID(rv_element.ApprovedBy) : new Models.User();
                
                file_list = M_Detail.GetFileByID(rv_element.ID, "Review", Session["TopicCode"].ToString(), rv_element.Department);
                rv_element.FileList = file_list;
            }

            return rv_list;
        }

        public List<Resubmit> GetResubmitListByTopicID(){
            var temp_resubmit_list = M_Detail.GetResubmitByTopicID(Topic.Code);
            foreach(Resubmit resubmit in temp_resubmit_list){
                List<Response> response_list = new List<Response>();
                resubmit.Description = resubmit.Description.ReplaceNullWithDash();
                resubmit.Date = resubmit.Date.StringToDigitDate();
                resubmit.DueDate = resubmit.DueDate.StringToDigitDate3();
                resubmit.Profile = M_Detail.getUserByID(resubmit.User);             
                resubmit.FileList = M_Detail.GetFileByFKID(resubmit.ID, "Resubmit", Session["TopicCode"].ToString(), resubmit.Dept);

                var result = M_Detail.GetResponseByResubmitID(resubmit.ID);
                foreach(var response in result){
                    response.Date = response.Date.StringToDigitDate();
                    response.Profile = M_Detail.getUserByID(response.User);
                }
                if(result != null){
                    response_list = result;
                }
                resubmit.Responses = response_list;
                foreach(var response in resubmit.Responses){
                    var rs_file_list = M_Detail.GetFileByFKID(response.ID, "Response", Session["TopicCode"].ToString(), response.Department);
                    response.FileList = rs_file_list;
                }
                resubmit.RelatedList = M_Detail.GetRelatedByID(resubmit.Related);
                foreach(var rsm_rl in resubmit.RelatedList){
                    if(response_list.Exists( e => e.Department == rsm_rl.Department)){
                        rsm_rl.Response = 1;
                    }
                }
            }
            return temp_resubmit_list;
        }

        public List<Trial> GetTrialListByTopicCode(){
            List<Trial> trial_list = new List<Trial>();
            List<FileItem> file_list = new List<FileItem>();
            trial_list = M_Detail.GetTrialByTopicCode(Topic.Code);
            foreach(Trial tr_element in trial_list){
                var tr_file_list = M_Detail.GetFileByID(tr_element.ID, "Trial", Session["TopicCode"].ToString(), tr_element.Department);
                tr_element.FileList = tr_file_list;
                tr_element.Date =  tr_element.Date.StringToDigitDate();
                if(tr_element.ApprovedDate.AsNullIfEmpty() != null) tr_element.ApprovedDate = tr_element.ApprovedDate.StringToDigitDate();
                tr_element.Profile = (tr_element.User.AsNullIfEmpty() != null) ? M_Detail.getUserByID(tr_element.User) : new Models.User();
                tr_element.Approver =(tr_element.ApprovedBy.AsNullIfEmpty() != null) ?  M_Detail.getUserByID(tr_element.ApprovedBy) : new Models.User();
            }
            return trial_list;
        }

        public List<Confirm> GetConfirmListByTopicCode(){
            List<Confirm> Confirm_list = new List<Confirm>();
            List<FileItem> file_list = new List<FileItem>();
            Confirm_list = M_Detail.GetConfirmByTopicCode(Topic.Code);
            foreach(Confirm cf_element in Confirm_list){
                var tr_file_list = M_Detail.GetFileByID(cf_element.ID, "Confirm", Session["TopicCode"].ToString(), cf_element.Department);
                cf_element.FileList = tr_file_list;
                cf_element.Date = cf_element.Date.StringToDigitDate();
                if(cf_element.ApprovedDate.AsNullIfEmpty() != null) cf_element.ApprovedDate = cf_element.ApprovedDate.StringToDigitDate();
                cf_element.Profile = (cf_element.User.AsNullIfEmpty() != null) ? M_Detail.getUserByID(cf_element.User) : new Models.User();
                cf_element.Approver = (cf_element.ApprovedBy.AsNullIfEmpty() != null) ?  M_Detail.getUserByID(cf_element.ApprovedBy) : new Models.User();
            }
            return Confirm_list;
        }

        public ActionResult RequestResubmit(string desc, string due_date,string topic_code)
        {
            try{
                //M_Detail.InsertResubmit(desc, due_date, (long) Session["RelatedID"], Topic.Code, (string)Session["User"], Topic.Status, (string)Session["Department"]);
                Session["ResubmitID"] = M_Detail.InsertResubmit(desc, due_date, (long) Session["RelatedID"], topic_code, (string)Session["User"], Topic.Status, (string)Session["Department"]);
                return Json(new { status = "success" },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { status = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult InsertRelated(List<String> dept_list){
            try{
                Session["RelatedID"] = M_Detail.InsertRelated(dept_list, Session["User"].ToString());
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult InsertResponse(string desc, long resubmit_id){
            try{
                
                Session["ResponseID"] = M_Detail.InsertResponse(desc, Session["Department"].ToString(), Session["User"].ToString(), date, resubmit_id);
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult InsertFileResponse(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value; 


            if (file_item.file != null && file_item.file.ContentLength > 0){
                var input_file_name = Path.GetFileName(date_ff);
                var server_path = Path.Combine("D:/File/Topic/" + input_file_name);
                file_item.file.SaveAs(server_path);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["ResponseID"], "Response", file_item.description, Session["User"], file_item.code, Session["Department"].ToString(), date_ff);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult InsertFileResubmit(RawFile file_item)
        {
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;

            if (file_item.file != null && file_item.file.ContentLength > 0)
            {
                var input_file_name = Path.GetFileName(date_ff);
                var server_path = Path.Combine("D:/File/Topic/" + input_file_name);
                file_item.file.SaveAs(server_path);
                if (file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long)Session["ResubmitID"], "Resubmit", file_item.description, Session["User"], file_item.code, Session["Department"].ToString(), date_ff);
            }
            return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
            //catch (Exception err)
            //{
            //    return Json(new { error = err }, JsonRequestBehavior.AllowGet);
            //}
        }

        [HttpPost]
        public ActionResult UpdateTopicStatus(string topic_code, int status){
            try{
                M_Detail.UpdateTopicStatus(topic_code,status);
                if(status == 9){
                    M_Detail.UpdateTopicApproveReview(Session["User"].ToString(), Topic.Code);
                }else if(status == 10){
                    M_Detail.UpdateTopicApproveTrial(Session["User"].ToString(), Topic.Code);
                }else if(status == 11 || status == 12){
                    M_Detail.UpdateTopicApproveClose(Session["User"].ToString(), Topic.Code);
                }
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult InsertTrial(string desc,string topic_code){
            try{
                Session["TrialID"] = M_Detail.InsertTrial((long) Session["TopicID"], topic_code, desc, Session["Department"].ToString(), Session["User"].ToString());
                return Json(new { code = true, mail = "EmailTrialed", dept=Session["Department"].ToString(), pos = "Approver" },JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateReview(string topic_id, string topic_code){
            if(!CheckCurrentStatus()){
                throw new Exception("Status has changed");
            }
            var updated_rv = M_Detail.UpdateReview(Convert.ToInt64(topic_id), topic_code, Session["User"].ToString(), Session["Department"].ToString());
            Session["ReviewID"] = updated_rv.ID;
            Session["ReviewRev"] = updated_rv.Revision.ToString();
            return Json(new {code = 1,dept = Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTrial(string topic_id, string topic_code, string desc){
            if(!CheckCurrentStatus()){
                throw new Exception("Status has changed");
            }
            try{
                var updated_tr = M_Detail.UpdateTrial(Convert.ToInt64(topic_id), topic_code, desc, Session["Department"].ToString(), Session["User"].ToString());
                Session["TrialID"] = updated_tr.ID;
                Session["TrialRev"] = updated_tr.Revision;
            }catch(Exception err){

            }
            return Json(new {code = 1, dept = Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateConfirm(string topic_id, string topic_code, string desc){
            if(!CheckCurrentStatus()){
                throw new Exception("Status has changed");
            }
            var updated_cf = M_Detail.UpdateConfirm(Convert.ToInt64(topic_id), topic_code, desc, Session["Department"].ToString(), Session["User"].ToString());
            Session["ConfirmID"] = updated_cf.ID;
            Session["ConfirmRev"] = updated_cf.Revision;
            return Json(new {code = 1, dept = Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InsertFileTrial(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["TrialID"], "Trial", file_item.description, Session["User"], file_item.code, Session["Department"].ToString(), date_ff);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InsertConfirm(string topic_id, string topic_code, string desc){
            try{
                Session["ConfirmID"] = M_Detail.InsertConfirm(Convert.ToInt64(topic_id), topic_code, desc, Session["Department"].ToString(), Session["User"].ToString());

                return Json(new { code = true ,mail = "EmailConfirmed", dept=Session["Department"].ToString(), pos = "Approver"},JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult InsertFileConfirm(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["ConfirmID"], "Confirm", file_item.description, Session["User"], file_item.code, Session["Department"].ToString(), date_ff);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveReview(string topic_code, long review_id){
            var rv_list = Session["ReviewList"] as List<Review>;
            Topic = M_Detail.GetTopicByCodeAndOwned(topic_code, Session["Department"].ToString());

            Topic.RelatedListAlt = M_Detail.GetRelatedByID(Topic.Related);
            try
            {
                var mail = "";
                var dept = "";
                    
                M_Detail.ApproveReview(review_id,(string) Session["User"]);
                if(Topic.Status == 7 && (ViewBag.PEAudit.Contains(Session["Department"].ToString())) && Topic.Type == "Internal"){ //PE_Process open review case
                    M_Detail.UpdateTopicStatus(topic_code, 8);
                    M_Detail.UpdateTopicApproveRequest(Session["User"].ToString(),Topic.Code);
                    mail = "InformUser";
                }else if(Topic.Status == 8 && ViewBag.QCAudit.Contains(Session["Department"].ToString()) ){
                    if(rv_list.Exists(e => e.Item.Exists(d => d.Type == 24 && d.Status == 1))){ //Request trial not exist
                        M_Detail.UpdateTopicStatus(topic_code, 9);
                        mail = "StartTrial";
                    }else{
                        M_Detail.UpdateTopicStatus(topic_code, 10);
                        mail = "StartConfirm";
                    }
                    M_Detail.UpdateTopicApproveReview(Session["User"].ToString(),Topic.Code);
                }else if(Topic.Status == 7 ||Topic.Status == 8){
                    var qc_audit = Topic.RelatedListAlt.Find(e => (ViewBag.QCAudit.Contains(e.Department)));
                    if((Topic.RelatedListAlt.Count(e => e.Review == 0) == 1 || rv_list.Exists(e => e.Department == qc_audit.Department)) && M_Detail.CheckAllReviewApproved(topic_code)){
                        mail = "ReviewApproved";
                        dept = qc_audit.Department;
                    }
                }
                return Json(new {code=true,mail,dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveTrial(string topic_code, long trial_id){
            var tr_list = Session["TrialList"] as List<Review>;
            try{
                var mail = "";
                var dept = "";
                M_Detail.ApproveTrial(trial_id,(string) Session["User"]);
                if(Topic.Status == 9 && ViewBag.QCAudit.Contains(Session["Department"].ToString()) ){
                    M_Detail.UpdateTopicStatus(topic_code, 10);
                    M_Detail.UpdateTopicApproveTrial(Session["User"].ToString(),Topic.Code);
                    mail = "StartConfirm";
                }else if(Topic.Status == 9){
                    var qc_audit = Topic.RelatedListAlt.Find(e => (ViewBag.QCAudit.Contains(e.Department)));
                    if(Topic.RelatedListAlt.Exists(e => e.Trial == 2 && (ViewBag.QCAudit.Contains(e.Department))) && !Topic.RelatedListAlt.Exists(e => e.Trial == 0) &&  M_Detail.CheckAllTrialApproved(topic_code)){
                        mail = "TrialApproved";
                        dept = qc_audit.Department;
                    }
                }
                return Json(new {code=true,mail,dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveConfirm(string topic_code, long confirm_id){
            var cf_list = Session["ConfirmList"] as List<Review>;
            try{
                var confirm_dept_list = M_Home.GetConfirmDeptList();
                
                var mail = "";
                var dept = "ConfirmApproved";
                M_Detail.ApproveConfirm(confirm_id,(string) Session["User"]);
                if(Topic.Status == 10 && ViewBag.QCAudit.Contains(Session["Department"].ToString()) ){
                    M_Detail.UpdateTopicStatus(topic_code, 11);
                    M_Detail.UpdateTopicApproveClose(Session["User"].ToString(),Topic.Code);
                }else if(Topic.Status == 10){
                    var qc_audit = Topic.RelatedListAlt.Find(e => (ViewBag.QCAudit.Contains(e.Department)));
                    if(Topic.RelatedListAlt.Exists(e => e.Confirm == 0 && (ViewBag.QCAudit.Contains(e.Department)))  && !Topic.RelatedListAlt.Exists(e => e.Confirm == 0 && confirm_dept_list.Contains(e.Department)) && M_Detail.CheckAllConfirmApproved(topic_code)){
                        mail = "ConfirmApproved";
                        dept = qc_audit.Department;
                    }
                }
                return Json(new {code=true,mail,dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        public void FilterReviewRelated(List<Review> rv_list){
            foreach(var rl_alt in Topic.RelatedListAlt){
                if(rv_list.Exists( e => e.Department == rl_alt.Department)){
                    rl_alt.Review = 1;
                }
            }
        }

        public void FilterTrialRelated(List<Review> rv_list, List<Trial> tr_list){
            foreach (var rv in rv_list){
                    var temp_tr = new Trial();
                        Topic.RelatedListAlt.ForEach(rl_alt => {
                            if(rv.Item.Exists(rv_e => rv_e.Type == 24 && rv_e.Status == 1 && rv.Department == rl_alt.Department)){ // Can trial
                                temp_tr = tr_list.Find( e => e.Department == rv.Department);
                                if (temp_tr != null && rl_alt.Department == temp_tr.Department){ //Trialed
                                    rl_alt.Trial = 1;
                                }else if(rl_alt.Department == rv.Department){ //Not trial
                                    rl_alt.Trial = 0;
                                }
                            }else{ //Cannot trial
                                if(rl_alt.Department == rv.Department){
                                    rl_alt.Trial = 2;
                                }
                            }
                        });
            }
        }

        public void FilterConfirmRelated(List<Confirm> cf_list) {
            var confirm_dept_list = M_Detail.GetConfirmDeptList();
            ViewBag.cf_list = confirm_dept_list;

            foreach (var cf in cf_list){
                if(confirm_dept_list.Contains(cf.Department)){
                    Topic.RelatedListAlt.ForEach(x => {
                        if(x.Department == cf.Department){
                            x.Confirm = 1;
                        }
                    });
                }
            }
        }

        public bool CheckAllReviewBeforeApprove(string topic_code){
            var dept = (string) Session["Department"];
            var result = true;
            if(ViewBag.QCAudit.Contains(dept)){
                List<Review> rv_list = M_Detail.CheckAllReviewBeforeApprove(topic_code);
                TopicAlt topic = Session["Topic"] as TopicAlt;
                    if(rv_list.Count != 0){
                        if(topic.Type == "Internal" && ViewBag.PEAudit.Contains(Session["Department"].ToString()) ){
                            var rv_pe_process = rv_list.Find(x => ViewBag.PEAudit.Contains(x.Department));
                            if(rv_pe_process != null) rv_list.Remove(rv_pe_process);
                            result = (rv_list.Count != 0) ? !rv_list.Exists(e => e.Status == 3 && ViewBag.PEAudit.Contains(e.Department)) : true;
                        }else{
                            result = !rv_list.Exists(e => e.Status == 3);
                        }
                    }else{
                        result = false;
                    }
            }
            return result;
        }

        public bool CheckAllTrialBeforeApprove(string topic_code){
            var dept = (string) Session["Department"];
            if(ViewBag.QCAudit.Contains(dept)){
                List<Trial> result = M_Detail.CheckAllTrialBeforeApprove(topic_code);
                return (result.Count != 0) ? !result.Exists(e => e.Status == 3) : false;
            }else{
                return true;
            }
        }

        public bool CheckAllConfirmBeforeApprove(string topic_code){
            var dept = (string) Session["Department"];
            if(ViewBag.QCAudit.Contains(dept)){
                List<Confirm> result = M_Detail.CheckAllConfirmBeforeApprove(topic_code);
                return (result.Count != 0) ? !result.Exists(e => e.Status == 3) : true;
            }else{
                return true;
            }
        }

        public ActionResult RejectTopic(string topic_status, string topic_dept, string topic_code,string desc){
            try{
                var mail = "";
                var dept = "";
                this.UpdateTopicStatus(topic_code,12);
                M_Detail.RejectTopic(topic_code,desc,(string) Session["User"], (string) Session["Department"]);
                mail = "TopicReject";
                if(Convert.ToInt32(topic_status) == 7){
                    dept = topic_dept;
                }
                return Json(new {status="success", mail , dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
            }
        }

        public Reject GetRejectMessageByTopicCode(string topic_code){
            try{
                return M_Detail.GetRejectMessageByTopicCode(topic_code);
            }catch(Exception err){
                return null;
            }
        }

        public ActionResult CheckApproveIPP(string topic_code){
            try{
                List<Review> temp_rv_list = (List<Review>) Session["ReviewList"];
                if(temp_rv_list.Exists(rv => rv.Item.Exists(rv_item => rv_item.Type == 26 && rv_item.Status == 1)) && ViewBag.QCAudit.Contains(Session["Department"].ToString())){
                    string[] pt_list = {"P1","P2","P3A","P3M","P4","P5","P6","P7"};
                    string[] qcf_list = {"QC_FINAL1", "QC_FINAL2", "QC_FINAL3"};

                    var related_pt_qcf = Topic.RelatedListAlt.FindAll(e => e.Review == 1 && (pt_list.Contains(e.Department) || qcf_list.Contains(e.Department)));
                    return Json(new {status="success",data=related_pt_qcf.Select(s => s.Department).ToList()}, JsonRequestBehavior.AllowGet);
                }else{
                    return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
                }
            }catch(Exception err){
                return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
            }
        }

        public bool CheckCurrentStatus(){
            try{
            var temp_topic = (TopicAlt) Session["Topic"];
            var tp_status = M_Detail.GetTopicStatusByCode(temp_topic.Code);
                return temp_topic.Status == tp_status;
            }catch(Exception err){
                return false;
            }
        }

        public ActionResult ApproveTopic(string topic_code){
            M_Detail.ApproveTopicByCode(topic_code, Session["User"].ToString());
            return Json(new {status="success"}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTopicRelated(string topic_code){
            M_Detail.UpdateTopicRelated(topic_code, (long) Session["RelatedID"]);
            return Json(new {status="success"}, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult DeleteFileByNameFormat(string name_format){
            try{
                M_Detail.DeleteFileByNameFormat(name_format);
                return Json(new {status="success"}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAuditNotification(){
            return Json(M_Detail.GetAuditNotification(), JsonRequestBehavior.AllowGet);
        }

    }
}