using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class FileItem{
        public int ID { get; set; }
        public string ID_File { get; set; }
        public string Name { get; set; }
        public string Name_Format{ get; set; }
        public string Description { get; set; }
        public int? Size { get; set;}
        public string Time_Insert { get; set; }
        public string User_Insert { get; set; }
    }
}