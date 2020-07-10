using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
        private RequestModel M_Req;
        public List<Review> ReviewList = new List<Review>();
        public RequestController(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            M_Req = new RequestModel();
        }
        public class Value{
            public List<string> value { get; set; }
        }
        public class RawFile{
            public HttpPostedFileBase file {get; set;}
            public string description {get; set;}
        }
        public static TopicAlt Topic;
        public static string topic_code;
        public static long topic_id, related_id;
        public static bool isEditMode = false;
        public List<GetID> topic_detail = new List<GetID>();
        private string date = DateTime.Now.ToString("yyyyMMddHHmmss");
        private string date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        public ActionResult Index(string ID){
            Topic = null;
            ViewBag.mode = "Insert";
            if ((string)(Session["User"]) == null){
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
            
            var formChangeItem = M_Req.GetChangeItem(); //Get list of change items radio
            ViewData["FormChangeItem"] = formChangeItem;

            var formProductType = M_Req.GetProductType(); //Get list of product type radio
            ViewData["FormProductType"] = formProductType;

            var DepartmentGroup = M_Req.GetDepartmentGroup(); //Get raw group of departments

            List<DepartmentList> departmentList = new List<DepartmentList>();
            foreach(string GroupName in DepartmentGroup){
                List<Department> department = new List<Department>();
                department = M_Req.GetDepartmentByGroup(GroupName);
                departmentList.Add(new DepartmentList(){Name = GroupName.Replace(" ", "_"), Department = department}); //Convert raw group into department list for radio
            }
            ViewData["DepartmentList"] = departmentList;

            if(ID != null){ // In case of edit mode
                isEditMode = true;
                Topic = M_Req.GetTopicByID(ID);
                var temp_topic = Topic;
                var TopicRelatedList = M_Req.GetRelatedByID(temp_topic.Related);
                temp_topic.RelatedList = TopicRelatedList;
                
                var topic_file_list = M_Req.GetFileByID(temp_topic.ID, "Topic");
                if(topic_file_list != null){
                  temp_topic.FileList = topic_file_list;
                  ViewData["Topic"] = temp_topic;
                }
            }
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Submit(string changeType,int changeItem,int productType,string model,string partNo, string partName, string processName,string appRadio, string appDescription,string subject,string detail,string timing){
          int status;
          var mode = "Insert";
          var revision = 1;

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
                  if (changeType == "Internal") topic_code = "IN-" + date.Substring(2, 4) + "001";
                  else topic_code = "EX-" + date.Substring(2, 4) + "001";
              }else{
                  int id = Int32.Parse(topic_detail[0].Code.Substring(7, 3)) + 1;
                  if (changeType == "Internal") topic_code = "IN-" + date.Substring(2, 4) + "" + id.ToString("000") + "";
                  else topic_code = "EX-" + date.Substring(2, 4) + "" + id.ToString("000") + "";
              }
              Session["Foreignkey"] = topic_code;
              Session["Revision"] = revision;
              status = 7;
              var temp_topic = new Topic((string)(Session["Foreignkey"]), changeType, changeItem, productType, revision, model,partNo, partName, processName, status, appDescription, subject, detail, timing, related_id,(string)(Session["User"]), date );

              topic_id = M_Req.InsertTopic(temp_topic);
              M_Req.InsertTopicApprove(topic_code);
              return Json(topic_code,JsonRequestBehavior.AllowGet);
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
          var revision = Topic.Revision + 1;
          var status = Topic.Status;

          Session["Mode"] = mode;
          try{
                var new_topic = new Topic(Topic.Code, Topic.Type, changeItem, productType, revision, model,partNo, partName, processName, status, appDescription, subject, detail, timing, related_id,(string)(Session["User"]), date );
                topic_id = M_Req.UpdateTopic(new_topic);

                return Json(Topic.Code, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex){

                System.Diagnostics.Debug.WriteLine("Exception");
                System.Diagnostics.Debug.WriteLine(ex);

                return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SubmitRelated(string IT,string MKT,string PC1,string PC2,string PT1,string PT2,string PT3A,string PT3M,string PT4,string PT5,string PT6,string PT7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH1,string PCH2,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3){
            try{
                Related related = new Related(IT,MKT,PC1,PC2,PT1,PT2,PT3A,PT3M,PT4,PT5,PT6,PT7,PE1,PE2,PE2_SMT,PE2_PCB,PE2_MT,PE1_Process,PE2_Process,PCH1,PCH2,QC_IN1,QC_IN2,QC_IN3,QC_FINAL1,QC_FINAL2,QC_FINAL3,QC_NFM1,QC_NFM2,QC_NFM3,QC1,QC2,QC3);
                related_id = M_Req.InsertRelated(related);
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
                M_Req.InsertFile(file_item.file, topic_id, "Topic", file_item.description, Session["User"]);
            }
            return Json(topic_code);
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
              var MNo = "1";
              var Msub = "1";
              var Mdetail = "1";
                //string body = string.Format("&nbsp&nbsp&nbsp&nbsp Dear SafeDrive Committee<br>"//.Text
                //         + "&nbsp&nbsp&nbsp&nbsp Meeting No <font color='#CC3300'><b>" + MNo + "</b></font><br>"
                //         //+ "&nbsp&nbsp&nbsp&nbsp Meeting Date <font color='#CC3300'><b>" + Convert.ToDateTime(Mdate).ToString("dd/MM/yyyy") + "</b></font><br>"
                //         + "&nbsp&nbsp&nbsp&nbsp Subject <font color='#CC3300'><b>" + Msub + "</b></font><br>"
                //         + "&nbsp&nbsp&nbsp&nbsp Details <font color='#CC3300'><b>" + Mdetail + "</b></font><br>"
                //         + "&nbsp&nbsp&nbsp&nbsp<a href = http://172.27.170.23/safedrive >คุณสามารถเข้าไปดูรายละเอียดได้ที่นี้</a><br> "
                //         //+ "&nbsp&nbsp&nbsp&nbsp <a href= http://172.27.170.23/support/CheckStatus.aspx ">คุณสามารถเข้าไปดูสถานะงานได้ที่นี้ </a><br> "
                //         + "---------------------------z-------------------------------------------------------------------------<br>");      
           
             string body = @"<!DOCTYPE html> <html lang = ""en"" xmlns = ""http://www.w3.org/1999/xhtml"" xmlns: v = ""urn:schemas-microsoft-com:vml""xmlns: o = ""urn:schemas-microsoft-com:office:office"">
<head>
<meta charset = ""utf-8"">
<meta name = ""description"" content = ""Nagios Email Notification Alert"">
<meta name = ""viewport"" content = ""width=device-width"">
  <meta http - equiv = ""X-UA-Compatible"" content = ""IE=edge"">
  <meta name = ""x-apple-disable-message-reformatting"">
  <title> test </title>
  <link href = ""https://font.internet.fo/stylesheet.css"" rel = ""stylesheet"" type = ""text/css"">
  <link href = ""https://fonts.googleapis.com/css?family=Open+Sans"" rel = ""stylesheet"">
  <style>
    html,
    body {
                margin: 0 auto!important;
                padding: 0!important;
                height: 50 % !important;
                width: 50 % !important;
                }

                * {
                    -ms - text - size - adjust: 50%;
                    -webkit - text - size - adjust: 50%;
                }

                div[style *= ""margin: 16px 0""] {
                margin: 0!important;
                }

                table,
    td {
                    mso - table - lspace: 0pt!important;
                    mso - table - rspace: 0pt!important;
                }

                table {
                    border - spacing: 0!important;
                    border - collapse: collapse!important;
                    table - layout: fixed !important;
                    margin: 0 auto!important;
                }

                table table table {
                    table - layout: auto;
                }

                img {
                    -ms - interpolation - mode: bicubic;
                }

                *[x - apple - data - detectors],
    /* iOS */
    .x - gmail - data - detectors,
    /* Gmail */
    .x - gmail - data - detectors *,
    .aBn {
                    border - bottom: 0!important;
                cursor: default!important;
                color: inherit!important;
                    text - decoration: none!important;
                    font - size: inherit!important;
                    font - family: inherit!important;
                    font - weight: inherit!important;
                    line - height: inherit!important;
                }

    .a6S {
                display: none!important;
                opacity: 0.01!important;
                }

                img.g - img + div {
                display: none!important;
                }

    .button - link {
                    text - decoration: none!important;
                }

        

    .button - td,
    .button - a {
                transition: all 100ms ease-in;
                }

    .button - td:hover,
    .button - a:hover {
                background: #555555 !important;
      border - color: #555555 !important;
    }

                @media screen and(max - width: 600px) {
      .email - container p {
                        font - size: 17px!important;
                        line - height: 22px!important;
                    }
                }
  </style>
</head>

<body width = ""50%"" bgcolor = ""#f6f6f6""
  style = ""margin: 0;line-height:1.4;padding:0;-ms-text-size-adjust:50%;-webkit-text-size-adjust:50%;"">
  <center style = ""width: 50%; background: #f6f6f6; text-align: left;"">
 
     <div
      style = ""display:none;font-size:1px;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;mso-hide:all;font-family: sans-serif;"">
      testttttttttttt
    </div>
    <div style = ""max-width: 200px; padding: 10px 0; margin: auto;"" class=""email-container"">
      <table role = ""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" align=""center"" width=""95%""
        style=""max-width: 200px;"">
        <tr>
          <td bgcolor = ""#ffffff""
            style=""border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:50%;-webkit-border-top-right-radius: 25px;-webkit-border-top-left-radius: 25px;-moz-border-top-right-radius: 25px;-moz-border-top-left-radius: 25px;border-top-right-radius: 25px;border-top-left-radius: 25px;-webkit-border-bottom-right-radius: 25px;-webkit-border-bottom-left-radius: 25px;-moz-border-bottom-right-radius: 25px;-moz-border-bottom-left-radius: 25px;border-bottom-right-radius: 25px;border-bottom-left-radius: 25px;"">
            <table role = ""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""50%"" align=""center""
              style=""border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:50%;-webkit-border-top-right-radius: 25px;-webkit-border-top-left-radius: 25px;-moz-border-top-right-radius: 25px;-moz-border-top-left-radius: 25px;border-top-right-radius: 25px;border-top-left-radius: 25px;"">
              <tbody>
                <tr>
                  <td
                    style = ""background-color:#424242;-webkit-border-top-right-radius: 25px;-webkit-border-top-left-radius: 25px;-moz-border-top-right-radius: 25px;-moz-border-top-left-radius: 25px;border-top-right-radius: 25px;border-top-left-radius: 25px;"">
                    <h2
                      style=""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; color:#ffffff; text-align:center;"">
                      Alert Notification</h2>
                  </td>
                </tr>
                <tr>
                  <td style = ""background-color:#74992e;"">
                    <h1
                      style= ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; padding:0; margin:10px; color:#ffffff; text-align:center;"">
                      testttttttttttt </h1>
                  </td>
                </tr>
              </tbody>
            </table>
            <table border= ""0"" cellpadding= ""0"" cellspacing= ""0""
              style= ""border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;border-left-style: solid;border-right-style: solid;border-color: #d3d3d3;border-width: 1px;"">
              <td style= ""font-size:16px;vertical-align:top;""> &nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    <h5
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; color:#b0b0b0; text-align:right; padding-right:5%;"">
                      testttttttttttt </h5>
                    <h4
                      style=""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:15px; color:#b0b0b0; text-align:center; text-decoration:underline;"">
                      Host:</h4>
                    <h2
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:26px; text-align:center;"">
                      testttttttttttt </h2>
                    <h5
                      style=""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; color:#b0b0b0; text-align:center;"">
                      is</h5>
                    <h1
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:30px; color:' .$f_color. ';text-align:center;"">
                      testttttttttttt </h1>
                    <h5
                      style=""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; color:#b0b0b0; text-align:center;"">
                      testttttttttttt</h5>
                  </td>
                </tr>
              </tbody>
              <td style = ""font-size:9px;vertical-align:top;""> &nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    <h4
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:15px; color:#b0b0b0; padding-left:3%; text-decoration:underline;"">
                      Hostalias:</h4>
                    <h2
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:20px; padding-left:5%;"">
                      testttttttttttt </h2>
                  </td>
                </tr>
              </tbody>
              <td style=""font-size:9px;vertical-align:top;"">&nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    <h4
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:15px; color:#b0b0b0; padding-left:3%;text-decoration:underline;"">
                      Host Address:</h4>
                    <h2
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:20px; padding-left:5%;"">
                      testttttttt </h2>
                  </td>
                </tr>
              </tbody>
              <td style=""font-size:9px;vertical-align:top;"">&nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    <h4
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:15px; color:#b0b0b0; padding-left:3%;text-decoration:underline;"">
                      Last Check:</h4>
                    <h2
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:20px; padding-left:5%;"">
                      testttttttt </h2>
                  </td>
                </tr>
              </tbody>
              <td style=""font-size:9px;vertical-align:top;"">&nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    <h4
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:15px; color:#b0b0b0; padding-left:3%;text-decoration:underline;"">
                      Status Information:</h4>
                    <h2
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:20px; padding-left:5%;"">
                      testttttttt </h2>
                  </td>
                </tr>
              </tbody>
              <td style=""font-size:9px;vertical-align:top;"">&nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    <h4
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:15px; color:#b0b0b0; padding-left:3%;text-decoration:underline;"">
                      Notified Recipients:</h4>
                    <h2
                      style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; font-size:20px; padding-left:5%;"">
                      testttttttt </h2>
                  </td>
                </tr>
              </tbody>
              <td style=""font-size:9px;vertical-align:top;"">&nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    testttttttt
                  </td>
                </tr>
              </tbody>
              <td style = ""font-size:9px;vertical-align:top;""> &nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    testttttttt
                  </td>
                </tr>
              </tbody>
              <td style = ""font-size:9px;vertical-align:top;""> &nbsp;</td>
              <tbody>
                <tr>
                  <td width = ""98%"" style=""vertical-align:middle;font-size:14px;width:98%;margin:0 10px 0 10px;"">
                    testttttttt
                  </td>
                </tr>
              </tbody>
              <td style = ""font-size:16px;vertical-align:top;""> &nbsp;</td>
            </table>
            <table role = ""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""
              style=""border-collapse:separate;mso-table-lspace:0pt;mso-table-rspace:0pt;width:100%;-webkit-border-bottom-right-radius: 25px;-webkit-border-bottom-left-radius: 25px;-moz-border-bottom-right-radius: 25px;-moz-border-bottom-left-radius: 25px;border-bottom-right-radius: 25px;border-bottom-left-radius: 25px;"">
              <tbody>
                <tr>
                  <td style = ""background-color:' .$f_color. ';"">
                    <h1
                      style=""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; padding:0; margin:10px; color:#ffffff; text-align:center;"">
                      testttttttt</h1>
                  </td>
                </tr>
                <tr>
                  <td
                    style = ""background-color:#424242;-webkit-border-bottom-right-radius: 25px;-webkit-border-bottom-left-radius: 25px;-moz-border-bottom-right-radius: 25px;-moz-border-bottom-left-radius: 25px;border-bottom-right-radius: 25px;border-bottom-left-radius: 25px;"">
                    <h2
                      style=""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; margin:0; color:#ffffff; text-align:center;"">
                      Alert Notification</h2>
                  </td>
                </tr>
              </tbody>
            </table>
          </td>
        </tr>
      </table>
      <table role = ""presentation"" cellspacing= ""0"" cellpadding= ""0"" border= ""0"" align= ""center"" width= ""100%""
        style= ""max-width: 680px;"">
        <tr>
          <td
            style= ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; vertical-align:middle; color: #999999; text-align: center; padding: 40px 10px;width: 100%;""
            class=""x-gmail-data-detectors"">
            <span
              style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; color:#999999; text-align:center;"">
              Copyright &#169; 2014-2020 &#124; <a href=""mailto:hhan@mail.fo?subject=Nagios-HTML-Email-Notifications""
                style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; text-decoration:underline; color:#000000;""> Heini
                Holm Andersen</a>
            </span>
            <br>
            <span
              style = ""font-family: CoconPro-BoldCond, Open Sans, Verdana, sans-serif; color:#999999; text-align:center;"">
              All Rights Reserved.
            </span>
            <br>
            <br>
          </td>
        </tr>
      </table>
    </div>
  </center>
</body>
</html>";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = body;
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
            if(temp_email.Count != 0){
              Email.Add(temp_email[0].Email);

              if (Type == "Internal"){
                  sql = "SELECT Email FROM Department WHERE ID_Department ='11'";
              }
              else{
                  sql = "SELECT Email FROM Department WHERE ID_Department ='32'";
              }
          
              //item = _dbCCS.Database.SqlQuery<GetMail>(sql);
              //mail = item.ToList();
              //Email.Add(mail[0].Email);
              SendMail(Email);
              return Json(new { code = 1 }, JsonRequestBehavior.AllowGet);
            }else{
              return Json(new { code = -1 }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public ActionResult test()
        //{
        //    string filePath = "20200403151812";
        //    string fullName = "D:/File/Topic/";
        //    byte[] fileBytes = GetFile(fullName + filePath);
        //    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "test.txt");
        //}

        //byte[] GetFile(string s)
        //{
        //    System.IO.FileStream fs = System.IO.File.OpenRead(s);
        //    byte[] data = new byte[fs.Length];
        //    int br = fs.Read(data, 0, data.Length);
        //    if (br != fs.Length)
        //        throw new System.IO.IOException(s);
        //    return data;
        //}


        [HttpPost]
        public ActionResult DownloadFile()
        {
            var r = Request.Form["load"];
            var temp = r.Split('^');
            //string filePath = "km0024.txt";
            //string fullName = Server.MapPath("~/upload/");
            string filePath = temp[0];
            string fullName = "D:/File/Topic/";
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