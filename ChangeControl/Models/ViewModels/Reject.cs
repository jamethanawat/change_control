using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Reject{
        public string Topic { get; set;}
        public string Description { get; set;}
        public string User { get; set;}
        public string Department { get; set;}
        public string Date { get; set;}
        public User Profile { get; set;}
    }
}