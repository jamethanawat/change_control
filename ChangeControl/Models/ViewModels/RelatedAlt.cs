using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    
    public class RelatedAlt{
        public string Department { get; set; }
        public int Review { get; set; }
        public int Trial { get; set; }
        public int Confirm { get; set; }
        public int Response { get; set; }

        public bool RelatedAndNotResponsed(string dept){
            return this.Response == 0 && Department == dept;
        }
        public bool Related(string dept){
            return Department == dept;
        }

    }
}