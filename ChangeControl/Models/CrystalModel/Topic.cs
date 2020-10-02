using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChangeControl.Helpers;

namespace ChangeControl.Models.CrystalModel
{
    public class Topic{
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
        public string SubStatus{ get; set; }
        public string App{ get; set; }
        public string Subject{ get; set; }
        public string Detail{ get; set; }
        public string Timing { get; set; }
        public string TimingDesc { get; set; }
        // public Related RelatedList{ get; set; }
        public string User{ get; set; }
        public string Date{ get; set; }
        public string ApprovedBy{ get; set; }
        public string ApprovedDate{ get; set; }
    }
}