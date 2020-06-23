using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace ChangeControl.Controllers
{
    public class DateHelper : Controller{
        
        public static DateTime StringToDateTime(string date_time){
            var Year = date_time.Substring(0,4);
            var Month = date_time.Substring(4,2);
            var Day = date_time.Substring(6,2);
            var Hour = date_time.Substring(8,2);
            var Minute = date_time.Substring(10,2);
            var Second = date_time.Substring(12,2);
            var result = DateTime.ParseExact($"{Year}-{Month}-{Day} {Hour}:{Minute}:{Second}", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            return result;
        }
        
        public static DateTime StringToDateTime2(string date_time){
            var Day = date_time.Substring(0,2);
            var Month = date_time.Substring(3,2);
            var Year = date_time.Substring(6,4);
            var Hour = date_time.Substring(8,2);
            var Minute = "00";
            var Second = "00";
            var result = DateTime.ParseExact($"{Year}-{Month}-{Day} {Hour}:{Minute}:{Second}", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            return result;
        }
    }
}