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
            var result = DB_CCS.Database.SqlQuery<Department>(sql).First();
            return result;
        }

        public TopicAlt GetTopicByCode(string topic_code){
            try{
                var sql = $@"SELECT  Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, 
                ID FROM CCS.dbo.Topic 
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
                var sql = $@"SELECT Email FROM CCS.dbo.[User] where Dept = '{dept}';";
                var result = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                return result;
            }catch(Exception ex){
                return null;
            }

        }

        public Related GetRelatedByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT P1, P2, P3A, P3M, P4, P5, P6, P7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
                FROM CCS.dbo.Related 
                LEFT JOIN Topic ON Related.ID = Topic.Related 
                WHERE Topic.Code = '{topic_code}';";
                var result = DB_CCS.Database.SqlQuery<Related>(sql).First();
                return result;
            }catch(Exception ex){
                Related blank_related = new Related();
                return blank_related;
            }
        }
    }
        
}