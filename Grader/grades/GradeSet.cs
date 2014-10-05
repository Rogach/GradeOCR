using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibUtil;

namespace Grader.grades {
    public class GradeSet {
        public Военнослужащий soldier { get; set; }
        public Звание rank { get; set; }
        public Подразделение subunit { get; set; }
        public Dictionary<string, int> grades;
        public DateTime gradeDate;
        public GradeSet() { }
        public GradeSet(Военнослужащий soldier, Звание rank, Подразделение subunit, DateTime gradeDate) {
            this.soldier = soldier;
            this.rank = rank;
            this.subunit = subunit;
            this.gradeDate = gradeDate;
            this.grades = new Dictionary<string, int>();
        }
        public void AddGrade(string subj, int value) {
            grades.AddOrReplace(subj, value);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("GradeSet( ");
            foreach (var g in grades) {
                sb.Append(String.Format("{0} = {1} ", g.Key, g.Value));
            }
            sb.Append(")");
            return sb.ToString();
        }
    }

    public class ExtendedGradeSet {
        public Dictionary<int, Оценка> grades;
        
        public ExtendedGradeSet() { }

        public void AddGrade(Оценка grade) {
            grades.AddOrReplace(grade.КодПредмета, grade);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("GradeSet( ");
            foreach (var g in grades) {
                if (g.Value.ЭтоКомментарий) {
                    sb.Append(String.Format("{0} = {1} ", g.Key, g.Value.Текст));
                } else {
                    sb.Append(String.Format("{0} = {1} ", g.Key, g.Value.Значение));
                }
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}
