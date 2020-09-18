using ChangeControl.Models;
using ChangeControl.Models.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChangeControl.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        private HomeModel M_Home;
        private ReportModel M_Report;
        public ReportController()
        {
            M_Home = new HomeModel();
            M_Report = new ReportModel();
            if (ViewBag.QCAudit == null) ViewBag.QCAudit = M_Home.GetQcAudit();
            if (ViewBag.PEAudit == null) ViewBag.PEAudit = M_Home.GetPEAudit();
        }
        public ActionResult Index(string id)
        {
            if ((string)(Session["User"]) == null || (string)(Session["Department"]) == null)
            {
                Session["RedirectID"] = id ?? null;
                Session["RedirectMode"] = "Report";
                return RedirectToAction("Index", "LogIn");
            }
            GenerateTopicList(Session["Department"].ToString(), Session["Position"].ToString());
            return View();
        }
        [HttpPost]
        public void GenerateTopicList(string Department, string Position)
        {
            var dept = Department ?? "Guest";
            var pos = Position ?? "Guest";
            bool isApprover = ViewBag.isApprover = (pos == "Approver") || (pos == "Admin") || (pos == "Special");
            var isPEProcess = ViewBag.isPEProcess = (ViewBag.PEAudit.Contains(dept));
            var isQC = ViewBag.isQC = (ViewBag.QCAudit.Contains(dept));
            var confirm_dept_list = M_Home.GetConfirmDeptList();


            List<TopicNoti> req_list = new List<TopicNoti>();
            List<TopicNoti> rv_list = new List<TopicNoti>();
            List<TopicNoti> tr_list = new List<TopicNoti>();
            List<TopicNoti> cf_list = new List<TopicNoti>();

            if (dept != null)
            {
                rv_list.AddRange(M_Home.GetReviewPendingByDepartment(dept));
                if (isApprover)
                {
                    req_list.AddRange(M_Home.GetRequestIssuedByDepartment(dept));
                    rv_list.AddRange(M_Home.GetReviewIssuedByDepartment(dept));
                }
                if (isPEProcess)
                {
                    req_list.AddRange(M_Home.GetRequestApprovedByDepartment(dept));
                }
                if(isQC){
                    rv_list.AddRange(M_Home.GetReviewApproved(dept));
                    tr_list.AddRange(M_Home.GetTrialApproved(dept));
                    cf_list.AddRange(M_Home.GetConfirmApproved(dept));
                }
                if (confirm_dept_list.Contains(dept))
                {
                    cf_list.AddRange(M_Home.GetConfirmPendingByDepartment(dept));
                    if (isApprover)
                    {
                        cf_list.AddRange(M_Home.GetConfirmIssuedByDepartment(dept));
                    }
                }
                if (M_Home.CheckTrialableByDepartment(dept))
                {
                    tr_list.AddRange(M_Home.GetTrialPendingByDepartment(dept));
                    if (isApprover)
                    {
                        tr_list.AddRange(M_Home.GetTrialIssuedByDepartment(dept));
                    }
                }
                ViewData["TopicRequestList"] = req_list;
                ViewData["TopicReviewList"] = rv_list;
                ViewData["TopicTrialList"] = tr_list;
                ViewData["TopicList"] = cf_list;
            }
        }

      
        public void GenerateReport(string StartDate, string EndDate)
        {
 
            List<ReportExcel> result = new List<ReportExcel>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            result= M_Report.GetReport(StartDate,EndDate);
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Issued "+ StartDate + " To "+ EndDate + "");
            Sheet.Cells["A1:U1"].Style.Font.Bold = true;
            Sheet.Cells["A1"].Value = "CCSNO";
            Sheet.Cells["B1"].Value = "ChangeItem";
            Sheet.Cells["C1"].Value = "ProductType";
            Sheet.Cells["D1"].Value = "REV";
            Sheet.Cells["E1"].Value = "DateRequest";
            Sheet.Cells["F1"].Value = "RequestBy";
            Sheet.Cells["G1"].Value = "ApprovedDateRequest";
            Sheet.Cells["H1"].Value = "Department";
            Sheet.Cells["I1"].Value = "Status";
            Sheet.Cells["J1"].Value = "ReviewFinishWithin";
            Sheet.Cells["K1"].Value = "TrialConfirmFinishWithin";
            Sheet.Cells["L1"].Value = "InitialProductionFinishWithin";
            Sheet.Cells["M1"].Value = "REV.Review";
            Sheet.Cells["N1"].Value = "IssueDateReview";
            Sheet.Cells["O1"].Value = "ApprovedDateReview";
            Sheet.Cells["P1"].Value = "REV.TrialConfirm";
            Sheet.Cells["Q1"].Value = "IssueDateTrialConfirm";
            Sheet.Cells["R1"].Value = "ApprovedDateTrialConfirm";
            Sheet.Cells["S1"].Value = "REV.InitialProduction";
            Sheet.Cells["T1"].Value = "IssueDateInitialProduction";
            Sheet.Cells["U1"].Value = "ApprovedDateInitialProduction";
           
            int row = 2;
            string nameshow;
            foreach (var item in result)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = item.TNSNO ?? "-";
                Sheet.Cells[string.Format("B{0}", row)].Value = item.ChangeItem ?? "-";
                Sheet.Cells[string.Format("C{0}", row)].Value = item.ProductType ?? "-";
                Sheet.Cells[string.Format("D{0}", row)].Value = item.Rev ?? "-";
                Sheet.Cells[string.Format("E{0}", row)].Value = item.DateRequest ?? "-";
          
                try
                {
                    var fullname = item.RequestBy.Split('@');
                    var tmpname = fullname[0].Split('.');
                     nameshow = (tmpname[1].ToString()).Substring(0, 1) + "." + tmpname[0].ToString();
                }
                catch (Exception err)
                {
                    nameshow = "";
                }
          
                Sheet.Cells[string.Format("F{0}", row)].Value = nameshow;
                Sheet.Cells[string.Format("G{0}", row)].Value = item.ApprovedDateRequest ?? "-";
                Sheet.Cells[string.Format("H{0}", row)].Value = item.Department ?? "-";
                Sheet.Cells[string.Format("I{0}", row)].Value = item.Status ?? "-";
                Sheet.Cells[string.Format("J{0}", row)].Value = item.ReviewFinishWithin ?? "-";
                Sheet.Cells[string.Format("K{0}", row)].Value = item.TrialConfirmFinishWithin ?? "-";
                Sheet.Cells[string.Format("L{0}", row)].Value = item.InitialProductionFinishWithin ?? "-";
                Sheet.Cells[string.Format("M{0}", row)].Value = item.RevReview ?? "-";
                Sheet.Cells[string.Format("N{0}", row)].Value = item.IssueDateReview ?? "-";
                Sheet.Cells[string.Format("O{0}", row)].Value = item.ApprovedDateReview ?? "-";
                Sheet.Cells[string.Format("P{0}", row)].Value = item.RevTrialConfirm ?? "-";
                Sheet.Cells[string.Format("Q{0}", row)].Value = item.IssueDateTrialConfirm ?? "-";
                Sheet.Cells[string.Format("R{0}", row)].Value = item.ApprovedDateTrialConfirm ?? "-";
                Sheet.Cells[string.Format("S{0}", row)].Value = item.RevInitialProduction ?? "-";
                Sheet.Cells[string.Format("T{0}", row)].Value = item.IssueDateInitialProduction ?? "-";
                Sheet.Cells[string.Format("U{0}", row)].Value = item.ApprovedDateInitialProduction ?? "-";  
                row++;
            }


            Sheet.Cells["A:AZ"].AutoFitColumns();
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=SummaryReport.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();
        }
        public ActionResult CrystalReportReport()
        {

            List<ReportExcel> result = new List<ReportExcel>();
     
            result = M_Report.GetReport("20200901", "20200902");
            List<test> result2 = new List<test>();

            result2 = M_Report.test();

            ReportDocument rd = new ReportDocument();
            rd.Load(Server.MapPath("~/CrystalReport/CCS_V_2.rpt"));
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();



            IEnumerable<ReportExcel> customers = result;
            IEnumerable<test> products = result2;
            //List<object> everything = new List<object>();
            //everything.Add(customers);
            //everything.Add(products);
            ArrayList Mainlst = new ArrayList();
            Mainlst.Add(customers);
            Mainlst.Add(products);
            rd.Database.Tables[0].SetDataSource(customers);
            rd.Database.Tables[1].SetDataSource(products);



            //rd.SetDataSource(list);
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            return new FileStreamResult(stream, "application/pdf");
           // return File(stream, "application/pdf", "CustomerList.pdf");
        
        }

    }
}