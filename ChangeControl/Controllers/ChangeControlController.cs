using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChangeControl.Controllers
{
    public class ChangeControlController : Controller
    {
        private HomeModel M_Home;
        public ChangeControlController(){
            M_Home = new HomeModel();
        }
        // GET: ChangeControl
        [HttpPost]
        public ActionResult DownloadFile(){
            var r = Request.Form["load"];
            var temp = r.Split('^');
            string filePath = temp[0];
            string fullName = Server.MapPath("~/topic_file/");
            byte[] fileBytes = GetFile(fullName + filePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, temp[1]);
        }

        byte[] GetFile(string s){
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if(br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

                [HttpPost]
        public void GenerateTopicList(string Department, string Position){
            //noti top layout
            var dept = Department ?? "Guest";
            var pos = Position ?? "Guest";
            bool isApprover = ViewBag.isApprover = (pos == "Approver") || (pos == "Admin") || (pos == "Special") ;
            var isPEProcess = ViewBag.isPEProcess = (ViewBag.PEAudit.Contains(dept));
            var isQC = ViewBag.isQC = (ViewBag.QCAudit.Contains(dept));
            var confirm_dept_list = M_Home.GetConfirmDeptList();


            List<TopicNoti> req_list = new List<TopicNoti>();
            List<TopicNoti> rv_list = new List<TopicNoti>();
            List<TopicNoti> tr_list = new List<TopicNoti>();
            List<TopicNoti> cf_list = new List<TopicNoti>();

            if(dept != null){
                if(!isPEProcess) rv_list.AddRange(M_Home.GetReviewPendingByDepartment(dept)); //Default case
                if(isApprover){
                    req_list.AddRange(M_Home.GetRequestIssuedByDepartment(dept));
                    rv_list.AddRange(M_Home.GetReviewIssuedByDepartment(dept));
                }
                if(isPEProcess){
                    rv_list.AddRange(M_Home.GetReviewPendingPEByDepartment(dept));
                    rv_list.AddRange(M_Home.GetReviewIssuedPEByDepartment(dept));
                }
                if(isQC){
                    rv_list.AddRange(M_Home.GetReviewApproved(dept));
                    tr_list.AddRange(M_Home.GetTrialApproved(dept));
                    cf_list.AddRange(M_Home.GetConfirmApproved(dept));
                }
                if(confirm_dept_list.Contains(dept)){
                    cf_list.AddRange(M_Home.GetConfirmPendingByDepartment(dept));
                    if(isApprover){
                        cf_list.AddRange(M_Home.GetConfirmIssuedByDepartment(dept));
                    }
                }
                if(M_Home.CheckTrialableByDepartment(dept)){
                    tr_list.AddRange(M_Home.GetTrialPendingByDepartment(dept));
                    if(isApprover){
                        tr_list.AddRange(M_Home.GetTrialIssuedByDepartment(dept));
                    }
                }
                ViewData["TopicRequestList"] = req_list;
                ViewData["TopicReviewList"] = rv_list;
                ViewData["TopicTrialList"] = tr_list;
                ViewData["TopicList"] = cf_list;
            }
        }
    }
}