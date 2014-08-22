using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using LibUtil.templates;
using Grader.util;
using LibUtil.wrapper.excel;
using LibUtil;
using Grader.enums;
using ARCode;
using System.Drawing;
using System.IO;

namespace Grader.registers {
    public class RegisterSettings {
        public Подразделение subunit { get; set; }
        public string subunitName { get; set; }
        public DateTime registerDate { get; set; }
        public RegisterType registerType { get; set; }
        public bool onlyKMN { get; set; }
        public bool strikeKMN { get; set; }
        public List<Военнослужащий> soldiers { get; set; }
        public bool forOCR { get; set; }
        public string registerNamePrefix { get; set; }
        public string registerTags { get; set; }

        public bool isExam {
            get {
                return registerType == RegisterType.экзамен;
            }
        }
    }
}
