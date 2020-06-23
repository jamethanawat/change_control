using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class Topic
    {
        public string ID_Topic{ get; set;}
        public string Topic_type{ get; set; }
        public int Change_item{ get; set; }
        public int Product_type{ get; set; }
        public int Revision{ get; set; }
        public string Model{ get; set; }
        public string PartNo{ get; set; } 
        public string PartName{ get; set; } 
        public string ProcessName{ get; set; }
        public int Status{ get; set; }
        public string App{ get; set; }
        public string Subject{ get; set; }
        public string Detail{ get; set; }
        public string Timing{ get; set; }
        public string File{ get; set; }
        public string Related{ get; set; }
        public string User_insert{ get; set; }
        public string Time_insert{ get; set; }
        public int ID { get; set;}
        
        public Topic(string ID_Topic,string changeType,int changeItem,int productType,int revision ,string model,string partNo,string partName,string processName,int status,string appDescription,string subject,string detail,string timing , string file, string related,string UserInsert,string TimeInsert){
            this.ID_Topic = ID_Topic;
            this.Topic_type = changeType;
            this.Change_item = changeItem;
            this.Product_type = productType;
            this.Revision = revision;
            this.Model = model;
            this.PartNo = partNo;
            this.PartName = partName;
            this.ProcessName = processName;
            this.Status = status;
            this.App = appDescription;
            this.Subject = subject;
            this.Detail = detail;
            this.Timing = timing;
            this.File = file;
            this.Related = related;
            this.User_insert = UserInsert;
            this.Time_insert = TimeInsert;
        }

        public Topic(){
            this.ID_Topic = null;
            this.Topic_type = null;
            this.Change_item = 0;
            this.Product_type = 0;
            this.Revision = 0;
            this.PartNo = null;
            this.PartName = null;
            this.ProcessName = null;
            this.Status = 0;
            this.App = null;
            this.Subject = null;
            this.Detail = null;
            this.Timing = null;
            this.File = null;
            this.Related = null;
            this.User_insert = null;
            this.Time_insert = null;
        }
    }
}