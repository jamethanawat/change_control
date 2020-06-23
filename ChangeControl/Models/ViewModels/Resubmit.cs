using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Resubmit{
        public int ID { get; set; }
        public string Description{ get; set; }
        public string DueDate { get; set; }
        public string Date { get; set; }
        public int Related { get; set; }
        public List<RelatedAlt> RelatedList { get; set; }
        public int Topic { get; set; }
        public string User { get; set; }
        public List<Response> Responses { get; set; }
    }
}