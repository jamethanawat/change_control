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
        public static TopicAlt Topic;
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
        private static string temp_user , temp_department, topic_code = "";
        private static long topic_id;

        public ActionResult Index(string id){
            if((string)(Session["User"]) == null){
                return RedirectToAction("Index", "Home");
            }
            temp_user = Session["User"].ToString();
            temp_department = Session["Department"].ToString();
            if (temp_user == null){
                Session["url"] = "Detail";
                return RedirectToAction("Index", "Login");
            }

            if(id != null){
                topic_code = id;
                Session["TopicCode"] = id;
            }else{
                topic_code = M_Detail.GetFirstTopic();
            }

            Topic = M_Detail.GetTopicByCode(topic_code);
            Topic.Profile = M_Detail.getUserByID(Topic.User_insert);
            topic_code = Topic.Code;
            topic_id = Topic.ID;



            ViewData["ViewReviewItem"] = this.GetReviewListByTopicID();
            ViewData["ResubmitList"] = this.GetResubmitListByTopicID();
            ViewData["TrialList"] = this.GetTrialListByTopicCode();
            ViewData["ConfirmList"] = this.GetConfirmListByTopicCode();

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

        public ActionResult SubmitReview(){
            // long temp_topic_id = Topic.ID;
            var temp_user = Session["User"].ToString();
            var temp_department = Session["Department"].ToString();
            if(Topic.Status == 7 && Topic.Type == "Internal" && (int)Session["DepartmentID"] == 32 || (int)Session["DepartmentID"] == 33 ){
                M_Detail.UpdateTopicStatus(topic_code, 8);
                M_Detail.UpdateTopicApproveRequest(Session["User"].ToString(),Topic.Code);
            }else if(Topic.Status == 7 && Topic.Type == "External"){
                M_Detail.UpdateTopicStatus(topic_code, 8);
            }
            review_id = M_Detail.InsertReview(topic_code, temp_user, temp_department);
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateReview(){
            review_id = M_Detail.UpdateReview(topic_code, temp_user, temp_department);
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitReviewItem(string status, string description, int id){
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
            rv_list = M_Detail.GetReviewByTopicCode(topic_code);
            
            isTrialable = M_Detail.GetTrialStatusByTopicAndDept(topic_code,tmp_dept_id);
            ViewData["isTrialable"] = isTrialable;

            foreach(Review rv_element in rv_list){
                rv_item_list = M_Detail.GetReviewItemByReviewID(rv_element.ID_Review);
                rv_element.Item = rv_item_list;
                rv_element.Date = rv_element.Date.StringToDateTime();
                rv_element.Profile = M_Detail.getUserByID(rv_element.User);
                rv_element.ApprovedDate = rv_element.ApprovedDate.StringToDateTime();
                rv_element.Approver = M_Detail.getUserByID(rv_element.ApprovedBy);
                
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
                resubmit.Date = resubmit.Date.StringToDateTime();
                resubmit.DueDate = resubmit.DueDate.StringToDateTime3();
                resubmit.Profile = M_Detail.getUserByID(resubmit.User);

                var result = M_Detail.GetResponseByResubmitID(resubmit.ID);
                foreach(var response in result){
                    response.Date = response.Date.StringToDateTime();
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
            foreach(Trial trial in trial_list){
                var tr_file_list = M_Detail.GetFileByID(trial.ID, "Trial");
                trial.FileList = tr_file_list;
                trial.Profile = M_Detail.getUserByID(trial.User);
                trial.Date =  trial.Date.StringToDateTime();
                trial.ApprovedDate = trial.ApprovedDate.StringToDateTime();
                trial.Approver = M_Detail.getUserByID(trial.ApprovedBy);
            }
            return trial_list;
        }

        public List<Confirm> GetConfirmListByTopicCode(){
            List<Confirm> Confirm_list = new List<Confirm>();
            List<FileItem> file_list = new List<FileItem>();
            Confirm_list = M_Detail.GetConfirmByTopicCode(Topic.Code);
            foreach(Confirm Confirm in Confirm_list){
                var tr_file_list = M_Detail.GetFileByID(Confirm.ID, "Confirm");
                Confirm.FileList = tr_file_list;
                Confirm.Profile = M_Detail.getUserByID(Confirm.User);
                Confirm.Date = Confirm.Date.StringToDateTime();
                Confirm.ApprovedDate = Confirm.ApprovedDate.StringToDateTime();
                Confirm.Approver = M_Detail.getUserByID(Confirm.ApprovedBy);
            }
            return Confirm_list;
        }

        public ActionResult RequestResubmit(string desc, string due_date){
            try{
                M_Detail.InsertResubmit(desc, due_date, related_id, Topic.Code, (string)Session["User"], Topic.Status);
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitRelated(string IT,string MKT,string PC1,string PC2,string P1,string P2,string P3A,string P3M,string P4,string P5,string P6,string P7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH1,string PCH2,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3){
            try{
                Related related = new Related(IT,MKT,PC1,PC2,P1,P2,P3A,P3M,P4,P5,P6,P7,PE1,PE2,PE2_SMT,PE2_PCB,PE2_MT,PE1_Process,PE2_Process,PCH1,PCH2,QC_IN1,QC_IN2,QC_IN3,QC_FINAL1,QC_FINAL2,QC_FINAL3,QC_NFM1,QC_NFM2,QC_NFM3,QC1,QC2,QC3);
                related_id = M_Detail.InsertRelated(related);
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitResponse(string desc, long resubmit_id){
            try{
                
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
                trial_id = M_Detail.InsertTrial(Topic.Code ,desc, temp_department, temp_user);
                return Json(new { code = true },JsonRequestBehavior.AllowGet);
            }catch (Exception ex){
                return Json(new { code = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateTrial(string desc){
            review_id = M_Detail.UpdateTrial(topic_code, desc, temp_department, temp_user);
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateConfirm(string desc){
            review_id = M_Detail.UpdateConfirm(topic_code, desc, temp_department, temp_user);
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
                M_Detail.InsertFile(file_item.file, trial_id, "Trial", file_item.description, Session["User"]);
            }
            return Json(new {code=1}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SubmitConfirm(string desc){
            try{
                confirm_id = M_Detail.InsertConfirm(Topic.Code ,desc, temp_department, temp_user);
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
        public ActionResult ApproveReview(long review_id){
            try{
                M_Detail.ApproveReview(review_id,(string) Session["User"]);
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveTrial(long trial_id){
            try{
                M_Detail.ApproveTrial(trial_id,(string) Session["User"]);
                return Json(new {code=true}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {code=false}, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ApproveConfirm(long confirm_id){
            try{
                M_Detail.ApproveConfirm(confirm_id,(string) Session["User"]);
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
            //string fullName = Server.MapPath("~/upload/");
            string filePath = temp[0];
            string fullName = "D:/File/Topic/";
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

    }
}