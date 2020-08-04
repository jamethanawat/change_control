using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChangeControl.Helpers;

namespace ChangeControl.Models{
    public class TopicAlt{
        public long ID { get; set;}
        public string Code{ get; set;}
        public string Type{ get; set; }
        public string Change_item{ get; set; }
        public string Product_type{ get; set; }
        public int Revision{ get; set; }
        public string Department{ get; set; }
        public string Model{ get; set; }
        public string PartNo{ get; set; } 
        public string PartName{ get; set; } 
        public string ProcessName{ get; set; }
        public int Status{ get; set; }
        public string FullStatus{ get; set; }
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
        public string Date{ get; set; }
        public User Profile{ get; set;}
        public TopicAlt(string Code,string Type,string ChangeItem,string ProductType,int Revision ,string Model,string PartNo,string PartName,string ProcessName,int Status,string App,string Subject,string Detail,string Timing ,long Related,string UserInsert,string TimeInsert){
            this.Code = Code.ReplaceNullWithDash();
            this.Type = Type.ReplaceNullWithDash();
            this.Change_item = ChangeItem.ReplaceNullWithDash();
            this.Product_type = ProductType.ReplaceNullWithDash();
            this.Revision = Revision;
            this.Model = Model.ReplaceNullWithDash();
            this.PartNo = PartNo.ReplaceNullWithDash();
            this.PartName = PartName.ReplaceNullWithDash();
            this.ProcessName = ProcessName.ReplaceNullWithDash();
            this.Status = Status;
            this.App = App.ReplaceNullWithDash();
            this.Subject = Subject.ReplaceNullWithDash();
            this.Detail = Detail.ReplaceNullWithDash();
            this.Timing = Timing;
            this.Related = Related;
            this.User_insert = UserInsert;
            this.Time_insert = TimeInsert;
        }

        public TopicAlt(){
            this.Code = "ER-00000000";
            this.Type = "ERR0R";
            this.Change_item = "ERR0R";
            this.Product_type = "ERR0R";
            this.Revision = 0;
            this.Model = "ERR0R";
            this.PartNo = "ERR0R";
            this.PartName = "ERR0R";
            this.ProcessName = "ERR0R";
            this.Status = 0;
            // this.App = "ERR0R";
            // this.Subject = "ERR0R";
            // this.Detail = "ERR0R";
            // this.Timing = "ERR0R";
            this.Related = 0;
            this.User_insert = "IT Admin";
            this.Time_insert = DateTime.Now.ToString("d MMMM yyyy");
            this.ID = 9999;
            this.Profile = new User();
            this.Profile.FullName = "Pakawat Smutkun";
            this.Profile.Name = "Pakawat";
            this.Profile.SurName = "Smutkun";
        }
    }
}