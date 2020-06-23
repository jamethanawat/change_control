using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Response{
        public int ID { get; set; }
        public string Description{ get; set; }
        public string File { get; set; }
        public string Department{ get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public List<FileItem> FileList { get; set; }
    }
}