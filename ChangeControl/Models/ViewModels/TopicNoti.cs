using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class TopicNoti{
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
        public string Subject{ get; set; }
        public string Detail{ get; set; }
        public string Timing{ get; set; }
        public string User_insert{ get; set; }
        public string Time_insert{ get; set; }
        public string Date{ get; set; }
        public string SubStatus{ get; set; }

    }
}