using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class Department
    {
        public int ID { get; set; }
        public int GetID(){
            return this.ID;
        }
        public string Dept { get; set; }
        public string Name { get; set; }
        public string NameRaw { get; set; }
        public List<int> Value { get; set;}        
    }
}