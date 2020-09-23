using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ChangeControl.Helpers;

namespace ChangeControl.Controllers{
    public class HomeController : ChangeControlController{

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

    }
}