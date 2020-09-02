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
    public class MailModel{
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        public List<Department> A = new List<Department>();
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public MailModel(){
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

        public Department GetDepartmentIdByDepartmentName(string Department){
            var sql = $"SELECT TOP (1) ID AS Id FROM dbo.[Department] WHERE Department.Name = '{Department}' OR Department.Name LIKE '{Department}%'";
            var result = DB_CCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }

        public TopicAlt GetTopicByCode(string topic_code){
            try{
                var sql = $@"SELECT  Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Status, Status.Name AS FullStatus, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, 
                Topic.ID FROM Topic 
                LEFT JOIN Status ON Topic.Status = Status.id 
                LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                WHERE  Code = '{topic_code}' ORDER BY Revision DESC;";
                var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql).First();
                return Topic;
            }catch(Exception ex){
                TopicAlt blank_topic = new TopicAlt();
                return blank_topic;
            }
        }

        public List<string> GetEmailByDept(string dept){
            try{
                var sql = $@"SELECT [User].Email FROM Permission
                            LEFT JOIN [User] ON [User].Code = Permission.[User]
                            WHERE Permission.Department = '{dept}'
                            AND Active = '1'
                            AND Subscribe = '1'
                            ;";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
            }catch(Exception ex){
                return null;
            }
        }
        public List<string> GetEmailByDeptAndPosition(string dept, string pos = ""){
            try{
                var sql = $@"SELECT [User].Email FROM Permission
                            LEFT JOIN [User] ON [User].Code = Permission.[User]
                            WHERE Permission.Department = '{dept}' ";
                if(pos == "Approver"){
                    sql += $"AND ([Position] = 'Approver' OR [Position] = 'Special' OR [Position] = 'Admin') ";
                }
                sql += @"AND Active = '1'
                        AND Subscribe = '1';";

                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
            }catch(Exception ex){
                return null;
            }
        }

        public List<string> GetRelatedByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT Related.Department
                    FROM Related 
                    LEFT JOIN Topic On Related.PK_Related = Topic.Related
                    WHERE Topic.Code = '{topic_code}'
                    AND Topic.Revision  = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code) 
                    ;";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<string>();
            }
        }
    }
        
}