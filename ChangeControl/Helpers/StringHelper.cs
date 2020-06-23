using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace ChangeControl.Controllers
{
    public class StringHelper : Controller{
        
        public static string UppercaseFirst(string s){
            if (string.IsNullOrEmpty(s)){
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}