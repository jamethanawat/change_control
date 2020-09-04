using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using ChangeControl.Helpers;

namespace ChangeControl.Controllers{
    public class UserController : Controller{
        // GET: Detail
        private HomeModel M_Home;
        public UserController(){
            M_Home = new HomeModel();
            if(ViewBag.QCAudit == null) ViewBag.QCAudit = M_Home.GetQcAudit();
            if(ViewBag.PEAudit == null) ViewBag.PEAudit = M_Home.GetPEAudit();
        }

        public ActionResult Index(string id){
            if((string)(Session["User"]) == null || (string)(Session["Department"]) == null){
                Session["RedirectID"] = id ?? null;
                Session["RedirectMode"] = "User";
                return RedirectToAction("Index", "LogIn");
            }
            GenerateTopicList(Session["Department"].ToString(), Session["Position"].ToString());
            return View();
        }

        [HttpPost]
        public void GenerateTopicList(string Department, string Position){
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
                rv_list.AddRange(M_Home.GetReviewPendingByDepartment(dept));
                if(isApprover){
                    req_list.AddRange(M_Home.GetRequestIssuedByDepartment(dept));
                    rv_list.AddRange(M_Home.GetReviewIssuedByDepartment(dept));
                }
                if(isPEProcess){
                    req_list.AddRange(M_Home.GetRequestApprovedByDepartment(dept));
                }
                if(isQC){
                    rv_list.AddRange(M_Home.GetReviewApproved());
                    tr_list.AddRange(M_Home.GetTrialApproved());
                    cf_list.AddRange(M_Home.GetConfirmApproved());
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