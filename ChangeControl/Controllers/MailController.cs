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
using System.Text;

namespace ChangeControl.Controllers{
    public class MailController : Controller{
        
        private DetailModel M_Detail;
        private MailModel M_Mail;
        public static TopicAlt Topic;
        public List<Department> A = new List<Department>();
        private string admin = "63014";
        public MailController(){
            M_Detail = new DetailModel();
            M_Mail = new MailModel();
            Topic = new TopicAlt();
        }

        public static string DevMode = "on";

        public ActionResult Index(string Mode,TopicAlt Topic){
            if((string)(Session["User"]) == null){
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Mode = Mode;
            ViewBag.Url = $"https://{Request.Url.Host}:{Request.Url.Port}/Detail/Index/?id={Topic.Code}";
            ViewBag.Topic = Topic;

            Topic = new TopicAlt();
            var email = RenderView("~/Views/Mail/index.cshtml",Topic);
            List<string> address_list = new List<string>();
            address_list.Add("pakawat.smutkun@email.thns.co.th");
            AddListMail(email,address_list);
            return View(Topic);
        }

        public ActionResult Generate(string mode,string topic_code,List<string> address_list){
            try{
                ViewBag.Mode = mode;
                if(Topic.Type == "ERR0R"){
                    Topic = M_Mail.GetTopicByCode(topic_code);
                }
                // ViewBag.Url = $"https://{Request.Url.Host}:{Request.Url.Port}/Detail/Index/?id={Topic.Code}";
                ViewBag.Url = $"{Request.Url.Host}:{Request.Url.Port}/{Request.ApplicationPath}/Detail/Index/?id={Topic.Code}";
                ViewBag.Topic = Topic;

                var email = RenderView("~/Views/Mail/index.cshtml",Topic);
                AddListMail(email,address_list);
                return Json(new {status = true}, JsonRequestBehavior.AllowGet);
            // return View(Topic);
            }catch(Exception err){
                return Json(new {error = err}, JsonRequestBehavior.AllowGet);
            }
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
            List <string> email = new List<string>();
            List <ListMail> temp_email = new List<ListMail>();
            foreach(string address in address_list){

            var sql = "";
            // temp_email = M_Req.GetEmail((string)(Session["User"]));
            // if(temp_email.Count != 0){
              // Email.Add(temp_email[0].Email);
              email.Add(address);

              // if (Type == "Internal"){
              //     sql = "SELECT Email FROM Department WHERE ID_Department ='11'";
              // }
              // else{
              //     sql = "SELECT Email FROM Department WHERE ID_Department ='32'";
              // }
          
              //item = _dbCCS.Database.SqlQuery<GetMail>(sql);
              //mail = item.ToList();
              //Email.Add(mail[0].Email);
            }
              SendMail(email_body,email);
              return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
            // }else{
            //   return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            // }
        }
        public void SendMail(string email_body,List<string> mail){
            foreach(var item in mail){
                var datemail = DateTime.Now.ToString("dd/MM/yyy HH:mm:ss");
                string FromName = "Control Revision System";
                string FromEmail = "ControlRevisionSystem@thns.co.th";
                System.Net.Mail.SmtpClient smtp = new SmtpClient("172.27.170.11");
                smtp.EnableSsl = false;
                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
                mailMessage.From = new System.Net.Mail.MailAddress(FromEmail, FromName);
                mailMessage.To.Add(new System.Net.Mail.MailAddress(item));
                var MNo = "1";
                var Msub = "1";
                var Mdetail = "1";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = email_body;
                
                switch(ViewBag.Mode){
                    case "EmailRequestor" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Issue request.";
                        break;
                    case "EmailReviewed" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please review.";
                        break;
                    case "InformPE" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please review.";
                        break;
                    case "InformUser" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Issue.";
                        break;
                    case "RequestTrial" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Production request Trial please followup";
                        break;
                    case "ReviewApproved" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please approve change request.";
                        break;
                    case "StartTrial" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please start trial change.";
                        break;
                    case "InformIPP" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please attach IPP label.";
                        break;
                    case "TrialApproved" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Review & Trial already please followup.";
                        break;
                    case "StartConfirm" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please confirm.";
                        break;
                    case "ConfirmApproved" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Please close confirm.";
                        break;
                    case "TopicUpdate" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} {ViewBag.Topic.Department} Revised review data to Rev{ViewBag.Topic.Revision}.";
                        break;
                    case "ReviewUpdate" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} {ViewBag.Topic.Profile.FullName} Revised review data to Rev{ViewBag.Topic.Revision}.";
                        break;
                    case "TopicReject" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} Not approve.";
                        break;
                    case "RequestDocument" :
                        mailMessage.Subject = $"Process change no. {ViewBag.Topic.Code} QA Request more documents.";
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
}