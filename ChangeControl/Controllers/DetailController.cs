using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using DateHelper;
using StringHelper;

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
                return RedirectToAction("Index", "LogIn");
            }
            if (Session["User"].ToString() == null){
                Session["url"] = "Detail";
                return RedirectToAction("Index", "Login");
            }

            if(id != null){
                Session["TopicCode"] = id;
                Session["TopicCode"] = id;
            }else{
                Session["TopicCode"] = M_Detail.GetFirstTopic();
            }

            Topic = M_Detail.GetTopicByCode(Session["TopicCode"] as string as string);
            Topic.Profile = M_Detail.getUserByID(Topic.User_insert);
            Session["TopicCode"] = Topic.Code;
            Session["TopicID"] = Topic.ID;

            ViewBag.ResubmitList = this.GetResubmitListByTopicID();
            Session["ReviewList"]  = ViewBag.ReviewList = this.GetReviewListByTopicID();
            Session["TrialList"]  = ViewBag.TrialList = this.GetTrialListByTopicCode();
            Session["ConfirmList"] = ViewBag.ConfirmList = this.GetConfirmListByTopicCode();

            var DepartmentGroup = M_Detail.GetDepartmentGroup(); //Get raw group of departments
            List<DepartmentList> departmentList = new List<DepartmentList>();
            foreach(string GroupName in DepartmentGroup){
                List<Department> department = new List<Department>();
                department = M_Detail.GetDepartmentByGroup(GroupName);
                departmentList.Add(new DepartmentList(){Name = GroupName.Replace(" ", "_"), Department = department}); //Convert raw group into department list for radio
            }
            ViewData["DepartmentList"] = departmentList;

            var Related = M_Detail.GetRelatedByID(Topic.Related);
            List<RelatedAlt> TrialRelatedList = new List<RelatedAlt>();
            ViewData["ReviewRelatedList"] = Session["ReviewRelatedList"] = FilterReviewRelated(Related, Session["ReviewList"] as List<Review>);
            ViewData["TrialRelatedList"] = Session["TrialRelatedList"] = ViewBag.TrialRelatedList = FilterTrialRelated(Session["ReviewList"] as List<Review>, Session["TrialList"] as List<Trial>);
            ViewData["ConfirmRelatedList"] = Session["ConfirmRelatedList"] = ViewBag.ConfirmRelatedList = FilterConfirmRelated(Related,Session["ConfirmList"] as List<Confirm>);

            // if(CheckAllReviewApproved(Session["ReviewRelatedList"], C_ReviewList) || CheckAllTrialApproved(Session["TrialRelatedList"], C_TrialList) || CheckAllConfirmApproved(Session["ConfirmRelatedList"], C_ConfirmList)){
            //     return RedirectToAction("Index", "Detail" , new {@id=topic_code});
            // }
            
            return View();
        }

        public ActionResult SubmitReview(){
            // long temp_Session["TopicID"] = Topic.ID;
            var mail = "";
            if(Topic.Status == 7 && Topic.Type == "External"){
                M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 8);
                mail = "InformUser";
                // C_Mail.Generate("InformUser",topic_code);
            }
            Session["ReviewID"] = M_Detail.InsertReview(Session["TopicCode"] as string, Session["User"].ToString(), Session["Department"].ToString());
            CheckAllReviewApproved(Session["ReviewRelatedList"] as List<RelatedAlt>, Session["ReviewList"] as List<Review>);

            return Json(new {code=1,mail,dept=Session["Department"].ToString()}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateReview(){
            Session["ReviewID"] = M_Detail.UpdateReview(Session["TopicCode"] as string, Session["User"].ToString(), Session["Department"].ToString());
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
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
            ViewData["Topic"] = new_tp;
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
        public ActionResult SubmitRelated(string IT,string MKT,string PC1,string PC2,string P1,string P2,string P3A,string P3M,string P4,string P5,string P6,string P7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH1,string PCH2,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3){
            try{
                Related related = new Related(IT,MKT,PC1,PC2,P1,P2,P3A,P3M,P4,P5,P6,P7,PE1,PE2,PE2_SMT,PE2_PCB,PE2_MT,PE1_Process,PE2_Process,PCH1,PCH2,QC_IN1,QC_IN2,QC_IN3,QC_FINAL1,QC_FINAL2,QC_FINAL3,QC_NFM1,QC_NFM2,QC_NFM3,QC1,QC2,QC3);
                Session["RelatedID"] = M_Detail.InsertRelated(related);
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
                }else if(status == 11){
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
                return Json(new { code = true },JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateTrial(string desc){
            Session["TrialID"] = M_Detail.UpdateTrial(Session["TopicCode"] as string, desc, Session["Department"].ToString(), Session["User"].ToString());
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateConfirm(string desc){
            Session["ConfirmID"] = M_Detail.UpdateConfirm(Session["TopicCode"] as string, desc, Session["Department"].ToString(), Session["User"].ToString());
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
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

                return Json(new { code = true },JsonRequestBehavior.AllowGet);
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
                M_Detail.ApproveReview(review_id,(string) Session["User"]);
                // if(Topic.Status == 7 && Topic.Type == "Internal" && (int)Session["DepartmentID"] == 32 || (int)Session["DepartmentID"] == 33 ){ //PE_Process open review case
                if(Topic.Status == 7 && (int)Session["DepartmentID"] == 32 || (int)Session["DepartmentID"] == 33 ){ //PE_Process open review case
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 8);
                    M_Detail.UpdateTopicApproveRequest(Session["User"].ToString(),Topic.Code);
                    mail = "InformUser";
                }else if(Topic.Status == 8 && Session["Department"].ToString() == "QC1" || Session["Department"].ToString() == "QC2" || Session["Department"].ToString() == "QC3" ){
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 9);
                    M_Detail.UpdateTopicApproveReview(Session["User"].ToString(),Topic.Code);
                }
                return Json(new {code=true,mail}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveTrial(long trial_id){
            try{
                M_Detail.ApproveTrial(trial_id,(string) Session["User"]);
                if(Topic.Status == 9 && Session["Department"].ToString() == "QC1" || Session["Department"].ToString() == "QC2" || Session["Department"].ToString() == "QC3" ){
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 10);
                    M_Detail.UpdateTopicApproveTrial(Session["User"].ToString(),Topic.Code);
                }
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveConfirm(long confirm_id){
            try{
                M_Detail.ApproveConfirm(confirm_id,(string) Session["User"]);
                if(Topic.Status == 10 && Session["Department"].ToString() == "QC1" || Session["Department"].ToString() == "QC2" || Session["Department"].ToString() == "QC3" ){
                    M_Detail.UpdateTopicStatus(Session["TopicCode"] as string, 11);
                    M_Detail.UpdateTopicApproveClose(Session["User"].ToString(),Topic.Code);
                }
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
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
                if(review.Item.Exists(e => e.Type == 24)){
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
    }
}