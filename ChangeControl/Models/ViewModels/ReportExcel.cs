using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models.ViewModels
{
    public class ReportExcel
    {
        public string TNSNO { get; set; }
        public string ChangeItem { get; set; }
        public string ProductType { get; set; }
        public string Rev { get; set; }
        public string DateRequest { get; set; }
        public string RequestBy { get; set; }
        public string RequestDept { get; set; }
        public string ApprovedDateRequest { get; set; }
        public string ChangeDate { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public string ReviewFinishWithin { get; set; }
        public string TrialConfirmFinishWithin { get; set; }
        public string InitialProductionFinishWithin { get; set; }

        public string RevReview { get; set; }
        public string IssueDateReview { get; set; }
        public string ApprovedDateReview { get; set; }

        public string RevTrialConfirm { get; set; }
        public string IssueDateTrialConfirm { get; set; }
        public string ApprovedDateTrialConfirm { get; set; }

        public string RevInitialProduction { get; set; }
        public string IssueDateInitialProduction { get; set; }
        public string ApprovedDateInitialProduction { get; set; }

    }
}