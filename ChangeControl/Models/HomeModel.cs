using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ChangeControl.Helpers;

namespace ChangeControl.Models
{
    public class HomeModel
    {
        private DbTapics _dbtapics;
        private DbCCS DB_CCS;
        public HomeModel(){
            _dbtapics = new DbTapics();
            DB_CCS = new DbCCS();
        }
        private class Line{
            public string line { get; set; }
        }
        public object GetLine(string Production){
            var sql = "SELECT line FROM bm_line where proddpt ='"+Production+"' ORDER BY line ASC";
            var result = _dbtapics.Database.SqlQuery<Line>(sql);
            return result.ToList();  
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
        
        public List<TopicAlt> GetSearch(SearchAttribute model){
            var where="";
            var sql = "";
            var ov_command = "";
            var job_command = "";
            if(model.Overstatus == 2){
                ov_command = $@"(
                    SELECT COUNT(*) 
                    ROM Related WHERE Related.PK_Related =  Topic.Related AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3')
		        =	(SELECT COUNT(Review.ID) FROM Review WHERE  
					 Review.Revision  = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department) AND  Review.Topic = Topic.Code 
                     AND Review.Status = 1
                     AND Review.Department NOT IN ('QC1', 'QC2', 'QC3')";
            }else if(model.Overstatus == 3){
                ov_command = $@"(SELECT COUNT(Review.ID) FROM Review 
                            LEFT JOIN Review_Item ON Review_Item.FK_Review_ID = Review.ID 
                            WHERE Review.Revision  = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department) AND  Review.Topic = Topic.Code
                            AND Review_Item.FK_Item_Type = 24
                            AND Review_Item.Status = 1)
                =   (SELECT COUNT(Trial.ID) FROM Trial WHERE
					 Trial.Revision  = (SELECT MAX(t.Revision) FROM Trial t WHERE t.Topic = Trial.Topic AND t.Department = Trial.Department) AND Trial.Topic = Topic.Code 
                     AND Trial.Status = 1
                     AND Trial.Department NOT IN ('QC1', 'QC2', 'QC3')";

            }else if(model.Overstatus == 4){
                ov_command = $@"(SELECT COUNT(*) FROM Related  
						LEFT JOIN Department d ON Related.Department = d.Name 
						WHERE Related.PK_Related =  Topic.Related 
						AND (d.[Group] = 'Production')
                        AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3'
						)
		        =	(SELECT COUNT(Confirm.ID) FROM Confirm WHERE
					 Confirm.Revision  = (SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = Confirm.Topic AND c.Department = Confirm.Department) AND  Confirm.Topic = Topic.Code 
                     AND Confirm.Status = 1
                     AND Confirm.Department NOT IN ('QC1', 'QC2', 'QC3')";
            }

            if(model.Department.AsNullIfWhiteSpace() != null && model.Status != 0){
                var phase = "";
                switch(model.Status){
                case 7:
                    job_command = $@",CASE 
                                        WHEN Topic.Status = 3 THEN 'Issued'
                                        WHEN Topic.Status = 7 THEN 'Approved' 
                                    END AS SubStatus";
                    break;
                case 8:
                    phase = "Review";
                    break;
                case 9:
                    phase = "Trial";
                    break;
                case 10:
                    phase = "Confirm";
                    break;
                }
                if(model.Status != 7){
                    job_command = $@", ISNULL((SELECT CASE
                                WHEN e.Status = 3 THEN 'Issued'
                                WHEN e.Status = 1 THEN 'Approve'
                                    END AS SubStatus 
                                    FROM {phase} e
                                WHERE e.Department = Related.Department 
                                AND e.Topic = Topic.Code 
                                AND e.Revision = (SELECT MAX(e2.Revision) FROM {phase} e2 WHERE e.Topic = e2.Topic AND e.Department = e2.Department)
                                ),'Pending') AS SubStatus	
                                ";

                }
            }

