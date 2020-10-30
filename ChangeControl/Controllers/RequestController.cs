using ChangeControl.Models;
using ChangeControl.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Text.Json;
namespace ChangeControl.Controllers{
    public class RequestController : ChangeControlController{
        // GET: Request
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        private DetailModel M_Detail;
        private HomeModel M_Home;
        private RequestModel M_Req;
        public List<Review> ReviewList = new List<Review>();
        public RequestController(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            M_Detail = new DetailModel();
            M_Req = new RequestModel();
            M_Home = new HomeModel();
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

        // public class C_Inherit_Home : HomeController{


        // }
        public List<GetID> topic_detail = new List<GetID>();
        private string date = DateTime.Now.ToString("yyyyMMddHHmmss");
        private string date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        public ActionResult Index(string ID){
            if((string)(Session["User"]) == null || (string)(Session["Department"]) == null || (string)(Session["Department"]) == "Guest"){
                Session["RedirectID"] = (ID != null) ? ID : null;
                Session["RedirectMode"] = "Request";
                return RedirectToAction("Index", "LogIn");
            }

            GenerateTopicList(Convert.ToString(Session["Department"]), Convert.ToString(Session["Position"]));
            Session["RedirectID"] = null;

            if(ID == null || M_Req.CheckTopicOwner((string) Session["User"], ID)){
                Session["Topic"] = null;
                ViewBag.mode = "Insert";
                if ((string)(Session["User"]) == null || (string)(Session["Department"]) == null){
                    Session["url"] = "Request";
                    return RedirectToAction("Index", "Login");
                }  
                List<GetID> last_iTopic = new List<GetID>();
                List<GetID> last_eTopic = new List<GetID>();
                last_iTopic = M_Req.GetInternalTopicId();
                last_eTopic = M_Req.GetExternalTopicId();

                if (last_iTopic.Count == 0){ //Case Internal Topic not exist
                    ViewData["iTopic_id"] = "IN-" + date.Substring(2, 4) + "001"; //Then new ID = IN2006001
                }else if(last_iTopic.Count > 0){ //Case Internal Topic exist
                    int i_id = Int32.Parse(last_iTopic[0].Code.Substring(7, 3)) + 1;
                    ViewData["iTopic_id"] = "IN-" + date.Substring(2, 4) + "" + i_id.ToString("000") + ""; //Then new ID = last Topic ID +1
                }
                
                if(last_eTopic.Count == 0){ //Case External Topic not exist
                    ViewData["eTopic_id"] = "EX-" + date.Substring(2, 4) + "001"; //Then new id = EX2006001
                }else if(last_eTopic.Count > 0){ //Case External Topic exist
                    int e_id = Int32.Parse(last_eTopic[0].Code.Substring(7, 3)) + 1;
                    ViewData["eTopic_id"] = "EX-" + date.Substring(2, 4) + "" + e_id.ToString("000") + ""; //Then new ID = last Topic ID +1
                }
                
                ViewData["FormChangeItem"] = M_Req.GetChangeItem(); //Get list of change items radio
                ViewData["FormProductType"] = M_Req.GetProductType(); //Get list of product type radio

                var DepartmentGroup = M_Req.GetDepartmentGroup(); //Get raw group of departments

                List<DepartmentList> departmentList = new List<DepartmentList>();
                foreach(string GroupName in DepartmentGroup){
                    List<Department> department = new List<Department>();
                    department = M_Req.GetDepartmentByGroup(GroupName);
                    departmentList.Add(new DepartmentList(){Name = GroupName.Replace(" ", "_"), Department = department}); //Convert raw group into department list for radio
                }
                ViewData["DepartmentList"] = departmentList;

                if(ID != null){ // In case of edit mode
                    Session["isEditMode"] = true;
                    TopicAlt temp_topic = M_Req.GetTopicByID(ID);

                    if(temp_topic.Status != 3 && temp_topic.Status != 7 ){
                        return View("~/Views/Shared/404/index.cshtml");
                    }else{
                        temp_topic.RelatedListAlt = M_Req.GetRelatedByID(temp_topic.Related);
                        temp_topic.Timing = temp_topic.Timing.StringToDigitDate2();
                        
                        Session["Topic"] = ViewData["Topic"] = temp_topic;

                        var topic_file_list = M_Req.GetFileByID(temp_topic.ID, "Topic", temp_topic.Code, temp_topic.Department); //Get file by topic id
                        if(topic_file_list != null){
                            temp_topic.FileList = topic_file_list;
                            ViewData["Topic"] = temp_topic;
                        }
                    }
                }
                return View();
            }else{
                return View("~/Views/Shared/404/index.cshtml");
            }
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Submit(string changeType,int changeItem,int productType,string model,string partNo, string partName, string processName,string appRadio, string appDescription,string subject,string detail,string timing,string timingDesc){
          int status;
          var mode = "Insert";
          var revision = 0;

          Session["Mode"] = mode;
          Session["Foreignkey"] = null;
          Session["Revision"] = null;
          Session["TxtFile"] = null;

          try{
              if(changeType == "Internal"){
                  topic_detail = M_Req.GetInternalTopicId();
              }
              else{
                  topic_detail = M_Req.GetExternalTopicId();
              }
              if (topic_detail.Count == 0){
                  if (changeType == "Internal") Session["TopicCode"] = "IN-" + date.Substring(2, 4) + "001";
                  else Session["TopicCode"] = "EX-" + date.Substring(2, 4) + "001";
              }else{
                  int id = Int32.Parse(topic_detail[0].Code.Substring(7, 3)) + 1;
                  if (changeType == "Internal") Session["TopicCode"] = "IN-" + date.Substring(2, 4) + "" + id.ToString("000") + "";
                  else Session["TopicCode"] = "EX-" + date.Substring(2, 4) + "" + id.ToString("000") + "";
              }
              Session["Revision"] = revision;
              status = 3;
              
              var new_timing = timing.Split('-');
              timing = $"{new_timing[2]}{new_timing[1]}{new_timing[0]}000000";
              
              var temp_topic = new Topic((string)(Session["TopicCode"]), changeType, changeItem, productType, revision , (string) Session["Department"], model.ReplaceSingleQuote(),partNo.ReplaceSingleQuote(), partName.ReplaceSingleQuote(), processName.ReplaceSingleQuote(), status, appDescription.ReplaceSingleQuote() , subject.ReplaceSingleQuote(), detail.ReplaceSingleQuote(), timing.ReplaceSingleQuote(), timingDesc.ReplaceSingleQuote(), (long)Session["RelatedID"],(string)(Session["User"]), date );

              Session["TopicID"] = M_Req.InsertTopic(temp_topic);
              M_Req.InsertTopicApprove((string)Session["TopicCode"]);
              return Json(new { status = "success", id = Session["TopicCode"].ToString(), mail = "EmailRequested", dept=Session["Department"].ToString(), pos = "Approver" },JsonRequestBehavior.AllowGet);
          }
          catch (Exception ex){
              System.Diagnostics.Debug.WriteLine("Exception");
              System.Diagnostics.Debug.WriteLine(ex);
              return Json(new { status = "error", id = Session["TopicCode"].ToString(), mail = "EmailRequested", dept=Session["Department"].ToString(), pos = "Approver" },JsonRequestBehavior.AllowGet);
          }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateRequest(int changeItem,int productType,string model,string partNo, string partName, string processName,string appRadio, string appDescription,string subject,string detail,string timing,string timingDesc){
            //update revision (edit)
          var mode = "Edit";
          var mail = "";
          var temp_topic = (TopicAlt) Session["Topic"];
        //   var status = temp_topic.Status;
          var status = 3;

          Session["Mode"] = mode;
          try{
                var new_timing = timing.Split('-');
                timing = $"{new_timing[2]}{new_timing[1]}{new_timing[0]}000000";
                var new_topic = new Topic(temp_topic.Code, temp_topic.Type, changeItem, productType, temp_topic.Revision,temp_topic.Department, model.ReplaceSingleQuote(),partNo.ReplaceSingleQuote(), partName.ReplaceSingleQuote(), processName.ReplaceSingleQuote(), status, appDescription.ReplaceSingleQuote(), subject.ReplaceSingleQuote(), detail.ReplaceSingleQuote(), timing.ReplaceSingleQuote(), timingDesc.ReplaceSingleQuote(), (long)Session["RelatedID"],(string)(Session["User"]), date );
                if(temp_topic.Status == 3){
                    //ยังไม่approve

                    mail = "EmailRequested";
                    Session["TopicID"] = M_Req.UpdateTopic(new_topic);
                }else{
                    //approve
                    mail = "TopicUpdate";
                    Session["TopicID"] = M_Req.UpdateTopicWithRev(new_topic);
                }

              return Json(new { status = "success", id = temp_topic.Code, mail, dept=temp_topic.Department, pos = "Approver" },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){

                System.Diagnostics.Debug.WriteLine("Exception");
                System.Diagnostics.Debug.WriteLine(ex);

              return Json(new { status = "error", id = temp_topic.Code, mail, dept=Session["Department"].ToString(), pos = "Approver" },JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult InsertRelated(List<String> dept_list){
            try{
                Session["RelatedID"] = M_Req.InsertRelated(dept_list, Session["User"].ToString());
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
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
                M_Req.InsertFile(file_item.file, (long) Session["TopicID"], "Topic", file_item.description.ReplaceSingleQuote(), Session["User"].ToString(), file_item.code, Session["Department"].ToString(), date_ff);
            }
            return Json((string)Session["TopicCode"]);
        }

        [HttpPost]
        public ActionResult UpdateFile(string id , string description){
            M_Req.UpdateFile(id, description);
            return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveDATA(){
            try{
                M_Req.RemoveData(0,(string)(Session["Foreignkey"]));
                return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public int InsertOtherChangeItem(string desc){
            return M_Req.InsertOtherChangeItem(desc.ReplaceSingleQuote());
        }

        public int InsertOtherProductType(string desc){
            return M_Req.InsertOtherProductType(desc.ReplaceSingleQuote());
        }
        [HttpPost]
        public ActionResult GetDepartment(){
            return Json(new { data = M_Req.GetDepartment() }, JsonRequestBehavior.AllowGet);
        }

        public void SendMail(List<string> mail){
            foreach(var item in mail){
              var datemail = DateTime.Now.ToString("dd/MM/yyy HH:mm:ss");
              string FromName = "Control Revision System";
              string FromEmail = "ControlRevisionSystem@thns.co.th";
              System.Net.Mail.SmtpClient smtp = new SmtpClient("172.27.170.11");
              smtp.EnableSsl = false;
              System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
              mailMessage.From = new System.Net.Mail.MailAddress(FromEmail, FromName);
              mailMessage.To.Add(new System.Net.Mail.MailAddress(item));
            string mail_body = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,@"Mails\template.cshtml"));
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = mail_body;
            mailMessage.Subject = "test"; 
            smtp.Send(mailMessage);
            mailMessage.Dispose();
            }          
        }

        [HttpPost]
        public ActionResult AddListMail(string Type){
            List <string> Email = new List<string>();
            List <ListMail> temp_email = new List<ListMail>();
   
            var sql = "";
            temp_email = M_Req.GetEmail((string)(Session["User"]));
              Email.Add("pakawat.smutkun@email.thns.co.th");
              SendMail(Email);
              return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
        }

    }
}