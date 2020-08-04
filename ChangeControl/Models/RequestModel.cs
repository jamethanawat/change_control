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
    public class RequestModel{
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;


        private DataSet ds = new DataSet();
        private DataSet ds2 = new DataSet();
        private SqlDataAdapter da = new SqlDataAdapter();
        private SqlDataAdapter da2 = new SqlDataAdapter();
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        private string date;
        private string date_ff;
        private object ForeignKey;
        public class Value
        {
            public List<string> value { get; set; }
        }

        public RequestModel(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            date = DateTime.Now.ToString("yyyyMMddHHmmss");
            date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        }

        public string GetRequest(string ID){
            cn.Open();

            string sql =    $@"SELECT  dbo.Topic.*, dbo.Related.*, dbo.[File].*
                            FROM dbo.Topic left JOIN dbo.Related ON dbo.Topic.Related = dbo.Related.ID
                                left JOIN  dbo.[File] ON dbo.Topic.[File] = dbo.[File].ID_File
                            WHERE dbo.Topic.Type LIKE 'ID'";

            da = new SqlDataAdapter(sql, cn);
            da.Fill(ds, "item");
            var old_file = ds.Tables["item"].Rows[0][14].ToString();
            
            cn.Close();

            return old_file;
        } 

        public int GetCountFile(){
            cn.Open();

            string sqlFile =    @"SELECT  COUNT(ID_File) AS count
                                FROM   dbo.[File]
                                WHERE   ID_File = '" + ds.Tables["item"].Rows[0][14].ToString() + "'";

            da2 = new SqlDataAdapter(sqlFile, cn);
            da2.Fill(ds2, "itemrow");
            var count_item = Int32.Parse(ds2.Tables["itemrow"].Rows[0][0].ToString());
            
            cn.Close();

            return count_item;
        }

        public DataSet GetRequestDataSet(){
            return ds;
        }

        public bool CheckTopicOwner(string user_id, string topic_code){
            try{
                string query = $@"SELECT CAST(COUNT(*) AS BIT) FROM CCS.dbo.Topic WHERE Code='{topic_code}' AND User_insert ='{user_id}';";
                var result = DB_CCS.Database.SqlQuery<bool>(query).First();
                return result;
            }catch(Exception err){
                return false;
            }
        }

        public void SetForeignKey(object ForeignKey){
            this.ForeignKey = ForeignKey;
        }

        public void DeleteFile(){
            string del = "DELETE FROM [File] WHERE ID_File like '%" + ForeignKey + "%' ";
            DB_CCS.Database.ExecuteSqlCommand(del);
        }

        public long InsertFile(HttpPostedFileBase file, long fk_id, string type, string description, object session_user){
            string query = $@"INSERT INTO [File] (FK_ID, [Type], Name, Size, Name_Format, Description, Time_Insert, User_Insert) 
            OUTPUT Inserted.ID VALUES({fk_id}, '{type}', '{file.FileName.ToString()}','{file.ContentLength}','{date_ff}','{description}','{date}','{session_user}');";
            long result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public void UpdateFile(string id, object description){
            long ID = int.Parse(id);
            string query = $"UPDATE [File] SET Description= '{description}', Time_Insert ='{date}' WHERE ID = {ID}";
            DB_CCS.Database.ExecuteSqlCommand(query);
        }
        
        public List<GetID> GetExternalTopicId(){
            var sql = $"SELECT TOP(1) Code FROM Topic WHERE Code LIKE 'EX-{date.Substring(2,2)}%' ORDER BY Code DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public List<GetID> GetInternalTopicId(){
            var sql = $"SELECT TOP(1) Code FROM Topic WHERE Code LIKE 'IN-{date.Substring(2,2)}%' ORDER BY Code DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }

        public long InsertTopic(Topic m){
            string query = $@"INSERT INTO Topic (Code, [Type], Change_item, Product_type, Revision , Department, Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing ,Related, User_insert, Time_insert)  OUTPUT Inserted.ID
            VALUES( '{m.Code}','{m.Type}', {m.Change_item}, '{m.Product_type}', '{m.Revision}', '{m.Department}', '{m.Model}', '{m.PartNo}', '{m.PartName}', '{m.ProcessName}', '{m.Status}', '{m.App}' , '{m.Subject}' , '{m.Detail}', '{m.Timing}', '{m.Related}','{m.User_insert}','{m.Time_insert}' );";
            var result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

         public long UpdateTopic(Topic m){
            string query = $@"INSERT INTO Topic (Code, [Type], Change_item, Product_type, Revision , Department, Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing ,Related, User_insert, Time_insert)  
            OUTPUT Inserted.ID VALUES( '{m.Code}','{m.Type}', {m.Change_item} , '{m.Product_type}' , '{m.Revision}' ,'{m.Department}' ,'{m.Model}', '{m.PartNo}', '{m.PartName}', '{m.ProcessName}', '{m.Status}', '{m.App}' , '{m.Subject}' , '{m.Detail}', '{m.Timing}','{m.Related}','{m.User_insert}','{m.Time_insert}' );";
            var result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public void DeleteRelated(long related_id){
            string del = $"DELETE FROM Related WHERE ID = {related_id}' ";
            DB_CCS.Database.ExecuteSqlCommand(del);
        }
        public long InsertRelated(Related obj){
            string query = $@"INSERT INTO Related (P1, P2, P3A, P3M, P4, P5, P6, P7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process) OUTPUT Inserted.ID 
            VALUES('{obj.P1}', '{obj.P2}', '{obj.P3A}', '{obj.P3M}', '{obj.P4}', '{obj.P5}', '{obj.P6}', '{obj.P7}', '{obj.IT}', '{obj.MKT}', '{obj.PC1}', '{obj.PC2}', '{obj.PCH1}', '{obj.PCH2}', '{obj.PE1}', '{obj.PE2}', '{obj.PE2_SMT}', '{obj.PE2_PCB}', '{obj.PE2_MT}', '{obj.QC_IN1}', '{obj.QC_IN2}', '{obj.QC_IN3}', '{obj.QC_FINAL1}', '{obj.QC_FINAL2}', '{obj.QC_FINAL3}', '{obj.QC_NFM1}', '{obj.QC_NFM2}', '{obj.QC_NFM3}', '{obj.QC1}', '{obj.QC2}', '{obj.QC3}', '{obj.PE1_Process}', '{obj.PE2_Process}');";
            var result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public Related GetRelatedByID(long related_id){
            string query = $@"SELECT ID, P1, P2, P3A, P3M, P4, P5, P6, P7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related WHERE ID = {related_id}";
            var result = DB_CCS.Database.SqlQuery<Related>(query).First();
            return result;
        }

        public void RemoveData(long related_id,string session_key){
            string DEquery = $"DELETE Related WHERE ID =  {related_id}";
                DB_CCS.Database.ExecuteSqlCommand(DEquery);
                //  DEquery = "DELETE [File] WHERE ID_File = '" + key + "'";
                // DB_CCS.Database.ExecuteSqlCommand(DEquery);
                //  DEquery = "DELETE Topic WHERE Code = '"+ session_key + "'";
                // DB_CCS.Database.ExecuteSqlCommand(DEquery);
        }

        public List<Summernote> GetSummernote(string ID){
             string query = $"SELECT [APP/IPP] as APP , Subject , Detail, Timing FROM Topic WHERE Code =  '{ID}'";
            var result = DB_CCS.Database.SqlQuery<Summernote>(query).ToList();
            return result;
        }

        public List<ListMail> GetEmail(string user){
            var sql = "SELECT Email FROM dbo.[User] WHERE Name ='" + user + "'";
            var result = DB_CCS.Database.SqlQuery<ListMail>(sql).ToList();
            return result;
        }

        public List<ChangeItem> GetChangeItem(){
            var sql = "SELECT ID_Change_item as ID, Name as Name FROM CCS.dbo.Change_Item WHERE ID_Change_item BETWEEN 1 AND 12;";
            var result = DB_CCS.Database.SqlQuery<ChangeItem>(sql);
            return result.ToList();
        }
        
        public List<ProductType> GetProductType(){
            var sql = "SELECT ID_Product_Type as ID, Name as Name FROM CCS.dbo.Product_Type WHERE ID_Product_Type BETWEEN 1 AND 8;";
            var result = DB_CCS.Database.SqlQuery<ProductType>(sql);
            return result.ToList();
        }

        public List<string> GetDepartmentGroup(){
            var sql = "SELECT [Group] FROM CCS.dbo.Department GROUP BY [Group];";
            var result = DB_CCS.Database.SqlQuery<string>(sql);
            return result.ToList();    
        }

        public List<Department> GetDepartmentByGroup(string DepartmentGroup){
            var sql = $"SELECT ID_Department as ID, Name, Email, [Group] FROM CCS.dbo.Department WHERE [Group] = '{DepartmentGroup}';";
            var result = DB_CCS.Database.SqlQuery<Department>(sql);
            return result.ToList();    
        }

        public TopicAlt GetTopicByID(string topic_code){
            var sql = $"SELECT TOP(1) Code, [Type], Change_Item.Name as Change_item, Product_Type.Name AS Product_Type  , Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, ID FROM CCS.dbo.Topic LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type WHERE Code = '{topic_code}' ORDER BY Revision DESC;";
            var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql);
            return Topic.First();
        }
        public List<FileItem> GetFileByID(long fk_id, string type){
            try{
                var sql = $"SELECT ID, FK_ID, [Type], Name, Name_Format, Description, [Size], Time_Insert, User_Insert FROM CCS.dbo.[File] WHERE FK_ID = {fk_id} AND [Type] = '{type}'";
                var FileList = DB_CCS.Database.SqlQuery<FileItem>(sql).ToList();
                return FileList;
            }catch(Exception err){
                return null;
            }
        }

        public void InsertTopicApprove(string topic_code){
            var sql = $"INSERT INTO CCS.dbo.Topic_Approve (Topic, RequestBy, RequestDate, ReviewBy, ReviewDate, TrialBy, TrialDate, CloseBy, CloseDate) VALUES('{topic_code}', '', '', '', '', '', '', '', ''); ";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public int InsertOtherChangeItem(string desc){
            var sql = $"INSERT INTO CCS.dbo.Change_Item (Name) OUTPUT Inserted.ID_Change_Item VALUES('{desc}');";
            var result = DB_CCS.Database.SqlQuery<int>(sql).First();
            return result;
        }

        public int InsertOtherProductType(string desc){
            var sql = $"INSERT INTO CCS.dbo.Product_Type (Name) OUTPUT Inserted.ID_Product_Type VALUES('{desc}');";
            var result = DB_CCS.Database.SqlQuery<int>(sql).First();
            return result;
        }

    }
}