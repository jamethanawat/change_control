using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace ChangeControl.Controllers{
    public class DetailController : Controller{
        // GET: Detail
        private DetailModel M_Detail;
        private RequestModel M_Req;
        public static TopicAlt Topic;
        public static string topic_code;
        public static long related_id, response_id, trial_id, review_id, confirm_id;
        public static long? file_id;
       
        public DetailController(){
            M_Detail = new DetailModel();
            M_Req = new RequestModel();
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

        public ActionResult Index(string id){
            if ((string)(Session["User"]) == null){
                Session["url"] = "Detail";
                return RedirectToAction("Index", "Login");
            }

            if(id != null){
                topic_code = id;
                Session["TopicID"] = id;
            }else{
                topic_code = M_Detail.GetFirstTopic();
            }

            ViewData["ViewReviewItem"] = this.GetReviewListByTopicID();
            ViewData["ResubmitList"] = this.GetResubmitListByTopicID();
            ViewData["TrialList"] = this.GetTrialListByTopicID();
            ViewData["ConfirmList"] = this.GetConfirmListByTopicID();

            var DepartmentGroup = M_Detail.GetDepartmentGroup(); //Get raw group of departments
            List<DepartmentList> departmentList = new List<DepartmentList>();
            foreach(string GroupName in DepartmentGroup){
                List<Department> department = new List<Department>();
                department = M_Detail.GetDepartmentByGroup(GroupName);
                departmentList.Add(new DepartmentList(){Name = GroupName.Replace(" ", "_"), Department = department}); //Convert raw group into department list for radio
            }
            ViewData["DepartmentList"] = departmentList;
            return View();
        }

        public ActionResult InsertReview(){
            long temp_topic_id = Topic.ID;
            var temp_user = Session["User"].ToString();
            var temp_department = Session["Department"].ToString();
            if(Topic.Status == 7 && Topic.Topic_type == "Internal" && (int)Session["DepartmentID"] == 32 || (int)Session["DepartmentID"] == 33 ){
                M_Detail.UpdateTopicStatus(temp_topic_id,8);
                M_Detail.UpdateTopicApproveRequest(Session["User"].ToString(),Topic.ID);
            }else if(Topic.Status == 7 && Topic.Topic_type == "External"){
                M_Detail.UpdateTopicStatus(temp_topic_id,8);
            }
            review_id = M_Detail.InsertReview(temp_topic_id, null, temp_user, temp_department);
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InsertReviewItem(string status, string description, int id){
            M_Detail.InsertReviewItem(status, description, id, review_id);
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
                M_Detail.InsertFile(file_item.file, review_id, "Review", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateFileDesc(string desc){
            var fileID = file_id;
            M_Detail.UpdateFileDesc(file_id,desc);
            return Json(true);
        }

        public ActionResult GetReviewByTopicID(int TopicID){
            var Review = M_Detail.GetReviewByTopicID(TopicID);
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

            Topic = M_Detail.GetTopicByID(topic_code);
            var temp_topic = Topic;
            var tc_file_list = M_Detail.GetFileByID(Topic.ID, "Topic");
            temp_topic.FileList = tc_file_list;
            var temp_topid_id  = Topic.ID;

            ViewData["FormReviewItem"] = null;
            var temp_department = (string) Session["Department"];

            // Set review item for each department
            if(temp_department != "PE1_Process" && temp_department != "PE1_Process" && temp_department != "QC"){
                var rv_form_item = M_Detail.GetReviewItemByDepartment((int)Session["DepartmentID"]);
                ViewData["FormReviewItem"] = rv_form_item;
            }
            var temp_department_id = (int)Session["DepartmentID"];
            rv_list = M_Detail.GetReviewByTopicID(temp_topid_id);
            
            isTrialable = M_Detail.GetTrialStatusByTopicAndDept(Topic.ID,(int) Session["DepartmentID"]);
            ViewData["isTrialable"] = isTrialable;

            foreach(Review rv_element in rv_list){
                rv_item_list = M_Detail.GetReviewItemByReviewID(rv_element.ID_Review);
                rv_element.Item = rv_item_list;
                DateTime temp_date = DateHelper.StringToDateTime(rv_element.Date);
                rv_element.Date = temp_date.ToString("d MMMM yyyy");
                rv_element.Profile = M_Detail.getUserByID(rv_element.User);
                rv_element.Profile.Name = StringHelper.UppercaseFirst(rv_element.Profile.Name);
                rv_element.Profile.SurName = StringHelper.UppercaseFirst(rv_element.Profile.SurName);
                
                file_list = M_Detail.GetFileByID(rv_element.ID_Review, "Review");
                rv_element.FileList = file_list;
            }


            var Related = M_Detail.GetRelatedByTopicID(temp_topid_id);
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
            temp_topic.RelatedListAlt = RelatedList;
            ViewData["Topic"] = temp_topic;
            return rv_list;
        }

        public List<Resubmit> GetResubmitListByTopicID(){
            var temp_resubmit_list = M_Detail.GetResubmitByTopicID(Topic.ID);
            foreach(Resubmit resubmit in temp_resubmit_list){
                List<Response> response_list = new List<Response>();

                DateTime temp_date = DateHelper.StringToDateTime(resubmit.Date);
                DateTime temp_due_date = DateHelper.StringToDateTime2(resubmit.DueDate);
                resubmit.Date = temp_date.ToString("d MMMM yyyy");
                resubmit.DueDate = temp_due_date.ToString("d MMMM yyyy");

                var result = M_Detail.GetResponseByResubmitID(resubmit.ID);
                foreach(var response in result){
                    DateTime temp_res_date = DateHelper.StringToDateTime(response.Date);
                    response.Date = temp_res_date.ToString("d MMMM yyyy");
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

        public List<Trial> GetTrialListByTopicID(){
            List<Trial> trial_list = new List<Trial>();
            List<FileItem> file_list = new List<FileItem>();
            trial_list = M_Detail.GetTrialByTopicID(Topic.ID);
            foreach(Trial trial in trial_list){
                var tr_file_list = M_Detail.GetFileByID(trial.ID, "Trial");
                trial.FileList = tr_file_list;
                trial.Profile = M_Detail.getUserByID(trial.User);
                trial.Profile.Name = StringHelper.UppercaseFirst(trial.Profile.Name);
                trial.Profile.SurName = StringHelper.UppercaseFirst(trial.Profile.SurName);
                var temp_date = DateHelper.StringToDateTime(trial.Date);
                trial.Date = temp_date.ToString("d MMMM yyyy");
            }
            return trial_list;
        }

        public List<Confirm> GetConfirmListByTopicID(){
            List<Confirm> Confirm_list = new List<Confirm>();
            List<FileItem> file_list = new List<FileItem>();
            Confirm_list = M_Detail.GetConfirmByTopicID(Topic.ID);
            foreach(Confirm Confirm in Confirm_list){
                var tr_file_list = M_Detail.GetFileByID(Confirm.ID, "Confirm");
                Confirm.FileList = tr_file_list;
                Confirm.Profile = M_Detail.getUserByID(Confirm.User);
                Confirm.Profile.Name = StringHelper.UppercaseFirst(Confirm.Profile.Name);
                Confirm.Profile.SurName = StringHelper.UppercaseFirst(Confirm.Profile.SurName);
                var temp_date = DateHelper.StringToDateTime(Confirm.Date);
                Confirm.Date = temp_date.ToString("d MMMM yyyy");
            }
            return Confirm_list;
        }

        public ActionResult RequestResubmit(string desc, string due_date){
            try{
                M_Detail.InsertResubmit(desc, due_date, related_id, Topic.ID, (string)Session["User"], Topic.Status);
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitRelated(string IT,string MKT,string PC1,string PC2,string PT1,string PT2,string PT3A,string PT3M,string PT4,string PT5,string PT6,string PT7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH1,string PCH2,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3){
            try{
                Related related = new Related(IT,MKT,PC1,PC2,PT1,PT2,PT3A,PT3M,PT4,PT5,PT6,PT7,PE1,PE2,PE2_SMT,PE2_PCB,PE2_MT,PE1_Process,PE2_Process,PCH1,PCH2,QC_IN1,QC_IN2,QC_IN3,QC_FINAL1,QC_FINAL2,QC_FINAL3,QC_NFM1,QC_NFM2,QC_NFM3,QC1,QC2,QC3);
                related_id = M_Detail.InsertRelated("R" + Topic.Related.Trim().Substring(1), related);
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitResponse(string desc, long resubmit_id){
            try{
                var temp_user = Session["User"].ToString();
                var temp_department = Session["Department"].ToString();
                response_id = M_Detail.InsertResponse(desc, temp_department, temp_user, date, resubmit_id);
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
                M_Detail.InsertFile(file_item.file, response_id, "Response", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTopicStatus(int topic_id, int status){
            try{
                M_Detail.UpdateTopicStatus(topic_id,status);
                if(status == 9){
                    M_Detail.UpdateTopicApproveReview(Session["User"].ToString(), Topic.ID);
                }else if(status == 10){
                    M_Detail.UpdateTopicApproveTrial(Session["User"].ToString(), Topic.ID);
                }else if(status == 11){
                    M_Detail.UpdateTopicApproveClose(Session["User"].ToString(), Topic.ID);
                }
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitTrial(string desc){
            try{
                var temp_user = Session["User"].ToString();
                var temp_department = Session["Department"].ToString();
                trial_id = M_Detail.InsertTrial(Topic.ID ,desc, temp_department, temp_user);
                return Json(new { code = true },JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
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
                M_Detail.InsertFile(file_item.file, trial_id, "Trial", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

                [HttpPost]
        public ActionResult SubmitConfirm(string desc){
            try{
                var temp_user = Session["User"].ToString();
                var temp_department = Session["Department"].ToString();
                confirm_id = M_Detail.InsertConfirm(Topic.ID ,desc, temp_department, temp_user);
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
                M_Detail.InsertFile(file_item.file, confirm_id, "Confirm", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ReviewApprove(long rv_id){
            try{
                M_Detail.ReviewApprove(review_id,(string) Session["User"]);
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

    }
}