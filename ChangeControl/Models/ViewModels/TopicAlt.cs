using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class TopicAlt
    {
        public long ID { get; set;}
        public string Code{ get; set;}
        public string Type{ get; set; }
        public string Change_item{ get; set; }
        public string Product_type{ get; set; }
        public int Revision{ get; set; }
        public string Model{ get; set; }
        public string PartNo{ get; set; } 
        public string PartName{ get; set; } 
        public string ProcessName{ get; set; }
        public int Status{ get; set; }
        public string App{ get; set; }
        public string Subject{ get; set; }
        public string Detail{ get; set; }
        public string Timing { get; set; }
        public List<FileItem> FileList { get; set; }
        public long Related{ get; set; }
        public Related RelatedList{ get; set; }
        public List<RelatedAlt> RelatedListAlt{ get; set; }
        public string User_insert{ get; set; }
        public string Time_insert{ get; set; }
        
        public TopicAlt(string Code,string Type,string ChangeItem,string ProductType,int Revision ,string Model,string PartNo,string PartName,string ProcessName,int Status,string App,string Subject,string Detail,string Timing ,long Related,string UserInsert,string TimeInsert){
            this.Code = Code.Trim() == "" ? "-" : Code;
            this.Type = Type.Trim() == "" ? "-" : Type;
            this.Change_item = ChangeItem.Trim() == "" ? "-" : ChangeItem;
            this.Product_type = ProductType.Trim() == "" ? "-" : ProductType;
            this.Revision = Revision;
            this.PartNo = PartNo.Trim() == "" ? "-" : PartNo;
            this.PartName = PartName.Trim() == "" ? "-" : PartName;
            this.ProcessName = ProcessName.Trim() == "" ? "-" : ProcessName;
            this.Status = Status;
            this.App = App.Trim() == "" ? "-" : App;
            this.Subject = Subject.Trim() == "" ? "-" : Subject;
            this.Detail = Detail.Trim() == "" ? "-" : Detail;
            this.Timing = Timing;
            this.Related = Related;
            this.User_insert = UserInsert;
            this.Time_insert = TimeInsert;
        }

        public TopicAlt(){
            this.Code = "ER-00000000";
            this.Type = null;
            this.Change_item = null;
            this.Product_type = null;
            this.Revision = 0;
            this.PartNo = null;
            this.PartName = null;
            this.ProcessName = null;
            this.Status = 0;
            this.App = null;
            this.Subject = null;
            this.Detail = null;
            this.Timing = null;
            this.Related = 0;
            this.User_insert = null;
            this.Time_insert = null;
            this.ID = 9999;
        }
    }
}