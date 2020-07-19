using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using StringHelper;

namespace ChangeControl.Models
{
    public class HomeModel
    {
        private DbTapics _dbtapics;
        private DbCCS _dbCCS;
        public HomeModel(){
            _dbtapics = new DbTapics();
            _dbCCS = new DbCCS();
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
            if (model.Type != null){
                where += (model.Type == "Internal Change") ? "WHERE Topic.Type='Internal'" : "WHERE Topic.Type='External'";
            }
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Status=" + model.Status+"" : " AND Topic.Status=" + model.Status + "";
                // where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Status=" + model.Status+"" : " AND Topic.Status=" + model.Status + "";
            if(model.ProductType.AsNullIfWhiteSpace() != null){
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Product_type=" + model.ProductType + "" : " AND Topic.Product_type=" + model.ProductType + "";
            }
            //ยัง
           // where += (where.IndexOf("WHERE") == -1) ? "WHERE Status=" + Overstatus + "" : " AND Status=" + Overstatus + "";
            if(model.Changeitem.AsNullIfWhiteSpace() != null){
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Change_item=" + model.Changeitem + "" : " AND Topic.Change_item=" + model.Changeitem + "";
            }
            if (model.ControlNo != ""){
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Code ='" + model.ControlNo + "'" : " AND Topic.Code ='" + model.ControlNo + "'";             
            }
            if (model.Model != ""){
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Model ='" + model.Model + "'" : " AND Topic.Model ='" + model.Model + "'";
            }
            //ยัง
           // where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Status=" + Chosechangeitem + "" : " AND Topic.Status=" + Chosechangeitem + "";

            if(model.Partno != ""){
               where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.PartNo ='" + model.Partno + "'" : " AND Topic.PartNo ='" + model.Partno + "'";
            }
            if(model.Partname != ""){
               where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.PartName ='" + model.Partname + "'" : " AND Topic.PartName ='" + model.Partname + "'";
            }
            if(model.Processname != ""){
               where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.ProcessName ='" + model.Processname + "'" : " AND Topic.ProcessName ='" + model.Processname + "'";
            }
            sql = $@"SELECT Topic.Code, Topic.Type, Change_item.Name AS Change_item , Product_type.Name AS Product_type, Topic.Department, Topic.Revision,Topic.Model, Topic.PartNo, Topic.PartName, Topic.Detail,
                    Topic.ProcessName, Status.Name AS FullStatus, Topic.Related, Topic.User_insert, Topic.Time_insert AS Date FROM Topic 
                    LEFT JOIN Status ON Topic.Status = Status.id
                    LEFT JOIN Change_Item ON Topic.Change_item = ID_Change_item
                    LEFT JOIN Product_Type ON Topic.Product_Type = ID_Product_Type, 
                    (SELECT MAX(Revision) as Version, Code FROM CCS.dbo.Topic Group by Code) lastest
                    {where} AND Topic.Revision  = lastest.Version AND Topic.Code = lastest.Code" + (model.Department != "" ? $" AND Topic.Department LIKE '%{model.Department}%'" : " ") + "ORDER BY Topic.ID DESC";
            var result = _dbCCS.Database.SqlQuery<TopicAlt>(sql).ToList();
            return result;
        } 
    }
}