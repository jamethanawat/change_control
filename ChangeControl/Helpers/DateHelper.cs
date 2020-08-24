using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace ChangeControl.Helpers
{
    public static class DateHelper{
        public static string StringToDateTime(this string date_time){
            try{
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
            }catch(Exception err){
                return DateTime.Now.ToString("d MMMM yyyy");
            }
        }

        public static string StringToDateTimeShort(this string date_time){
            try{
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
                    return result.ToString("d MMM yyyy");
                }
            }catch(Exception err){
                return DateTime.Now.ToString("d MMM yyyy");
            }
        }
        
        public static string StringToDateTime2(this string date_time){
            try{
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
            }catch(Exception err){
                return DateTime.Now.ToString("d MMMM yyyy");
            }
        }
        
        public static string StringToDateTimeShort2(this string date_time){
            try{
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
                    return result.ToString("d MMM yyyy");
                }
            }catch(Exception err){
                return DateTime.Now.ToString("d MMM yyyy");
            }
        }

        public static string StringToDateTime3(this string date_time){ //dd-MM-yyyy
            try{
                if(date_time == null){
                    return DateTime.Now.ToString("d MMMM yyyy");
                }else{
                    var Day = date_time.Substring(0,2);
                    var Month = date_time.Substring(3,2);
                    var Year = date_time.Substring(6,4);
                    var result = DateTime.ParseExact($"{Year}-{Month}-{Day} 00:00:00", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    return result.ToString("d MMMM yyyy");
                }
            }catch(Exception err){
                return DateTime.Now.ToString("d MMMM yyyy");
            }
        }

        public static string StringToDigitDate(this string date_time){ //yyyy-MM-yyyy
            try{
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
                    return result.ToString("dd/MM/yy");
                }
            }catch(Exception err){
                return DateTime.Now.ToString("dd/MM/yy");
            }
        }

        public static string StringToDigitDate3(this string date_time){ //dd-MM-yyyy to dd/MM/yy
            try{
                if(date_time == null){
                    return DateTime.Now.ToString("dd/MM/yy");
                }else{
                    var Day = date_time.Substring(0,2);
                    var Month = date_time.Substring(3,2);
                    var Year = date_time.Substring(6,4);
                    var result = DateTime.ParseExact($"{Year}-{Month}-{Day} 00:00:00", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    return result.ToString("dd/MM/yy");
                }
            }catch(Exception err){
                return DateTime.Now.ToString("dd/MM/yy");
            }
        }

        public static string StringToDateTimeAddDay(this string date_time, int day){ 
            try{
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
                    return result.AddDays(day).ToString("d MMMM yyyy");
                }
            }catch(Exception err){
                return DateTime.Now.ToString("d MMMM yyyy");
            }
        }

        public static string DueDateOn(this DateTime date, int day){
            try{
                return date.AddDays(day).ToString("d MMMM yyyy");
            }catch(Exception err){
                return DateTime.Now.ToString("d MMMM yyyy");
            }
        }
        
    }
}