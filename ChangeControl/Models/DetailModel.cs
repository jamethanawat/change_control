using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ChangeControl.Helpers;
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
            var sql = $@"SELECT TOP(1) Code FROM 
            Topic WHERE Code LIKE 'EX-{date.Substring(2, 4)}%' ORDER BY Code DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public List<GetID> GetInternalTopicId()
        {
            var sql =$@"SELECT TOP(1) Code FROM Topic 
            WHERE Code LIKE 'IN-{date.Substring(2, 4)} %' ORDER BY Code DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public void InsertReviewItem(string status, string desc, int item_type, long review_id)
        {
            string query = null;
            if (status == null || status == ""){
                query = $@"INSERT INTO CCS.dbo.Review_Item (Description, FK_Item_Type, FK_Review_ID) 
                VALUES('{desc.ReplaceSingleQuote()}', {item_type}, {review_id});";
            }
            else{
                query = $@"INSERT INTO CCS.dbo.Review_Item (Status, Description, FK_Item_Type, FK_Review_ID) VALUES('{status}', '{desc.ReplaceSingleQuote()}', {item_type}, {review_id});";
            }
            
            DB_CCS.Database.ExecuteSqlCommand(query);
        }

        public long InsertReview(string topic_code, string us_id, string dept){
            string query = null;
            query = $"INSERT INTO CCS.dbo.Review (Topic,  [Date], [User], Department, Revision, UpdateBy, UpdateDate) OUTPUT Inserted.ID_Review VALUES('{topic_code}', {date}, '{us_id}', '{dept}', 1, '{us_id}', {date});";
            var review_id = DB_CCS.Database.SqlQuery<long>(query).First();
            return review_id;
        }

        public long UpdateReview(string topic_code, string us_id, string dept){
            string query = null;
            query = $@"INSERT INTO CCS.dbo.Review (Topic, [Date], [User], Department, Revision, UpdateBy, UpdateDate) OUTPUT Inserted.ID_Review 
            VALUES('{topic_code}', {date}, '{us_id}', '{dept}', (
                SELECT Revision+1 FROM CCS.dbo.Review,
                (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Review WHERE Topic = '{topic_code}' AND Department = '{dept}' Group by Department) lastest
                WHERE Topic = '{topic_code}' AND Department = '{dept}'
                AND Review.Revision  = lastest.Version AND Review.Department = lastest.dept 
            ), '{us_id}', {date});";
            var review_id = DB_CCS.Database.SqlQuery<long>(query).First();
            return review_id;
        }

        public long InsertFile(HttpPostedFileBase file, long fk_id, string type, string desc, object session_user){
            string query = $@"INSERT INTO [File] (FK_ID, [Type], Name, Size, Name_Format, Description, Time_Insert, User_Insert) 
            OUTPUT Inserted.ID VALUES({fk_id}, '{type}', '{file.FileName.ToString()}','{file.ContentLength}','{date_ff}','{desc}','{date}','{session_user}');";
            long result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public string GetFirstTopic(){
            string query = $"SELECT TOP(1) Code FROM CCS.dbo.Topic;";
            var result = DB_CCS.Database.SqlQuery<string>(query).First();
            return result;

        }
        public TopicAlt GetTopicByOriginID(string topic_code){ //every file review confirm is related to 
            try{
                var sql = $@"SELECT Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, 
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
        
        public TopicAlt GetTopicByCode(string topic_code){
            try{
                var sql = $@"SELECT  Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, Related, User_insert, Time_insert, 
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

        public void UpdateFileDesc(long? file_id, string Text){
            var sql = $"UPDATE CCS.dbo.[File] SET Description = '{Text}' WHERE ID = '{file_id}'";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<Review> GetReviewByTopicCode(string topic_code){
            var sql = $@"SELECT ID_Review, Topic, [Date], [User], Status, Department, ApprovedBy, ApprovedDate
            FROM CCS.dbo.Review ,
            (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Review WHERE Topic = '{topic_code}'  Group by Department) lastest
            WHERE Topic = '{topic_code}' 
            AND Review.Revision  = lastest.Version AND Review.Department = lastest.dept 
            ORDER BY ID_Review ASC;";
            var ReviewResult = DB_CCS.Database.SqlQuery<Review>(sql).ToList();
            return ReviewResult;
        }

        public List<ReviewItem> GetReviewItemByReviewID(long review_id){
            var sql = $@"SELECT Review_Item.ID, FK_Item_Type AS Type, Status, Description, Name FROM CCS.dbo.Review_Item 
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
            if(id == null){
                return new User(null, null, null, null, null, null);
            }else{
                System.Net.ServicePointManager.Expect100Continue = false;
                myAD.ADInfo conAD = new myAD.ADInfo();
                User temp_user = new User(conAD.ChkFullName(id), conAD.ChkName(id), conAD.ChkSurName(id), conAD.ChkEmail(id),conAD.ChkDept(id), conAD.ChkPosition(id));
                return temp_user;
            }
        }

        public void InsertResubmit(string desc, string due_date, long related, string topic_code, string user_id, int status){
            var sql = $@"INSERT INTO CCS.dbo.Resubmit (Description, DueDate, [Date], Related, Topic, [User], Status) 
            VALUES('{desc.ReplaceSingleQuote()}', '{due_date}', '{date}', '{related}', '{topic_code}', '{user_id}', {status});";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        
        public List<Resubmit> GetResubmitByTopicID(string topic_code){
            var sql = $@"SELECT ID, Description, DueDate, [Date], Related, Topic, [User] 
            FROM CCS.dbo.Resubmit WHERE Topic = '{topic_code}' ORDER BY [Date] DESC;";
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

        public Related GetRelatedByResubmitTopicID(string topic_code){
            var sql = $@"SELECT P1, P2, P3A, P3M, P4, P5, P6, P7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related 
            LEFT JOIN Resubmit ON Related.ID = Resubmit.Related 
            WHERE Resubmit.Topic = '{topic_code}';";
            var result = DB_CCS.Database.SqlQuery<Related>(sql).First();
            return result;
        }

        public long InsertRelated(Related obj){
            string query = $@"INSERT INTO Related (P1, P2, P3A, P3M, P4, P5, P6, P7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process) 
            OUTPUT Inserted.ID 
            VALUES('{obj.P1}', '{obj.P2}', '{obj.P3A}', '{obj.P3M}', '{obj.P4}', '{obj.P5}', '{obj.P6}', '{obj.P7}', '{obj.IT}', '{obj.MKT}', '{obj.PC1}', '{obj.PC2}', '{obj.PCH1}', '{obj.PCH2}', '{obj.PE1}', '{obj.PE2}', '{obj.PE2_SMT}', '{obj.PE2_PCB}', '{obj.PE2_MT}', '{obj.QC_IN1}', '{obj.QC_IN2}', '{obj.QC_IN3}', '{obj.QC_FINAL1}', '{obj.QC_FINAL2}', '{obj.QC_FINAL3}', '{obj.QC_NFM1}', '{obj.QC_NFM2}', '{obj.QC_NFM3}', '{obj.QC1}', '{obj.QC2}', '{obj.QC3}', '{obj.PE1_Process}', '{obj.PE2_Process}');";
            var result = DB_CCS.Database.SqlQuery<long>(query);
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
            VALUES('{desc.ReplaceSingleQuote()}', '{department}', '{user}', '{date}', {resubmit_id});";
            var result = DB_CCS.Database.SqlQuery<long>(sql);
            return result.First(); 
        }
        
        public Related GetRelatedByTopicID(string topic_code){
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

        public void UpdateTopicStatus(string topic_code, int status){
            var sql = $"UPDATE CCS.dbo.Topic SET Status={status} WHERE Code = '{topic_code}'";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public Related GetRelatedByID(long related_id){
            var sql=$@"SELECT ID, P1, P2, P3A, P3M, P4, P5, P6, P7, IT, MKT, PC1, PC2, PCH1, PCH2, PE1, PE2, PE2_SMT, PE2_PCB, PE2_MT, QC_IN1, QC_IN2, QC_IN3, QC_FINAL1, QC_FINAL2, QC_FINAL3, QC_NFM1, QC_NFM2, QC_NFM3, QC1, QC2, QC3, PE1_Process, PE2_Process 
            FROM CCS.dbo.Related 
            WHERE ID = {related_id};";
            var result = DB_CCS.Database.SqlQuery<Related>(sql).First();
            return result; 
        }

        public void UpdateTopicApproveRequest(string user, string topic_code){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET RequestBy='{user}', RequestDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveReview(string user, string topic_code){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET ReviewBy='{user}', ReviewDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveTrial(string user, string topic_code){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET TrialBy='{user}', TrialDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveClose(string user, string topic_code){
            var sql = $@"UPDATE CCS.dbo.Topic_Approve SET CloseBy='{user}', CloseDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public bool GetTrialStatusByTopicAndDept(string topic_code, int dept_id){
            try{
                var sql= $@"SELECT Review_Item.Status FROM CCS.dbo.Review 
                LEFT JOIN Review_Item ON Review.ID_Review = FK_Review_ID 
                LEFT JOIN Review_Item_Type_Department ON Review_Item.FK_Item_Type = FK_Item_ID 
                LEFT JOIN Department ON Review.Department = Department.Name 
                WHERE Topic = '{topic_code}'
                AND FK_Item_Type = 24 AND ID_Department = FK_Department_ID 
                AND FK_Department_ID = {dept_id}";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result; 
            }catch(Exception ex){
                return false;
            }
        }

        public long InsertTrial(string topic_code,string desc, string department, string user){
            try{
                var sql= $@"INSERT INTO CCS.dbo.Trial (Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID 
                VALUES('{topic_code}', '{desc.ReplaceSingleQuote()}', '{date}','{user}', 1, '{department}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public long UpdateTrial(string topic_code,string desc, string dept, string user){
            try{
                var sql= $@"INSERT INTO CCS.dbo.Trial (Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID 
                VALUES('{topic_code}', '{desc.ReplaceSingleQuote()}', '{date}','{user}', (
                    SELECT Revision+1 FROM CCS.dbo.Trial,
                    (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Trial WHERE Topic = '{topic_code}' AND Department = '{dept}' Group by Department) lastest
                    WHERE Topic = '{topic_code}' AND Department = '{dept}'
                    AND Trial.Revision  = lastest.Version AND Trial.Department = lastest.dept)
                , '{dept}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public long UpdateConfirm(string topic_code,string desc, string dept, string user){
            try{
                var sql= $@"INSERT INTO CCS.dbo.Confirm (Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID 
                VALUES('{topic_code}', '{desc.ReplaceSingleQuote()}', '{date}','{user}', (
                    SELECT Revision+1 FROM CCS.dbo.Confirm,
                    (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Confirm WHERE Topic = '{topic_code}' AND Department = '{dept}' Group by Department) lastest
                    WHERE Topic = '{topic_code}' AND Department = '{dept}'
                    AND Confirm.Revision  = lastest.Version AND Confirm.Department = lastest.dept)
                , '{dept}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public List<Trial> GetTrialByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, Detail, [Date], [User], Department, Status, Revision, ApprovedBy, UpdateBy, UpdateDate, ApprovedDate 
                FROM CCS.dbo.Trial,
                    (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Trial WHERE Topic = '{topic_code}' Group by Department) lastest
                WHERE Topic = '{topic_code}' 
                AND Trial.Revision  = lastest.Version AND Trial.Department = lastest.dept;";
                var trial = DB_CCS.Database.SqlQuery<Trial>(sql).ToList();
                return trial;
            }catch(Exception ex){
                List<Trial> blank_trial = new List<Trial>();
                return blank_trial;
            }
        }

        
        public long InsertConfirm(string topic_code,string desc, string department, string user){
            try{
                var sql= $@"INSERT INTO CCS.dbo.Confirm (Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID 
                VALUES('{topic_code}', '{desc.ReplaceSingleQuote()}', '{date}','{user}', 1, '{department}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public List<Confirm> GetConfirmByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, Detail, [Date], [User], Department, Status, Revision, ApprovedBy, UpdateBy, UpdateDate, ApprovedDate 
                FROM CCS.dbo.Confirm,
                    (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Confirm WHERE Topic = '{topic_code}' Group by Department) lastest
                WHERE Topic = '{topic_code}' 
                AND Confirm.Revision  = lastest.Version AND Confirm.Department = lastest.dept;";
                var trial = DB_CCS.Database.SqlQuery<Confirm>(sql).ToList();
                return trial;
            }catch(Exception ex){
                List<Confirm> blank_confirm = new List<Confirm>();
                return blank_confirm;
            }
        }

        public void ApproveReview(long review_id,string user){
            var sql = $@"UPDATE CCS.dbo.Review SET Status=1, ApprovedBy='{user}', ApprovedDate='{date}' 
            WHERE ID_Review={review_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public void ApproveTrial(long trial_id,string user){
            var sql = $@"UPDATE CCS.dbo.Trial SET Status=1, ApprovedBy='{user}', ApprovedDate='{date}' 
            WHERE ID={trial_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public void ApproveConfirm(long confirm_id,string user){
            var sql = $@"UPDATE CCS.dbo.Confirm SET Status=1, ApprovedBy='{user}', ApprovedDate='{date}' 
            WHERE ID={confirm_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<Review> CheckAllReviewBeforeApprove(string topic_code){
            try{
                var sql = $@"SELECT ID_Review , Topic,Department, Status FROM CCS.dbo.Review,
                (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Review WHERE Topic = '{topic_code}' Group by Department ) lastest
                WHERE Topic = '{topic_code}'                    
                AND Review.Revision  = lastest.Version AND Review.Department  = lastest.dept 
                AND Review.Department != 'QC1' AND Review.Department != 'QC2' AND Review.Department != 'QC3';";
                var result = DB_CCS.Database.SqlQuery<Review>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<Review>();
            }
        }

        public List<Trial> CheckAllTrialBeforeApprove(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic,Department, Status FROM CCS.dbo.Trial ,
                (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Trial WHERE Topic = '{topic_code}' Group by Department ) lastest
                WHERE Topic = '{topic_code}'                    
                AND Trial.Revision  = lastest.Version AND Trial.Department  = lastest.dept 
                AND Trial.Department != 'QC1' AND Trial.Department != 'QC2' AND Trial.Department != 'QC3';";
                var result = DB_CCS.Database.SqlQuery<Trial>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<Trial>();
            }
        }

        public List<Confirm> CheckAllConfirmBeforeApprove(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic,Department, Status FROM CCS.dbo.Confirm ,
                (SELECT MAX(Revision) as Version, Department as dept FROM CCS.dbo.Confirm WHERE Topic = '{topic_code}' Group by Department ) lastest
                WHERE Topic = '{topic_code}'                    
                AND Confirm.Revision  = lastest.Version AND Confirm.Department  = lastest.dept 
                AND Confirm.Department != 'QC1' AND Confirm.Department != 'QC2' AND Confirm.Department != 'QC3';";
                var result = DB_CCS.Database.SqlQuery<Confirm>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<Confirm>();
            }
        }

        public void RejectTopic(string topic_code, string desc, string user, string dept){
            var sql = $@"INSERT INTO CCS.dbo.Topic_Reject (Topic, Description, [User], Department, [Date]) VALUES('{topic_code}', '{desc}', '{user}', '{dept}', '{date}');";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public Reject GetRejectMessageByTopicCode(string topic_code){
            var sql = $@"SELECT Top(1) Topic, Description, [User], Department, [Date] FROM CCS.dbo.Topic_Reject WHERE Topic = '{topic_code}';";
            var result = DB_CCS.Database.SqlQuery<Reject>(sql).First();
            return result;
        }
    }
}