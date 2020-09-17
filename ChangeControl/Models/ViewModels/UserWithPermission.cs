using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class UserWithPermission{
        public string User { get; set; } 
        public string Name { get; set; } 
        public string Email { get; set; } 
        public string Dept { get; set; } 
        public string Position { get; set; } 
        public int? Active { get; set; } 
        public int? Subscribe { get; set; } 
        public List<UserWithPermission> children { get; set; } 
    }
}