using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models
{
    public class SearchAttribute
    {
        public string Type{ get; set; }
        public int Status{ get; set; }
        public string ProductType{ get; set; }
        public int Overstatus{ get; set; }
        public string Changeitem{ get; set; }
        public string ControlNo{ get; set; }
        public  string Model{ get; set; }
        public  string Detail{ get; set; }
        public string Chosechangeitem{ get; set; }
        public string Partno{ get; set; }
        public string Partname{ get; set; }
        public string Department{ get; set; }
        public string Processname{ get; set; }
        public string Production{ get; set; }
        public SearchAttribute(string Type,int Status,string ProductType,int Overstatus,string Changeitem,string ControlNo, string Model,string Chosechangeitem,string Partno,string Partname,string Department,string Processname ,string Production ,string Line){
            this.Type = Type;
            this.Status = Status;
            this.ProductType = ProductType;
            this.Overstatus = Overstatus;
            this.Changeitem = Changeitem;
            this.ControlNo = ControlNo;
            this.Model = Model;
            this.Chosechangeitem = Chosechangeitem;
            this.Partno = Partno;
            this.Partname = Partname;
            this.Department = Department;
            this.Processname = Processname;
            this.Production = Production;
        }
    }
}