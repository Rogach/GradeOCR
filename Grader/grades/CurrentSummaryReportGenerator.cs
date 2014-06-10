﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using Grader.util;
using LibUtil.templates;
using AccessApplication = Microsoft.Office.Interop.Access.Application;
using LibUtil;

namespace Grader.grades {
    public class CurrentSummaryReportGenerator {
        public static void GenerateCurrentSummaryReport(AccessApplication accessApp) {
            DataContext dc = accessApp.GetDataContext();

            IQueryable<Оценка> gradeQuery = Grades.GetGradeQuery(accessApp, dc);

            var perPlatoon = gradeQuery.ToListTimed().GroupBy(g => g.КодПодразделения);

            var doc = WordTemplates.CreateEmptyWordDoc();
            var sel = doc.Application.Selection;
            sel.TypeText("Справка хода ВЭ в 90 МРУЦ\n\n");
            sel.TypeText("По состоянию на " + DateTime.Now.ToString("HH:mm dd.MM.yyyy года") + "\n\n");
            //sel.TypeText(String.Format("Сдали ВЭ {0} учебных взводов\n\n", perPlatoon.Count()));
            sel.TypeText(String.Format("Обработаны результаты по {0} взводам, сдававших ВЭ.\n\n", perPlatoon.Count()));
            
            List<Предмет> subjects = (from subj in dc.GetTable<Предмет>() select subj).ToListTimed();
            foreach (var subj in subjects) {
                List<int> grades = gradeQuery.Where(g => g.КодПредмета == subj.Код).Select(g => g.Значение).ToListTimed();
                Func<int, int> countGrades = x => grades.Where(g => g == x).Count();
                Func<int, double> percentGrades = x => (double) countGrades(x) / grades.Count * 100;
                if (grades.Count > 0) {
                    sel.TypeText(String.Format(
                            "Средний балл по {0} - {1}, из них на:\n" +
                            "Отлично - {2} ({3:F1}%);\n" +
                            "Хорошо - {4} ({5:F1}%);\n" +
                            "Удовлетворительно - {6} ({7:F1}%);\n" +
                            "Неудовлетворительно - {8} ({9:F1}%).\n\n",
                            subj.Название, String.Format("{0:F2}", CollectionUtil.Mean(grades)).Replace(",","."),
                            countGrades(5), percentGrades(5),
                            countGrades(4), percentGrades(4),
                            countGrades(3), percentGrades(3),
                            countGrades(2), percentGrades(2)
                    ));
                }
            }

            doc.Saved = true;
            WordTemplates.ActivateWord(doc);
        }
    }
}
