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
        }
        public ActionResult Index(){
            ViewBag.Departments = GetDepartmentList();
            if ((string)(Session["User"]) == null || (string)(Session["Department"]) == null){
                Session["url"] = "Home";
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        public ActionResult Search(string user, string password){
            try{
                System.Net.ServicePointManager.Expect100Continue = false;

                CheckUser.LdapAuth chk = new CheckUser.LdapAuth();
                bool result = chk.checkLogin(user, password);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception err){
                return View();
            }

        }
        [HttpPost]
        public ActionResult GetLine(string Production){
            var result = M_Home.GetLine(Production);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSearch(string Type,int Status,string ProductType,string Overstatus,string Changeitem,string ControlNo, string Model,string Chosechangeitem,string Partno,string Partname,string Department,string Processname ,string Production ,string Line){
            var temp_search = new SearchAttribute(Type, Status, ProductType, Overstatus, Changeitem, ControlNo, Model, Chosechangeitem, Partno, Partname, Department, Processname, Production, Line);
            var TopicList = M_Home.GetSearch(temp_search);
            TopicList.ForEach(Topic => { 
                Topic.Date = Topic.Date.StringToDateTime(); 
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