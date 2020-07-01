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
        private string admin = "63014";
        public LoginController(){
            M_Login = new LoginModel();
        }

        public static string DevMode = "on";

        public ActionResult Index(){
            if(LoginController.DevMode == "on"){
                // Session["User"] = "Admin";
                // Session["FullName"] = "Admin";
                // Session["Name"] = "Admin";
                // Session["SurName"] = "QC";
                // Session["Email"] = $"Admin@QC.com";
                // Session["Department"] = "QC";
                // Session["DepartmentRawName"] = "QC";
                // Session["DepartmentID"] = 29;

                Session["User"] = "63014";
                Session["FullName"] = "Pakawat Smutkun";
                Session["Name"] = "Pakwat";
                Session["SurName"] = "Smutkun";
                Session["Email"] = $"Pakwat@IT.com";
                Session["Department"] = "IT";
                Session["DepartmentRawName"] = "IT";
                Session["DepartmentID"] = 9;
            }
            
            if ((string)(Session["User"]) != null){
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        public ActionResult CheckUser(string username,string password){
            var redirectUrl = "";

            dynamic res = new ExpandoObject();
                    res.message = "error";
                    res.data = null;

            res = M_Login.CheckUser(username, password);
            if(res.message == "error"){
                return Json("Error");
            }else{
                if ((string)(Session["url"]) != null){
                    redirectUrl = Url.Content("~/"+ Session["url"] + "/Index");
                }
                else{
                    redirectUrl = Url.Content("~/Home/Index");
                }

                var response = res.data;
                Session["User"] = username;
                Session["FullName"] = response.FullName;
                Session["Name"] = response.Name;
                Session["SurName"] = response.SurName;
                Session["Email"] = response.Email;

                var CuttedDepartment = response.Department.Replace("TNS_", "");
                var result_depart = this.CheckDepartment(CuttedDepartment).ToString();
                if(result_depart == "Error" || result_depart == "Not found" ){
                    // SweetAlert Select Department
                    Session["Department"] = "ERROR";
                }else{
                    Session["Department"] = result_depart;
                }
                Session["DepartmentRawName"] = response.Department;
                
                var DepartmentResult = M_Login.GetDepartmentIdByDepartmentName(CuttedDepartment); 
                var DepartmentID = DepartmentResult.ID;
                Session["DepartmentID"] = DepartmentID;
                Session["Position"] = response.Position;
                return Json(new { Url = redirectUrl });
            }
        }

        public ActionResult CheckAdmin(string username, string password, string department) {
            var redirectUrl = "";
            if(username == admin && (password == "OPERATOR" || password == "STAFF" || password == "MANAGER")){
                if ((string)(Session["url"]) != null){
                    redirectUrl = Url.Content("~/"+ Session["url"] + "/Index");
                }
                else{
                    redirectUrl = Url.Content("~/Home/Index");
                }

                Session["User"] = "Admin";
                Session["FullName"] = "Admin";
                Session["Name"] = "Admin";
                Session["SurName"] = department;
                Session["Email"] = $"Admin@{department}.com";
            
                var result_depart = this.CheckDepartment(department);
                if (result_depart != "Error" && result_depart != "Not found") {
                    // SweetAlert Select Department
                    Session["Department"] = result_depart;
                } else {
                    Session["Department"] = "ERROR";
                }
                Session["DepartmentRawName"] = department;

                var DepartmentResult = M_Login.GetDepartmentIdByDepartmentName(department);

                System.Diagnostics.Debug.WriteLine($"Department ID : {DepartmentResult}");

                if (DepartmentResult != null){
                    var DepartmentID = DepartmentResult.ID;
                    Session["DepartmentID"] = DepartmentID;
                }else{
                    Session["DepartmentID"] = -1;
                }
                Session["Position"] = password;
                return Json(new { Url = redirectUrl });
            }else{
                return this.CheckUser(username,password);
            }
        }

        public string CheckDepartment(string Department){
            string[] PE_Process = {"PE1_Process","PE2_Process"};
            string[] MKT = {"MKT","MKT1","MKT2"};
            string[] IT = {"IT","IT1","IT2"};
            string[] PE = {"PE","PE1","PE2","PE2_SMT","PE2_PCB","PE2_MT"};
            string[] PCH = {"PCH","PCH1","PCH2"};
            string[] PT = {"PT","PT1","PT2","PT3A","PT3M","PT4","PT5","PT6","PT7"};
            string[] PC = {"PC","PC1","PC2"};
            string[] QC = {"QC","QC1","QC2","QC3"};
            string[] QC_IN = {"QC_IN","QC_IN1","QC_IN2","QC_IN3"};
            string[] QC_NFM = {"QC_NFM","QC_NFM1","QC_NFM2","QC_NFM3"};
            string[] QC_FINAL = {"QC_FINAL","QC_FINAL1","QC_FINAL2","QC_FINAL3"};

            var result = "Error";
            
            if(PE_Process.Contains(Department)){
                result = "PE1_Process";
            }else if(MKT.Contains(Department)){
                result = "MKT";
            }else if (IT.Contains(Department)){ 
                result = "IT";
            }else if(PE.Contains(Department)){
                result = "PE";
            }else if(PCH.Contains(Department)){
                result = "PCH";
            }else if(PT.Contains(Department)){
                result = "PT";
            }else if(PC.Contains(Department)){
                result = "PC";
            }else if(QC.Contains(Department)){
                result = "QC";
            }else if(QC_IN.Contains(Department)){
                result = "QC_IN";
            }else if(QC_NFM.Contains(Department)){
                result = "QC_NFM";
            }else if(QC_FINAL.Contains(Department)){
                result = "QC_FINAL";
            }else{
                result = "Not found";
            }

             System.Diagnostics.Debug.WriteLine(result);
            return result;
        }

        public string GetDevMode(){
            return LoginController.DevMode;
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
        }

        public ActionResult SetPosition(string PositionName){
            Session["Position"] = PositionName;
            return Json(1);
        }
    }
}