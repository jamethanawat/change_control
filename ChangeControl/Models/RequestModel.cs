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
        private DbTapics _dbtapics;
        private DbCCS _dbCCS;


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
            _dbtapics = new DbTapics();
            _dbCCS = new DbCCS();
            date = DateTime.Now.ToString("yyyyMMddHHmmss");
            date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        }

        public string GetRequest(string ID){
            cn.Open();

            string sql =    @"SELECT  dbo.Topic.*, dbo.Related.*, dbo.[File].*
                            FROM dbo.Topic left JOIN dbo.Related ON dbo.Topic.Related = dbo.Related.ID_Related
                                left JOIN  dbo.[File] ON dbo.Topic.[File] = dbo.[File].ID_File
                            WHERE dbo.Topic.ID_Topic like '" + ID+"'";

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

        public void SetForeignKey(object ForeignKey){
            this.ForeignKey = ForeignKey;
        }

        public void DeleteFile(){
            string del = "DELETE FROM [File] WHERE ID_File like '%" + ForeignKey + "%' ";
            _dbCCS.Database.ExecuteSqlCommand(del);
        }

        public int InsertFile(HttpPostedFileBase file, string FileCode, string description, object session_user){
            string query = $"INSERT INTO [File] ( ID_File, Name, Size, Name_Format, Description, Time_Insert, User_Insert) OUTPUT Inserted.ID VALUES( '{FileCode}','{file.FileName.ToString()}','{file.ContentLength}','{date_ff}','{description}','{date}','{session_user}');";
            // _dbCCS.Database.ExecuteSqlCommand(query);
            var result = _dbCCS.Database.SqlQuery<int>(query).First();
            return result;
        }

        public void UpdateFile(string id, object description){
            int ID = int.Parse(id);
            string query = $"UPDATE [File] SET Description= '{description}', Time_Insert ='{date}' WHERE ID = {ID}";
            _dbCCS.Database.ExecuteSqlCommand(query);
        }
        public void UpdateTopicFileCode(int ReviewID, string FileCode){
            string query = $"UPDATE CCS.dbo.Topic SET [File]='{FileCode}' WHERE ID = {ReviewID}";
            _dbCCS.Database.ExecuteSqlCommand(query);
        }
        
        public List<GetID> GetExternalTopicId(){
            var sql = $"SELECT TOP(1) ID_Topic FROM Topic WHERE ID_Topic LIKE 'EX-{date.Substring(2,2)}%' ORDER BY ID_Topic DESC";
            var dept = _dbCCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public List<GetID> GetInternalTopicId(){
            var sql = $"SELECT TOP(1) ID_Topic FROM Topic WHERE ID_Topic LIKE 'IN-{date.Substring(2,2)}%' ORDER BY ID_Topic DESC";
            var dept = _dbCCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }

        public int InsertTopic(Topic m){
            string query = "INSERT INTO Topic (ID_Topic,Topic_type, Change_item, Product_type, Revision , Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing ,Related, User_insert, Time_insert)  OUTPUT Inserted.ID ";
            query = query + $"VALUES( '{m.ID_Topic}','{m.Topic_type}', {m.Change_item} , '{m.Product_type}' , '{m.Revision}' ,'{m.Model}', '{m.PartNo}', '{m.PartName}', '{m.ProcessName}', '{m.Status}', '{m.App}' , '{m.Subject}' , '{m.Detail}', '{m.Timing}', '{m.Related}','{m.User_insert}','{m.Time_insert}' );";
            var result = _dbCCS.Database.SqlQuery<int>(query).First();
            return result;
        }

         public int UpdateTopic(Topic m){
            string query = $@"INSERT INTO Topic (ID_Topic,Topic_type, Change_item, Product_type, Revision , Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing , [File],Related, User_insert, Time_insert)  OUTPUT Inserted.ID VALUES( '{m.ID_Topic}','{m.Topic_type}', {m.Change_item} , '{m.Product_type}' , '{m.Revision}' ,'{m.Model}', '{m.PartNo}', '{m.PartName}', '{m.ProcessName}', '{m.Status}', '{m.App}' , '{m.Subject}' , '{m.Detail}', '{m.Timing}','{m.File}','{m.Related}','{m.User_insert}','{m.Time_insert}' );";
            var result = _dbCCS.Database.SqlQuery<int>(query).First();
            return result;
        }

        public void DeleteRelated(string ID_Related){
            string del = $"DELETE FROM Related WHERE ID_Related like '% {ID_Related}%' ";
            _dbCCS.Database.ExecuteSqlCommand(del);
        }
        public void InsertRelated(string key,Related obj){
            string query = "INSERT INTO Related (ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process)";
            query = query + $"VALUES( '{key}','{obj.PT1}', '{obj.PT2}', '{obj.PT3A}', '{obj.PT3M}', '{obj.PT4}', '{obj.PT5}', '{obj.PT6}', '{obj.PT7}', '{obj.IT}', '{obj.MKT}', '{obj.PC1}', '{obj.PC2}', '{obj.PCH1}', '{obj.PCH2}', '{obj.PE1}', '{obj.PE2}', '{obj.PE2_SMT}', '{obj.PE2_PCB}', '{obj.PE2_MT}', '{obj.QC_IN1}', '{obj.QC_IN2}', '{obj.QC_IN3}', '{obj.QC_FINAL1}', '{obj.QC_FINAL2}', '{obj.QC_FINAL3}', '{obj.QC_NFM1}', '{obj.QC_NFM2}', '{obj.QC_NFM3}', '{obj.QC1}', '{obj.QC2}', '{obj.QC3}', '{obj.PE1_Process}', '{obj.PE2_Process}');";
            _dbCCS.Database.ExecuteSqlCommand(query);
        }

        public Related GetRelatedByID(string ID){
            string query = $"SELECT ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process FROM CCS.dbo.Related WHERE ID_Related = '{ID}'";
            var result = _dbCCS.Database.SqlQuery<Related>(query).First();
            return result;
        }

        public void RemoveData(string key,string session_key){
            string DEquery = "DELETE Related WHERE ID_Related =  '" + key + "'";
                _dbCCS.Database.ExecuteSqlCommand(DEquery);
                 DEquery = "DELETE [File] WHERE ID_File = '" + key + "'";
                _dbCCS.Database.ExecuteSqlCommand(DEquery);
                 DEquery = "DELETE Topic WHERE ID_Topic = '"+ session_key + "'";
                _dbCCS.Database.ExecuteSqlCommand(DEquery);
        }

        public List<Summernote> GetSummernote(string ID){
             string query = $"SELECT [APP/IPP] as APP , Subject , Detail, Timing FROM Topic WHERE ID_Topic =  '{ID}'";
            var result = _dbCCS.Database.SqlQuery<Summernote>(query).ToList();
            return result;
        }

        public List<ListMail> GetEmail(string user){
            var sql = "SELECT Email FROM dbo.[User] WHERE Name ='" + user + "'";
            var result = _dbCCS.Database.SqlQuery<ListMail>(sql).ToList();
            return result;
        }

        public List<ChangeItem> GetChangeItem(){
            var sql = "SELECT ID_Change_item as ID, Item as Name FROM CCS.dbo.Change_Item;";
            var result = _dbCCS.Database.SqlQuery<ChangeItem>(sql);
            return result.ToList();
        }
        
        public List<ProductType> GetProductType(){
            var sql = "SELECT ID_Product_Type as ID, Product_Type as Name FROM CCS.dbo.Product_Type;";
            var result = _dbCCS.Database.SqlQuery<ProductType>(sql);
            return result.ToList();
        }

        public List<string> GetDepartmentGroup(){
            var sql = "SELECT [Group] FROM CCS.dbo.Department GROUP BY [Group];";
            var result = _dbCCS.Database.SqlQuery<string>(sql);
            return result.ToList();    
        }

        public List<Department> GetDepartmentByGroup(string DepartmentGroup){
            var sql = $"SELECT ID_Department as ID, Name, Email, [Group] FROM CCS.dbo.Department WHERE [Group] = '{DepartmentGroup}';";
            var result = _dbCCS.Database.SqlQuery<Department>(sql);
            return result.ToList();    
        }

        public TopicAlt GetTopicByID(string TopicID){
            var sql = $"SELECT TOP(1) ID_Topic, Topic_type, Item as Change_item, Product_Type.Product_type, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, [File], Related, User_insert, Time_insert, ID FROM CCS.dbo.Topic LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type WHERE ID_Topic = '{TopicID}' ORDER BY Revision DESC;";
            var Topic = _dbCCS.Database.SqlQuery<TopicAlt>(sql);
            return Topic.First();
        }
        public List<FileItem> GetFileByID(string FileCode){
            var sql = $"SELECT ID, ID_File, Name, Size, Name_Format, Description, Time_Insert, User_Insert FROM CCS.dbo.[File] WHERE ID_File = '{FileCode}'; ";
            var FileList = _dbCCS.Database.SqlQuery<FileItem>(sql).ToList();
            return FileList;
        }

        public void InsertTopicApprove(int topic_id){
            var sql = $"INSERT INTO CCS.dbo.Topic_Approve (Topic, RequestBy, RequestDate, ReviewBy, ReviewDate, TrialBy, TrialDate, CloseBy, CloseDate) VALUES({topic_id}, '', '', '', '', '', '', '', ''); ";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }

    }
}