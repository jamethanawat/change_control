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
    public class UserModel{
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        public List<Department> A = new List<Department>();
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public UserModel(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
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
                    User temp_user = new User(conAD.ChkFullName(user), conAD.ChkName(user), conAD.ChkSurName(user),conAD.ChkDept(user), conAD.ChkPosition(user));
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

    }
        
}