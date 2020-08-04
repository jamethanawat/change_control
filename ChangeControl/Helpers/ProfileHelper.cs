using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
namespace ChangeControl.Helpers{
    public static class ProfileHelper{
        
        public static string ToFullName(this string user){
            myAD.ADInfo conAD = new myAD.ADInfo();
            return conAD.ChkFullName(user);
        }

        public static string ToShortFullName(this string user){
            myAD.ADInfo conAD = new myAD.ADInfo();
            var name = conAD.ChkName(user);
            var sur_name = conAD.ChkSurName(user);
            return $"{name.Substring(0,1)}. {sur_name.UppercaseFirst()}";
        }

        // public static string ToShortFullName(this string user){
        //     myAD.ADInfo conAD = new myAD.ADInfo();
        //     return conAD.ChkFullName(user);
        // }
        
    }
}