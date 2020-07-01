using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

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
        private class Line
        {
            public string line { get; set; }
        }
        public object GetLine(string Production){
            var sql = "SELECT line FROM bm_line where proddpt ='"+Production+"' ORDER BY line ASC";
            var result = _dbtapics.Database.SqlQuery<Line>(sql);
            return result.ToList();  
        } 
        
        public object GetSearch(SearchAttribute model){
            var where="";
            var sql = "";
            var temp_product_type = 0;
            var temp_change_item = 0;
            if (model.Type != null)
            {
                where += (model.Type == "Internal Change") ? "WHERE Topic.Topic_type='Internal'" : "WHERE Topic.Topic_type='External'";
            }
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Status=" + model.Status+"" : " AND Topic.Status=" + model.Status + "";
                // where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Status=" + model.Status+"" : " AND Topic.Status=" + model.Status + "";
            if (model.ProductType != null)
            {
               if (model.ProductType== "Meter 2R") temp_product_type = 1;
                else if (model.ProductType == "Meter 4R") temp_product_type = 2;
                else if (model.ProductType == "Agriculture") temp_product_type = 3;
                else if (model.ProductType == "FU/SP") temp_product_type = 4;
                else if(model.ProductType == "D/P") temp_product_type = 5;
                else if(model.ProductType == "Pointer") temp_product_type = 6;
                else if(model.ProductType == "Consumer") temp_product_type = 7;
                else if(model.ProductType == "Others") temp_product_type = 8;
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Product_type=" + temp_product_type + "" : " AND Topic.Product_type=" + temp_product_type + "";
            }
            //ยัง
           // where += (where.IndexOf("WHERE") == -1) ? "WHERE Status=" + Overstatus + "" : " AND Status=" + Overstatus + "";
            if (model.Changeitem != null)
            {
                if (model.Changeitem == "New Part") temp_change_item = 1;
                else if (model.Changeitem == "Material change") temp_change_item = 2;
                else if (model.Changeitem == "Manufacturing Method change") temp_change_item = 3;
                else if (model.Changeitem == "Inspection Method change") temp_change_item = 4;
                else if (model.Changeitem == "Jig /Tool change") temp_change_item = 5;
                else if (model.Changeitem == "Design Change") temp_change_item = 6;
                else if (model.Changeitem == "New Supplier") temp_change_item = 7;
                else if (model.Changeitem == "Machine change") temp_change_item = 8;
                else if (model.Changeitem == "Manufacturing Process order change") temp_change_item = 9;
                else if (model.Changeitem == "Packing ,Transportation change") temp_change_item = 10;
                else if (model.Changeitem == "Die / Mold change") temp_change_item = 11;
                else if (model.Changeitem == "Others") temp_change_item = 12;
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Change_item=" + temp_change_item + "" : " AND Topic.Change_item=" + temp_change_item + "";
            }
            if (model.ControlNo != "")
            {
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.ID_Topic ='" + model.ControlNo + "'" : " AND Topic.ID_Topic ='" + model.ControlNo + "'";             
            }
            if (model.Model != "")
            {
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Model ='" + model.Model + "'" : " AND Topic.Model ='" + model.Model + "'";
            }
            //ยัง
           // where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.Status=" + Chosechangeitem + "" : " AND Topic.Status=" + Chosechangeitem + "";

            if (model.Partno != "")
            {
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.PartNo ='" + model.Partno + "'" : " AND Topic.PartNo ='" + model.Partno + "'";
            }
            if (model.Partname != "")
            {
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.PartName ='" + model.Partname + "'" : " AND Topic.PartName ='" + model.Partname + "'";
            }
            if (model.Processname != "")
            {
                where += (where.IndexOf("WHERE") == -1) ? "WHERE Topic.ProcessName ='" + model.Processname + "'" : " AND Topic.ProcessName ='" + model.Processname + "'";
            }
            if (model.Related != null)
            {
                var remove = where.Replace("WHERE","");
                sql = @"SELECT Topic.ID, Topic.ID_Topic, Topic.Topic_type, Topic.Change_item, Topic.Product_type, Topic.Revision,Topic.Model, Topic.PartNo, Topic.PartName, 
                       Topic.ProcessName, Topic.Status, Topic.Related, Topic.User_insert, Topic.Time_insert FROM Related INNER JOIN
                       Topic ON Related.ID_Related = Topic.Related , (SELECT MAX(Revision) as Version, ID_Topic FROM CCS.dbo.Topic Group by ID_Topic) lastest WHERE (Related." + model.Related + " <> 0) AND "+remove+" AND Topic.Revision  = lastest.Version AND Topic.ID_Topic = lastest.ID_Topic Topic.ID ORDER BY DESC";
            }
            else
            {
                sql = @"SELECT Topic.ID, Topic.ID_Topic, Topic.Topic_type, Topic.Change_item, Topic.Product_type, Topic.Revision,Topic.Model, Topic.PartNo, Topic.PartName, 
                       Topic.ProcessName, Topic.Status, Topic.Related, Topic.User_insert, Topic.Time_insert FROM Topic , (SELECT MAX(Revision) as Version, ID_Topic FROM CCS.dbo.Topic Group by ID_Topic) lastest "+where+ " AND Topic.Revision  = lastest.Version AND Topic.ID_Topic = lastest.ID_Topic ORDER BY Topic.ID DESC";
            }
            //sql = "SELECT ID_Topic,Topic_type,Change_item,Product_type,Revision,Model,PartNo,PartName,User_insert FROM Topic INNER JOIN";
            //sql = sql + "[User] ON Topic.User_insert = [User].Name";
            // Session["sql"] = sql;
            var result = _dbCCS.Database.SqlQuery<SearchResult>(sql).ToList();
            return result;
        } 
    }
}