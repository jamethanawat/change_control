using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class User{
        public string FullName { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public User(string FullName, string Name, string SurName, string Email, string Department, string Position){
            this.FullName = FullName;
            this.Name = Name;
            this.SurName = SurName;
            this.Email = Email;
            this.Department = Department;
            this.Position = Position;
        }
    }
}