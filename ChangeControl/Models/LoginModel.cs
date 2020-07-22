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

namespace ChangeControl.Models{
    public class LoginModel{
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        public List<Department> A = new List<Department>();
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public LoginModel(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
        }
        public object CheckUser(string user, string password){

            dynamic response = new ExpandoObject();
                    response.status = "error";
                    response.data = null;
            try{
                System.Net.ServicePointManager.Expect100Continue = false;
                myAD.ADInfo conAD = new myAD.ADInfo();
                bool FoundUser = conAD.ChkAuth(user,password);
                if (FoundUser){
                    User temp_user = new User(conAD.ChkFullName(user), conAD.ChkName(user), conAD.ChkSurName(user), conAD.ChkEmail(user),null, conAD.ChkPosition(user));
                    response.message = "success";
                    response.data = temp_user;
                    return response;
                }else{
                    if(conAD.ChkFullName(user) != ""){
                        response.status = "wrong_pwd";
                    }else{
                        response.status = "wrong_us";
                    }
                    return response;
                }
             
            }
            catch (Exception er){
                return response;
            }  
         } 

        public Department GetDepartmentIdByDepartmentName(string Department){
            var sql = $"SELECT TOP (1) ID_Department AS Id FROM dbo.[Department] WHERE Department.Name = '{Department}' OR Department.Name LIKE '{Department}%'";
            var result = DB_CCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }

        public Department GetDepartmentByDepartmentName(string Department){
            var sql = $"SELECT TOP (1) ID_Department AS ID , Name FROM dbo.[Department] WHERE Department.Name = '{Department}' OR Department.Name LIKE '{Department}%'";
            var result = DB_CCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }

        
    }
}