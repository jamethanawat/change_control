using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Topic_Approve{
        public long ID { get; set; }
        public long Topic { get; set; }
        public string RequestBy { get; set; }
        public string RequestDate { get; set; }
        public string ReviewBy { get; set; }
        public string ReviewDate { get; set; }
        public string TrialBy { get; set; }
        public string TrialDate { get; set; }
        public string CloseBy { get; set; }
        public string CloseDate { get; set; }

    }
}