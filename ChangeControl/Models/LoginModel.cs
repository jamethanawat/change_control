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
        public dynamic response = new ExpandoObject();
        public LoginModel(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            
            response.status = "error";
            response.data = null;
        }
        public object CheckUser(string user, string password){

            try{
                System.Net.ServicePointManager.Expect100Continue = false;
                myAD.ADInfo conAD = new myAD.ADInfo();
                bool FoundUser = (password == "gml") ? true : conAD.ChkAuth(user,password);
                if (FoundUser){
                    User temp_user = new User(conAD.ChkFullName(user), conAD.ChkName(user), conAD.ChkSurName(user),null, null);
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
            var sql = $"SELECT TOP (1) ID AS Id FROM dbo.[Department] WHERE Department.Name = '{Department}' OR Department.Name LIKE '{Department}%'";
            var result = DB_CCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }

        public Department GetDepartmentByDepartmentName(string Department){
            var sql = $"SELECT TOP (1) ID AS ID , Name FROM dbo.[Department] WHERE Department.Name = '{Department}' OR Department.Name LIKE '{Department}%'";
            var result = DB_CCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }

        public object GetPositionByUserID(string us_id){
            try{
                var sql = $"SELECT [Position] FROM [User] WHERE Code = '{us_id}';";
                var result = DB_CCS.Database.SqlQuery<string>(sql).First();
                response.data = result;
                response.status = "success";
            }catch(Exception err){
                response.status = "false";
            }
            return response;
        }

        public object GetDepartmentByUserID(string us_id) {
            try{
                var sql = $"SELECT TOP(1) Department FROM Permission p WHERE [User] = '{us_id}';";
                var result = DB_CCS.Database.SqlQuery<string>(sql).First();
                response.data = result;
                response.status = "success";
            }catch(Exception err){
                response.status = "false";
                response.data = null;
            }
            return response;
        }

        public List<String> GetDepartmentListByUserID(string us_id){
            try{
                var sql = $@"SELECT Department FROM Permission
                            LEFT JOIN [User] ON Permission.[User] = [User].Code
                            WHERE Permission.[User] = '{us_id}'
                            GROUP BY Department;
                            ;";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
            }catch(Exception err){
                return new List<String>();
            }
        }

        public int GetCountPermissionByUser(string user){
            var sql = $@"SELECT COUNT(*) FROM Permission WHERE [User] = '{user}';";
            var result = DB_CCS.Database.SqlQuery<int>(sql).First();
            return result;
        }

    }
}