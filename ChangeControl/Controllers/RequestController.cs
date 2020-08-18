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
    public class RequestController : Controller{
        // GET: Request
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        private DetailModel M_Detail;
        private RequestModel M_Req;
        public List<Review> ReviewList = new List<Review>();
        public RequestController(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            M_Detail = new DetailModel();
            M_Req = new RequestModel();
        }
        public class Value{
            public List<string> value { get; set; }
        }
        public class RawFile{
            public HttpPostedFileBase file {get; set;}
            public string description {get; set;}
        }
        public List<GetID> topic_detail = new List<GetID>();
        private string date = DateTime.Now.ToString("yyyyMMddHHmmss");
        private string date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        public ActionResult Index(string ID)    {
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
                    Session["Topic"] = temp_topic;
                    if(temp_topic.Status != 3 && temp_topic.Status != 7 ){
                        return View("~/Views/Shared/404/index.cshtml");
                    }else{
                        var TopicRelatedList = M_Req.GetRelatedByID(temp_topic.Related);
                        temp_topic.RelatedList = TopicRelatedList;
                        

                        var topic_file_list = M_Req.GetFileByID(temp_topic.ID, "Topic"); //Get file by topic id
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
        public ActionResult Submit(string changeType,int changeItem,int productType,string model,string partNo, string partName, string processName,string appRadio, string appDescription,string subject,string detail,string timing){
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
              var temp_topic = new Topic((string)(Session["TopicCode"]), changeType, changeItem, productType, revision , (string) Session["Department"], model.ReplaceSingleQuote(),partNo.ReplaceSingleQuote(), partName.ReplaceSingleQuote(), processName.ReplaceSingleQuote(), status, appDescription.ReplaceSingleQuote() , subject.ReplaceSingleQuote(), detail.ReplaceSingleQuote(), timing.ReplaceSingleQuote(), (long)Session["RelatedID"],(string)(Session["User"]), date );

              Session["TopicID"] = M_Req.InsertTopic(temp_topic);
              M_Req.InsertTopicApprove((string)Session["TopicCode"]);
              return Json((string)Session["TopicCode"],JsonRequestBehavior.AllowGet);
          }
          catch (Exception ex){
              System.Diagnostics.Debug.WriteLine("Exception");
              System.Diagnostics.Debug.WriteLine(ex);
              return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
          }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateRequest(int changeItem,int productType,string model,string partNo, string partName, string processName,string appRadio, string appDescription,string subject,string detail,string timing){
          var mode = "Edit";
          var temp_topic = (TopicAlt) Session["Topic"];
          var status = temp_topic.Status;

          Session["Mode"] = mode;
          try{
                var new_topic = new Topic(temp_topic.Code, temp_topic.Type, changeItem, productType, temp_topic.Revision,(string) Session["Department"], model.ReplaceSingleQuote(),partNo.ReplaceSingleQuote(), partName.ReplaceSingleQuote(), processName.ReplaceSingleQuote(), status, appDescription.ReplaceSingleQuote(), subject.ReplaceSingleQuote(), detail.ReplaceSingleQuote(), timing.ReplaceSingleQuote(), (long)Session["RelatedID"],(string)(Session["User"]), date );
                if(temp_topic.Status == 3){
                    Session["TopicID"] = M_Req.UpdateTopic(new_topic);
                }else{
                    Session["TopicID"] = M_Req.UpdateTopicWithRev(new_topic);
                }

                return Json(temp_topic.Code, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){

                System.Diagnostics.Debug.WriteLine("Exception");
                System.Diagnostics.Debug.WriteLine(ex);

                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SubmitRelated(string IT,string MKT,string PC1,string PC2,string P1,string P2,string P3A,string P3M,string P4,string P5,string P6,string P7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3, string P5_ProcessDesign, string P6_ProcessDesign){
            try{
                Related related = new Related(IT,MKT,PC1,PC2,P1,P2,P3A,P3M,P4,P5,P6,P7,PE1,PE2,PE2_SMT,PE2_PCB,PE2_MT,PE1_Process,PE2_Process,PCH,QC_IN1,QC_IN2,QC_IN3,QC_FINAL1,QC_FINAL2,QC_FINAL3,QC_NFM1,QC_NFM2,QC_NFM3,QC1,QC2,QC3, P5_ProcessDesign, P6_ProcessDesign);
                Session["RelatedID"] = M_Req.InsertRelated(related, Session["User"].ToString());
                return Json(new { code = 1 },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){
                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SubmitFile(RawFile file_item){

            Value temp_file = new Value();
            temp_file = Session["TxtFile"] as Value;
            
            if (file_item.file != null && file_item.file.ContentLength > 0){
                var InputFileName = Path.GetFileName(date_ff);
                var ServerSavePath = Path.Combine("D:/File/Topic/" + InputFileName);
                file_item.file.SaveAs(ServerSavePath);
                if(file_item.description == "null" || file_item.description == null) file_item.description = " ";
                M_Req.InsertFile(file_item.file, (long) Session["TopicID"], "Topic", file_item.description, Session["User"]);
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
            return M_Req.InsertOtherChangeItem(desc);
        }

        public int InsertOtherProductType(string desc){
            return M_Req.InsertOtherProductType(desc);
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
        public ActionResult AddListMail(string Type)
        {
            List <string> Email = new List<string>();
            List <ListMail> temp_email = new List<ListMail>();
   
            var sql = "";
            temp_email = M_Req.GetEmail((string)(Session["User"]));
              Email.Add("pakawat.smutkun@email.thns.co.th");
              SendMail(Email);
              return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DownloadFile()
        {
            var r = Request.Form["load"];
            var temp = r.Split('^');
            //string filePath = "km0024.txt";
            //string fullName = Server.MapPath("~/upload/");
            string filePath = temp[0];
            // string fullName = "D:/File/Topic/";
            string fullName = Server.MapPath("~/topic_file/");
            byte[] fileBytes = GetFile(fullName + filePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, temp[1]);
        }

        byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

    }
}