using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
namespace ChangeControl.ProfileHelper{
    public static class ProfileHelper{
        
        public static string ToFullName(this string user){
            myAD.ADInfo conAD = new myAD.ADInfo();
            return conAD.ChkFullName(user);
        }

        // public static string ToShortFullName(this string user){
        //     myAD.ADInfo conAD = new myAD.ADInfo();
        //     return conAD.ChkFullName(user);
        // }
        
    }
}