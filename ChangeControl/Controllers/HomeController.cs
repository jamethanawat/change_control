using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChangeControl.Helpers;

namespace ChangeControl.Controllers{
    public class HomeController : Controller{

        private HomeModel M_Home;
        public class Line{
            public string line { get; set; }
        }

        public HomeController(){
            M_Home = new HomeModel();
            if(ViewBag.QCAudit == null) ViewBag.QCAudit = M_Home.GetQcAudit();
            if(ViewBag.PEAudit == null) ViewBag.PEAudit = M_Home.GetPEAudit();
        }
        public ActionResult Index(){
            ViewBag.Departments = GetDepartmentList();
            if ((string)(Session["User"]) == null || (string)(Session["Department"]) == null){
                Session["url"] = "Home";
                return RedirectToAction("Index", "Login");
            }
            ViewBag.SummaryTopic = M_Home.GetSummaryTopic();
            GenerateTopicList(Session["Department"].ToString(), Session["Position"].ToString());
            return View();
        }

        //public ActionResult Search(string user, string password){
        //    try{
        //        System.Net.ServicePointManager.Expect100Continue = false;

        //        CheckUser.LdapAuth chk = new CheckUser.LdapAuth();
        //        bool result = chk.checkLogin(user, password);
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception err){
        //        return View();
        //    }

        //}
        [HttpPost]
        public ActionResult GetLine(string Production){
            var result = M_Home.GetLine(Production);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSearch(string Type,int Status,string StartDate,string EndDate,string ProductType,int Overstatus,string Changeitem,string ControlNo, string Model,string Chosechangeitem,string Partno,string Partname,string Department,string Processname ,string Production ,string Line){
            var temp_search = new SearchAttribute(Type, Status, StartDate, EndDate, ProductType, Overstatus, Changeitem, ControlNo, Model, Chosechangeitem, Partno, Partname, Department, Processname, Production, Line);
            var TopicList = M_Home.GetSearch(temp_search);
            TopicList.ForEach(Topic => { 
                Topic.Date = Topic.Date.StringToDateTimeShort(); 
                Topic.Timing = Topic.Timing.StringToDateTimeShort();
                Topic.Detail = Topic.Detail.StripTagsRegex();
            });
            
            return Json(TopicList, JsonRequestBehavior.AllowGet);
        }

        public List<login> A = new List<login>();

        [HttpPost]
        public List<String> GetDepartmentList(){
            return M_Home.GetDepartmentList();
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