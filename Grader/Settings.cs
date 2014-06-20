using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using LibUtil;
using System.Windows.Forms;

namespace Grader {
    public class Settings {
        public string dbLocation { get; set; }
        public string gradeViewTags { get; set; }

        public void Save() {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlElement settings = doc.CreateElement("settings");
            doc.AppendChild(settings);

            XmlElement dbLocation_setting = doc.CreateElement("dbLocation");
            dbLocation_setting.AppendChild(doc.CreateTextNode(dbLocation));
            settings.AppendChild(dbLocation_setting);

            XmlElement gradeViewTags_setting = doc.CreateElement("gradeViewTags");
            gradeViewTags_setting.AppendChild(doc.CreateTextNode(gradeViewTags));
            settings.AppendChild(gradeViewTags_setting);

            XmlWriter xw = XmlWriter.Create(GetSettingsLocation());
            doc.WriteTo(xw);
            xw.Flush();
        }

        public static Option<Settings> Load() {
            Settings settings = new Settings();
            if (File.Exists(GetSettingsLocation())) {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetSettingsLocation());

                XmlNodeList dbLocation_setting = doc.SelectNodes("/settings/dbLocation");
                if (dbLocation_setting.Count > 0) {
                    settings.dbLocation = dbLocation_setting[0].InnerText;
                } else {
                    Option<string> userDbLocation = AskForDbLocation();
                    if (userDbLocation.NonEmpty()) {
                        settings.dbLocation = userDbLocation.Get();
                    } else {
                        return new None<Settings>();
                    }
                }

                XmlNodeList gradeViewTags_setting = doc.SelectNodes("/settings/gradeViewTags");
                if (gradeViewTags_setting.Count > 0) {
                    settings.gradeViewTags = gradeViewTags_setting[0].InnerText;
                } else {
                    settings.gradeViewTags = "";
                }
            } else {
                Option<string> userDbLocation = AskForDbLocation();
                if (userDbLocation.NonEmpty()) {
                    settings.dbLocation = userDbLocation.Get();
                } else {
                    return new None<Settings>();
                }

		        settings.gradeViewTags = "";
                settings.Save();
            }
            return new Some<Settings>(settings);
        }

        private static string GetSettingsLocation() {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + 
                Path.DirectorySeparatorChar + 
                "grader.settings";
        }

        public static Option<string> AskForDbLocation() {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK) {
                return new Some<string>(ofd.FileName);
            } else {
                return new None<string>();
            }
        }
    }
}
