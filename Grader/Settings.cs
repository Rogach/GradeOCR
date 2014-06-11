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

        public void Save() {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlElement settings = doc.CreateElement("settings");
            doc.AppendChild(settings);
            XmlElement dbLocationSetting = doc.CreateElement("dbLocation");
            dbLocationSetting.AppendChild(doc.CreateTextNode(dbLocation));
            settings.AppendChild(dbLocationSetting);

            XmlWriter xw = XmlWriter.Create(GetSettingsLocation());
            doc.WriteTo(xw);
            xw.Flush();
        }

        public static Option<Settings> Load() {
            Settings settings = new Settings();
            if (File.Exists(GetSettingsLocation())) {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetSettingsLocation());

                XmlNodeList dbLocationSetting = doc.SelectNodes("/settings/dbLocation");
                if (dbLocationSetting.Count > 0) {
                    settings.dbLocation = dbLocationSetting[0].InnerText;
                } else {
                    Option<string> userDbLocation = AskForDbLocation();
                    if (userDbLocation.NonEmpty()) {
                        settings.dbLocation = userDbLocation.Get();
                    } else {
                        return new None<Settings>();
                    }
                }
            } else {
                Option<string> userDbLocation = AskForDbLocation();
                if (userDbLocation.NonEmpty()) {
                    settings.dbLocation = userDbLocation.Get();
                } else {
                    return new None<Settings>();
                }
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
