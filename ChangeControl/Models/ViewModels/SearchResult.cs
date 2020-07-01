using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class SearchResult
    {
        public long ID { get; set; }
        public string ID_Topic { get; set; }
        public string Topic_type { get; set; }
        public int Change_item { get; set; }
        public int Product_type { get; set; }
        public int Revision { get; set; }
        public string Model { get; set; }
        public string PartNo { get; set; }
        public string PartName { get; set; }
        public int Status { get; set; }
        public string User_insert { get; set; }
        public string Time_insert { get; set; }

    }
}