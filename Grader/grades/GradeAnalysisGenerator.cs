using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Access;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using LibUtil;

namespace Grader.grades {
    public static class GradeAnalysisGenerator {
        public static void GenerateGradeAnalysis(Application accessApp) {
            try {
                Form f = accessApp.GetForm("ПоОценкам").Get();
                string subjectName = f.GetControl("SubjectSelect").Get().StringValue();
                string analysisType = f.GetControl("SelectAnalysisType").Get().StringValue();
                DataContext dc = accessApp.GetDataContext();
                IQueryable<Оценка> gradeQuery = Grades.GetGradeQuery(accessApp, dc);

                List<Подразделение> subunits;
                Func<Подразделение, List<int>> subunitGrades;
                Func<Подразделение, Option<double>> avgSubunitGrade;
                
                if (analysisType == "по курсантам на циклах") {
                    subunits = Querying.GetSubunitsByType(dc, "цикл");
                    subunitGrades = cycle => Grades.GetSubjectGrades(Grades.GetGradesOnCycle(dc, gradeQuery, cycle.Код), dc, subjectName);
                } else if (analysisType == "по батальонам/ротам") {
                    subunits = Querying.GetSubunitsByType(dc, "батальон");
                    subunitGrades = batallion => Grades.GetSubjectGrades(Grades.GetGradesForSubunit(dc, gradeQuery, batallion.Код), dc, subjectName);
                } else if (analysisType == "по батальонам/циклам") {
                    if (f.GetControl("SelectContract").Get().BooleanValue() != true && f.GetControl("SelectContractAndPermanent").Get().BooleanValue() != true) {
                        throw new Exception("expecting contract soldiers for this analysis type");
                    }
                    subunits = Querying.GetSubunitsByType(dc, "батальон");
                    subunitGrades = subunit => Grades.GetSubjectGrades(Grades.GetGradesForSubunit(dc, gradeQuery, subunit.Код), dc, subjectName);
                } else if (analysisType == "по циклам") {
                    subunits = Querying.GetSubunitsByType(dc, "цикл");
                    subunitGrades = cycle => Grades.GetSubjectGrades(Grades.GetGradesForSubunit(dc, gradeQuery, cycle.Код), dc, subjectName);
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
                            cycle.Командир(dc).Map(c => c.GetFullName()).GetOrElse("???"),
                            avgSubunitGrade(cycle).Get()
                            );
                    res = "\tЛучшие результаты " + resFunc(maxSubunit.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get()) + ".";
                } else if (analysisType == "по батальонам/ротам") {
                    if (f.GetControl("SelectCadets").Get().BooleanValue()) {
                        List<Подразделение> companies = Querying.GetSubunitsByType(dc, "рота");
                        Подразделение maxCompany = companies.MaxByOption(avgSubunitGrade).Get();
                        Подразделение minCompany = companies.MinByOption(avgSubunitGrade).Get();
                        Func<Подразделение, Подразделение, string> resFunc = (batallion, company) =>
                            String.Format("показали курсанты {0}, командир - {1} (средний балл - {2:F2}), {3}, командир - {4} (средний балл - {5:F2})",
                                batallion.ИмяРодительный,
                                batallion.Командир(dc).Map(b => b.GetFullName()).GetOrElse("???"),
                                avgSubunitGrade(batallion).Get(),
                                company.ИмяРодительный,
                                company.Командир(dc).Map(b => b.GetFullName()).GetOrElse("???"),
                                avgSubunitGrade(company).Get()
                                );
                        res = "\tЛучшие результаты " + resFunc(maxSubunit.Get(), maxCompany) + ".\n\tНиже результаты " + resFunc(minSubunit.Get(), minCompany) + ".";
                    } else {
                        Func<Подразделение, string> resFunc = subunit =>
                            String.Format("показал постоянный состав {0}, командир - {1} (средний балл - {2:F2})",
                                subunit.ИмяРодительный,
                                subunit.Командир(dc).Map(c => c.GetFullName()).GetOrElse("???"),
                                avgSubunitGrade(subunit).Get()
                                );
                        res = "\tЛучшие результаты " + resFunc(maxSubunit.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get()) + ".";
                    }
                } else if (analysisType == "по батальонам/циклам") {
                    List<Подразделение> cycles = Querying.GetSubunitsByType(dc, "цикл");
                    Option<Подразделение> maxCycle = cycles.MaxByOption(avgSubunitGrade);
                    Option<Подразделение> minCycle = cycles.MinByOption(avgSubunitGrade);
                    if (maxCycle.IsEmpty() || minCycle.IsEmpty()) {
                        System.Windows.Forms.MessageBox.Show("Нет оценок на циклах!");
                        return;
                    }
                    Func<Подразделение, Подразделение, string> resFunc = (batallion, cycle) =>
                        String.Format("показал постоянный состав {0}, командир - {1} (средний балл - {2:F2}), {3}, начальник цикла - {4} (средний балл - {5:F2})",
                            batallion.ИмяРодительный,
                            batallion.Командир(dc).Map(b => b.GetFullName()).GetOrElse("???"),
                            avgSubunitGrade(batallion).Get(),
                            cycle.ИмяРодительный,
                            cycle.Командир(dc).Map(b => b.GetFullName()).GetOrElse("???"),
                            avgSubunitGrade(cycle).Get());
                    res = "\tЛучшие результаты " + resFunc(maxSubunit.Get(), maxCycle.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get(), minCycle.Get()) + ".";
                } else if (analysisType == "по циклам") {
                    Func<Подразделение, string> resFunc = cycle =>
                        String.Format("показал постоянный состав {0}, начальник цикла - {1} (средний балл - {2:F2})",
                            cycle.ИмяРодительный,
                            cycle.Командир(dc).Map(c => c.GetFullName()).GetOrElse("???"),
                            avgSubunitGrade(cycle).Get()
                            );
                    res = "\tЛучшие результаты " + resFunc(maxSubunit.Get()) + ".\n\tНиже результаты " + resFunc(minSubunit.Get()) + ".";
                } else {
                    throw new Exception("Unknown analysis type: " + analysisType);
                }

                string fullRes = 
                    GradeCalcGroup.ОбщаяОценка(accessApp, dc, gradeQuery, subjectName)
                    .Map(summGrade =>
                        String.Format("\tОбщая оценка: «{0}», средний балл - {1:F2}.\n{2}", 
                        ReadableTextUtil.HumanReadableGradeLong(summGrade), 
                        Grades.GetSubjectGrades(gradeQuery, dc, subjectName).Mean(), 
                        res)
                    ).GetOrElse(String.Format("\tНет общей оценки.\n{0}", res));

                var doc = WordTemplates.CreateEmptyWordDoc();
                doc.Range().Select();
                doc.Application.Selection.TypeText(fullRes);
                doc.Saved = true;
                WordTemplates.ActivateWord(doc);
            } catch (Exception e) {
                Logger.Log(e.ToString());
                throw e;
            } finally {
                Logger.Log("Call to GradeAnalysis.GenerateAnalysis done");
            }
        }
    }
}
