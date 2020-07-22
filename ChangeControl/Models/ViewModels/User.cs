using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StringHelper;
namespace ChangeControl.Models{
    public class User{
        public string FullName { get; set; }
        public string ShortFullName { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public User(string FullName, string Name, string SurName, string Email, string Department, string Position){
            this.FullName = FullName.UppercaseFirst();
            this.ShortFullName = $"{SurName.Substring(0,1).UppercaseFirst()}. {Name.UppercaseFirst()}";
            this.Name = Name.UppercaseFirst();
            this.SurName = SurName.UppercaseFirst();
            this.Email = Email;
            this.Department = Department;
            this.Position = Position;
        }
        public User(){
            this.FullName = "Pakawat Smutkun";
            this.Name = "Pakawat";
            this.SurName = "Smutkun";
            this.Email = "pakawat.smutkun@email.thns.co.th";
            this.Department = "IT";
            this.Position = "Staff";
        }
    }

}