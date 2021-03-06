using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models.CrystalModel
{
    public class Confirm{
        public long ID { get; set; }
        public string Topic { get; set; }
        public string Detail { get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public string ApprovedBy { get; set;}
        public string ApprovedDate { get; set;}
        public string Department { get; set; }
        public int Status { get; set; }
        public int Revision { get; set; }
    
    }
}