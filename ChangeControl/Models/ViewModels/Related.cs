using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Related{
        public int IT { get; set; }
        public int MKT { get; set; }
        public int PC1 { get; set; }
        public int PC2 { get; set; }
        public int PT1 { get; set; }
        public int PT2 { get; set; }
        public int PT3A { get; set; }
        public int PT3M { get; set; }
        public int PT4 { get; set; }
        public int PT5 { get; set; }
        public int PT6 { get; set; }
        public int PT7 { get; set; }
        public int PE1 { get; set; }
        public int PE2 { get; set; }
        public int PE2_MT { get; set; }
        public int PE2_PCB { get; set; }
        public int PE2_SMT { get; set; }
        public int PE1_Process { get; set; }
        public int PE2_Process { get; set; }
        public int PCH1 { get; set; }
        public int PCH2 { get; set; }
        public int QC_IN1 { get; set; }
        public int QC_IN2 { get; set; }
        public int QC_IN3 { get; set; }
        public int QC_FINAL1 { get; set; }
        public int QC_FINAL2 { get; set; }
        public int QC_FINAL3 { get; set; }
        public int QC_NFM1 { get; set; }
        public int QC_NFM2 { get; set; }
        public int QC_NFM3 { get; set; }
        public int QC1 { get; set; }
        public int QC2 { get; set; }
        public int QC3 { get; set; }

        public Related(){
            IT = 0;
            MKT = 0;
            PC1 = 0;
            PC2 = 0;
            PT1 = 0;
            PT2 = 0;
            PT3A = 0;
            PT3M = 0;
            PT4 = 0;
            PT5 = 0;
            PT6 = 0;
            PT7 = 0;
            PE1 = 0;
            PE2 = 0;
            PE2_MT = 0;
            PE2_PCB = 0;
            PE2_SMT = 0;
            PE1_Process = 0;
            PE2_Process = 0;
            PCH1 = 0;
            PCH2 = 0;
            QC_IN1 = 0;
            QC_IN2 = 0;
            QC_IN3 = 0;
            QC_FINAL1 = 0;
            QC_FINAL2 = 0;
            QC_FINAL3 = 0;
            QC_NFM1 = 0;
            QC_NFM2 = 0;
            QC_NFM3 = 0;
            QC1 = 0;
            QC2 = 0;
            QC3 = 0;
        }
        public Related(string IT,string MKT,string PC1,string PC2,string PT1,string PT2,string PT3A,string PT3M,string PT4,string PT5,string PT6,string PT7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH1,string PCH2,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3){
            this.IT = (IT != null)? int.Parse(IT) : 0;
            this.MKT = (MKT != null)? int.Parse(MKT) : 0;
            this.PC1 = (PC1 != null)? int.Parse(PC1) : 0;
            this.PC2 = (PC2 != null)? int.Parse(PC2) : 0;
            this.PT1 = (PT1 != null)? int.Parse(PT1) : 0;
            this.PT2 = (PT2 != null)? int.Parse(PT2) : 0;
            this.PT3A = (PT3A != null)? int.Parse(PT3A) : 0;
            this.PT3M = (PT3M != null)? int.Parse(PT3M) : 0;
            this.PT4 = (PT4 != null)? int.Parse(PT4) : 0;
            this.PT5 = (PT5 != null)? int.Parse(PT5) : 0;
            this.PT6 = (PT6 != null)? int.Parse(PT6) : 0;
            this.PT7 = (PT7 != null)? int.Parse(PT7) : 0;
            this.PE1 = (PE1 != null)? int.Parse(PE1) : 0;
            this.PE2 = (PE2 != null)? int.Parse(PE2) : 0;
            this.PE2_MT = (PE2 != null)? int.Parse(PE2) : 0;
            this.PE2_PCB = (PE2_PCB != null)? int.Parse(PE2_PCB) : 0;
            this.PE2_SMT = (PE2_SMT != null)? int.Parse(PE2_SMT) : 0;
            this.PE1_Process = (PE1_Process != null)? int.Parse(PE1_Process) : 0;
            this.PE2_Process = (PE2_Process != null)? int.Parse(PE2_Process) : 0;
            this.PCH1 = (PCH1 != null)? int.Parse(PCH1) : 0;
            this.PCH2 = (PCH2 != null)? int.Parse(PCH2) : 0;
            this.QC_IN1 = (QC_IN1 != null)? int.Parse(QC_IN1) : 0;
            this.QC_IN2 = (QC_IN2 != null)? int.Parse(QC_IN2) : 0;
            this.QC_IN3 = (QC_IN3 != null)? int.Parse(QC_IN3) : 0;
            this.QC_FINAL1 = (QC_FINAL1 != null)? int.Parse(QC_FINAL1) : 0;
            this.QC_FINAL2 = (QC_FINAL2 != null)? int.Parse(QC_FINAL2) : 0;
            this.QC_FINAL3 = (QC_FINAL3 != null)? int.Parse(QC_FINAL3) : 0;
            this.QC_NFM1 = (QC_NFM1 != null)? int.Parse(QC_NFM1) : 0;
            this.QC_NFM2 = (QC_NFM2 != null)? int.Parse(QC_NFM2) : 0;
            this.QC_NFM3 = (QC_NFM3 != null)? int.Parse(QC_NFM3) : 0;
            this.QC1 = (QC1 != null)? int.Parse(QC1) : 0;
            this.QC2 = (QC2 != null)? int.Parse(QC2) : 0;
            this.QC3 = (QC3 != null)? int.Parse(QC3) : 0;
        }
    }
}