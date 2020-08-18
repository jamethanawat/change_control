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
    public class DetailController : Controller{
        // GET: Detail
        private DetailModel M_Detail;
        private RequestModel M_Req;
        private MailController C_Mail;
       
        public DetailController(){
            M_Detail = new DetailModel();
            M_Req = new RequestModel();
            C_Mail = new MailController();
        }
        public class Value{
            public List<string> value { get; set; }
        }
        public class RawFile{
            public HttpPostedFileBase file {get; set;}
            public string description {get; set;}
            public int id {get; set;}
        }

        public bool isTrialable = false;
        private string date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        private string date = DateTime.Now.ToString("yyyyMMddHHmmss");
        public static TopicAlt Topic;
        public static long? file_id;

        public ActionResult Index(string id){
            if((string)(Session["User"]) == null || (string)(Session["Department"]) == null){
                Session["RedirectID"] = (id != null) ? id : null;
                return RedirectToAction("Index", "LogIn");
            }

            Session["RedirectID"] = null;

            if(id != null){
                Session["TopicCode"] = id;
            }else{
                Session["TopicCode"] = M_Detail.GetFirstTopic();
            }

            Topic = M_Detail.GetTopicByCode(Session["TopicCode"] as string as string);
            Topic.Profile = M_Detail.getUserByID(Topic.User_insert);
            Topic.Time_insert = Topic.Time_insert.StringToDateTime();
            Session["TopicCode"] = Topic.Code;
            Session["TopicID"] = Topic.ID;
            if(Topic.ApprovedDate != null) {Topic.ApprovedDate = Topic.ApprovedDate.StringToDateTime();}
            if(Topic.ApprovedBy != null) {Topic.ApproverProfile = M_Detail.getUserByID(Topic.ApprovedBy);}

            ViewBag.ResubmitList = this.GetResubmitListByTopicID();
            Session["ReviewList"]  = ViewBag.ReviewList = this.GetReviewListByTopicID();
            Session["TrialList"]  = ViewBag.TrialList = this.GetTrialListByTopicCode();
            Session["ConfirmList"] = ViewBag.ConfirmList = this.GetConfirmListByTopicCode();

            var DepartmentGroup = M_Detail.GetDepartmentGroup(); //Get raw group of departments
            List<DepartmentList> departmentList = new List<DepartmentList>();
            
            TopicAlt tp = ViewData["Topic"] as TopicAlt;
            
            foreach(string GroupName in DepartmentGroup){
                List<Department> departments = new List<Department>();
                departments = M_Detail.GetDepartmentByGroup(GroupName);
                foreach(Department department in departments){
                    var rl = tp.RelatedListAlt.Find(e => e.name == department.Name);
                    if(rl != null){
                        department.Status = 1;
                    }
                }
                departmentList.Add(new DepartmentList(){Name = GroupName.Replace(" ", "_"), Department = departments}); //Convert raw group into department list for radio
            }
            ViewData["DepartmentList"] = departmentList;

            var Related = M_Detail.GetRelatedByID(Topic.Related);
            List<RelatedAlt> TrialRelatedList = new List<RelatedAlt>();
            ViewData["ReviewRelatedList"] = Session["ReviewRelatedList"] = ViewBag.ReviewRelatedList = FilterReviewRelated(Related, Session["ReviewList"] as List<Review>);
            ViewData["TrialRelatedList"] = Session["TrialRelatedList"] = ViewBag.TrialRelatedList = FilterTrialRelated(Session["ReviewList"] as List<Review>, Session["TrialList"] as List<Trial>);
            ViewData["ConfirmRelatedList"] = Session["ConfirmRelatedList"] = ViewBag.ConfirmRelatedList = FilterConfirmRelated(Related,Session["ConfirmList"] as List<Confirm>);

            if(Topic.Status == 12){
                var RejectMessage = GetRejectMessageByTopicCode(Topic.Code);
                RejectMessage.Profile =  M_Detail.getUserByID(RejectMessage.User);
                RejectMessage.Date =  RejectMessage.Date.StringToDateTime();
                ViewBag.RejectMessage = RejectMessage;
            }

            return View();
        }

        public ActionResult SubmitReview(){
            var mail = "";
            if(Topic.Status == 7 && Topic.Type == "External"){
                M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 8);
            }
            mail = "EmailReviewed";
            Session["ReviewID"] = M_Detail.InsertReview(Session["TopicCode"] as string, Session["User"].ToString(), Session["Department"].ToString());
            // CheckAllReviewApproved(Session["ReviewRelatedList"] as List<RelatedAlt>, Session["ReviewList"] as List<Review>);

            return Json(new {code=1,mail,dept=Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitReviewItem(string status, string description, int id){
            M_Detail.InsertReviewItem(status, description, id, (long) Session["ReviewID"]);
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitFile(RawFile file_item){
            TopicAlt temp_topic = Topic;
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["ReviewID"], "Review", file_item.description, Session["User"]);
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

            var tc_file_list = M_Detail.GetFileByID(Topic.ID, "Topic");
            var new_tp = Topic;
            new_tp.FileList = tc_file_list;

            ViewData["FormReviewItem"] = null;
            var tmp_dept = (string) Session["Department"];
            var tmp_dept_id = (int)Session["DepartmentID"];

            // Set review item for each department
            if(tmp_dept != "PE1_Process" && tmp_dept != "PE1_Process" && tmp_dept != "QC"){
                var rv_form_item = M_Detail.GetReviewItemByDepartment(tmp_dept_id);
                ViewData["FormReviewItem"] = rv_form_item;
            }
            rv_list = M_Detail.GetReviewByTopicCode(Session["TopicCode"] as string);
            
            isTrialable = M_Detail.GetTrialStatusByTopicAndDept(Session["TopicCode"] as string,tmp_dept_id);
            ViewData["isTrialable"] = isTrialable;

            foreach(Review rv_element in rv_list){
                rv_item_list = M_Detail.GetReviewItemByReviewID(rv_element.ID_Review);
                rv_element.Item = rv_item_list;
                rv_element.Date = rv_element.Date.StringToDigitDate();
                if(rv_element.ApprovedDate.AsNullIfEmpty() != null) rv_element.ApprovedDate = rv_element.ApprovedDate.StringToDigitDate();
                rv_element.Profile = (rv_element.User.AsNullIfEmpty() != null) ? M_Detail.getUserByID(rv_element.User) : new Models.User();
                rv_element.Approver = (rv_element.ApprovedBy.AsNullIfEmpty() != null) ? M_Detail.getUserByID(rv_element.ApprovedBy) : new Models.User();
                
                file_list = M_Detail.GetFileByID(rv_element.ID_Review, "Review");
                rv_element.FileList = file_list;
            }


            var Related = M_Detail.GetRelatedByID(Topic.Related);
            // var Related = M_Detail.GetRelatedByTopicID(tp_code);
            List<RelatedAlt> RelatedList = new List<RelatedAlt>();
            foreach (var prop in Related.GetType().GetProperties()){
                var prop_val = prop.GetValue(Related, null);
                var isRelated = ((int) prop_val == 1)? true : false;
                    if(isRelated){
                        var isResponsed = rv_list.Exists( e => e.Department == prop.Name);
                        var status = (isResponsed)? 1 : 0 ;
                        if((int) prop_val == 1){
                            RelatedList.Add(new RelatedAlt{name = prop.Name, status = status});
                        }
                    }
            }
            new_tp.RelatedListAlt = RelatedList;
            Session["Topic"] = ViewData["Topic"] = new_tp;
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
                    var rs_file_list = M_Detail.GetFileByID(response.ID, "Response");
                    response.FileList = rs_file_list;
                }
                var Related = M_Detail.GetRelatedByID(resubmit.Related);
                List<RelatedAlt> RelatedList = new List<RelatedAlt>();
                foreach (var prop in Related.GetType().GetProperties()){
                    var prop_val = prop.GetValue(Related, null);
                    var isRelated = ((int) prop_val == 1)? true : false;
                        if(isRelated){
                            var isResponsed = response_list.Exists( e => e.Department == prop.Name);
                            var status = (isResponsed)? 1 : 0 ;
                            if((int) prop_val == 1){
                                RelatedList.Add(new RelatedAlt{name = prop.Name, status = status});
                            }
                        }
                }
                resubmit.RelatedList = RelatedList;
            }
            return temp_resubmit_list;
        }

        public List<Trial> GetTrialListByTopicCode(){
            List<Trial> trial_list = new List<Trial>();
            List<FileItem> file_list = new List<FileItem>();
            trial_list = M_Detail.GetTrialByTopicCode(Topic.Code);
            foreach(Trial tr_element in trial_list){
                var tr_file_list = M_Detail.GetFileByID(tr_element.ID, "Trial");
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
                var tr_file_list = M_Detail.GetFileByID(cf_element.ID, "Confirm");
                cf_element.FileList = tr_file_list;
                cf_element.Date = cf_element.Date.StringToDigitDate();
                if(cf_element.ApprovedDate.AsNullIfEmpty() != null) cf_element.ApprovedDate = cf_element.ApprovedDate.StringToDigitDate();
                cf_element.Profile = (cf_element.User.AsNullIfEmpty() != null) ? M_Detail.getUserByID(cf_element.User) : new Models.User();
                cf_element.Approver = (cf_element.ApprovedBy.AsNullIfEmpty() != null) ?  M_Detail.getUserByID(cf_element.ApprovedBy) : new Models.User();
            }
            return Confirm_list;
        }

        public ActionResult RequestResubmit(string desc, string due_date){
            try{
                M_Detail.InsertResubmit(desc, due_date, (long) Session["RelatedID"], Topic.Code, (string)Session["User"], Topic.Status);
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitRelated(string IT,string MKT,string PC1,string PC2,string P1,string P2,string P3A,string P3M,string P4,string P5,string P6,string P7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3, string P5_ProcessDesign, string P6_ProcessDesign){
            try{
                Related related = new Related(IT,MKT,PC1,PC2,P1,P2,P3A,P3M,P4,P5,P6,P7,PE1,PE2,PE2_SMT,PE2_PCB,PE2_MT,PE1_Process,PE2_Process,PCH,QC_IN1,QC_IN2,QC_IN3,QC_FINAL1,QC_FINAL2,QC_FINAL3,QC_NFM1,QC_NFM2,QC_NFM3,QC1,QC2,QC3, P5_ProcessDesign, P6_ProcessDesign);
                Session["RelatedID"] = M_Detail.InsertRelated(related, Session["User"].ToString());
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitResponse(string desc, long resubmit_id){
            try{
                
                Session["ResponseID"] = M_Detail.InsertResponse(desc, Session["Department"].ToString(), Session["User"].ToString(), date, resubmit_id);
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitFileResponse(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var input_file_name = Path.GetFileName(date_ff);
                var server_path = Path.Combine("D:/File/Topic/" + input_file_name);
                file_item.file.SaveAs(server_path);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["ResponseID"], "Response", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
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
        public ActionResult SubmitTrial(string desc){
            try{
                Session["TrialID"] = M_Detail.InsertTrial(Topic.Code ,desc, Session["Department"].ToString(), Session["User"].ToString());
                // CheckAllTrialApproved(Session["TrialRelatedList"], C_TrialList);
                return Json(new { code = true, mail = "EmailReviewed" },JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateReview(){
            if(!CheckCurrentStatus()){
                throw new Exception("Status has changed");
            }
            var updated_rv = M_Detail.UpdateReview(Session["TopicCode"] as string, Session["User"].ToString(), Session["Department"].ToString());
            Session["ReviewID"] = updated_rv.ID_Review;
            Session["ReviewRev"] = updated_rv.Revision;
            return Json(new {code = 1,dept = Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTrial(string desc){
            if(!CheckCurrentStatus()){
                throw new Exception("Status has changed");
            }
            var updated_tr = M_Detail.UpdateTrial(Session["TopicCode"] as string, desc, Session["Department"].ToString(), Session["User"].ToString());
            Session["TrialID"] = updated_tr.ID;
            Session["TrialRev"] = updated_tr.Revision;
            return Json(new {code = 1, dept = Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateConfirm(string desc){
            if(!CheckCurrentStatus()){
                throw new Exception("Status has changed");
            }
            var updated_cf = M_Detail.UpdateConfirm(Session["TopicCode"] as string, desc, Session["Department"].ToString(), Session["User"].ToString());
            Session["ConfirmID"] = updated_cf.ID;
            Session["ConfirmRev"] = updated_cf.Revision;
            return Json(new {code = 1, dept = Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitFileTrial(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["TrialID"], "Trial", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitConfirm(string desc){
            try{
                Session["ConfirmID"] = M_Detail.InsertConfirm(Topic.Code ,desc, Session["Department"].ToString(), Session["User"].ToString());
                // CheckAllConfirmApproved(Session["ConfirmRelatedList"], C_ConfirmList);

                return Json(new { code = true ,mail = "EmailReviewed"},JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitFileConfirm(RawFile file_item){
            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Detail.InsertFile(file_item.file, (long) Session["ConfirmID"], "Confirm", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ApproveReview(long review_id){
            try{
                var mail = "";
                var dept = "";
                M_Detail.ApproveReview(review_id,(string) Session["User"]);
                if(Topic.Status == 7 && (int)Session["DepartmentID"] == 32 || (int)Session["DepartmentID"] == 33 && Topic.Type == "Internal"){ //PE_Process open review case
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 8);
                    M_Detail.UpdateTopicApproveRequest(Session["User"].ToString(),Topic.Code);
                    mail = "InformUser";
                }else if(Topic.Status == 8 && Session["Department"].ToString() == "QC1" || Session["Department"].ToString() == "QC2" || Session["Department"].ToString() == "QC3" ){
                    var rv_list = Session["ReviewList"] as List<Review>;
                    if(rv_list.Exists(e => e.Item.Exists(d => d.Type == 24 && d.Status == 1))){ //Request trial not exist
                        M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 9);
                        mail = "StartTrial";
                    }else{
                        M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 10);
                        mail = "StartConfirm";
                    }
                    M_Detail.UpdateTopicApproveReview(Session["User"].ToString(),Topic.Code);
                }else if(Topic.Status == 7 ||Topic.Status == 8){
                    List<RelatedAlt> rv_related_list = Session["ReviewRelatedList"] as List<RelatedAlt>;
                    var qc_audit = rv_related_list.Find(e => e.status == 0 && (e.name == "QC1" || e.name == "QC2" || e.name == "QC2"));
                    if(rv_related_list.Count(e => e.status == 0) == 1){
                        mail = "ReviewApproved";
                        dept = qc_audit.name;
                    }
                }
                return Json(new {code=true,mail,dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveTrial(long trial_id){
            try{
                var mail = "";
                var dept = "";
                M_Detail.ApproveTrial(trial_id,(string) Session["User"]);
                if(Topic.Status == 9 && Session["Department"].ToString() == "QC1" || Session["Department"].ToString() == "QC2" || Session["Department"].ToString() == "QC3" ){
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 10);
                    M_Detail.UpdateTopicApproveTrial(Session["User"].ToString(),Topic.Code);
                    mail = "StartConfirm";
                }else if(Topic.Status == 9){
                    List<RelatedAlt> rv_related_list = Session["ReviewRelatedList"] as List<RelatedAlt>;
                    var qc_audit = rv_related_list.Find(e => (e.name == "QC1" || e.name == "QC2" || e.name == "QC2"));
                    List<RelatedAlt> tr_related_list = Session["TrialRelatedList"] as List<RelatedAlt>;
                    if(!tr_related_list.Exists(e => e.status == 0)){
                        mail = "TrialApproved";
                        dept = qc_audit.name;
                    }
                }
                return Json(new {code=true,mail,dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveConfirm(long confirm_id){
            try{
                var mail = "";
                var dept = "ConfirmApproved";
                M_Detail.ApproveConfirm(confirm_id,(string) Session["User"]);
                if(Topic.Status == 10 && Session["Department"].ToString() == "QC1" || Session["Department"].ToString() == "QC2" || Session["Department"].ToString() == "QC3" ){
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 11);
                    M_Detail.UpdateTopicApproveClose(Session["User"].ToString(),Topic.Code);
                }else if(Topic.Status == 10){
                    List<RelatedAlt> cf_related_list = Session["ConfirmRelatedList"] as List<RelatedAlt>;
                    var qc_audit = cf_related_list.Find(e => e.status == 0);
                    if(cf_related_list.Count(e => e.status == 0) == 1){
                        mail = "ConfirmApproved";
                        dept = qc_audit.name;
                    }
                }
                return Json(new {code=true,mail,dept}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult DownloadFile(){
            var r = Request.Form["load"];
            var temp = r.Split('^');
            //string filePath = "km0024.txt";
            // string fullName = Server.MapPath("~/upload/");
            string filePath = temp[0];
            string fullName = Server.MapPath("~/topic_file/");
            // string fullName = "\\\\172.27.170.19\\File\\Topic";
            // string fullName = "D:/File/Topic/";
            byte[] fileBytes = GetFile(fullName + filePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, temp[1]);
        }

        byte[] GetFile(string s){
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

        public bool CheckAllReviewApproved(List<RelatedAlt> rv_related_list,List<Review> review_list){
            if(Topic.Status == 8){
                if(!rv_related_list.Exists(e => e.status == 0)){
                    if(!review_list.Exists(rv => rv.Status == 3)){
                        this.UpdateTopicStatus(Session["TopicCode"] as string, 9);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckAllTrialApproved(List<RelatedAlt> tr_related_list,List<Trial> trial_list){
            if(Topic.Status == 9){
                if(!tr_related_list.Exists(e => e.status == 0)){
                    if(!trial_list.Exists(rv => rv.Status == 3)){
                        this.UpdateTopicStatus(Session["TopicCode"] as string, 10);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckAllConfirmApproved(List<RelatedAlt> cf_related_list, List<Confirm> confirm_list){
            if(Topic.Status == 10){
                if(!cf_related_list.Exists(e => e.status == 0)){
                    if(!confirm_list.Exists(rv => rv.Status == 3)){
                        this.UpdateTopicStatus(Session["TopicCode"] as string, 11);
                        return true;
                    }
                }
            }
            return false;
        }

        public List<RelatedAlt> FilterReviewRelated(Related related, List<Review> review_list) {
            List<RelatedAlt> ReviewRelatedList = new List<RelatedAlt>();
            foreach (var prop in related.GetType().GetProperties()){
                var prop_val = prop.GetValue(related, null);
                var isRelated = ((int) prop_val == 1)? true : false;
                if(isRelated){
                    var isResponsed = review_list.Exists( e => e.Department == prop.Name);
                    var status = (isResponsed)? 1 : 0 ;
                    if((int) prop_val == 1){
                        ReviewRelatedList.Add(new RelatedAlt{name = prop.Name, status = status});
                    }
                }
            }
            return ReviewRelatedList;
        }

        public List<RelatedAlt> FilterTrialRelated(List<Review> review_list, List<Trial> trial_list){
            List<RelatedAlt> TrialRelatedList = new List<RelatedAlt>();
            foreach (var review in review_list){
                if(review.Item.Exists(e => e.Type == 24 && e.Status == 1)){
                    var isResponsed = trial_list.Exists( e => e.Department == review.Department);
                    var status = (isResponsed)? 1 : 0 ;
                    TrialRelatedList.Add(new RelatedAlt{name = review.Department, status = status});
                }
            }
            return TrialRelatedList;
        }

        public List<RelatedAlt> FilterConfirmRelated(Related related, List<Confirm> confirm_list) {
            List<RelatedAlt> ConfirmRelatedList = new List<RelatedAlt>();
            foreach (var prop in related.GetType().GetProperties()){
                var prop_val = prop.GetValue(related, null);
                var isRelated = ((int) prop_val == 1)? true : false;
                if(isRelated){
                    var isResponsed = confirm_list.Exists( e => e.Department == prop.Name);
                    var status = (isResponsed)? 1 : 0 ;
                    if((int) prop_val == 1){
                        ConfirmRelatedList.Add(new RelatedAlt{name = prop.Name, status = status});
                    }
                }
            }
            return ConfirmRelatedList;
        }

        public bool CheckAllReviewBeforeApprove(string topic_code){
            var dept = (string) Session["Department"];
            var result = true;
            if(dept == "PE1_Process" || dept == "PE1_Process" || dept == "QC1" || dept == "QC2" || dept == "QC3"){
                List<Review> rv_list = M_Detail.CheckAllReviewBeforeApprove(topic_code);
                TopicAlt topic = Session["Topic"] as TopicAlt;
                    if(rv_list.Count != 0){
                        if(topic.Type == "Internal" && (string) Session["Department"] == "PE1_Process" || (string)  Session["Department"] == "PE2_Process" ){
                            var rv_pe_process = rv_list.Find(x => x.Department == "PE1_Process" || x.Department == "PE2_Process");
                            if(rv_pe_process != null) rv_list.Remove(rv_pe_process);
                            result = (rv_list.Count != 0) ? !rv_list.Exists(e => e.Status == 3 && e.Department != "PE1_Process" || e.Department != "PE2_Process") : true;
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
            if(dept == "QC1" || dept == "QC2" || dept == "QC3"){
                List<Trial> result = M_Detail.CheckAllTrialBeforeApprove(topic_code);
                return (result.Count != 0) ? !result.Exists(e => e.Status == 3) : false;
            }else{
                return true;
            }
        }

        public bool CheckAllConfirmBeforeApprove(string topic_code){
            var dept = (string) Session["Department"];
            if(dept == "QC1" || dept == "QC2" || dept == "QC3"){
                List<Confirm> result = M_Detail.CheckAllConfirmBeforeApprove(topic_code);
                return (result.Count != 0) ? !result.Exists(e => e.Status == 3) : false;
            }else{
                return true;
            }
        }

        public bool RejectTopic(string topic_code,string desc){
            try{
                this.UpdateTopicStatus(topic_code,12);
                M_Detail.RejectTopic(topic_code,desc,(string) Session["User"], (string) Session["Department"]);
                return true;
            }catch(Exception err){
                return false;
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
                List<RelatedAlt> temp_related_rv_list = (List<RelatedAlt>) Session["ReviewRelatedList"];
                List<Review> temp_rv_list = (List<Review>) Session["ReviewList"];
                if(temp_rv_list.Exists(rv => rv.Item.Exists(rv_item => rv_item.Type == 26 && rv_item.Status == 1))){
                    string[] pt_list = {"P1","P2","P3A","P3M","P4","P5","P6","P7"};
                    string[] qcf_list = {"QC_FINAL1", "QC_FINAL2", "QC_FINAL3"};
                    var related_pt_qcf = temp_related_rv_list.FindAll(e => e.status == 1 && (pt_list.Contains(e.name) || qcf_list.Contains(e.name)));
                    return Json(new {status="success",data=related_pt_qcf.Select(s => s.name).ToList()}, JsonRequestBehavior.AllowGet);
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

        public ActionResult ApproveTopic(){
            var temp_topic = (TopicAlt) Session["Topic"];
            M_Detail.ApproveTopicByCode(temp_topic.Code, Session["User"].ToString());
            return Json(new {status="success"}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTopicRelated(){
            M_Detail.UpdateTopicRelated(Session["TopicCode"].ToString(), (long) Session["RelatedID"]);
            return Json(new {status="success"}, JsonRequestBehavior.AllowGet);
        }


    }
}