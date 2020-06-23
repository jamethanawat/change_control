using ChangeControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;

namespace ChangeControl.Controllers
{
    public class FileService : Controller{
        [HttpPost]
        public ActionResult DownloadFile(string path_name, string file_name)
        {
            string filePath = path_name;
            string fullName = "D:/File/Topic/";
            byte[] fileBytes = GetFile(fullName + filePath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, file_name);
        }
         byte[] GetFile(string s)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(s);
            byte[] data = new byte[fs.Length];
            int br = fs.Read(data, 0, data.Length);
            if (br != fs.Length)
                throw new System.IO.IOException(s);
            return data;
        }

    }
}