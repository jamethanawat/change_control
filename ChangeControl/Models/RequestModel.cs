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
using ChangeControl.Helpers;

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
                string query = $@"SELECT CAST(COUNT(*) AS BIT) FROM Topic WHERE Code='{topic_code}' AND User_insert ='{user_id}';";
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

        public long InsertFile(HttpPostedFileBase file, long fk_id, string type, string description, object session_user, string topic_code, string dept){
            string query = $@"INSERT INTO [File] (FK_ID, [Type], Name, Size, Name_Format, Description, Time_Insert, User_Insert, Topic, Department) 
            OUTPUT Inserted.ID VALUES({fk_id}, '{type}', '{file.FileName.ToString().ReplaceSingleQuote()}','{file.ContentLength}','{date_ff}','{description}','{date}','{session_user}', '{topic_code}', '{dept}');";
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
            string query = $@"INSERT INTO Topic (Code, [Type], Change_item, Product_type, Revision , Department, Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing , TimingDesc ,Related, User_insert, Time_insert)  OUTPUT Inserted.ID
            VALUES( '{m.Code}','{m.Type}', {m.Change_item}, '{m.Product_type}', '{m.Revision}', '{m.Department}', '{m.Model}', '{m.PartNo}', '{m.PartName}', '{m.ProcessName}', '{m.Status}', '{m.App}' , '{m.Subject}' , '{m.Detail}', '{m.Timing}', '{m.TimingDesc}', '{m.Related}','{m.User_insert}','{m.Time_insert}' );";
            var result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

         public long UpdateTopicWithRev(Topic m){
            string query = $@"INSERT INTO Topic (Code, [Type], Change_item, Product_type, Revision , Department, Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing , TimingDesc ,Related, User_insert, Time_insert)  
            OUTPUT Inserted.ID VALUES( '{m.Code}','{m.Type}', {m.Change_item} , '{m.Product_type}' , 
                (SELECT MAX(t.Revision)+1 FROM Topic t WHERE t.Code = '{m.Code}') 
             ,'{m.Department}' ,'{m.Model}', '{m.PartNo}', '{m.PartName}', '{m.ProcessName}', '{m.Status}', '{m.App}' , '{m.Subject}' , '{m.Detail}', '{m.Timing}','{m.TimingDesc}', '{m.Related}','{m.User_insert}','{m.Time_insert}' );";
            var result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public long UpdateTopic(Topic m){
            string query = $@"UPDATE Topic SET 
            [Type] = '{m.Type}', Change_item = {m.Change_item} , Product_type = '{m.Product_type}' , Revision  = '{m.Revision}' , Department = '{m.Department}' , Model = '{m.Model}', PartNo =  '{m.PartNo}', PartName = '{m.PartName}', ProcessName = '{m.ProcessName}', Status =  '{m.Status}', [APP/IPP] =  '{m.App}' , Subject = '{m.Subject}' , Detail =  '{m.Detail}', Timing =  '{m.Timing}', TimingDesc =  '{m.TimingDesc}', Related = '{m.Related}', User_insert = '{m.User_insert}', Time_insert = '{m.Time_insert}' OUTPUT Inserted.ID WHERE Code = '{m.Code}' AND Revision = '{m.Revision}';";
            var result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public void DeleteRelated(long related_id){
            string del = $"DELETE FROM Related WHERE ID = {related_id}' ";
            DB_CCS.Database.ExecuteSqlCommand(del);
        }
        public long InsertRelated(List<String> dept_list, string us_id){
            string query = $@"INSERT INTO Related (PK_Related , Department, UpdatedBy) 
            OUTPUT Inserted.PK_Related VALUES ";
            dept_list.ForEach(dept => {
                query += $"((SELECT TOP(1) MAX(PK_Related)+1 FROM Related), '{dept}', '{us_id}'),";
            });
            query = query.TrimEnd(',');
            
            var result = DB_CCS.Database.SqlQuery<long>(query);
            return result.First();   
        }

        public List<RelatedAlt> GetRelatedByID(long related_id){
            var sql=$@"SELECT Department
                FROM Related 
                WHERE PK_Related = {related_id};";
            var result = DB_CCS.Database.SqlQuery<RelatedAlt>(sql).ToList();
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
            var sql = "SELECT ID_Change_item as ID, Name as Name FROM Change_Item WHERE ID_Change_item BETWEEN 1 AND 12;";
            var result = DB_CCS.Database.SqlQuery<ChangeItem>(sql);
            return result.ToList();
        }
        
        public List<ProductType> GetProductType(){
            var sql = "SELECT ID_Product_Type as ID, Name as Name FROM Product_Type WHERE ID_Product_Type BETWEEN 1 AND 8;";
            var result = DB_CCS.Database.SqlQuery<ProductType>(sql);
            return result.ToList();
        }

        public List<Department> GetDepartmentByGroup(string DepartmentGroup){
            var sql = $"SELECT ID as ID, Name, [Group] FROM Department WHERE [Group] = '{DepartmentGroup}';";
            var result = DB_CCS.Database.SqlQuery<Department>(sql);
            return result.ToList();    
        }

        public List<string> GetDepartmentGroup(){
            var sql = "SELECT [Group] FROM Department WHERE [Group] != 'Guest' GROUP BY [Group];";
            var result = DB_CCS.Database.SqlQuery<string>(sql);
            return result.ToList();    
        }
        public List<string> GetDepartment(){
            var sql = $"SELECT Name FROM Department;";
            var result = DB_CCS.Database.SqlQuery<string>(sql);
            return result.ToList();    
        }

        public TopicAlt GetTopicByID(string topic_code){
            var sql = $"SELECT TOP(1) Code, [Type], Change_Item.Name as Change_item, Product_Type.Name AS Product_Type  , Department, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, ID FROM Topic LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type WHERE Code = '{topic_code}' ORDER BY Revision DESC;";
            var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql);
            return Topic.First();
        }
        public List<FileItem> GetFileByID(long fk_id, string type, string topic_code, string dept){
            try{
                var sql = $"SELECT ID, FK_ID, [Type], Name, Name_Format, Description, [Size], Time_Insert, User_Insert FROM [File] WHERE [Type] = '{type}' AND Topic = '{topic_code}' AND Department = '{dept}'";
                var FileList = DB_CCS.Database.SqlQuery<FileItem>(sql).ToList();
                return FileList;
            }catch(Exception err){
                return null;
            }
        }

        public void InsertTopicApprove(string topic_code){
            var sql = $"INSERT INTO Topic_Approve (Topic, RequestBy, RequestDate, ReviewBy, ReviewDate, TrialBy, TrialDate, CloseBy, CloseDate) VALUES('{topic_code}', '', '', '', '', '', '', '', ''); ";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public int InsertOtherChangeItem(string desc){
            var sql = $"INSERT INTO Change_Item (Name) OUTPUT Inserted.ID_Change_Item VALUES('{desc}');";
            var result = DB_CCS.Database.SqlQuery<int>(sql).First();
            return result;
        }

        public int InsertOtherProductType(string desc){
            var sql = $"INSERT INTO Product_Type (Name) OUTPUT Inserted.ID_Product_Type VALUES('{desc}');";
            var result = DB_CCS.Database.SqlQuery<int>(sql).First();
            return result;
        }

    }
}