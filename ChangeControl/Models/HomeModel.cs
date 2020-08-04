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
            
            sql = $@"SELECT Topic.Code, Topic.Type, Change_item.Name AS Change_item , Product_type.Name AS Product_type, Topic.Department, Topic.Revision,Topic.Model, Topic.PartNo, Topic.PartName, Topic.Detail,
                    Topic.ProcessName, Status.Name AS FullStatus, Topic.Related, Topic.User_insert, Topic.Time_insert AS Date FROM Topic 
                    LEFT JOIN Status ON Topic.Status = Status.id
                    LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item " +
                    (model.ProductType.AsNullIfWhiteSpace() != null ? $"LEFT JOIN Related ON Topic.Related = Related.ID " : "") + 
                    $@"LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type WHERE Topic.Revision  = (SELECT MAX(t.Revision) FROM Topic t WHERE t.Code = Topic.Code) " +
                    (model.Type != null ?  $"AND Topic.Type = '{model.Type}' " : "") +
                    $"AND Topic.Status = {model.Status} " +
                    (model.ProductType.AsNullIfWhiteSpace() != null ? $"AND Topic.Product_type = {model.ProductType} " : "") +
                    (model.Changeitem.AsNullIfWhiteSpace() != null ? $"AND Topic.Change_item = {model.Changeitem} " : "") +
                    (model.Processname.AsNullIfWhiteSpace() != null ? $"AND Topic.ProcessName = '{model.Processname}' " : "") +
                    (model.Partname.AsNullIfWhiteSpace() != null ? $"AND Topic.Partname = '{model.Partname}' " : "") +
                    (model.Partno.AsNullIfWhiteSpace() != null ? $"AND Topic.PartNo ='{model.Partno}' " : "") +
                    (model.Model.AsNullIfWhiteSpace() != null ? $"AND Topic.Model ='{model.Model}' " : "") +
                    (model.ControlNo.AsNullIfWhiteSpace() != null ? "AND Topic.Code = '{model.ControlNo}' " : "") +
                    (model.Department.AsNullIfWhiteSpace() != null ? $"AND (Topic.Department LIKE '%{model.Department}%' OR Related.{model.Department} = 1 )" : "") + 
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