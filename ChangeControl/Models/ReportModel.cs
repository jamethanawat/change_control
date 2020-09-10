using ChangeControl.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class ReportModel
    {

        private DbCCS DB_CCS;
     
        public ReportModel()
        {
          
            DB_CCS = new DbCCS();
    

        }
        public List<ReportExcel> GetReport(string StartDate, string EndDate)
        {
            var sql = $@"
                           
                                    select t.Code AS TNSNO,(select Name from Change_Item where ID_Change_item =t.Change_item) as ChangeItem,(select Name from Change_Item where ID_Change_item =t.Product_type) as ProductType , CAST(t.Revision as varchar(5)) as Rev, convert(VARCHAR(10),(cast( left(t.Time_insert ,4)+substring(t.Time_insert ,5,2)+substring(t.Time_insert,7,2) as datetime)),6) as DateRequest,
                                    (select Email from [User] where Code =t.User_insert) as RequestBy,
                                      convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) as ApprovedDateRequest,
                                      RE.Department,
                                        CASE
                                        -- QC1,2,3
 	                                     WHEN RE.Department in (SELECT Name FROM Department a where (a.[Group] in('Quality Control')and a.Audit=1 )) THEN
  	                                    'Review and Trial & Confirm and Initial Production'
 		                                     --มีสิทธิtrial ?
	                                    WHEN EXISTS (SELECT * FROM Department a LEFT JOIN Review_Item_Type_Department b ON b.FK_Department_ID = a.ID  where a.Name =RE.Department and b.FK_Item_ID=24) then 		
			                                    CASE 
			                                    --เลือกtrial ?
			                                    WHEN EXISTS (SELECT * FROM Review_Item LEFT JOIN Review ON Review_Item.FK_Review_ID = Review.ID where Review.ID =r.ID and FK_Item_Type=24) then 
			                                    --เลือก 'Review and Trial'
				                                    CASE WHEN EXISTS(SELECT * FROM Department  where Name=RE.Department and Name in (SELECT Name FROM Department a where a.[Group] ='Production' or (a.[Group] in('Quality Control')and a.Audit=1 ) ) )THEN			
				                                    'Review and Trial & Confirm and Initial Production'
				                                    ELSE
				                                    'Review and Trial & Confirm'
				                                    END		
			                                    ELSE
			                                    --ไม่เลือก			
				                                    CASE WHEN EXISTS(SELECT * FROM Department  where Name=RE.Department and Name in (SELECT Name FROM Department a where a.[Group] ='Production' or (a.[Group] in('Quality Control')and a.Audit=1 ) ) )THEN			
				                                    'Review and Initial Production'
				                                    ELSE 'Review Only'
				                                    END
			                                    END
                                         ELSE   'Review Only'  
	                                    END as Status,
	                                    --คำนวณวันที่

	                                    CAST(DATEDIFF(dd, convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) ,
                                     convert(VARCHAR(10),(cast( left(r.ApprovedDate,4)+substring(r.ApprovedDate ,5,2)+substring(r.ApprovedDate,7,2) as datetime)),6))as varchar(5)) AS ReviewFinishWithin,	
 
                                      CAST(DATEDIFF(dd, convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) ,
                                     convert(VARCHAR(10),(cast( left(tr.ApprovedDate,4)+substring(tr.ApprovedDate ,5,2)+substring(tr.ApprovedDate,7,2) as datetime)),6)) as varchar(5)) AS TrialConfirmFinishWithin,
                                     CAST(DATEDIFF(dd, convert(VARCHAR(10),(cast( left(t.ApprovedDate  ,4)+substring(t.ApprovedDate ,5,2)+substring(t.ApprovedDate,7,2) as datetime)),6) ,
                                      convert(VARCHAR(10),(cast( left(c.ApprovedDate,4)+substring(c.ApprovedDate ,5,2)+substring(c.ApprovedDate,7,2) as datetime)),6))as varchar(5)) AS InitialProductionFinishWithin,
                                    CAST(r.Revision as varchar(5)) as RevReview,

                                       convert(VARCHAR(10),(cast( left(r.[Date],4)+substring(r.[Date] ,5,2)+substring(r.[Date],7,2) as datetime)),6) as IssueDateReview,
                                     convert(VARCHAR(10),(cast( left(r.ApprovedDate,4)+substring(r.ApprovedDate ,5,2)+substring(r.ApprovedDate,7,2) as datetime)),6) as ApprovedDateReview,

                                      CAST( tr.Revision as varchar(5))as RevTrialConfirm,
                                     convert(VARCHAR(10),(cast( left(tr.[Date],4)+substring(tr.[Date] ,5,2)+substring(tr.[Date],7,2) as datetime)),6) as IssueDateTrialConfirm,
                                     convert(VARCHAR(10),(cast( left(tr.ApprovedDate,4)+substring(tr.ApprovedDate ,5,2)+substring(tr.ApprovedDate,7,2) as datetime)),6) as ApprovedDateTrialConfirm,
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
                                    and(SUBSTRING(Time_insert, 0, 9) >= {StartDate} and SUBSTRING(Time_insert, 0, 9) <= {EndDate})
                                    --and t.Code = 'IN-2009018'
                                    Order BY TNSNO,DateRequest";
            var result = DB_CCS.Database.SqlQuery<ReportExcel>(sql);
         
            return result.ToList();
        }
    }
}