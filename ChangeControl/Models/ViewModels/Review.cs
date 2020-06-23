using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Review{
        public int ID_Review { get; set; }
        public int Topic { get; set; }
        public string File{ get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public string Department { get; set; }
        public int Status { get; set; }
        public List<ReviewItem> Item { get; set; }
        public List<FileItem> FileList { get; set; }
        public User Profile {get; set;} 
    }
}