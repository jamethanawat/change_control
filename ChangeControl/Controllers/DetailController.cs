using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Reflection;

namespace ChangeControl.Controllers
{
    public class DetailController : Controller
    {
        // GET: Detail
        private DetailModel M_Detail;
        private RequestModel M_Req;
        public static TopicAlt Topic;
        public static string TopicID;
        public static long related_id;
        public static long response_id;
        public static long trial_id;
        public List<Review> ReviewList = new List<Review>();
        public static long review_id;
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
                DetailController.TopicID = id;
                Session["TopicID"] = id;
            }else{
                DetailController.TopicID = M_Detail.GetFirstTopic();
            }

            ViewData["ViewReviewItem"] = this.GetReviewListByTopicID();

            var ResubmitList = M_Detail.GetResubmitByTopicID(Topic.ID);
            foreach(Resubmit Resubmit in ResubmitList){
                List<Response> ResponseList = new List<Response>();

                DateTime temp_date = DateHelper.StringToDateTime(Resubmit.Date);
                DateTime temp_due_date = DateHelper.StringToDateTime2(Resubmit.DueDate);
                Resubmit.Date = temp_date.ToString("d MMMM yyyy");
                Resubmit.DueDate = temp_due_date.ToString("d MMMM yyyy");

                var result = M_Detail.GetResponseByResubmitID(Resubmit.ID);
                foreach(var Response in result){
                    DateTime temp_res_date = DateHelper.StringToDateTime(Response.Date);
                    Response.Date = temp_res_date.ToString("d MMMM yyyy");
                }
                if(result != null){
                    ResponseList = result;
                }
                Resubmit.Responses = ResponseList;
                foreach(var Response in Resubmit.Responses){
                    var ResponseFileList = M_Detail.GetFileByID(Response.ID, "Response");
                    Response.FileList = ResponseFileList;
                }
                var Related = M_Detail.GetRelatedByID(Resubmit.Related);
                List<RelatedAlt> RelatedList = new List<RelatedAlt>();
                foreach (var prop in Related.GetType().GetProperties()){
                    var prop_val = prop.GetValue(Related, null);
                    var isRelated = ((int) prop_val == 1)? true : false;
                        if(isRelated){
                            var isResponsed = ResponseList.Exists( e => e.Department == prop.Name);
                            var status = (isResponsed)? 1 : 0 ;
                            if((int) prop_val == 1){
                                RelatedList.Add(new RelatedAlt{name = prop.Name, status = status});
                            }
                        }
                }
                Resubmit.RelatedList = RelatedList;
            }
            
            ViewData["ResubmitList"] = ResubmitList;
            
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
            long temp_topic_id = DetailController.Topic.ID;
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
            TopicAlt temp_topic = DetailController.Topic;
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
            var fileID = DetailController.file_id;
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
            List<ReviewItem> ReviewItemList = new List<ReviewItem>();
            List<FileItem> FileList = new List<FileItem>();
            Topic = M_Detail.GetTopicByID(TopicID);
            var temp_topic = Topic;
            var TopicFileList = M_Detail.GetFileByID(Topic.ID, "Topic");
            temp_topic.FileList = TopicFileList;
            var temp_topid_id  = Topic.ID;

            
            ViewData["FormReviewItem"] = null;
            
            var temp_department = (string) Session["Department"];

            // Set review item for each department
            if(temp_department != "PE1_Process" && temp_department != "PE1_Process" && temp_department != "QC"){
                var formReviewItem = M_Detail.GetReviewItemByDepartment((int)Session["DepartmentID"]);
                ViewData["FormReviewItem"] = formReviewItem;
            }
            var temp_department_id = (int)Session["DepartmentID"];
            ReviewList = M_Detail.GetReviewByTopicID(temp_topid_id);
            
            isTrialable = M_Detail.GetTrialStatusByTopicAndDept(Topic.ID,(int) Session["DepartmentID"]);
            ViewData["isTrialable"] = isTrialable;

            foreach(Review ReviewElement in ReviewList){
                ReviewItemList = M_Detail.GetReviewItemByReviewID(ReviewElement.ID_Review);
                ReviewElement.Item = ReviewItemList;
                DateTime temp_date = DateHelper.StringToDateTime(ReviewElement.Date);
                ReviewElement.Date = temp_date.ToString("d MMMM yyyy");
                ReviewElement.Profile = M_Detail.getUserByID(ReviewElement.User);
                ReviewElement.Profile.Name = StringHelper.UppercaseFirst(ReviewElement.Profile.Name);
                ReviewElement.Profile.SurName = StringHelper.UppercaseFirst(ReviewElement.Profile.SurName);
                
                FileList = M_Detail.GetFileByID(ReviewElement.ID_Review, "Review");
                ReviewElement.FileList = FileList;
            }


            var Related = M_Detail.GetRelatedByTopicID(temp_topid_id);
            List<RelatedAlt> RelatedList = new List<RelatedAlt>();
            foreach (var prop in Related.GetType().GetProperties()){
                var prop_val = prop.GetValue(Related, null);
                var isRelated = ((int) prop_val == 1)? true : false;
                    if(isRelated){
                        var isResponsed = ReviewList.Exists( e => e.Department == prop.Name);
                        var status = (isResponsed)? 1 : 0 ;
                        if((int) prop_val == 1){
                            RelatedList.Add(new RelatedAlt{name = prop.Name, status = status});
                        }
                    }
            }
            temp_topic.RelatedListAlt = RelatedList;
            ViewData["Topic"] = temp_topic;
            return ReviewList;
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
                }else if(status == 6){
                    M_Detail.UpdateTopicApproveTrial(Session["User"].ToString(), Topic.ID);
                }
                    // _detailModel.UpdateTopicApproveClose(Session["User"].ToString(), Topic.ID);
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

    }
}