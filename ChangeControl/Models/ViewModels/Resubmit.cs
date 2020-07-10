using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Resubmit{
        public long ID { get; set; }
        public string Topic { get; set; }
        public string Description{ get; set; }
        public string DueDate { get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public long Related { get; set; }
        public List<RelatedAlt> RelatedList { get; set; }
        public List<Response> Responses { get; set; }
        public User Profile {get; set;} 

    }
}