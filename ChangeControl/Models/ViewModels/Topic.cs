using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class Topic
    {
        public long ID { get; set;}
        public string Code{ get; set;}
        public string Type{ get; set; }
        public int Change_item{ get; set; }
        public int Product_type{ get; set; }
        public int Revision{ get; set; }
        public string Department{ get; set; }
        public string Model{ get; set; }
        public string PartNo{ get; set; } 
        public string PartName{ get; set; } 
        public string ProcessName{ get; set; }
        public int Status{ get; set; }
        public string App{ get; set; }
        public string Subject{ get; set; }
        public string Detail{ get; set; }
        public string Timing{ get; set; }
        public string TimingDesc{ get; set; }
        public long Related{ get; set; }
        public string User_insert{ get; set; }
        public string Time_insert{ get; set; }
        public string Date{ get; set; }
        public string Risk{ get; set; }
        public string Specify{ get; set; }
        public string Management_Plan { get; set; }
        
        public Topic(string Code,string changeType,int changeItem,int productType,int revision ,string department,string model,string partNo,string partName,string processName,int status,string appDescription,string subject,
            string detail,string timing, string timingDesc,long related,string UserInsert,string TimeInsert,string risk ,string specify,string management_Plan)
        {
            this.Code = Code;
            this.Type = changeType;
            this.Change_item = changeItem;
            this.Product_type = productType;
            this.Revision = revision;
            this.Department = department;
            this.Model = model;
            this.PartNo = partNo;
            this.PartName = partName;
            this.ProcessName = processName;
            this.Status = status;
            this.App = appDescription;
            this.Subject = subject;
            this.Detail = detail;
            this.Timing = timing;
            this.TimingDesc = timingDesc;
            this.Related = related;
            this.User_insert = UserInsert;
            this.Time_insert = TimeInsert;
            this.Risk = risk;
            this.Specify = specify;
            this.Management_Plan = management_Plan;
        }

        public Topic(){
            this.Code = null;
            this.Type = null;
            this.Change_item = 0;
            this.Product_type = 0;
            this.Revision = 0;
            this.Department = null;
            this.Model = null;
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
            this.Risk = null;
            this.Specify = null;
            this.Management_Plan = null;
        }
    }
}