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
        private DbTapics _dbtapics;
        private DbCCS _dbCCS;
        public List<Department> A = new List<Department>();
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public LoginModel(){
            _dbtapics = new DbTapics();
            _dbCCS = new DbCCS();
        }
        public object CheckUser(string user, string password){

            dynamic response = new ExpandoObject();
                    response.message = "error";
                    response.data = null;
            try{
                System.Net.ServicePointManager.Expect100Continue = false;
                myAD.ADInfo conAD = new myAD.ADInfo();
                bool FoundUser = conAD.ChkAuth(user,password);
                if (FoundUser){
                    User temp_user = new User(conAD.ChkFullName(user), conAD.ChkName(user), conAD.ChkSurName(user), conAD.ChkEmail(user),conAD.ChkDept(user), conAD.ChkPosition(user));
                    response.message = "success";
                    response.data = temp_user;
                    return response;
                }else{
                    return response;
                }
             
            }
            catch (Exception er){
                return response;
            }  
         } 

        public Department GetDepartmentIdByDepartmentName(string Department){
            var sql = $"SELECT TOP (1) ID_Department AS Id FROM dbo.[Department] WHERE Department.Name = '{Department}' OR Department.Name LIKE '{Department}%'";
            var result = _dbCCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }
        
    }
}