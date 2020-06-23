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
        private DbTapics _dbtapics;
        private DbCCS _dbCCS;
        public string key;
        private string date;
        private string date_ff;
        private object ForeignKey;

        public int ReviewID;
        private SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS"].ConnectionString);
        public DetailModel()
        {
            _dbtapics = new DbTapics();
            _dbCCS = new DbCCS();
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
        public List<FormReviewItem> GetReviewItemByDepartment(int DepartmentId)
        {
            var sql = $@"SELECT Review_Item_Type.Name AS Name, Review_Item_Type.ID AS ID  FROM Department
                        LEFT JOIN Review_Item_Type_Department ON Department.ID_Department = Review_Item_Type_Department.FK_Department_ID
                        LEFT JOIN Review_Item_Type ON Review_Item_Type_Department.FK_Item_ID = Review_Item_Type.ID
                        WHERE Department.ID_Department = '{DepartmentId}'
                        GROUP BY Review_Item_Type.Name , Review_Item_Type.Seq ,Review_Item_Type.ID
                        ORDER BY Review_Item_Type.Seq";
            var Review_Item = _dbCCS.Database.SqlQuery<FormReviewItem>(sql).ToList();
            return Review_Item;
        }

        public void SetForeignKey(object ForeignKey)
        {
            this.ForeignKey = ForeignKey;
        }
        public List<GetID> GetExternalTopicId()
        {
            var sql = $@"SELECT TOP(1) ID_Topic FROM 
            Topic WHERE ID_Topic LIKE 'EX-{date.Substring(2, 4)}%' ORDER BY ID_Topic DESC";
            var dept = _dbCCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public List<GetID> GetInternalTopicId()
        {
            var sql =$@"SELECT TOP(1) ID_Topic FROM Topic 
            WHERE ID_Topic LIKE 'IN-{date.Substring(2, 4)} %' ORDER BY ID_Topic DESC";
            var dept = _dbCCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public void SetKey(string key)
        {
            this.key = key;
        }
        public void InsertReviewItem(string Status, string Description, int ItemType, int ReviewID)
        {
            string query = null;
            if (Status == null || Status == ""){
                query = $@"INSERT INTO CCS.dbo.Review_Item (Description, FK_Item_Type, FK_Review_ID) 
                VALUES('{Description}', {ItemType}, {ReviewID});";
            }
            else{
                query = $@"INSERT INTO CCS.dbo.Review_Item (Status, Description, FK_Item_Type, FK_Review_ID) VALUES('{Status}', '{Description}', {ItemType}, {ReviewID});";
            }
            
            _dbCCS.Database.ExecuteSqlCommand(query);
        }

        public int InsertReview(int TopicID, string FileID, string Date, string UserID, string Department)
        {
            string query = null;
            if (FileID == null || FileID == ""){
                query = $"INSERT INTO CCS.dbo.Review (Topic,  [Date], [User], Department, UpdateBy, UpdateDate) OUTPUT Inserted.ID_Review VALUES('{TopicID}', {Date}, '{UserID}', '{Department}', '{UserID}', {Date});";
            }
            else{
                query = $"INSERT INTO CCS.dbo.Review (Topic, [File], [Date], [User], Department, UpdateBy, UpdateDate) OUTPUT Inserted.ID_Review VALUES('{TopicID}', '{FileID}', {Date}, '{UserID}', '{Department}', '{UserID}', {Date});";
            }
            var ReviewID = _dbCCS.Database.SqlQuery<int>(query).First();
            return ReviewID;
        }

        public void UpdateReviewFileCode(int ReviewID, string FileCode)
        {
            string query = $"UPDATE CCS.dbo.Review SET [File]='{FileCode}' WHERE ID_Review={ReviewID}; ";
            _dbCCS.Database.ExecuteSqlCommand(query);
        }

        public int InsertFile(HttpPostedFileBase file, string FileCode, string description, object session_user){
            string query = $"INSERT INTO [File] ( ID_File, Name, Size, Name_Format, Description, Time_Insert, User_Insert) OUTPUT Inserted.ID VALUES( '{FileCode}','{file.FileName.ToString()}','{file.ContentLength}','{date_ff}','{description}','{date}','{session_user}');";
            // _dbCCS.Database.ExecuteSqlCommand(query);
            var result = _dbCCS.Database.SqlQuery<int>(query).First();
            return result;
        }

        public string GetFirstTopic(){
            string query = $"SELECT TOP(1) ID_Topic FROM CCS.dbo.Topic;";
            var result = _dbCCS.Database.SqlQuery<string>(query).First();
            return result;

        }
        public TopicAlt GetTopicByID(string TopicID)
        {
            try
            {
                var sql = $@"SELECT ID_Topic, Topic_type, Item as Change_item, Product_Type.Product_type, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, [File], Related, User_insert, Time_insert, 
                ID FROM CCS.dbo.Topic 
                LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                WHERE ID_Topic = '{TopicID}' ORDER BY Revision DESC;";
                var Topic = _dbCCS.Database.SqlQuery<TopicAlt>(sql).First();
                return Topic;
            }catch(Exception ex){
                TopicAlt blank_topic = new TopicAlt();
                return blank_topic;
            }
            
        }

        public void UpdateFileDesc(int? FileID, string Text)
        {
            var sql = $"UPDATE CCS.dbo.[File] SET Description = '{Text}' WHERE ID = '{FileID}'";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }

        public List<Review> GetReviewByTopicID(int TopicID)
        {
            var sql = $@"SELECT ID_Review, Topic, [File], [Date], [User], Status, Department 
            FROM CCS.dbo.Review 
            WHERE Topic = {TopicID} ORDER BY ID_Review ASC;";
            var ReviewResult = _dbCCS.Database.SqlQuery<Review>(sql).ToList();
            return ReviewResult;
        }

        public List<ReviewItem> GetReviewItemByReviewID(int ReviewID)
        {
            var sql = $@"SELECT Review_Item.ID, Status, Description, Name FROM CCS.dbo.Review_Item 
            LEFT JOIN Review_Item_Type ON FK_Item_Type = Review_Item_Type.ID 
            WHERE FK_Review_ID = {ReviewID} ORDER BY Review_Item_Type.Seq ASC;";
            var ReviewItem = _dbCCS.Database.SqlQuery<ReviewItem>(sql).ToList();
            return ReviewItem;
        }

        public List<FileItem> GetFileByID(string FileCode)
        {
            var sql = $@"SELECT ID, ID_File, Name, Size, Name_Format, Description, Time_Insert, User_Insert 
            FROM CCS.dbo.[File] WHERE ID_File = '{FileCode}'; ";
            var FileList = _dbCCS.Database.SqlQuery<FileItem>(sql).ToList();
            return FileList;
        }

        public User getUserByID(string id){
            System.Net.ServicePointManager.Expect100Continue = false;
            myAD.ADInfo conAD = new myAD.ADInfo();
            bool chk = false;
                User temp_user = new User(conAD.ChkFullName(id), conAD.ChkName(id), conAD.ChkSurName(id), conAD.ChkEmail(id),conAD.ChkDept(id), conAD.ChkPosition(id));
            return temp_user;
        }

        public void InsertResubmit(string desc, string due_date, int related, int topic_id, string user_id, int status){
            var sql = $@"INSERT INTO CCS.dbo.Resubmit (Description, DueDate, [Date], Related, Topic, [User], Status) 
            VALUES('{desc}', '{due_date}', '{date}', '{related}', '{topic_id}', '{user_id}', {status});";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }
        
        public List<Resubmit> GetResubmitByTopicID(int topic_id){
            var sql = $@"SELECT ID, Description, DueDate, [Date], Related, Topic, [User] 
            FROM CCS.dbo.Resubmit WHERE Topic = '{topic_id}' ORDER BY [Date] DESC;";
            var result = _dbCCS.Database.SqlQuery<Resubmit>(sql).ToList();
            return result;
        }
        
        public List<Response> GetResponseByResubmitID(int resubmit_id){
            var sql = $@"SELECT ID, Description, [File], Department, [User], [Date] 
            FROM CCS.dbo.Response 
            WHERE Resubmit_ID = {resubmit_id} ORDER BY [Date];";
            var result = _dbCCS.Database.SqlQuery<Response>(sql).ToList();
            return result;
        }

        public Related GetRelatedByResubmitTopicID(int topic_id){
            var sql = $@"SELECT ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related 
            LEFT JOIN Resubmit ON Related.ID = Resubmit.Related 
            WHERE Resubmit.Topic = {topic_id};";
            var result = _dbCCS.Database.SqlQuery<Related>(sql).First();
            return result;
        }

        public int InsertRelated(string key,Related obj){
            string query = $@"INSERT INTO Related (ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process) 
            OUTPUT Inserted.ID 
            VALUES( '{key}','{obj.PT1}', '{obj.PT2}', '{obj.PT3A}', '{obj.PT3M}', '{obj.PT4}', '{obj.PT5}', '{obj.PT6}', '{obj.PT7}', '{obj.IT}', '{obj.MKT}', '{obj.PC1}', '{obj.PC2}', '{obj.PCH1}', '{obj.PCH2}', '{obj.PE1}', '{obj.PE2}', '{obj.PE2_SMT}', '{obj.PE2_PCB}', '{obj.PE2_MT}', '{obj.QC_IN1}', '{obj.QC_IN2}', '{obj.QC_IN3}', '{obj.QC_FINAL1}', '{obj.QC_FINAL2}', '{obj.QC_FINAL3}', '{obj.QC_NFM1}', '{obj.QC_NFM2}', '{obj.QC_NFM3}', '{obj.QC1}', '{obj.QC2}', '{obj.QC3}', '{obj.PE1_Process}', '{obj.PE2_Process}');";
            var result = _dbCCS.Database.SqlQuery<int>(query);
            return result.First();   
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

        public int InsertResponse(string desc, string department, string user, string date,int resubmit_id){
            string sql = $@"INSERT INTO CCS.dbo.Response (Description, Department, [User], [Date], Resubmit_ID) 
            OUTPUT Inserted.ID 
            VALUES('{desc}', '{department}', '{user}', '{date}', {resubmit_id});";
            var result = _dbCCS.Database.SqlQuery<int>(sql);
            return result.First(); 
        }
        
        public void UpdateResponseFileCode(int response_id,string file_code){
            string sql = $"UPDATE CCS.dbo.Response SET [File]='{file_code}' WHERE ID={response_id};";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }    

        public Related GetRelatedByTopicID(int topic_id){
            try{
                var sql = $@"SELECT ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
                FROM CCS.dbo.Related 
                LEFT JOIN Topic ON ID_Related = Topic.Related 
                WHERE Topic.ID = {topic_id};";
                var result = _dbCCS.Database.SqlQuery<Related>(sql).First();
                return result;
            }catch(Exception ex){
                Related blank_related = new Related();
                return blank_related;
            }
        }

        public void UpdateTopicStatus(int topic_id, int status){
            var sql = $"UPDATE CCS.dbo.Topic SET Status={status} WHERE ID={topic_id}";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }

        public Related GetRelatedByID(int related_id){
            var sql=$@"SELECT ID, ID_Related, PT1, PT2, PT3A, PT3M, PT4, PT5, PT6, PT7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related 
            WHERE ID = {related_id};";
            var result = _dbCCS.Database.SqlQuery<Related>(sql);
            return result.First(); 
        }

        public void UpdateTopicApproveRequest(string user, int topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET RequestBy='{user}', RequestDate='{date}' 
            WHERE Topic = {topic_id};";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveReview(string user, int topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET ReviewBy='{user}', ReviewDate='{date}' 
            WHERE Topic = {topic_id};";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveTrial(string user, int topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET TrialBy='{user}', TrialDate='{date}' 
            WHERE Topic = {topic_id};";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveClose(string user, int topic_id){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET CloseBy='{user}', CloseDate='{date}' 
            WHERE Topic = {topic_id};";
            _dbCCS.Database.ExecuteSqlCommand(sql);
        }

        public bool GetTrialStatusByTopicAndDept(int topic_id, int dept_id){
            try{
                var sql= $@"SELECT Review_Item.Status FROM CCS.dbo.Review 
                LEFT JOIN Review_Item ON Review.ID_Review = FK_Review_ID 
                LEFT JOIN Review_Item_Type_Department ON Review_Item.FK_Item_Type = FK_Item_ID 
                LEFT JOIN Department ON Review.Department = Department.Name 
                WHERE Topic = {topic_id} 
                AND FK_Item_Type = 24 AND ID_Department = FK_Department_ID 
                AND FK_Department_ID = {dept_id}";
                var result = _dbCCS.Database.SqlQuery<bool>(sql).First();
                return result; 
            }catch(Exception ex){
                return false;
            }
        }
    }
}