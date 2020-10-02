using ChangeControl.Models;
using M_CS = ChangeControl.Models.CrystalModel;
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
using ChangeControl.Helpers;

namespace ChangeControl.Controllers
{
    public class ReportController : ChangeControlController
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
        public ActionResult CrystalReportReport(string id){

            List<M_CS.Topic> q_Topic = M_Report.GetTopicByCode(id);
            List<M_CS.Review> q_Review = M_Report.GetReviewByTopicCode(id);
            List<M_CS.ReviewItem> q_ReviewItem = M_Report.GetReviewItemByTopicCode(id);
            List<M_CS.Trial> q_Trial = M_Report.GetTrialByTopicCode(id);
            List<M_CS.Confirm> q_Confirm = M_Report.GetConfirmByTopicCode(id);
            string related_str = M_Report.GetRelatedByTopicCode(id);

            q_Topic.ForEach(e => e.Timing = e.Timing.StringToDateTime());
            ReportDocument rd = new ReportDocument();
            rd.Load(Server.MapPath("~/CrystalReport/CCS_v_2.rpt"));
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            IEnumerable<M_CS.Topic> Topic = q_Topic;
            IEnumerable<M_CS.Review> Review = q_Review;
            IEnumerable<M_CS.ReviewItem> ReviewItem = q_ReviewItem;
            IEnumerable<M_CS.Trial> Trial = q_Trial;
            IEnumerable<M_CS.Confirm> Confirm = q_Confirm;

            ArrayList Mainlst = new ArrayList();
            Mainlst.Add(Topic);
            Mainlst.Add(Review);
            Mainlst.Add(ReviewItem);
            Mainlst.Add(Trial);
            Mainlst.Add(Confirm);
            
            rd.Database.Tables[0].SetDataSource(Topic);
            rd.Database.Tables[1].SetDataSource(Review);
            rd.Database.Tables[2].SetDataSource(ReviewItem);
            rd.Database.Tables[3].SetDataSource(Trial);
            rd.Database.Tables[4].SetDataSource(Confirm);

            rd.SetParameterValue("PrintDate", DateTime.Now.ToString("d MMM yy"));
            rd.SetParameterValue("RelatedStr", related_str);
            
            rd.SetParameterValue("CountReview", q_Review.Count());
            rd.SetParameterValue("CountTrial", q_Trial.Count());
            rd.SetParameterValue("CountConfirm", q_Confirm.Count());

            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            rd.Close();
            rd.Dispose();
            return new FileStreamResult(stream, "application/pdf");
        }

    }
}