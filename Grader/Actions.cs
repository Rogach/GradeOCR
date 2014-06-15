using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using Grader.grades;
using Grader.util;
using Grader.registers;
using LibUtil.wrapper;
using LibUtil;

namespace Grader {
    public class Actions {
        public Actions() { }
        
        public static void doCall(Action act) {
            Logger.AddLogFile("C:/Grader.log");
            var st = new System.Diagnostics.StackTrace();
            string methodName = st.GetFrame(1).GetMethod().Name;
            QueryTiming.ResetCounter();
            OfficeTiming.ResetCounter();
            Cache.DropCache();
            DateTime stt = DateTime.Now;
            try {
                Logger.Log(">>> Actions.{0}", methodName);
                act.Invoke();
            } catch (Exception e) {
                Logger.Log(e.ToString());
                System.Windows.Forms.MessageBox.Show(e.Message);
            } finally {
                DateTime end = DateTime.Now;
                double totalElapsed = (end - stt).TotalMilliseconds;
                double queryTime = QueryTiming.TotalQueryTime;
                double officeTime = OfficeTiming.TotalOfficeTime;
                double otherTime = totalElapsed - queryTime - officeTime;
                Logger.Log("<<< Actions.{0}", methodName);
                Logger.Log("  total : {0:F0} ms", totalElapsed);
                Logger.Log("  query : {0:F0} ms ({1:F0}%), query count = {2}, avg {3:F2} ms / query", 
                    queryTime, queryTime / totalElapsed * 100, QueryTiming.TotalQueryCount, QueryTiming.TotalQueryTime / QueryTiming.TotalQueryCount);
                Logger.Log("  office: {0:F0} ms ({1:F0}%)", officeTime, officeTime / totalElapsed * 100);
                Logger.Log("  other : {0:F0} ms ({1:F0}%)", otherTime, otherTime / totalElapsed * 100);
            }
        }

        public void GenerateGradeSummary(AccessApplication accessApp) {
            doCall(() => {
                GradeSummaryGenerator.GenerateSummary(accessApp);
            });
        }

        public void GenerateTableWithResultsBySubunitType(AccessApplication accessApp, string subunitType, bool byVus) {
            doCall(() => {
                AverageGradeTableGenerator.GenerateTableWithResultsBySubunitType(accessApp, subunitType, byVus);
            });
        }

        public void GenerateGradeAnalysis(AccessApplication accessApp) {
            doCall(() => {
                GradeAnalysisGenerator.GenerateGradeAnalysis(accessApp);
            });
        }

        public void GeneratePerDayGradeSummary(AccessApplication accessApp) {
            doCall(() => {
                PerDaySummaryGenerator.GenerateSummaryPerDay(accessApp);
            });
        }

        public void ExportToOldGrader(AccessApplication accessApp) {
            doCall(() => {
                OldGraderExporter.ExportToOldGrader(accessApp);
            });
        }

        public void ExportGradeList(AccessApplication accessApp) {
            doCall(() => {
                GradeListExporter.ContractGradeListExport(accessApp);
            });
        }

        public void GenerateAverageGradeSummary(AccessApplication accessApp) {
            doCall(() => {
                AverageGradeSummaryGenerator.GenerateAverageGradeSummary(accessApp);
            });
        }

        public void GenerateClassAct(AccessApplication accessApp) {
            doCall(() => {
                ClassActGenerator.GenerateClassAct(accessApp);
            });
        }

        public void GenerateAllClassActs(AccessApplication accessApp) {
            doCall(() => {
                ClassActGenerator.GenerateAllClassActs(accessApp);
            });
        }

        public void GenerateClassList(AccessApplication accessApp) {
            doCall(() => {
                ClassListGenerator.GenerateClassList(accessApp);
            });
        }

        public void GenerateCurrentSummaryReport(AccessApplication accessApp) {
            doCall(() => {
                CurrentSummaryReportGenerator.GenerateCurrentSummaryReport(accessApp);
            });
        }
    }
}
