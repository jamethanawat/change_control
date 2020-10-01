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
    public class UserController : ChangeControlController{
        // GET: Detail
        private HomeModel M_Home;
        private UserModel M_User;
        public UserController(){
            M_Home = new HomeModel();
            M_User = new UserModel();
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
            ViewBag.Positions = (Session["Position"].ToString() == "Admin") ? M_User.GetAdminPosition() : M_User.GetPosition();
            ViewBag.Users = M_User.GetUserCodeByDept(Session["Department"].ToString());
            ViewBag.Depts = (Session["Position"].ToString() == "Admin") ? M_User.GetDepartmentList() : M_User.GetDepartmentByGroup(Session["Department"].ToString());
            return View();
        }

        public ActionResult AddUser(string user, string name, int position, string email){
            try{
                var status = "";
                if(M_User.CheckExistsUser(user)){
                    status = "duplicated";
                }else{
                    M_User.InsertUser(user, name, Session["Department"].ToString(), position, email);
                    status = "success";
                }
                return Json(new {status}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AddPermission(string user, string dept, int receive_mail){
            try{
                var status = "";
                if(M_User.CheckExistsPermission(user,dept)){
                    status = "duplicated";
                }else{
                    M_User.InsertPermission(user, dept, receive_mail);
                    status = "success";
                }
                return Json(new {status}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetUserCodeByDept(string dept){
            return Json(new {data= M_User.GetUserCodeByDept(dept)}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUserWithPermissionByDept(string dept){
            try{
                List<UserWithPermission> us_list = M_User.GetUserByDeptGroup(dept);
                us_list.ForEach(us => {
                   us.children = M_User.GetPermissionByUser(us.User,us.Dept); 
                   us.children.ForEach(x => x.User = us.User);
                });
                return Json(new {status = "success", data = us_list}, JsonRequestBehavior.AllowGet);
            }catch(Exception err){
                return Json(new {status="error"}, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdatePosition(string user,string pos){
            return Json(new {status= M_User.UpdatePosition(user, pos)}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSubscribe(string user, string dept, int status){
            return Json(new {status= M_User.UpdateSubscribe(user, dept, status)}, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult DeletePermission(string dept,string user){
            return Json(new {status= M_User.DeletePermission(dept, user)}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUser(string user){
            return Json(new {status= M_User.DeleteUser(user)}, JsonRequestBehavior.AllowGet);
        }

    }
}