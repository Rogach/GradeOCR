using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using LibUtil;
using System.Windows.Forms;
using System.Drawing;

namespace Grader.grades {
    public static class GradeAnalysisGenerator {
        public static void GenerateGradeAnalysis(
                RichTextBox resultBox,
                Entities et, 
                IQueryable<Оценка> gradeQuery, 
                string subjectName, 
                string analysisType,
                bool selectCadets,
                bool selectPermanent,
                bool selectContract) {

            try {
                List<Подразделение> subunits;
                Func<Подразделение, List<int>> subunitGrades;
                Func<Подразделение, Option<double>> avgSubunitGrade;
                
                if (analysisType == "по курсантам на циклах") {
                    subunits = Querying.GetSubunitsByType(et, "цикл").ToList();
                    subunitGrades = cycle => Grades.GetSubjectGrades(Grades.GetGradesOnCycle(et, gradeQuery, cycle.Код), et, subjectName);
                } else if (analysisType == "по батальонам/ротам") {
                    subunits = Querying.GetSubunitsByType(et, "батальон").ToList();
                    subunitGrades = batallion => Grades.GetSubjectGrades(Grades.GetGradesForSubunit(et, gradeQuery, batallion.Код), et, subjectName);
                } else if (analysisType == "по батальонам/циклам") {
                    if (selectCadets) {
                        throw new Exception("expecting contract soldiers for this analysis type");
                    }
                    subunits = Querying.GetSubunitsByType(et, "батальон").ToList();
                    subunitGrades = subunit => Grades.GetSubjectGrades(Grades.GetGradesForSubunit(et, gradeQuery, subunit.Код), et, subjectName);
                } else if (analysisType == "по циклам") {
                    subunits = Querying.GetSubunitsByType(et, "цикл").ToList();
                    subunitGrades = cycle => Grades.GetSubjectGrades(Grades.GetGradesForSubunit(et, gradeQuery, cycle.Код), et, subjectName);
                } else {
                    throw new Exception("Unknown analysis type: " + analysisType);
                }
                subunitGrades = Util.Cached(subunitGrades);
                avgSubunitGrade = subunit => subunitGrades(subunit).MeanOption();
                avgSubunitGrade = Util.Cached(avgSubunitGrade);

                Option<Подразделение> maxSubunit = subunits.MaxByOption(avgSubunitGrade);
                Option<Подразделение> minSubunit = subunits.MinByOption(avgSubunitGrade);

                if (maxSubunit.IsEmpty() || minSubunit.IsEmpty()) {
                    System.Windows.Forms.MessageBox.Show("Нет оценок!");
                    return;
                }

                string res;
                if (analysisType == "по курсантам на циклах") {
                    Func<Подразделение, string> resFunc = cycle =>
                        String.Format("показали курсанты, обучающиеся на {0}, командир - {1} (средний балл - {2:F2})",
                            cycle.ИмяПредложный,
                            Querying.GetCommander(et, cycle.Код).Map(c => c.GetFullName(et)).GetOrElse("???"),
                            avgSubunitGrade(cycle).Get()
                            );
                    res = "\tЛучшие результаты " + resFunc(maxSubunit.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get()) + ".";
                } else if (analysisType == "по батальонам/ротам") {
                    if (selectCadets) {
                        List<Подразделение> companies = Querying.GetSubunitsByType(et, "рота").ToList();
                        Подразделение maxCompany = companies.MaxByOption(avgSubunitGrade).Get();
                        Подразделение minCompany = companies.MinByOption(avgSubunitGrade).Get();
                        Func<Подразделение, Подразделение, string> resFunc = (batallion, company) =>
                            String.Format("показали курсанты {0}, командир - {1} (средний балл - {2:F2}), {3}, командир - {4} (средний балл - {5:F2})",
                                batallion.ИмяРодительный,
                                Querying.GetCommander(et, batallion.Код).Map(b => b.GetFullName(et)).GetOrElse("???"),
                                avgSubunitGrade(batallion).Get(),
                                company.ИмяРодительный,
                                Querying.GetCommander(et, company.Код).Map(b => b.GetFullName(et)).GetOrElse("???"),
                                avgSubunitGrade(company).Get()
                                );
                        res = "\tЛучшие результаты " + resFunc(maxSubunit.Get(), maxCompany) + ".\n\tНиже результаты " + resFunc(minSubunit.Get(), minCompany) + ".";
                    } else {
                        Func<Подразделение, string> resFunc = subunit =>
                            String.Format("показал постоянный состав {0}, командир - {1} (средний балл - {2:F2})",
                                subunit.ИмяРодительный,
                                Querying.GetCommander(et, subunit.Код).Map(c => c.GetFullName(et)).GetOrElse("???"),
                                avgSubunitGrade(subunit).Get()
                                );
                        res = "\tЛучшие результаты " + resFunc(maxSubunit.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get()) + ".";
                    }
                } else if (analysisType == "по батальонам/циклам") {
                    List<Подразделение> cycles = Querying.GetSubunitsByType(et, "цикл").ToList();
                    Option<Подразделение> maxCycle = cycles.MaxByOption(avgSubunitGrade);
                    Option<Подразделение> minCycle = cycles.MinByOption(avgSubunitGrade);
                    if (maxCycle.IsEmpty() || minCycle.IsEmpty()) {
                        System.Windows.Forms.MessageBox.Show("Нет оценок на циклах!");
                        return;
                    }
                    Func<Подразделение, Подразделение, string> resFunc = (batallion, cycle) =>
                        String.Format("показал постоянный состав {0}, командир - {1} (средний балл - {2:F2}), {3}, начальник цикла - {4} (средний балл - {5:F2})",
                            batallion.ИмяРодительный,
                            Querying.GetCommander(et, batallion.Код).Map(b => b.GetFullName(et)).GetOrElse("???"),
                            avgSubunitGrade(batallion).Get(),
                            cycle.ИмяРодительный,
                            Querying.GetCommander(et, cycle.Код).Map(b => b.GetFullName(et)).GetOrElse("???"),
                            avgSubunitGrade(cycle).Get());
                    res = "\tЛучшие результаты " + resFunc(maxSubunit.Get(), maxCycle.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get(), minCycle.Get()) + ".";
                } else if (analysisType == "по циклам") {
                    Func<Подразделение, string> resFunc = cycle =>
                        String.Format("показал постоянный состав {0}, начальник цикла - {1} (средний балл - {2:F2})",
                            cycle.ИмяРодительный,
                            Querying.GetCommander(et, cycle.Код).Map(c => c.GetFullName(et)).GetOrElse("???"),
                            avgSubunitGrade(cycle).Get()
                            );
                    res = "\tЛучшие результаты " + resFunc(maxSubunit.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get()) + ".";
                } else {
                    throw new Exception("Unknown analysis type: " + analysisType);
                }

                resultBox.Clear();
                resultBox.Text +=
                    GradeCalcGroup.ОбщаяОценка(et, gradeQuery, subjectName, selectCadets)
                    .Map(summGrade =>
                        String.Format("\tОбщая оценка: «{0}», средний балл - {1:F2}.\n{2}",
                        ReadableTextUtil.HumanReadableGradeLong(summGrade),
                        Grades.GetSubjectGrades(gradeQuery, et, subjectName).Mean(),
                        res)
                    ).GetOrElse(String.Format("\tНет общей оценки.\n{0}", res));
                resultBox.SelectAll();
                resultBox.SelectionFont = new Font("Times New Roman", 14f, FontStyle.Regular);
                Clipboard.SetText(resultBox.Rtf, TextDataFormat.Rtf);
            } catch (Exception e) {
                Logger.Log(e.ToString());
                throw e;
            } finally {
                Logger.Log("Call to GradeAnalysis.GenerateAnalysis done");
            }
        }
    }
}
