using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ChangeControl.Helpers;
namespace ChangeControl.Models{
    public class User{
        public string FullName { get; set; }
        public string ShortFullName { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public User(string FullName, string Name, string SurName, string Department, string Position){
            this.FullName = FullName.UppercaseFirst();
            this.ShortFullName = $"{SurName.Substring(0,1).UppercaseFirst()}. {Name.UppercaseFirst()}";
            this.Name = Name.UppercaseFirst();
            this.SurName = SurName.UppercaseFirst();
            this.Department = Department;
            this.Position = Position;
        }
        public User(){
            this.FullName = "Thanawat thanamahamongkol";
            this.Name = "Thanawat";
            this.SurName = "thanamahamongkol";
            this.Position = "Issue";
        }
    }

}