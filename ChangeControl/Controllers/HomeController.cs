﻿using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DateHelper;
using StringHelper;

namespace ChangeControl.Controllers{
    public class HomeController : Controller{

        private HomeModel M_Home;
        public static List<SearchResult> searchResult = new List<SearchResult>();
  
        public class Line{
            public string line { get; set; }
        }
        public HomeController(){
            M_Home = new HomeModel();

        }
        public ActionResult Index(){
            if ((string)(Session["User"]) == null){
                Session["url"] = "Home";
                return RedirectToAction("Index", "Login");
            }
            return View(searchResult);
        }


        public ActionResult About(){
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact(){
            ViewBag.Message = "Your contact page.";
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
        public ActionResult RedirectTo(string ID){
            var rs="";
            rs = Url.Content("~/Detail");
            Session["TopicCode"] = ID;
            return Json(new { redirecturl = rs, id = ID }, JsonRequestBehavior.AllowGet);
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
    }
}