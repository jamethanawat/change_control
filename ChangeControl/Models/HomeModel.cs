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
        
        public List<TopicAlt> GetSearch(SearchAttribute model){
            var where="";
            var sql = "";
            var ov_command = "";
            var count_related_command = $@"(SELECT SUM(P1 + P2 + P3A + P3M + P4 + P5 + P6 + P7 + IT + MKT + PC1 + PC2 + PCH + PE1 + PE2 + PE2_SMT + PE2_PCB + PE2_MT + QC_IN1 + QC_IN2 + QC_IN3 + QC_FINAL1 + QC_FINAL2 +   
                QC_FINAL3 + QC_NFM1 + QC_NFM2 + QC_NFM3 + PE1_Process + PE2_Process + P5_ProcessDesign + P6_ProcessDesign) 
                FROM Related WHERE Related.ID = Topic.Related )";
            if(model.Overstatus == 2){
                ov_command = $@"{count_related_command} 
		        -	(SELECT COUNT(Review.ID) FROM Review WHERE  
					 Review.Revision  = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic) AND  Review.Topic = Topic.Code 
                     AND Review.Status = 1
                     AND Review.Department != 'QC1' AND Review.Department != 'QC2' AND Review.Department != 'QC3')= 0";
            }else if(model.Overstatus == 3){
                ov_command = $@"(SELECT COUNT(ID) FROM Review 
                            LEFT JOIN Review_Item ON Review_Item.FK_Review_ID = Review.ID 
                            WHERE Review.Revision  = (SELECT MAX(r.Revision) FROM Review r WHERE r.Topic = Review.Topic) AND  Review.Topic = Topic.Code
                            AND Review_Item.FK_Item_Type = 24
                            AND Review_Item.Status = 1)
                -   (SELECT COUNT(Trial.ID) FROM Trial WHERE
					 Trial.Revision  = (SELECT MAX(t.Revision) FROM Trial t WHERE t.Topic = Trial.Topic) AND Trial.Topic = Topic.Code 
                     AND Trial.Status = 1
                     AND Trial.Department != 'QC1' AND Trial.Department != 'QC2' AND Trial.Department != 'QC3' )= 0";

            }else if(model.Overstatus == 4){
                ov_command = $@"{count_related_command} 
		        -	(SELECT COUNT(Confirm.ID) FROM Confirm WHERE
					 Confirm.Revision  = (SELECT MAX(c.Revision) FROM Confirm c WHERE c.Topic = Confirm.Topic) AND  Confirm.Topic = Topic.Code 
                     AND Confirm.Status = 1
                     AND Confirm.Department != 'QC1' AND Confirm.Department != 'QC2' AND Confirm.Department != 'QC3')= 0";
            }
            sql = $@"SELECT Topic.Code, Topic.Type, Change_item.Name AS Change_item , Product_type.Name AS Product_type, Topic.Department, Topic.Revision,Topic.Model, Topic.PartNo, Topic.PartName, Topic.Detail,
                    Topic.ProcessName, Status.Name AS FullStatus, Topic.Related, Topic.User_insert, Topic.Time_insert AS Date FROM Topic 
                    LEFT JOIN Status ON Topic.Status = Status.id 
                    LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item 
                    LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type " +
                    (model.Department.AsNullIfWhiteSpace() != null ? $"LEFT JOIN Related ON Topic.Related = Related.ID " : "") +
                    "WHERE Topic.Revision  = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code) " +
                    (model.Type.AsNullIfWhiteSpace() != null ?  $"AND Topic.Type = '{model.Type}' " : "") +
                    (ov_command != "" ?  $"AND {ov_command} " : "") +
                    (model.Status != 0 ?  $"AND Topic.Status = '{model.Status}' " : "") +
                    (model.ProductType.AsNullIfWhiteSpace() != null ? $"AND Topic.Product_type = {model.ProductType} " : "") +
                    (model.Changeitem.AsNullIfWhiteSpace() != null ? $"AND Topic.Change_item = {model.Changeitem} " : "") +
                    (model.Processname.AsNullIfWhiteSpace() != null ? $"AND Topic.ProcessName = '{model.Processname}' " : "") +
                    (model.Partname.AsNullIfWhiteSpace() != null ? $"AND Topic.Partname = '{model.Partname}' " : "") +
                    (model.Partno.AsNullIfWhiteSpace() != null ? $"AND Topic.PartNo ='{model.Partno}' " : "") +
                    (model.Model.AsNullIfWhiteSpace() != null ? $"AND Topic.Model ='{model.Model}' " : "") +
                    (model.ControlNo.AsNullIfWhiteSpace() != null ? $"AND Topic.Code LIKE '{model.ControlNo}%' " : "") +
                    (model.Department.AsNullIfWhiteSpace() != null ? $"AND (Topic.Department LIKE '{model.Department}' OR Related.{model.Department} = 1 )" : "") + 
                    " ORDER BY Topic.ID DESC";
            var result = DB_CCS.Database.SqlQuery<TopicAlt>(sql).ToList();
            return result;
        } 

        public List<String> GetDepartmentList(){
            var sql = $"SELECT Name FROM CCS.dbo.Department;";
            var result = DB_CCS.Database.SqlQuery<String>(sql).ToList();
            return result;
        }
    }
}