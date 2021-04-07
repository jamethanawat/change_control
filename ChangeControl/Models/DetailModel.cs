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
        public DetailModel(){
            DB_Tapics = new DbTapics();
            DB_CCS = new DbCCS();
            date = DateTime.Now.ToString("yyyyMMddHHmmss");
            date_ff = DateTime.Now.ToString("yyyyMMddHHmmss.fff");
        }

         public class Noti{
            public int ID { get; set; }
            public string Type { get; set; }
            public string Position { get; set; }
            public string Message { get; set; }
        }

        public List<FormReviewItem> GetReviewItemByDepartment(int department_id)
        {
            try{
                var sql = $@"SELECT Review_Item_Type.Name AS Name, Review_Item_Type.ID AS ID  FROM Department
                            LEFT JOIN Review_Item_Type_Department ON Department.ID = Review_Item_Type_Department.FK_Department_ID
                            LEFT JOIN Review_Item_Type ON Review_Item_Type_Department.FK_Item_ID = Review_Item_Type.ID
                            WHERE Department.ID = '{department_id}'
                            GROUP BY Review_Item_Type.Name , Review_Item_Type.Seq ,Review_Item_Type.ID
                            ORDER BY Review_Item_Type.Seq";
                var Review_Item = DB_CCS.Database.SqlQuery<FormReviewItem>(sql).ToList();
                return Review_Item;
            }catch(Exception err){
                return new List<FormReviewItem>();
            }
        }

        public void SetForeignKey(object foreign_key){
            this.foreign_key = foreign_key;
        }
        public List<GetID> GetExternalTopicId(){
            var sql = $@"SELECT TOP(1) Code FROM 
            Topic WHERE Code LIKE 'EX-{date.Substring(2, 4)}%' ORDER BY Code DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public List<GetID> GetInternalTopicId(){
            var sql =$@"SELECT TOP(1) Code FROM Topic 
            WHERE Code LIKE 'IN-{date.Substring(2, 4)} %' ORDER BY Code DESC";
            var dept = DB_CCS.Database.SqlQuery<GetID>(sql);
            return dept.ToList();
        }
        public void InsertReviewItem(string status, string desc, int item_type, long review_id){
            string query = null;
            if (status == null || status == ""){
                query = $@"INSERT INTO Review_Item (Description, FK_Item_Type, FK_Review_ID) 
                VALUES('{desc.ReplaceSingleQuote()}', {item_type}, {review_id});";
            }else{
                query = $@"INSERT INTO Review_Item (Status, Description, FK_Item_Type, FK_Review_ID) VALUES('{status}', '{desc.ReplaceSingleQuote()}', {item_type}, {review_id});";
            }
            
            DB_CCS.Database.ExecuteSqlCommand(query);
        }

        public long InsertReview(long topic_id, string topic_code, string us_id, string dept){
            string query = null;
            query = $"INSERT INTO Review (Topic, FK_Topic, [Date], [User], Department, Revision, UpdateBy, UpdateDate) OUTPUT Inserted.ID VALUES('{topic_code}', {topic_id}, {date}, '{us_id}', '{dept}', 0, '{us_id}', {date});";
            var review_id = DB_CCS.Database.SqlQuery<long>(query).First();
            return review_id;
        }

        public Review UpdateReview(long topic_id, string topic_code, string us_id, string dept){
            string query = null;
            query = $@"INSERT INTO Review (Topic, FK_Topic, [Date], [User], Department, Revision, UpdateBy, UpdateDate) OUTPUT Inserted.ID , Inserted.Revision 
            VALUES('{topic_code}', {topic_id}, {date}, '{us_id}', '{dept}', (
                SELECT Revision+1 FROM Review,
                (SELECT MAX(Revision) as Version, Department as dept FROM Review WHERE Topic = '{topic_code}' AND Department = '{dept}' Group by Department) lastest
                WHERE Topic = '{topic_code}' AND Department = '{dept}'
                AND Review.Revision  = lastest.Version AND Review.Department = lastest.dept 
            ), '{us_id}', {date});";
            var review_id = DB_CCS.Database.SqlQuery<Review>(query).First();
            return review_id;
        }

        public long InsertFile(HttpPostedFileBase file, long fk_id, string type, string desc, object session_user, string topic_code, string dept, string file_name){
            string query = $@"INSERT INTO [File] (FK_ID, [Type], Name, Size, Name_Format, Description, Time_Insert, User_Insert, Topic, Department) 
            OUTPUT Inserted.ID VALUES({fk_id}, '{type}', '{file.FileName.ToString().ReplaceSingleQuote()}','{file.ContentLength}','{file_name}','{desc}','{date}','{session_user}','{topic_code}', '{dept}');";
            long result = DB_CCS.Database.SqlQuery<long>(query).First();
            return result;
        }

        public string GetFirstTopic(){
            string query = $"SELECT TOP(1) Code FROM Topic;";
            var result = DB_CCS.Database.SqlQuery<string>(query).First();
            return result;

        }
        public TopicAlt GetTopicByOriginID(string topic_code){ //every file review confirm is related to 
            try{
                var sql = $@"SELECT Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP] as App, Subject, Detail, Timing, TimingDesc, Related, User_insert, Time_insert, ApprovedBy, ApprovedDate, 
                ID FROM Topic 
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
            try{ //This topic must be approved
                var sql = $@"SELECT  Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Topic.Status, Status.Name AS FullStatus  , [APP/IPP] as App, Subject, Detail, Timing, TimingDesc, Related, User_insert, Time_insert , ApprovedBy, ApprovedDate, 
                Topic.ID FROM Topic 
                LEFT JOIN Status ON Topic.Status = Status.ID 
                LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                WHERE ApprovedBy IS NOT NULL
                AND Code = '{topic_code}' 
                ORDER BY Revision DESC;";
                var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql).First();
                return Topic;
            }catch(Exception ex){ //This topic dont have be approved
                var sql = $@"SELECT  Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Topic.Status, Status.Name AS FullStatus  , [APP/IPP] as App, Subject, Detail, Timing, TimingDesc, Related, User_insert, Time_insert , ApprovedBy, ApprovedDate, 
                Topic.ID FROM Topic 
                LEFT JOIN Status ON Topic.Status = Status.ID 
                LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                WHERE Code = '{topic_code}' 
                ORDER BY Revision DESC;";
                var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql).First();
                return Topic;
            }
        }
        public TopicAlt GetTopicByCodeAndOwned(string topic_code, string dept){
            try{  //Query by condition (User's department must be owner)
                var sql = $@"SELECT  Code, Type, Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Topic.Status, Status.Name AS FullStatus , [APP/IPP] as App, Subject, Detail, Timing, TimingDesc, Related, User_insert, Time_insert , ApprovedBy, ApprovedDate, 
                Topic.ID FROM Topic 
                LEFT JOIN Status ON Topic.Status = Status.ID 
                LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                WHERE  Code = '{topic_code}' 
                AND Department = '{dept}'
                ORDER BY Revision DESC;";
                var Topic = DB_CCS.Database.SqlQuery<TopicAlt>(sql).First();
                return Topic;
            }catch(Exception ex){
                //Query by condition (User's department is not be owner)
                return this.GetTopicByCode(topic_code);
            }
        }

        public void UpdateFileDesc(long? file_id, string Text){
            var sql = $"UPDATE [File] SET Description = '{Text}' WHERE ID = '{file_id}'";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<Review> GetReviewByTopicCode(string topic_code){
            var sql = $@"SELECT ID, Topic, [Date], [User], Status, Department, ApprovedBy, ApprovedDate
            FROM Review ,
            (SELECT MAX(Revision) as Version, Department as dept FROM Review WHERE Topic = '{topic_code}'  Group by Department) lastest
            WHERE Topic = '{topic_code}' 
            AND Review.Revision  = lastest.Version AND Review.Department = lastest.dept 
            ORDER BY ID ASC;";
            var ReviewResult = DB_CCS.Database.SqlQuery<Review>(sql).ToList();
            return ReviewResult;
        }

        public List<ReviewItem> GetReviewItemByReviewID(long review_id){
            var sql = $@"SELECT Review_Item.ID, FK_Item_Type AS Type, Status, Description, Name FROM Review_Item 
            LEFT JOIN Review_Item_Type ON FK_Item_Type = Review_Item_Type.ID 
            WHERE FK_Review_ID = {review_id} ORDER BY Review_Item_Type.Seq ASC;";
            var ReviewItem = DB_CCS.Database.SqlQuery<ReviewItem>(sql).ToList();
            return ReviewItem;
        }

        public List<FileItem> GetFileByID(long fk_id, string type, string topic_code, string dept){
            var sql = $"SELECT ID, FK_ID, [Type], Name, Name_Format, Description, [Size], Time_Insert, User_Insert FROM [File] WHERE [Type] = '{type}' AND Topic = '{topic_code}' AND Department = '{dept}'";
            var FileList = DB_CCS.Database.SqlQuery<FileItem>(sql).ToList();
            return FileList;
        }
        public List<FileItem> GetFileByFKID(long fk_id, string type, string topic_code, string dept)
        {
            var sql = $"SELECT ID, FK_ID, [Type], Name, Name_Format, Description, [Size], Time_Insert, User_Insert FROM [File] WHERE FK_ID='{fk_id}' AND [Type] = '{type}' AND Topic = '{topic_code}' AND Department = '{dept}'";
            var FileList = DB_CCS.Database.SqlQuery<FileItem>(sql).ToList();
            return FileList;
        }


        public User getUserByID(string id){
            if(id == null){
                return new User(null, null, null, null, null);
            }else{
                System.Net.ServicePointManager.Expect100Continue = false;
                myAD.ADInfo conAD = new myAD.ADInfo();
                User temp_user = new User(conAD.ChkFullName(id), conAD.ChkName(id), conAD.ChkSurName(id), conAD.ChkDept(id), conAD.ChkPosition(id));
                return temp_user;
            }
        }

        public long InsertResubmit(string desc, string due_date, long related, string topic_code, string user_id, int status,string dept){
            var sql = $@"INSERT INTO Resubmit (Description, DueDate, [Date], Related, Topic, [User], Status, Dept)
            VALUES('{desc.ReplaceSingleQuote()}', '{due_date}', '{date}', '{related}', '{topic_code}', '{user_id}', {status}, '{dept}');
            SELECT MAX(Resubmit.ID) FROM Resubmit;";
            //DB_CCS.Database.ExecuteSqlCommand(sql);
            var result = DB_CCS.Database.SqlQuery<long>(sql);
            return result.First();
        }
        
        public List<Resubmit> GetResubmitByTopicID(string topic_code){

            var sql = $@"SELECT ID, Description, DueDate, [Date], Related, Topic, [User] ,Dept
            FROM Resubmit WHERE Topic = '{topic_code}';" ;


            //var sql = $@" SELECT a.ID, a.Description, a.DueDate, a.[Date], a.Related, a.Topic, a.[User] ,a.Dept,b.[Type] 
            //FROM Resubmit a left join [File] b on a.Topic =b.Topic WHERE b.[Type]='Resubmit' and a.Topic = '{topic_code}'
            //ORDER BY [Date] DESC;";
            var result = DB_CCS.Database.SqlQuery<Resubmit>(sql).ToList();
            return result;
        }
        
        public List<Response> GetResponseByResubmitID(long resubmit_id){
            var sql = $@"SELECT ID, Description, Department, [User], [Date] 
            FROM Response 
            WHERE Resubmit = {resubmit_id} ORDER BY [Date];";
            var result = DB_CCS.Database.SqlQuery<Response>(sql).ToList();
            return result;
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
        public List<Department> GetDepartmentByGroup(string DepartmentGroup){
            var sql = $"SELECT Department.ID as ID, Name, [Group] FROM Department WHERE [Group] = '{DepartmentGroup}';";
            var result = DB_CCS.Database.SqlQuery<Department>(sql);
            return result.ToList();    
        }
        public List<string> GetDepartmentGroup(){
            var sql = "SELECT [Group] FROM Department WHERE [Group] != 'Guest' GROUP BY [Group];";
            var result = DB_CCS.Database.SqlQuery<string>(sql);
            return result.ToList();    
        }

        public long InsertResponse(string desc, string department, string user, string date,long resubmit_id){
            string sql = $@"INSERT INTO Response (Description, Department, [User], [Date], Resubmit) 
                            OUTPUT Inserted.ID 
                            VALUES('{desc.ReplaceSingleQuote()}', '{department}', '{user}', '{date}', {resubmit_id});";
            var result = DB_CCS.Database.SqlQuery<long>(sql);
            return result.First(); 
        }
        
        public void UpdateTopicStatus(string topic_code, int status){
            var sql = $"UPDATE Topic SET Status={status} WHERE Code = '{topic_code}'";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<RelatedAlt> GetRelatedByID(long related_id){
            var sql=$@"SELECT Department
                FROM Related 
                WHERE PK_Related = {related_id};";
            var result = DB_CCS.Database.SqlQuery<RelatedAlt>(sql).ToList();
            return result; 
        }

        public void UpdateTopicApproveRequest(string user, string topic_code){
            var sql = $@"UPDATE Topic_Approve SET RequestBy='{user}', RequestDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveReview(string user, string topic_code){
            var sql = $@"UPDATE Topic_Approve SET ReviewBy='{user}', ReviewDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveTrial(string user, string topic_code){
            var sql = $@"UPDATE Topic_Approve SET TrialBy='{user}', TrialDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }
        public void UpdateTopicApproveClose(string user, string topic_code){
            var sql = $@"UPDATE Topic_Approve SET CloseBy='{user}', CloseDate='{date}' 
            WHERE Topic = '{topic_code}';";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public bool GetTrialStatusByTopicAndDept(string topic_code, int dept_id){
            try{
                var sql= $@"SELECT TOP(1) Review_Item.Status FROM Review 
                LEFT JOIN Review_Item ON Review.ID = FK_Review_ID 
                LEFT JOIN Review_Item_Type_Department ON Review_Item.FK_Item_Type = FK_Item_ID 
                LEFT JOIN Department ON Review.Department = Department.Name 
                WHERE Topic = '{topic_code}'
                AND FK_Item_Type = 24 AND Department.ID = FK_Department_ID 
                AND FK_Department_ID = {dept_id}
                ORDER BY Review.Revision DESC";
                var result = DB_CCS.Database.SqlQuery<int>(sql).First();
                return (result == 1); 
            }catch(Exception ex){
                return false;
            }
        }

        public long InsertTrial(long topic_id, string topic_code,string desc, string department, string user){
            try{
                var sql= $@"INSERT INTO Trial (Topic, FK_Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID 
                VALUES('{topic_code}', {topic_id}, '{desc.ReplaceSingleQuote()}', '{date}','{user}', 0, '{department}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public Trial UpdateTrial(long topic_id, string topic_code,string desc, string dept, string user){
            try{
                var sql= $@"INSERT INTO Trial (Topic, FK_Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID, Inserted.Revision
                VALUES('{topic_code}', {topic_id}, '{desc.ReplaceSingleQuote()}', '{date}','{user}', (
                    SELECT Revision+1 FROM Trial,
                    (SELECT MAX(Revision) as Version, Department as dept FROM Trial WHERE Topic = '{topic_code}' AND Department = '{dept}' Group by Department) lastest
                    WHERE Topic = '{topic_code}' AND Department = '{dept}'
                    AND Trial.Revision  = lastest.Version AND Trial.Department = lastest.dept)
                , '{dept}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<Trial>(sql).First();
                return result;
            }catch(Exception ex){
                return new Trial();
            }
        }

        public Confirm UpdateConfirm(long topic_id, string topic_code,string desc, string dept, string user){
            try{
                var sql= $@"INSERT INTO Confirm (Topic, FK_Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID ,Inserted.Revision 
                VALUES('{topic_code}', {topic_id}, '{desc.ReplaceSingleQuote()}', '{date}','{user}', (
                    SELECT Revision+1 FROM Confirm,
                    (SELECT MAX(Revision) as Version, Department as dept FROM Confirm WHERE Topic = '{topic_code}' AND Department = '{dept}' Group by Department) lastest
                    WHERE Topic = '{topic_code}' AND Department = '{dept}'
                    AND Confirm.Revision  = lastest.Version AND Confirm.Department = lastest.dept)
                , '{dept}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<Confirm>(sql).First();
                return result;
            }catch(Exception ex){
                return new Confirm();
            }
        }

        public List<Trial> GetTrialByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, Detail, [Date], [User], Department, Status, Revision, ApprovedBy, UpdateBy, UpdateDate, ApprovedDate 
                FROM Trial,
                    (SELECT MAX(Revision) as Version, Department as dept FROM Trial WHERE Topic = '{topic_code}' Group by Department) lastest
                WHERE Topic = '{topic_code}' 
                AND Trial.Revision  = lastest.Version AND Trial.Department = lastest.dept;";
                var trial = DB_CCS.Database.SqlQuery<Trial>(sql).ToList();
                return trial;
            }catch(Exception ex){
                List<Trial> blank_trial = new List<Trial>();
                return blank_trial;
            }
        }

        
        public long InsertConfirm(long topic_id, string topic_code,string desc, string department, string user){
            try{
                var sql= $@"INSERT INTO Confirm (Topic, Detail, [Date], [User], Revision, Department, Status, UpdateDate, UpdateBy) 
                OUTPUT Inserted.ID 
                VALUES('{topic_code}', '{desc.ReplaceSingleQuote()}', '{date}','{user}', 0, '{department}', 3, '{date}', '{user}');";
                var result = DB_CCS.Database.SqlQuery<long>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public List<Confirm> GetConfirmByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, Detail, [Date], [User], Department, Status, Revision, ApprovedBy, UpdateBy, UpdateDate, ApprovedDate 
                FROM Confirm,
                    (SELECT MAX(Revision) as Version, Department as dept FROM Confirm WHERE Topic = '{topic_code}' Group by Department) lastest
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
            var sql = $@"UPDATE Review SET Status=1, ApprovedBy='{user}', ApprovedDate='{date}' 
            WHERE ID={review_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public void ApproveTrial(long trial_id,string user){
            var sql = $@"UPDATE Trial SET Status=1, ApprovedBy='{user}', ApprovedDate='{date}' 
            WHERE ID={trial_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public void ApproveConfirm(long confirm_id,string user){
            var sql = $@"UPDATE Confirm SET Status=1, ApprovedBy='{user}', ApprovedDate='{date}' 
            WHERE ID={confirm_id};";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<Review> CheckAllReviewBeforeApprove(string topic_code){
            try{
                var sql = $@"SELECT ID , Topic,Department, Status FROM Review,
                (SELECT MAX(Revision) as Version, Department as dept FROM Review WHERE Topic = '{topic_code}' Group by Department ) lastest
                WHERE Topic = '{topic_code}'                    
                AND Review.Revision  = lastest.Version AND Review.Department  = lastest.dept 
                AND Review.Department NOT IN (SELECT Name FROM Department WHERE [Group] = 'Quality Control' and Audit = 1 );";
                var result = DB_CCS.Database.SqlQuery<Review>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<Review>();
            }
        }

        public List<Trial> CheckAllTrialBeforeApprove(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic,Department, Status FROM Trial ,
                (SELECT MAX(Revision) as Version, Department as dept FROM Trial WHERE Topic = '{topic_code}' Group by Department ) lastest
                WHERE Topic = '{topic_code}'                    
                AND Trial.Revision  = lastest.Version AND Trial.Department  = lastest.dept 
                AND Trial.Department NOT IN (SELECT Name FROM Department WHERE [Group] = 'Quality Control' and Audit = 1 );";
                var result = DB_CCS.Database.SqlQuery<Trial>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<Trial>();
            }
        }

        public List<Confirm> CheckAllConfirmBeforeApprove(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic,Department, Status FROM Confirm ,
                (SELECT MAX(Revision) as Version, Department as dept FROM Confirm WHERE Topic = '{topic_code}' Group by Department ) lastest
                WHERE Topic = '{topic_code}'                    
                AND Confirm.Revision  = lastest.Version AND Confirm.Department  = lastest.dept 
                AND Confirm.Department NOT IN (SELECT Name FROM Department WHERE [Group] = 'Quality Control' and Audit = 1 );";
                var result = DB_CCS.Database.SqlQuery<Confirm>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<Confirm>();
            }
        }

        public void RejectTopic(string topic_code, string desc, string user, string dept){
            var sql = $@"INSERT INTO Topic_Reject (Topic, Description, [User], Department, [Date]) VALUES('{topic_code}', '{desc}', '{user}', '{dept}', '{date}');";
            DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public Reject GetRejectMessageByTopicCode(string topic_code){
            var sql = $@"SELECT Top(1) Topic, Description, [User], Department, [Date] FROM Topic_Reject WHERE Topic = '{topic_code}';";
            var result = DB_CCS.Database.SqlQuery<Reject>(sql).First();
            return result;
        }

        public bool CheckApproveIPP(string topic_code){
            var sql = $@"SELECT CASE WHEN EXISTS (
                        SELECT * FROM Topic
                        LEFT JOIN Review ON Review.Topic = Topic.Code
                        LEFT JOIN Review_Item ON Review_Item.FK_Review_ID  = Review.ID 
                        WHERE Topic.Revision  = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code) 
                        AND Review.Revision  = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic)
                        AND FK_Item_Type = 26 AND Review_Item.Status = 1 
                        AND Code = '{topic_code}')
                    THEN CAST(1 AS BIT)
                    ELSE CAST(0 AS BIT) END;";
            var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
            return result;
        }

        public int GetTopicStatusByCode(string topic_code){
            try{
                var sql = $@"SELECT TOP(1) Status FROM Topic 
                WHERE  Code = '{topic_code}' ORDER BY Revision DESC;";
                var result = DB_CCS.Database.SqlQuery<int>(sql).First();
                return result;
            }catch(Exception ex){
                return 0;
            }
        }

        public void ApproveTopicByCode(string topic_code,string us_id){
                var sql = $@"UPDATE Topic SET ApprovedBy = '{us_id}', ApprovedDate = '{date}', Status = 7 WHERE Topic.Code = '{topic_code}';";
                DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public void UpdateTopicRelated(string topic_code, long new_related_id){
                var sql = $@"INSERT INTO Topic 
                            SELECT TOP(1) Code, [Type], Change_item, Product_type, Revision+1 as Revision, Model, PartNo, PartName, ProcessName, Status, [APP/IPP], Subject, Detail, Timing, TimingDesc, {new_related_id}, User_insert, Time_insert, Department, ApprovedBy, ApprovedDate 
                            FROM Topic
                            WHERE Code = '{topic_code}'
                            ORDER BY Revision DESC;";
                DB_CCS.Database.ExecuteSqlCommand(sql);
        }

        public List<String> GetConfirmDeptList(){
            try{
                var sql = $@"SELECT Name FROM Department
                            WHERE [Group] = 'Production';";
                var result = DB_CCS.Database.SqlQuery<String>(sql).ToList();
                return result;
            }catch(Exception ex){
                return new List<String>();
            }
        }

        public bool CheckAllReviewApproved(string tp_code){
             try{
                var sql = $@"SELECT CASE 
                            WHEN NOT EXISTS (
                                SELECT Department, Status
                                FROM Review p
                                WHERE p.Topic = '{tp_code}'
                                AND p.Revision  = (SELECT MAX(c.Revision) FROM Review c WHERE c.Topic = p.Topic AND c.Department = p.Department )
                                AND p.Department NOT IN (SELECT Name FROM Department WHERE [Group] = 'Quality Control' and Audit = 1 )
                                AND p.Status = 3
                                )
                            THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT) END;
                            ";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result;
            }catch(Exception ex){
                return false;
            }
        }

        public bool CheckAllTrialApproved(string tp_code){
             try{
                var sql = $@"SELECT CASE 
                            WHEN NOT EXISTS (
                                SELECT Department, Status
                                FROM Trial p
                                WHERE p.Topic = '{tp_code}'
                                AND p.Revision  = (SELECT MAX(c.Revision) FROM Trial c WHERE c.Topic = p.Topic AND c.Department = p.Department )
                                AND p.Department NOT IN (SELECT Name FROM Department WHERE [Group] = 'Quality Control' and Audit = 1 )
                                AND p.Status = 3
                                )
                            THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT) END;
                            ";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result;
            }catch(Exception ex){
                return false;
            }
        }

        public bool CheckAllConfirmApproved(string tp_code){
             try{
                var sql = $@"SELECT CASE 
                            WHEN NOT EXISTS (
                                SELECT Department, Status
                                FROM Confirm p
                                WHERE p.Topic = '{tp_code}'
                                AND p.Revision  = (SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = p.Topic AND c.Department = p.Department )
                                AND p.Department NOT IN (SELECT Name FROM Department WHERE [Group] = 'Quality Control' and Audit = 1 )
                                AND p.Status = 3
                                )
                            THEN CAST(1 AS BIT)
                            ELSE CAST(0 AS BIT) END;
                            ";
                var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
                return result;
            }catch(Exception ex){
                return false;
            }
        }
        
        public bool DeleteFileByNameFormat(string name_format){
             try{
                var sql = $@"DELETE FROM [File] WHERE Name_Format = '{name_format}';";
                DB_CCS.Database.ExecuteSqlCommand(sql);
                return true;
            }catch(Exception ex){
                return false;
            }
        }

        public List<Noti> GetAuditNotification(){
            var sql = $@"SELECT ID, [Type], [Position], Message FROM Notification WHERE [Position] = 'Audit';";
            var result = DB_CCS.Database.SqlQuery<Noti>(sql).ToList();
            return result;
        }
    }
}