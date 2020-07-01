using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Response{
        public long ID { get; set; }
        public long Resubmit { get; set; }
        public string Description{ get; set; }
        public long File { get; set; }
        public string Department{ get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public List<FileItem> FileList { get; set; }
    }
}