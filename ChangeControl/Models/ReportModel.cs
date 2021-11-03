using ChangeControl.Models.ViewModels;
using M_CS = ChangeControl.Models.CrystalModel;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using ChangeControl.Helpers;

namespace ChangeControl.Models
{
    public class ReportModel
    {

        private DbCCS DB_CCS;
     
        public ReportModel(){
            DB_CCS = new DbCCS();
        }
        public List<ReportExcel> GetReport(string StartDate, string EndDate)
        {
            var sql = $@"select t.Code AS TNSNO,
            CASE 
            WHEN t.Change_item > 12 THEN 
	            CONCAT ('OTHER (',(select Name from Change_Item where ID_Change_item =t.Change_item)  , ')')
            ELSE 
	            (select Name from Change_Item where ID_Change_item =t.Change_item )
            END as ChangeItem,

            (select Name from Product_type where ID_Product_Type  =t.Product_type) as ProductType , 
            CAST(t.Revision as varchar(5)) as Rev, 
            convert(VARCHAR(10),(cast( left(t.Time_insert ,4)+substring(t.Time_insert ,5,2)+substring(t.Time_insert,7,2) as datetime)),6) as DateRequest,
            (select Name from [User] where Code =t.User_insert) as RequestBy,
            (select Dept from [User] where Code =t.User_insert) as RequestDept,
             CONVERT(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) as ApprovedDateRequest,
             CASE 
             WHEN  t.timing ='99990101000000'THEN
	            TimingDesc
             WHEN t.timing ='' THEN 
 	             '-'
             WHEN LEN(t.timing)=14 THEN 
  	            convert(VARCHAR(10),(cast( left(t.timing,4)+substring(t.timing ,5,2)+substring(t.timing,7,2) as datetime)),6)
             END as ChangeDate, 
              RE.Department,
              (select Name from Status where id  = t.status) as Status,
                    CAST(DATEDIFF(dd, convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) ,
                    convert(VARCHAR(10),(cast( left(r.ApprovedDate,4)+substring(r.ApprovedDate ,5,2)+substring(r.ApprovedDate,7,2) as datetime)),6))as varchar(5)) AS ReviewFinishWithin,    

 
                    CAST(DATEDIFF(dd, convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) ,
                    convert(VARCHAR(10),(cast( left(tr.ApprovedDate,4)+substring(tr.ApprovedDate ,5,2)+substring(tr.ApprovedDate,7,2) as datetime)),6)) as varchar(5)) AS TrialConfirmFinishWithin,
                    CAST(DATEDIFF(dd, convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) ,
                    convert(VARCHAR(10),(cast( left(c.ApprovedDate,4)+substring(c.ApprovedDate ,5,2)+substring(c.ApprovedDate,7,2) as datetime)),6))as varchar(5)) AS InitialProductionFinishWithin,
                    CAST(r.Revision as varchar(5)) as RevReview,

 
                    convert(VARCHAR(10),(cast( left(r.[Date],4)+substring(r.[Date] ,5,2)+substring(r.[Date],7,2) as datetime)),6) as IssueDateReview,
                    convert(VARCHAR(10),(cast( left(r.ApprovedDate,4)+substring(r.ApprovedDate ,5,2)+substring(r.ApprovedDate,7,2) as datetime)),6) as ApprovedDateReview,

                    
                    CASE
                    WHEN (t.status <>9 and t.status<>10 and t.status<>11 and t.status<>12) THEN 
                     '-'
                    WHEN EXISTS (SELECT * FROM Department a LEFT JOIN Review_Item_Type_Department b ON b.FK_Department_ID = a.ID  where a.Name =RE.Department and b.FK_Item_ID=24) then 		
                       --มีสิทธิtrial ?
                            CASE 
                            --เลือกtrial ?
                            WHEN EXISTS (SELECT * FROM Review_Item LEFT JOIN Review ON Review_Item.FK_Review_ID = Review.ID where Review.ID =r.ID and FK_Item_Type=24 and Review_Item.Status =1) then 
                            --เลือก 'Trial'     
                                             
                              	CAST( tr.Revision as varchar(5))
                            ELSE
                            --ไม่เลือก			
                               'NONE' 
                            END
                            
                    WHEN EXISTS(SELECT * FROM Department  where Name=RE.Department and Name in (SELECT Name FROM Department a where a.[Group] in('Quality Control')and a.Audit=1 )  )THEN
                    --QC เท่านั้น
	                CASE
                          WHEN EXISTS  (            
				                        SELECT b.Department,max(b.Revision),a.FK_Item_Type FROM Review_Item a
							             LEFT JOIN Review b ON a.FK_Review_ID =b.ID
							             where b.Topic = t.Code and a.FK_Item_Type=24      
							             and b.FK_Topic =(select max(FK_Topic )from Review where Topic = t.Code  )
							             and a.Status =1
							             group by b.Department,a.FK_Item_Type                
						            )   THEN 								  
                   	  		 CAST( tr.Revision as varchar(5))
		                    Else 	     
		               		      'NONE'	                     		   
	                     	END                                		                 	
                    ELSE   'NONE'  
                    END as RevTrialConfirm,
                ----------------------------------                  
                    CASE
                    WHEN (t.status <>9 and t.status<>10 and t.status<>11 and t.status<>12) THEN 
                     '-'
                    WHEN EXISTS (SELECT * FROM Department a LEFT JOIN Review_Item_Type_Department b ON b.FK_Department_ID = a.ID  where a.Name =RE.Department and b.FK_Item_ID=24) then 		
                       --มีสิทธิtrial ?
                            CASE 
                            --เลือกtrial ?
                            WHEN EXISTS (SELECT * FROM Review_Item LEFT JOIN Review ON Review_Item.FK_Review_ID = Review.ID where Review.ID =r.ID and FK_Item_Type=24 and Review_Item.Status =1) then 
                            --เลือก 'Trial'
                                  convert(VARCHAR(10),(cast( left(tr.[Date],4)+substring(tr.[Date] ,5,2)+substring(tr.[Date],7,2) as datetime)),6) 	
                            ELSE
                            --ไม่เลือก			
                               'NONE' 
                            END
                            
                    WHEN EXISTS(SELECT * FROM Department  where Name=RE.Department and Name in (SELECT Name FROM Department a where a.[Group] in('Quality Control')and a.Audit=1 ))THEN
                    --QC เท่านั้น
                          CASE
		                    WHEN EXISTS  (            
					                        SELECT b.Department,max(b.Revision),a.FK_Item_Type FROM Review_Item a
								             LEFT JOIN Review b ON a.FK_Review_ID =b.ID
								             where b.Topic = t.Code and a.FK_Item_Type=24      
								             and b.FK_Topic =(select max(FK_Topic )from Review where Topic = t.Code  )
								             and a.Status =1
								             group by b.Department,a.FK_Item_Type                
								          ) THEN 								  
		                   	  		convert(VARCHAR(10),(cast( left(tr.[Date],4)+substring(tr.[Date] ,5,2)+substring(tr.[Date],7,2) as datetime)),6) 
		                    Else 	     
		               		      'NONE'	                     		   
	                     	END                    	
                       
                    ELSE   'NONE'  
                    END as IssueDateTrialConfirm,
                    ----------------------------------------                                    
                    CASE
                    WHEN (t.status <>9 and t.status<>10 and t.status<>11 and t.status<>12) THEN 
                     '-'
                    WHEN EXISTS (SELECT * FROM Department a LEFT JOIN Review_Item_Type_Department b ON b.FK_Department_ID = a.ID  where a.Name =RE.Department and b.FK_Item_ID=24) then 		
                       --มีสิทธิtrial ?
                            CASE 
                            --เลือกtrial ?
                            WHEN EXISTS (SELECT * FROM Review_Item LEFT JOIN Review ON Review_Item.FK_Review_ID = Review.ID where Review.ID =r.ID and FK_Item_Type=24 and Review_Item.Status =1) then 
                            --เลือก 'Trial'
                                  convert(VARCHAR(10),(cast( left(tr.ApprovedDate,4)+substring(tr.ApprovedDate ,5,2)+substring(tr.ApprovedDate,7,2) as datetime)),6)
                            ELSE
                            --ไม่เลือก			
                               'NONE' 
                            END
                            
                    WHEN EXISTS(SELECT * FROM Department  where Name=RE.Department and Name in (SELECT Name FROM Department a where a.[Group] in('Quality Control')and a.Audit=1 ))THEN
                    --QC เท่านั้น
                     CASE
		                    WHEN  EXISTS  (            
					                         SELECT b.Department,max(b.Revision),a.FK_Item_Type FROM Review_Item a
								             LEFT JOIN Review b ON a.FK_Review_ID =b.ID
								             where b.Topic = t.Code and a.FK_Item_Type=24      
								             and b.FK_Topic =(select max(FK_Topic )from Review where Topic = t.Code  )
								             and a.Status =1
								             group by b.Department,a.FK_Item_Type                
								            ) THEN 								  
		                   	     convert(VARCHAR(10),(cast( left(tr.ApprovedDate,4)+substring(tr.ApprovedDate ,5,2)+substring(tr.ApprovedDate,7,2) as datetime)),6)
		                    Else 	     
		               		      'NONE'	                     		   
	                     	END         
                  
                    ELSE   'NONE'  
                    END as ApprovedDateTrialConfirm,
                    ---------------------------------------
                   --CAST( tr.Revision as varchar(5))as RevTrialConfirm,
                  --  convert(VARCHAR(10),(cast( left(tr.[Date],4)+substring(tr.[Date] ,5,2)+substring(tr.[Date],7,2) as datetime)),6) as IssueDateTrialConfirm,
                    --convert(VARCHAR(10),(cast( left(tr.ApprovedDate,4)+substring(tr.ApprovedDate ,5,2)+substring(tr.ApprovedDate,7,2) as datetime)),6) as ApprovedDateTrialConfirm,                                    
           
                     CAST(c.Revision as varchar(5)) as RevInitialProduction,
                     
                    convert(VARCHAR(10),(cast( left(c.[Date],4)+substring(c.[Date] ,5,2)+substring(c.[Date],7,2) as datetime)),6) as IssueDateInitialProduction,
                    convert(VARCHAR(10),(cast( left(c.ApprovedDate,4)+substring(c.ApprovedDate ,5,2)+substring(c.ApprovedDate,7,2) as datetime)),6) as ApprovedDateInitialProduction
                         
                from topic t
                left join 
                Related Re on t.Related = Re.PK_Related
                left join 
                Review r on t.Code =r.Topic and Re.Department = r.Department and  r.Revision= ((SELECT MAX(Revision) FROM Review  WHERE Topic =t.Code AND Department =r.Department))
                left join 
                Trial tr on t.Code = tr.Topic and Re.Department = tr.Department and tr.Revision= ((SELECT MAX(Revision) FROM Trial  WHERE Topic =t.Code AND Department =tr.Department))
                left join 
                Confirm c on t.Code = c.Topic and Re.Department = c.Department and c.Revision= ((SELECT MAX(Revision) FROM Confirm  WHERE Topic =t.Code AND Department =c.Department))
                where  t.Revision =(SELECT MAX(Revision) FROM topic where Code = t.Code)
                  and(SUBSTRING(Time_insert, 0, 9) >= { StartDate} and SUBSTRING(Time_insert, 0, 9) <= { EndDate})
                --and t.Code = 'IN-2105119'
                Order BY TNSNO,DateRequest;";


                      
            var result = DB_CCS.Database.SqlQuery<ReportExcel>(sql);
         
            return result.ToList();
        }
        public List<test> test()
        {
            var sql = @"select top (10) dept from [User]";
            var result = DB_CCS.Database.SqlQuery<test>(sql);

            return result.ToList();
        }

        public List<M_CS.Topic> GetTopicByCode(string topic_code){
            try{
                var sql = $@"SELECT TOP(1) Code, Type, User_insert AS [User], Change_Item.Name as Change_item, Product_Type.Name AS Product_Type, Department, Revision, Model, PartNo, PartName, ProcessName, Topic.Status, Status.Name AS FullStatus  , [APP/IPP] as App, dbo.udf_StripBreakingSpace(dbo.udf_StripHTML(Subject)) AS Subject, dbo.udf_StripBreakingSpace(dbo.udf_StripHTML(Detail)) AS Detail, Timing, TimingDesc, Related, User_insert, Time_insert AS [Date], ApprovedBy, ApprovedDate, 
                    Topic.ID FROM Topic 
                    LEFT JOIN Status ON Topic.Status = Status.ID 
                    LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                    LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type 
                    WHERE Code = '{topic_code}' 
                    ORDER BY Revision DESC;";
                    var Topic = DB_CCS.Database.SqlQuery<M_CS.Topic>(sql).ToList();
                    Topic.ForEach(e => {
                        this.ConvertUserAndDate(e, IssueUser => e.User = IssueUser, ApproverUser => e.ApprovedBy = ApproverUser , CreatedDate => e.Date = CreatedDate, ApprovedDate => e.ApprovedDate = ApprovedDate);
                    });
                return Topic;
            }catch(Exception err){
                return new List<M_CS.Topic>();
            }
        }

        public List<M_CS.Review> GetReviewByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, [Date], [User], Status, Department, ApprovedBy, ApprovedDate
                            FROM Review ,
                            (SELECT MAX(Revision) as Version, Department as dept FROM Review WHERE Topic = '{topic_code}'  Group by Department) lastest
                            WHERE Topic = '{topic_code}' 
                            AND Review.Revision  = lastest.Version AND Review.Department = lastest.dept 
                            ORDER BY ( CASE WHEN Substring(Topic,1,2) = 'IN' 
                            AND Review.Department = 
                                (SELECT Name FROM Department WHERE Department.[Group] = 'Production Engineer Process' 
                                AND Audit = 1 AND Name = Review.Department) THEN 0 ELSE 1 END) 
                            , [Date] ASC;";
                var Review = DB_CCS.Database.SqlQuery<M_CS.Review>(sql).ToList();
                Review.ForEach(e => {
                    this.ConvertUserAndDate(e, IssueUser => e.User = IssueUser, ApproverUser => e.ApprovedBy = ApproverUser , CreatedDate => e.Date = CreatedDate, ApprovedDate => e.ApprovedDate = ApprovedDate);
                });
                return Review;
            }catch(Exception err){
                return new List<M_CS.Review>();
            }
        }

        public List<M_CS.ReviewItem> GetReviewItemByTopicCode(string topic_code){
            var sql = $@"SELECT DISTINCT FK_Review_ID AS Review, Review_Item.ID, FK_Item_Type AS Type, CASE
                            WHEN Review_Item.Status IS NULL THEN 2
                            ELSE Review_Item.Status END AS Status, Description, Name, Seq FROM Review_Item 
                        LEFT JOIN Review_Item_Type ON FK_Item_Type = Review_Item_Type.ID 
                        LEFT JOIN Review ON Review_Item.FK_Review_ID = Review.ID 
                        LEFT JOIN Topic ON Review.Topic = Topic.Code
                        WHERE Topic.Code = '{topic_code}' 
                        AND Review.Revision = (SELECT MAX(rv.Revision) FROM Review rv WHERE rv.Topic = Topic.Code AND rv.Department = Review.Department)
                        ORDER BY Seq ASC;";
            var ReviewItem = DB_CCS.Database.SqlQuery<M_CS.ReviewItem>(sql).ToList();
            return ReviewItem;
        }

        public List<M_CS.Trial> GetTrialByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, Detail, [Date], [User], Department, Status, Revision, ApprovedBy, UpdateBy, UpdateDate, ApprovedDate 
                FROM Trial,
                    (SELECT MAX(Revision) as Version, Department as dept FROM Trial WHERE Topic = '{topic_code}' Group by Department) lastest
                WHERE Topic = '{topic_code}' 
                AND Trial.Revision  = lastest.Version AND Trial.Department = lastest.dept 
                ORDER BY [Date] ASC;";
                var Trial = DB_CCS.Database.SqlQuery<M_CS.Trial>(sql).ToList();
                Trial.ForEach(e => {
                    this.ConvertUserAndDate(e, IssueUser => e.User = IssueUser, ApproverUser => e.ApprovedBy = ApproverUser , CreatedDate => e.Date = CreatedDate, ApprovedDate => e.ApprovedDate = ApprovedDate);
                });
                return Trial;
            }catch(Exception ex){
                return new List<M_CS.Trial>();
            }
        }

        public List<M_CS.Confirm> GetConfirmByTopicCode(string topic_code){
            try{
                var sql = $@"SELECT ID, Topic, Detail, [Date], [User], Department, Status, Revision, ApprovedBy, UpdateBy, UpdateDate, ApprovedDate 
                FROM Confirm,
                    (SELECT MAX(Revision) as Version, Department as dept FROM Confirm WHERE Topic = '{topic_code}' Group by Department) lastest
                WHERE Topic = '{topic_code}' 
                AND Confirm.Revision  = lastest.Version AND Confirm.Department = lastest.dept
                ORDER BY [Date] ASC;";
                var Confirm = DB_CCS.Database.SqlQuery<M_CS.Confirm>(sql).ToList();
                Confirm.ForEach(e => {
                    this.ConvertUserAndDate(e, IssueUser => e.User = IssueUser, ApproverUser => e.ApprovedBy = ApproverUser , CreatedDate => e.Date = CreatedDate, ApprovedDate => e.ApprovedDate = ApprovedDate);
                });
                return Confirm;
            }catch(Exception ex){
                return new List<M_CS.Confirm>();
            }
        }

        private void ConvertUserAndDate(dynamic obj, Action<string> SetIssueUser, Action<string> SetApproverUser, Action<string> SetCreatedDate, Action<string> SetApprovedDate){
            string us = obj.User;
            string ap = obj.ApprovedBy;
            string cd = obj.Date;
            string ad = obj.ApprovedDate;
            if(us.AsNullIfWhiteSpace() != null) SetIssueUser(us.ToFullName());
            if(ap.AsNullIfWhiteSpace() != null) SetApproverUser(ap.ToFullName());
            if(cd.AsNullIfWhiteSpace() != null) SetCreatedDate(cd.StringToDateTime());
            if(ad.AsNullIfWhiteSpace() != null) SetApprovedDate(ad.StringToDateTime());
        }

        public string GetRelatedByTopicCode(string topic_code){
            try{
                var related_str = "";
                var i = 0;
                var sql = $@"SELECT Department FROM Related WHERE PK_Related = (SELECT TOP(1) Related FROM Topic WHERE Code = '{topic_code}' ORDER BY Revision DESC);";
                var Related = DB_CCS.Database.SqlQuery<string>(sql).ToList();
                Related.ForEach(e => {
                    related_str += (i == 0) ? $"{e} " : $", {e}";
                    i++;
                });
                return related_str;
            }catch(Exception ex){
                return "";
            }
        }
    }
}