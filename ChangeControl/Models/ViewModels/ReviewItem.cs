using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class ReviewItem{
        public long ID { get; set; }
        public long Type { get; set; }
        public bool? Status{ get; set; }
        public string Description { get; set;}
        public string Name { get; set;}

    }
}