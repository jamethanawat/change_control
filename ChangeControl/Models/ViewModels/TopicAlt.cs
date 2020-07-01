using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class TopicAlt
    {
        public long ID { get; set;}
        public string ID_Topic{ get; set;}
        public string Topic_type{ get; set; }
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
        public string Related{ get; set; }
        public Related RelatedList{ get; set; }
        public List<RelatedAlt> RelatedListAlt{ get; set; }
        public string User_insert{ get; set; }
        public string Time_insert{ get; set; }
        
        public TopicAlt(string ID_Topic,string Type,string ChangeItem,string ProductType,int Revision ,string Model,string PartNo,string PartName,string ProcessName,int Status,string App,string Subject,string Detail,string Timing ,string Related,string UserInsert,string TimeInsert){
            this.ID_Topic = ID_Topic;
            this.Topic_type = Type;
            this.Change_item = ChangeItem;
            this.Product_type = ProductType;
            this.Revision = Revision;
            this.PartNo = PartNo;
            this.PartName = PartName;
            this.ProcessName = ProcessName;
            this.Status = Status;
            this.App = App;
            this.Subject = Subject;
            this.Detail = Detail;
            this.Timing = Timing;
            this.Related = Related;
            this.User_insert = UserInsert;
            this.Time_insert = TimeInsert;
        }

        public TopicAlt(){
            this.ID_Topic = "ER-00000000";
            this.Topic_type = null;
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
            this.Related = null;
            this.User_insert = null;
            this.Time_insert = null;
            this.ID = 9999;
        }
    }
}