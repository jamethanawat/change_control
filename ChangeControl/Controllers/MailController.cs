using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using System.Web.Routing;
using System.Dynamic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.IO;
using System.Reflection;
using System.Text;
using ChangeControl.Helpers;

namespace ChangeControl.Controllers{
    public class MailController : Controller{
        
        private DetailModel M_Detail;
        private MailModel M_Mail;
        public TopicAlt Topic;
        public List<Department> A = new List<Department>();
        public MailController(){
            M_Detail = new DetailModel();
            M_Mail = new MailModel();
            Topic = new TopicAlt();
        }

        //public static string DevMode = "on";

        public ActionResult Index(string mode,string topic_code , string dept = null){
            try{
                ViewBag.Mode = mode;
                // if(Topic.Type == "ERR0R"){
                    Topic = M_Mail.GetTopicByCode(topic_code);
                // }
                // ViewBag.Url = $"https://17.27.170.19/ChangeControl/Detail/Index/?id={Topic.Code}";
                ViewBag.Url = $"{Request.Url.Host}:{Request.Url.Port}/{Request.ApplicationPath}/Detail/Index/?id={Topic.Code}";
                ViewBag.Topic = Topic;
                ViewBag.DueDate = DateTime.Now.DueDateOn(10);

                var email = RenderView("~/Views/Mail/index.cshtml",Topic);
                var related_list = M_Mail.GetRelatedByTopicCode(topic_code);

                Type type = related_list.GetType();
                PropertyInfo[] props = type.GetProperties();
                
                var address_list = new List<string>();
                foreach (var prop in props){
                    if((int) prop.GetValue(related_list) == 1 ){
                        address_list.AddRange(M_Mail.GetEmailByDept(prop.Name));
                    }
                }

                SendMail(email,address_list);
                // return Json(new {status = true}, JsonRequestBehavior.AllowGet);
                return View(Topic);
            }catch(Exception err){
                return Json(new {error = err}, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GenerateMail(string mode,string topic_code, string dept = null, string[] dept_arry=null, string due_date="", string pos="")  {
            try{
                Topic = M_Mail.GetTopicByCode(topic_code);
                ViewBag.Mode = mode;
                ViewBag.Url = $"http://172.27.170.19/ChangeControl/Detail/Index/?id={Topic.Code}";
                // ViewBag.Url = $"{Request.Url.Host}:{Request.Url.Port}/{Request.ApplicationPath}/Detail/Index/?id={Topic.Code}";
                ViewBag.Topic = Topic;
                ViewBag.DueDate = due_date;
                if(ViewBag.Topic.FullStatus == "Waiting") ViewBag.Topic.FullStatus = "Request";

                var email = RenderView("~/Views/Mail/index.cshtml",Topic);
                var address_list = new List<string>();
                List<String> temp_email_list = new List<string>();

                if(dept_arry != null){ //Dept as array
                    foreach(var temp_dept in dept_arry){
                        temp_email_list = M_Mail.GetEmailByDeptAndPosition(temp_dept,pos);
                        if (temp_email_list != null) address_list.AddRange(temp_email_list);
                    }
                }else if(dept == "" || dept == null){ //Default department that related
                    var related_list = M_Mail.GetRelatedByTopicCode(topic_code);

                    related_list.ForEach(rl => {
                            temp_email_list = M_Mail.GetEmailByDeptAndPosition(rl,pos);
                            if (temp_email_list != null) address_list.AddRange(temp_email_list);
                    });

                }else{ //Single department
                    temp_email_list = M_Mail.GetEmailByDeptAndPosition(dept,pos);
                    if (temp_email_list != null) address_list.AddRange(temp_email_list);
                }

                if(temp_email_list != null && temp_email_list.Count!=0)
                {
                    SendMail(email,address_list);
                } 
                return Json(new {status = true}, JsonRequestBehavior.AllowGet);
            // return View(Topic);
            }catch(Exception err){
                return Json(new {error = err}, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SendEmailOnDueDate(){
            return Content(DateTime.Now.ToString());
        }


        public ActionResult DefaultView(){
            if((string)(Session["User"]) == null){
                return RedirectToAction("Index", "Home");
            }
            // var email = RenderView("~/Views/Mail/Default.cshtml",model);
            // AddListMail(email);
            return View("~/Views/Mail/Default.cshtml",Topic);
        }

        public string RenderView(string viewName, TopicAlt model = null){
            ViewData.Model = model;
            using (var sw = new StringWriter()){
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public ActionResult AddListMail(string email_body,List<string> address_list)
        {
            List <ListMail> temp_email = new List<ListMail>();
              SendMail(email_body,address_list);
              return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
            // }else{
            //   return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            // }
        }
        public void SendMail(string email_body,List<string> address_list){
            
            var datemail = DateTime.Now.ToString("dd/MM/yyy HH:mm:ss");
            string FromName = "Change Control System";
            string FromEmail = "ccs@email.thns.co.th";
            System.Net.Mail.SmtpClient smtp = new SmtpClient("172.27.170.11");
            smtp.EnableSsl = false;
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.From = new System.Net.Mail.MailAddress(FromEmail, FromName);
            foreach(var address in address_list){
                if(address == null) continue;
                mailMessage.To.Add(new System.Net.Mail.MailAddress(address));
            }
            var MNo = "1";
            var Msub = "1";
            var Mdetail = "1";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = email_body;
            
            switch(ViewBag.Mode){
                case "EmailRequestor" : // Unused
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Issue request.";
                    break;
                case "EmailRequested" : //Approve on request
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Issued.";
                    break;
                case "EmailReviewed" : //Approve on review
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Issued.";
                    break;
                case "EmailTrialed" : //Approve on trial
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Issued.";
                    break;
                case "EmailConfirmed" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Issued.";
                    break;
                case "InformPE" : //Request PE_Process to review
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Requested.";
                    break;
                case "InformUser" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Please Review.";
                    break;
                case "RequestTrial" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Production request Trial please followup";
                    break;
                case "ReviewApproved" : //Request QC to Approved
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Approved by Related Department.";
                    break;
                case "StartTrial" : //Start Trial
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Please start Trial and Confirm.";
                    break;
                case "InformIPP" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Please attach IPP label.";
                    break;
                case "TrialApproved" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Approved by Related Department.";
                    break;
                case "StartConfirm" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Please Issue in Initial Production.";
                    break;
                case "ConfirmApproved" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Approved by Related Department.";
                    break;
                case "TopicUpdate" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} {Session["FullName"].ToString()} Revised data to Rev.{ViewBag.Topic.Revision.ToString("00")}";
                    break;
                case "ReviewUpdate" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} {Session["FullName"].ToString()} Revised data to Rev.{Convert.ToInt32(Session["ReviewRev"].ToString()).ToString("00")}";
                    break;
                case "TrialUpdate" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} {Session["FullName"].ToString()} Revised data to Rev.{Convert.ToInt32(Session["TrialRev"].ToString()).ToString("00")}";
                    break;
                case "ConfirmUpdate" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} {Session["FullName"].ToString()} Revised data to Rev{Convert.ToInt32(Session["ConfirmRev"].ToString()).ToString("00")}";
                    break;
                case "TopicReject" :
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} Rejected.";
                    break;
                case "RequestDocument" : //Resubmit
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} {Session["Department"].ToString()} Request more documents.";
                    break;
                case "ReplyResubmit" : //Resubmit
                    mailMessage.Subject = $"Phase {ViewBag.Topic.FullStatus} : Process change no. {ViewBag.Topic.Code} {Session["Department"].ToString()} Replied resubmit.";
                    break;
                default :
                    mailMessage.Subject = "ERROR";
                    break;
            }

            smtp.Send(mailMessage);
            mailMessage.Dispose();
        }

        
    }
}