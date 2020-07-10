using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace DateHelper
{
    public static class DateHelper{
        public static string StringToDateTime(this string date_time){
            if(date_time == null){
                return DateTime.Now.ToString("d MMMM yyyy");
            }else{
                var Year = date_time.Substring(0,4);
                var Month = date_time.Substring(4,2);
                var Day = date_time.Substring(6,2);
                var Hour = date_time.Substring(8,2);
                var Minute = date_time.Substring(10,2);
                var Second = date_time.Substring(12,2);
                var result = DateTime.ParseExact($"{Year}-{Month}-{Day} {Hour}:{Minute}:{Second}", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                return result.ToString("d MMMM yyyy");
            }
        }
        
        public static string StringToDateTime2(this string date_time){
            if(date_time == null){
                return DateTime.Now.ToString("d MMMM yyyy");
            }else{
                var Day = date_time.Substring(0,2);
                var Month = date_time.Substring(3,2);
                var Year = date_time.Substring(6,4);
                var Hour = date_time.Substring(8,2);
                var Minute = "00";
                var Second = "00";
                var result = DateTime.ParseExact($"{Year}-{Month}-{Day} {Hour}:{Minute}:{Second}", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                return result.ToString("d MMMM yyyy");
            }
        }

        public static string StringToDateTime3(this string date_time){
            if(date_time == null){
                return DateTime.Now.ToString("d MMMM yyyy");
            }else{
                var Year = date_time.Substring(6,4);
                var Month = date_time.Substring(3,2);
                var Day = date_time.Substring(0,2);
                var result = DateTime.ParseExact($"{Year}-{Month}-{Day} 00:00:00", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                return result.ToString("d MMMM yyyy");
            }
        }
        
    }
}