            sql = $@"SELECT DISTINCT Topic.Code, Topic.Type, Change_item.Name AS Change_item , Product_type.Name AS Product_type, Topic.Timing, Topic.Department, Topic.Revision,Topic.Model, Topic.PartNo, Topic.PartName, Topic.Detail,Topic.ProcessName, Status.Name AS FullStatus, Topic.Related, Topic.User_insert, Topic.Time_insert AS Date  {job_command} FROM Topic 
                    LEFT JOIN Status ON Topic.Status = Status.id 
                    LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                    LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type " +
                    (model.Department.AsNullIfWhiteSpace() != null ? $"LEFT JOIN Related ON Topic.Related = Related.PK_Related " : "") +
                    "WHERE Topic.Revision  = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code AND t.Department = Topic.Department) " +
                    (model.Type.AsNullIfWhiteSpace() != null ?  $"AND Topic.Type = '{model.Type}' " : "") +
                    (ov_command != "" ?  $"AND {ov_command} " : "") +
                    (model.Status != 0 ?  $"AND Topic.Status = '{model.Status}' " : "") +
                    (model.Status == 7 ?  $"OR Topic.Status = '3' " : "") +
                    (model.StartDate.AsNullIfWhiteSpace() != null && model.EndDate.AsNullIfWhiteSpace() != null ? $"AND SUBSTRING(Topic.Timing, 0 ,9) >= {model.StartDate} AND SUBSTRING(Topic.Timing, 0 ,9) <= {model.EndDate} " : "") +
                    (model.ProductType.AsNullIfWhiteSpace() != null ? $"AND Topic.Product_type = {model.ProductType} " : "") +
                    (model.Changeitem.AsNullIfWhiteSpace() != null ? $"AND Topic.Change_item = {model.Changeitem} " : "") +
                    (model.Processname.AsNullIfWhiteSpace() != null ? $"AND Topic.ProcessName = '{model.Processname}' " : "") +
                    (model.Partname.AsNullIfWhiteSpace() != null ? $"AND Topic.Partname = '{model.Partname}' " : "") +
                    (model.Partno.AsNullIfWhiteSpace() != null ? $"AND Topic.PartNo ='{model.Partno}' " : "") +
                    (model.Model.AsNullIfWhiteSpace() != null ? $"AND Topic.Model ='{model.Model}' " : "") +
                    (model.ControlNo.AsNullIfWhiteSpace() != null ? $"AND Topic.Code LIKE '{model.ControlNo}%' " : "") +
                    (model.Department.AsNullIfWhiteSpace() != null ? $@"AND Related.Department = '{model.Department}'" : "") + 
                    " ORDER BY Topic.Code DESC";
            var result = DB_CCS.Database.SqlQuery<TopicAlt>(sql).ToList();
            return result;
        } 

        public bool CheckTrialableByDepartment(string dept){
            var sql = $@"SELECT CASE WHEN EXISTS
                        (SELECT FK_Item_ID, FK_Department_ID , Name FROM Review_Item_Type_Department 
                        LEFT JOIN Department ON FK_Department_ID = Department.ID
                        WHERE FK_Item_ID = 24 AND Name = '{dept}')
                    THEN CAST(1 AS BIT)
                    ELSE CAST(0 AS BIT) END;";
            var result = DB_CCS.Database.SqlQuery<bool>(sql).First();
            return result;
        }
        public List<String> GetDepartmentList(){
            var sql = $"SELECT Name FROM Department;";
            var result = DB_CCS.Database.SqlQuery<String>(sql).ToList();
            return result;
        }
        
        public List<TopicNoti> GetRequestIssuedByDepartment(string dept){
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert , 'Issued' AS SubStatus 
                        FROM Topic
                        WHERE Topic.Department = '{dept}'
                            AND Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code AND t.Department = Topic.Department)
                            AND Topic.Status = 3;";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        public List<TopicNoti> GetRequestApprovedByDepartment(string dept){
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Approved' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        WHERE Related.Department = '{dept}'
                            AND Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code AND t.Department = Topic.Department)
                            AND Topic.[Type] = 'Internal'
                            AND Topic.Status = 7;";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        
        public List<TopicNoti> GetReviewIssuedByDepartment(string dept){
            var sql = $@"SELECT DISTINCT Code,	Topic.Revision,	Topic.Status,	Subject,	Detail,	Time_insert, 'Issued' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        WHERE Related.Department = '{dept}'
                            AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3' 
                            AND Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code AND t.Department = Topic.Department)
                            AND Topic.Status = 8
                            AND EXISTS ( 
                                SELECT * FROM Review WHERE Review.Department = Related.Department AND Review.Topic = Topic.Code AND Review.Status = 3
                                AND Review.Revision = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department)
                            );";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }

        public List<TopicNoti> GetReviewPendingByDepartment(string dept){
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Pending' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        WHERE Related.Department = '{dept}'
                            AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3' 
                            AND Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                            AND ((Topic.Status = 8 AND Topic.[Type] = 'Internal' ) OR ( Topic.Status IN (7,8) AND Topic.[Type] = 'External' ) )
                            AND NOT EXISTS ( 
                                SELECT * FROM Review WHERE Review.Department = Related.Department AND Review.Topic = Topic.Code 
                                AND Review.Revision = (
                                    SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department
                                    AND Review.Revision = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department)
                                )
                            );";

            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }

        public List<TopicNoti> GetReviewApproved(){ //For QC
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Approved' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        WHERE Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                            AND Topic.Status = 8
                          	AND 
                          		(SELECT COUNT(Review.ID) FROM Review WHERE Review.Topic = Topic.Code AND Review.Status = 1 AND Review.Department != 'QC1' AND Review.Department != 'QC2' AND Review.Department != 'QC3' AND Review.Revision = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department)) = 
                          		(SELECT COUNT(Related.ID) FROM Related WHERE Topic.Related = PK_Related AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3')
                            AND NOT EXISTS ( SELECT * FROM Review WHERE Review.Topic = Topic.Code AND Review.Status = 3 AND Review.Department != 'QC1' AND Review.Department != 'QC2' AND Review.Department != 'QC3'  AND Review.Revision = (SELECT MAX(rv.Revision) FROM Review rv WHERE rv.Topic = Review.Topic AND rv.Department = Review.Department));";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        public List<TopicNoti> GetTrialPendingByDepartment(string dept){ 
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Pending' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        LEFT JOIN Review ON Related.Department = Review.Department 
                        WHERE Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                        AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3' 

                            AND Topic.Status = 9
                            AND Review.Topic = Topic.Code
                          	AND EXISTS ( SELECT * FROM Review_Item ri WHERE Review.ID = ri.FK_Review_ID AND ri.FK_Item_Type = 24 AND ri.Status = 1 AND Review.Department = '{dept}')
                          	AND NOT EXISTS (SELECT * FROM Trial t2 WHERE t2.Topic = Topic.Code AND t2.Department = '{dept}' AND t2.Revision = (SELECT MAX(t3.Revision) FROM Trial t3 WHERE t3.Topic = t2.Topic AND t3.Department = t2.Department));";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        
        public List<TopicNoti> GetTrialIssuedByDepartment(string dept){ 
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Issued' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        LEFT JOIN Review ON Related.Department = Review.Department 
                        WHERE Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                            AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3' 
                            AND Topic.Status = 9
                            AND Review.Topic = Topic.Code
                          	AND EXISTS ( SELECT * FROM Review_Item ri WHERE Review.ID = ri.FK_Review_ID AND ri.FK_Item_Type = 24 AND ri.Status = 1 AND Review.Department = '{dept}')
                            AND EXISTS (SELECT * FROM Trial t2 WHERE t2.Topic = Topic.Code AND t2.Department = '{dept}' AND t2.Status = 3 AND t2.Revision = (SELECT MAX(t3.Revision) FROM Trial t3 WHERE t3.Topic = t2.Topic AND t3.Department = t2.Department));";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        public List<TopicNoti> GetTrialApproved(){ //For QC
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Approved' AS SubStatus 

                        FROM Topic
                        LEFT JOIN Review ON Review.Topic = Topic.Code
                        LEFT JOIN Review_Item ri ON Review.ID = ri.FK_Review_ID
                        WHERE Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                        	AND Review.Topic = Topic.Code
                            AND Topic.Status = 9
                            AND ri.FK_Item_Type = 24 AND ri.Status = 1
                            AND Review.Revision = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department )
							AND 
							(SELECT COUNT(Trial.ID) FROM Trial WHERE Trial.Topic = Topic.Code AND Trial.Status = 1 AND Trial.Department != 'QC1' AND Trial.Department != 'QC2' AND Trial.Department != 'QC3' AND Trial.Revision = (SELECT MAX(t.Revision) FROM Trial t WHERE t.Topic = Trial.Topic  AND t.Department = Trial.Department )) =

							(SELECT COUNT(*) 
							FROM Review 
							LEFT JOIN Review_Item ri ON FK_Review_ID = Review.ID
							WHERE Review.Revision = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic AND r.Department = Review.Department )
							AND Review.Topic = Topic.Code
							AND ri.FK_Item_Type = 24 AND ri.Status = 1);";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        public List<TopicNoti> GetConfirmPendingByDepartment(string dept){
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Pending' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        LEFT JOIN Department ON Related.Department = Department.Name
                        WHERE Related.Department = '{dept}'
                            AND Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                            AND Topic.Status = 10
                            AND (Department.[Group] = 'Production')
                            AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3' 
                            AND NOT EXISTS ( 
                                SELECT * FROM Confirm WHERE Confirm.Department = Confirm.Department AND Confirm.Topic = Topic.Code 
                                AND Confirm.Revision = (
                                    SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = Confirm.Topic AND c.Department = Confirm.Department
                                    AND Confirm.Revision = (SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = Confirm.Topic AND c.Department = Confirm.Department)
                                )
                            );";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        public List<TopicNoti> GetConfirmIssuedByDepartment(string dept){
            var sql = $@"SELECT DISTINCT Code,	Topic.Revision,	Topic.Status,	Subject,	Detail,	Time_insert , 'Issued' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        WHERE Related.Department = '{dept}'
                        AND Related.Department != 'QC1' AND Related.Department != 'QC2' AND Related.Department != 'QC3' 
                        AND Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                        AND Topic.Status = 10
                        AND EXISTS ( 
                            SELECT * FROM Confirm WHERE Confirm.Department = Related.Department AND Confirm.Topic = Topic.Code AND Confirm.Status = 3
                            AND Confirm.Revision = (SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = Confirm.Topic AND c.Department = Confirm.Department)
                        );";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }
        public List<TopicNoti> GetConfirmApproved(){
            var sql = $@"SELECT DISTINCT Code, Topic.Revision, Topic.Status, Subject, Detail, Time_insert, 'Approved' AS SubStatus 
                        FROM Topic
                        LEFT JOIN Related ON Topic.Related = PK_Related
                        LEFT JOIN Department ON Related.Department = Department.Name
                        WHERE Topic.Revision = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code  AND t.Department = Topic.Department)
                            AND Topic.Status = 10
	                        AND (Department.[Group] = 'Production')
                          	AND 
                        (SELECT COUNT(Confirm.ID) FROM Confirm WHERE Confirm.Topic = Topic.Code 
                        AND Confirm.Status = 1 AND Confirm.Department != 'QC1' AND Confirm.Department != 'QC2' AND Confirm.Department != 'QC3' AND Confirm.Revision = (SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = Confirm.Topic AND c.Department = Confirm.Department)) =
                                                        
                        (SELECT COUNT(Related.ID) FROM Related LEFT JOIN Department sub_d ON Related.Department = sub_d.Name 
                        WHERE Topic.Related = PK_Related AND Related.Department != 'QC1' AND Related.Department != 'QC2' 
                        AND Related.Department != 'QC3' AND (sub_d.[Group] = 'Production'))
                        
                        AND NOT EXISTS ( SELECT * FROM Confirm WHERE Confirm.Topic = Topic.Code AND Confirm.Status = 3 AND Confirm.Department != 'QC1' AND Confirm.Department != 'QC2' AND Confirm.Department != 'QC3' );";
            var result = DB_CCS.Database.SqlQuery<TopicNoti>(sql).ToList();
            return result;
        }

        


    }
}