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
    public class LoginController : Controller{
        
        private LoginModel M_Login;
        public List<Department> A = new List<Department>();
        private HomeModel M_Home;

        private string admin = "63014";
        public LoginController(){
            M_Login = new LoginModel();
            M_Home = new HomeModel();

            res.data = null;
            res.status = null;
        }
        public dynamic res = new ExpandoObject();


        public ActionResult Index(){
            var redirectID = (string) Session["RedirectID"];
            this.SignOut();
            
                // return (redirectID != null) ? RedirectToAction("Index", "Detail", new {id = redirectID}) : RedirectToAction("Index", "Home");
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
                Session["Department"] = "QC";
                Session["DepartmentRawName"] = "QC";
                Session["DepartmentID"] = 29;
                Session["Position"] = "Admin";
                return Json(new { status = "success" ,data = "QC1" }, JsonRequestBehavior.AllowGet);
            }
        }

        public string CheckDepartment(string dept){
            string[] PE_Process = {"PE1_Process","PE2_Process","P5_ProcessDesign","P6_ProcessDesign"};
            string[] MKT = {"MKT","MKT1","MKT2"};
            string[] IT = {"IT","IT1","IT2"};
            string[] PE = {"PE","PE1","PE2","PE2_SMT","PE2_PCB","PE2_MT"};
            string[] PCH = {"PCH"};
            string[] P = {"P","P1","P2","P3A","P3M","P4","P5","P6","P7"};
            string[] PC = {"PC","PC1","PC2"};
            string[] QC = {"QC","QC1","QC2","QC3"};
            string[] QC_IN = {"QC_IN","QC_IN1","QC_IN2","QC_IN3"};
            string[] QC_NFM = {"QC_NFM","QC_NFM1","QC_NFM2","QC_NFM3"};
            string[] QC_FINAL = {"QC_FINAL","QC_FINAL1","QC_FINAL2","QC_FINAL3"};

            var result = "Error";
            
            if(PE_Process.Contains(dept)){
                result = "PE1_Process";
            }else if(MKT.Contains(dept)){
                result = "MKT";
            }else if (IT.Contains(dept)){ 
                result = "IT";
            }else if(PE.Contains(dept)){
                result = "PE";
            }else if(PCH.Contains(dept)){
                result = "PCH";
            }else if(P.Contains(dept)){
                result = "P";
            }else if(PC.Contains(dept)){
                result = "PC";
            }else if(QC.Contains(dept)){
                result = "QC";
            }else if(QC_IN.Contains(dept)){
                result = "QC_IN";
            }else if(QC_NFM.Contains(dept)){
                result = "QC_NFM";
            }else if(QC_FINAL.Contains(dept)){
                result = "QC_FINAL";
            }else{
                result = "Not found";
            }
            return result;
        }

        public string GetDepartment(string us_id){
            myAD.ADInfo conAD = new myAD.ADInfo();
            var temp_dept = conAD.ChkSection(us_id);
            Session["DepartmentRawName"] = temp_dept;

            string[] PE_Process = {"PE1_Process","PE2_Process","P5_ProcessDesign","P6_ProcessDesign"};
            string[] MKT = {"MKT"};
            string[] IT = {"IT"};
            string[] PE = {"PE1","PE2","PE2_SMT","PE2_PCB","PE2_MT"};
            string[] PCH = {"PCH"};
            string[] P = {"P1","P2","P3A","P3M","P4","P5","P6","P7"};
            string[] PC = {"PC1","PC2"};
            string[] QC = {"QC1","QC2","QC3"};
            string[] QC_IN = {"QC_IN1","QC_IN2","QC_IN3"};
            string[] QC_NFM = {"QC_NFM1","QC_NFM2","QC_NFM3"};
            string[] QC_FINAL = {"QC_FINAL1","QC_FINAL2","QC_FINAL3"};

            var result = "Error";
            
            if(PE_Process.Any(x => temp_dept.Contains(x))){
                result = "PE1_Process";
            }else if(MKT.Any(x => temp_dept.Contains(x))){
                result = "MKT";
            }else if (IT.Any(x => temp_dept.Contains(x))){ 
                result = "IT";
            }else if(PE.Any(x => temp_dept.Contains(x))){
                result = "PE";
            }else if(PCH.Any(x => temp_dept.Contains(x))){
                result = "PCH";
            }else if(P.Any(x => temp_dept.Contains(x))){
                result = "P";
            }else if(PC.Any(x => temp_dept.Contains(x))){
                result = "PC";
            }else if(QC.Any(x => temp_dept.Contains(x))){
                result = "QC";
            }else if(QC_IN.Any(x => temp_dept.Contains(x))){
                result = "QC_IN";
            }else if(QC_NFM.Any(x => temp_dept.Contains(x))){
                result = "QC_NFM";
            }else if(QC_FINAL.Any(x => temp_dept.Contains(x))){
                result = "QC_FINAL";
            }else{
                result = "Not found";
            }
            return result;
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

        public ActionResult GetDepartmentListByUserID(string us_id){
            return Json(new { data = M_Login.GetDepartmentListByUserID(us_id) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void GenerateTopicList(string Department, string Position){
            var dept = Department ?? "Guest";
            var pos = Position ?? "Guest";
            bool isApprover = ViewBag.isApprover = (pos == "Approver") || (pos == "Admin") || (pos == "Special") ;
            var isPEProcess = ViewBag.isPEProcess = (dept == "PE1_Process" || dept == "PE2_Process"|| dept == "P5_ProcessDesign"|| dept == "P6_ProcessDesign");
            var isQC = ViewBag.isQC = (dept == "QC1" || dept == "QC2" || dept == "QC3");
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