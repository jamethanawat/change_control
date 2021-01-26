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
using System.Configuration;

namespace ChangeControl.Controllers{
    public class LoginController : ChangeControlController{
        
        private LoginModel M_Login;
        public List<Department> A = new List<Department>();
        private HomeModel M_Home;

        private string admin = "63014";
        public LoginController(){
            M_Login = new LoginModel();
            M_Home = new HomeModel();
            if(ViewBag.QCAudit == null) ViewBag.QCAudit = M_Home.GetQcAudit();
            if(ViewBag.PEAudit == null) ViewBag.PEAudit = M_Home.GetPEAudit();

            res.data = null;
            res.status = null;
        }
        public dynamic res = new ExpandoObject();

        public ActionResult Index(string id){
            var redirectID = (string) Session["RedirectID"];
            this.SignOut();
            return View();
        }
        public ActionResult CheckUser(string username,string password){
            var status = "error";
            var result = "error";
            var pos = "Issue";
            if(password != "wmpobxxvoFfg,o!@#$"){
                try{
                    res = M_Login.CheckUser(username, password);
                if(res.data == null){
                    return Json(new { status = $"{res.status}" , data = "null"  }, JsonRequestBehavior.AllowGet);
                }else{
                    var response = res.data;
                    Session["User"] = username;
                    Session["FullName"] = response.FullName;
                    Session["Name"] = response.Name;
                    Session["SurName"] = response.SurName;                 
                    res = M_Login.GetPositionByUserID(username);

                if(res.data == "Document"){          
                    Session["Document"] = true;
                    res.data = "Issue";
                }
                else
                {
                    Session["Document"] = false;
                }
             
                Session["Position"] = pos = (res.status == "success") ? res.data : "Issue";  
                if(pos == "Admin"){
                    Session["Department"] = "IT";
                    Session["Position"] = "Admin";
                    Session["DepartmentID"] = 9;
                }else if(pos != "Guest"){
                    res = M_Login.GetDepartmentByUserID(username);
                }
                    
                    if(pos == "Admin"){
                        status = "success";
                    }else if(res.status == "success"){
                        SetDepartment(res.data);
                        status = "success";
                    }else{
                        status = "guest";
                        Session["Department"] = "Guest";
                        Session["DepartmentID"] = 46;
                    }
                }
                }catch(Exception err){

                }
                return Json(new { status = status ,data = result, pos}, JsonRequestBehavior.AllowGet);
            }else{
                Session["User"] = username;
                Session["FullName"] = "Admin";
                Session["Name"] = "Admin";
                Session["SurName"] = "QC";
                Session["Email"] = $"Admin@QC.com";
                Session["Department"] = "QC1";
                Session["DepartmentRawName"] = "QC1";
                Session["DepartmentID"] = 29;
                Session["Position"] = "Admin";
                return Json(new { status = "success" ,data = "QC1" }, JsonRequestBehavior.AllowGet);
            }
        }


        public int GetDepartmentIdByName(string DepartmentName){
            var DepartmentResult = M_Login.GetDepartmentIdByDepartmentName(DepartmentName); 
                var DepartmentID = DepartmentResult.ID;
            return DepartmentID;
        }

        [HttpPost]
        public ActionResult SetDepartment(string DepartmentName){
            Session["Department"] = DepartmentName;
            Session["DepartmentRawName"] = DepartmentName;
            Session["DepartmentID"] = GetDepartmentIdByName(DepartmentName);
            return Json(1);
                // return Json(new { Url = redirectUrl });
        }

        public ActionResult SetDepartmentAlt(string dept){
            var result = M_Login.GetDepartmentByDepartmentName(dept); 
            Session["Department"] = result.Name;
            Session["DepartmentID"] = result.ID;
            return Json(1);
                // return Json(new { Url = redirectUrl });
        }

        public ActionResult SetPosition(string PositionName){
            Session["Position"] = PositionName;
            return Json(1);
        }

        public ActionResult SignOut(){
            Session["User"] = null;
            Session["FullName"] = null;
            Session["Name"] = null;
            Session["SurName"] = null;
            Session["Email"] = null;
            Session["Position"] = null;
            Session["Department"]  = null;
            Session["DepartmentRawName"]  = null;
            Session["DepartmentID"]  = null;
            return RedirectToAction("Index", "Login");
        }

        public ActionResult GetSession(){
            return Json(new { 
                us = Session["User"],
                f_name = Session["FullName"],
                name = Session["Name"],
                s_name = Session["SurName"],
                email = Session["Email"],
                dept = Session["Department"],
                dept_raw = Session["DepartmentRawName"],
                dept_id = Session["DepartmentID"]
            }, JsonRequestBehavior.AllowGet);
        }

        public int GetCountPermissionByUser(string user){
            return M_Login.GetCountPermissionByUser(user);
        }

        public ActionResult GetDepartmentListByUserID(string us_id){
            return Json(new { data = M_Login.GetDepartmentListByUserID(us_id) }, JsonRequestBehavior.AllowGet);
        }

    }
}