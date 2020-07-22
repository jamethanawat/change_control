using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace StringHelper
{
    public static class StringHelper{
        
        public static string UppercaseFirst(this string s){
            if (string.IsNullOrEmpty(s)){
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static string AsNullIfEmpty(this string items) {
            if (String.IsNullOrEmpty(items)) {
            return null;
            }
            return items;
        }

        public static string AsNullIfWhiteSpace(this string items) {
            if (String.IsNullOrWhiteSpace(items)) {
            return null;
            }
            return items;
        }
                
        public static IEnumerable<T> AsNullIfEmpty<T>(this IEnumerable<T> items) {
            if (items == null || !items.Any()) {
            return null;
            }
            return items;
        }

        public static string ReplaceNullWithDash(this string items) {
            return items.Trim().AsNullIfWhiteSpace() ?? "-";
        }

        public static string test(this string items) {
            return items;
        }

        public static string StripTagsRegex(this string source){
            source = Regex.Replace(source, "<.*?>", string.Empty);
            source = Regex.Replace(source, "&nbsp; ", " ");
            return source;
        }

        public static string ReplaceSingleQuote(this string source){
            if(source != null){
                source = Regex.Replace(source, "'", "\"");
                return source;
            }else{
                return null;
            }
        }


        
    }
}