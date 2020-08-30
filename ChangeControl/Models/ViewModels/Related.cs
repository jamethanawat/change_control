using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChangeControl.Models{
    public class Related2{
        public int IT { get; set; }
        public int MKT { get; set; }
        public int PC1 { get; set; }
        public int PC2 { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3A { get; set; }
        public int P3M { get; set; }
        public int P4 { get; set; }
        public int P5 { get; set; }
        public int P6 { get; set; }
        public int P7 { get; set; }
        public int PE1 { get; set; }
        public int PE2 { get; set; }
        public int PE2_MT { get; set; }
        public int PE2_PCB { get; set; }
        public int PE2_SMT { get; set; }
        public int PE1_Process { get; set; }
        public int PE2_Process { get; set; }
        public int P5_ProcessDesign { get; set; }
        public int P6_ProcessDesign { get; set; }
        public int PCH { get; set; }
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

        // public Related(){
        //     IT = 0;
        //     MKT = 0;
        //     PC1 = 0;
        //     PC2 = 0;
        //     P1 = 0;
        //     P2 = 0;
        //     P3A = 0;
        //     P3M = 0;
        //     P4 = 0;
        //     P5 = 0;
        //     P6 = 0;
        //     P7 = 0;
        //     PE1 = 0;
        //     PE2 = 0;
        //     PE2_MT = 0;
        //     PE2_PCB = 0;
        //     PE2_SMT = 0;
        //     PE1_Process = 0;
        //     PE2_Process = 0;
        //     P5_ProcessDesign = 0;
        //     P6_ProcessDesign = 0;
        //     PCH = 0;
        //     QC_IN1 = 0;
        //     QC_IN2 = 0;
        //     QC_IN3 = 0;
        //     QC_FINAL1 = 0;
        //     QC_FINAL2 = 0;
        //     QC_FINAL3 = 0;
        //     QC_NFM1 = 0;
        //     QC_NFM2 = 0;
        //     QC_NFM3 = 0;
        //     QC1 = 0;
        //     QC2 = 0;
        //     QC3 = 0;
        // }
        // public Related(string IT,string MKT,string PC1,string PC2,string P1,string P2,string P3A,string P3M,string P4,string P5,string P6,string P7,string PE1,string PE2,string PE2_SMT,string PE2_PCB,string PE2_MT,string PE1_Process,string PE2_Process,string PCH,string QC_IN1,string QC_IN2,string QC_IN3,string QC_FINAL1,string QC_FINAL2,string QC_FINAL3,string QC_NFM1,string QC_NFM2,string QC_NFM3,string QC1,string QC2,string QC3, string P5_ProcessDesign, string P6_ProcessDesign){
        //     this.IT = (IT != null)? int.Parse(IT) : 0;
        //     this.MKT = (MKT != null)? int.Parse(MKT) : 0;
        //     this.PC1 = (PC1 != null)? int.Parse(PC1) : 0;
        //     this.PC2 = (PC2 != null)? int.Parse(PC2) : 0;
        //     this.P1 = (P1 != null)? int.Parse(P1) : 0;
        //     this.P2 = (P2 != null)? int.Parse(P2) : 0;
        //     this.P3A = (P3A != null)? int.Parse(P3A) : 0;
        //     this.P3M = (P3M != null)? int.Parse(P3M) : 0;
        //     this.P4 = (P4 != null)? int.Parse(P4) : 0;
        //     this.P5 = (P5 != null)? int.Parse(P5) : 0;
        //     this.P6 = (P6 != null)? int.Parse(P6) : 0;
        //     this.P7 = (P7 != null)? int.Parse(P7) : 0;
        //     this.PE1 = (PE1 != null)? int.Parse(PE1) : 0;
        //     this.PE2 = (PE2 != null)? int.Parse(PE2) : 0;
        //     this.PE2_MT = (PE2 != null)? int.Parse(PE2) : 0;
        //     this.PE2_PCB = (PE2_PCB != null)? int.Parse(PE2_PCB) : 0;
        //     this.PE2_SMT = (PE2_SMT != null)? int.Parse(PE2_SMT) : 0;
        //     this.PE1_Process = (PE1_Process != null)? int.Parse(PE1_Process) : 0;
        //     this.PE2_Process = (PE2_Process != null)? int.Parse(PE2_Process) : 0;
        //     this.P5_ProcessDesign = (P5_ProcessDesign != null)? int.Parse(P5_ProcessDesign) : 0;
        //     this.P6_ProcessDesign = (P6_ProcessDesign != null)? int.Parse(P6_ProcessDesign) : 0;
        //     this.PCH = (PCH != null)? int.Parse(PCH) : 0;
        //     this.QC_IN1 = (QC_IN1 != null)? int.Parse(QC_IN1) : 0;
        //     this.QC_IN2 = (QC_IN2 != null)? int.Parse(QC_IN2) : 0;
        //     this.QC_IN3 = (QC_IN3 != null)? int.Parse(QC_IN3) : 0;
        //     this.QC_FINAL1 = (QC_FINAL1 != null)? int.Parse(QC_FINAL1) : 0;
        //     this.QC_FINAL2 = (QC_FINAL2 != null)? int.Parse(QC_FINAL2) : 0;
        //     this.QC_FINAL3 = (QC_FINAL3 != null)? int.Parse(QC_FINAL3) : 0;
        //     this.QC_NFM1 = (QC_NFM1 != null)? int.Parse(QC_NFM1) : 0;
        //     this.QC_NFM2 = (QC_NFM2 != null)? int.Parse(QC_NFM2) : 0;
        //     this.QC_NFM3 = (QC_NFM3 != null)? int.Parse(QC_NFM3) : 0;
        //     this.QC1 = (QC1 != null)? int.Parse(QC1) : 0;
        //     this.QC2 = (QC2 != null)? int.Parse(QC2) : 0;
        //     this.QC3 = (QC3 != null)? int.Parse(QC3) : 0;
        // }
    }
}