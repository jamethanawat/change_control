using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class DetailModel
    {
        private DbTapics DB_Tapics;
        private DbCCS DB_CCS;
        private string date;
        private string date_ff;
        private object foreign_key;

        public long review_id;
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public DetailModel()
        {
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            date = DateTime.Now.ToString("yyyyMMddHHmmss");
            date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        }

        // public class ReviewItem{
        //     public string Name { get; set; }
        // }
        // public class Department{
        //     //public List<string> ID_Department { get; set; }
        //      public string Id { get; set; }
        // }
        public List<FormReviewItem> GetReviewItemByDepartment(int department_id)
        {
            var sql = $@"SELECT Review_Item_Type.Name AS Name, Review_Item_Type.ID AS ID  FROM Department
                        LEFT JOIN Review_Item_Type_Department ON Department.ID_Department = Review_Item_Type_Department.FK_Department_ID
                        LEFT JOIN Review_Item_Type ON Review_Item_Type_Department.FK_Item_ID = Review_Item_Type.ID
                        WHERE Department.ID_Department = '{department_id}'
                        GROUP BY Review_Item_Type.Name , Review_Item_Type.Seq ,Review_Item_Type.ID
                        ORDER BY Review_Item_Type.Seq";
            var Review_Item = DB_CCS.Database.SqlQuery<FormReviewItem>(sql).ToList();
            return Review_Item;
        }

        public void SetForeignKey(object foreign_key)
        {
            this.foreign_key = foreign_key;
        }
        public List<GetID> GetExternalTopicId()
        {
            var sql = $@"SELECT TOP(1) ID_Topic FROM 
            Topic WHERE ID_Topic LIKE 'EX-{date.Substring(2, 4)}%' ORDER BY ID_Topic DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public List<GetID> GetInternalTopicId()
        {
            var sql =$@"SELECT TOP(1) ID_Topic FROM Topic 
            WHERE ID_Topic LIKE 'IN-{date.Substring(2, 4)} %' ORDER BY ID_Topic DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public void InsertReviewItem(string status, string desc, int item_type, long review_id)
        {
            string query = null;
            if (status == null || status == ""){
                query = $@"INSERT INTO CCS.dbo.Review_Item (Description, FK_Item_Type, FK_Review_ID) 
                VALUES('{desc}', {item_type}, {review_id});";
            }
            else{
                query = $@"INSERT INTO CCS.dbo.Review_Item (Status, Description, FK_Item_Type, FK_Review_ID) VALUES('{status}', '{desc}', {item_type}, {review_id});";
            }
            
            DB_CCS.Database.ExecuteSqlCommand(query);
        }

        public long InsertReview(long topic_id, string file_id, string UserID, string Department){
            string query = null;
            if (file_id == null || file_id == ""){
                query = $"INSERT INTO CCS.dbo.Review (Topic,  [Date], [User], Department, UpdateBy, UpdateDate) OUTPUT Inserted.ID_Review VALUES('{topic_id}', {date}, '{UserID}', '{Department}', '{UserID}', {date});";
            }else{
                query = $"INSERT INTO CCS.dbo.Review (Topic, [Date], [User], Department, UpdateBy, UpdateDate) OUTPUT Inserted.ID_Review VALUES('{topic_id}', '{file_id}', {date}, '{UserID}', '{Department}', '{UserID}', {date});";
            }
            var ReviewID = DB_CCS.Database.SqlQuery<long>(query).First();
            return ReviewID;
        }

        public long InsertFile(HttpPostedFileBase file, long fk_id, string type, string description, object session_user){
            string query = $@"INSERT INTO [File] (FK_ID, [Type], Name, Size, Name_Format, Description, Time_Insert, User_Insert) 
            OUTPUT Inserted.ID VALUES({fk_id}, '{type}', '{file.FileName.ToString()}','{file.ContentLength}','{date_ff}','{description}','{date}','{session_user}');";
            long result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public string GetFirstTopic(){
            string query = $"SELECT TOP(1) ID_Topic FROM CCS.dbo.Topic;";
            var result = DB_CCS.Database.SqlQuery<string>(query).First();
            return result;

        }
        public TopicAlt GetTopicByID(string TopicID){
            try{
                var sql = $@"SELECT ID_Topic, Topic_type, Item as Change_item, Product_Type.Product_type, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, 
                ID FROM CCS.dbo.Topic 
                LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                WHERE ID_Topic = '{TopicID}' ORDER BY Revision DESC;";
                var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql).First();
                return Topic;
            }catch(Exception ex){
                TopicAlt blank_topic = new TopicAlt();
                return blank_topic;
            }
            
        }

        public void UpdateFileDesc(long? file_id, string Text)
        {
            var sql = $"UPDATE CCS.dbo.[File] SET Description = '{Text}' WHERE ID = '{file_id}'";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<Review> GetReviewByTopicID(long topic_id)
        {
            var sql = $@"SELECT ID_Review, Topic, [Date], [User], Status, Department 
            FROM CCS.dbo.Review 
            WHERE Topic = {topic_id} ORDER BY ID_Review ASC;";
            var ReviewResult = DB_CCS.Database.SqlQuery<Review>(sql).ToList();
            return ReviewResult;
        }

        public List<ReviewItem> GetReviewItemByReviewID(long review_id)
        {
            var sql = $@"SELECT Review_Item.ID, Status, Description, Name FROM CCS.dbo.Review_Item 
            LEFT JOIN Review_Item_Type ON FK_Item_Type = Review_Item_Type.ID 
            WHERE FK_Review_ID = {review_id} ORDER BY Review_Item_Type.Seq ASC;";
            var ReviewItem = DB_CCS.Database.SqlQuery<ReviewItem>(sql).ToList();
            return ReviewItem;
        }

        public List<FileItem> GetFileByID(long fk_id, string type){
            var sql = $"SELECT ID, FK_ID, [Type], Name, Name_Format, Description, [Size], Time_Insert, User_Insert FROM CCS.dbo.[File] WHERE FK_ID = {fk_id} AND [Type] = '{type}'";
            var FileList = DB_CCS.Database.SqlQuery<FileItem>(sql).ToList();
            return FileList;
        }

        public User getUserByID(string id){
            System.Net.ServicePointManager.Expect100Continue = false;
            myAD.ADInfo conAD = new myAD.ADInfo();
            User temp_user = new User(conAD.ChkFullName(id), conAD.ChkName(id), conAD.ChkSurName(id), conAD.ChkEmail(id),conAD.ChkDept(id), conAD.ChkPosition(id));
            return temp_user;
        }

        public void InsertResubmit(string desc, string due_date, long related, long topic_id, string user_id, int status){
            var sql = $@"INSERT INTO CCS.dbo.Resubmit (Description, DueDate, [Date], Related, Topic, [User], Status) 
            VALUES('{desc}', '{due_date}', '{date}', '{related}', '{topic_id}', '{user_id}', {status});";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        
        public List<Resubmit> GetResubmitByTopicID(long topic_id){
            var sql = $@"SELECT ID, Description, DueDate, [Date], Related, Topic, [User] 
            FROM CCS.dbo.Resubmit WHERE Topic = '{topic_id}' ORDER BY [Date] DESC;";
            var result = DB_CCS.Database.SqlQuery<Resubmit>(sql).ToList();
            return result;
        }
        
        public List<Response> GetResponseByResubmitID(long resubmit_id){
            var sql = $@"SELECT ID, Description, Department, [User], [Date] 
            FROM CCS.dbo.Response 
            WHERE Resubmit = {resubmit_id} ORDER BY [Date];";
            var result = DB_CCS.Database.SqlQuery<Response>(sql).ToList();
            return result;
        }

        public Related GetRelatedByResubmitTopicID(long topic_id){
            var sql = $@"SELECT ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related 
            LEFT JOIN Resubmit ON Related.ID = Resubmit.Related 
            WHERE Resubmit.Topic = {topic_id};";
            var result = DB_CCS.Database.SqlQuery<Related>(sql).First();
            return result;
        }

        public int InsertRelated(string key,Related obj){
            string query = $@"INSERT INTO Related (ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process) 
            OUTPUT Inserted.ID 
            VALUES( '{key}','{obj.PT1}', '{obj.PT2}', '{obj.PT3A}', '{obj.PT3M}', '{obj.PT4}', '{obj.PT5}', '{obj.PT6}', '{obj.PT7}', '{obj.IT}', '{obj.MKT}', '{obj.PC1}', '{obj.PC2}', '{obj.PCH1}', '{obj.PCH2}', '{obj.PE1}', '{obj.PE2}', '{obj.PE2_SMT}', '{obj.PE2_PCB}', '{obj.PE2_MT}', '{obj.QC_IN1}', '{obj.QC_IN2}', '{obj.QC_IN3}', '{obj.QC_FINAL1}', '{obj.QC_FINAL2}', '{obj.QC_FINAL3}', '{obj.QC_NFM1}', '{obj.QC_NFM2}', '{obj.QC_NFM3}', '{obj.QC1}', '{obj.QC2}', '{obj.QC3}', '{obj.PE1_Process}', '{obj.PE2_Process}');";
            var result = DB_CCS.Database.SqlQuery<int>(query);
            return result.First();   
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

        public long InsertResponse(string desc, string department, string user, string date,long resubmit_id){
            string sql = $@"INSERT INTO CCS.dbo.Response (Description, Department, [User], [Date], Resubmit) 
            OUTPUT Inserted.ID 
            VALUES('{desc}', '{department}', '{user}', '{date}', {resubmit_id});";
            var result = DB_CCS.Database.SqlQuery<long>(sql);
            return result.First(); 
        }
        
        public Related GetRelatedByTopicID(long topic_id){
            try{
                var sql = $@"SELECT ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
                FROM CCS.dbo.Related 
                LEFT JOIN Topic ON ID_Related = Topic.Related 
                WHERE Topic.ID = {topic_id};";
                var result = DB_CCS.Database.SqlQuery<Related>(sql).First();
                return result;
            }catch(Exception ex){
                Related blank_related = new Related();
                return blank_related;
            }
        }

        public void UpdateTopicStatus(long topic_id, int status){
            var sql = $"UPDATE CCS.dbo.Topic SET Status={status} WHERE ID={topic_id}";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public Related GetRelatedByID(long related_id){
            var sql=$@"SELECT ID, ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related 
            WHERE ID = {related_id};";
            var result = DB_CCS.Database.SqlQuery<Related>(sql);
            return result.First(); 
        }

        public void UpdateTopicApproveRequest(string user, long topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET RequestBy='{user}', RequestDate='{date}' 
            WHERE Topic = {topic_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveReview(string user, long topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET ReviewBy='{user}', ReviewDate='{date}' 
            WHERE Topic = {topic_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveTrial(string user, long topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET TrialBy='{user}', TrialDate='{date}' 
            WHERE Topic = {topic_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveClose(string user, long topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET CloseBy='{user}', CloseDate='{date}' 
            WHERE Topic = {topic_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public bool GetTrialStatusByTopicAndDept(long topic_id, int dept_id){
            try{
                var sql= $@"SELECT Review_Item.Status FROM CCS.dbo.Review 
                LEFT JOIN Review_Item ON Review.ID_Review = FK_Review_ID 
                LEFT JOIN Review_Item_Type_Department ON Review_Item.FK_Item_Type = FK_Item_ID 
                LEFT JOIN Department ON Review.Department = Department.Name 
                WHERE Topic = {topic_id} 
                AND FK_Item_Type = 24 AND ID_Department = FK_Department_ID 
                AND FK_Department_ID = {dept_id}";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result; 
            }catch(Exception ex){
                return false;
            }
        }

        public long InsertTrial(long topic_id,string desc, string department, string user){
            try{
                var sql= $@"INSERT INTO CCS.dbo.Trial (Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID_Trial 
                VALUES({topic_id}, '{desc}', '{date}','{user}', 1, '{department}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }
    }
}