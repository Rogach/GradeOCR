using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using LibUtil;
using System.Windows.Forms;
using Grader.gui;

namespace Grader {
    public interface Setting<out T> {
        T GetValue();
        void SetValue(object value);
        bool init();
        bool read(XmlDocument doc);
        void save(XmlDocument doc, XmlElement settings);
    }

    public abstract class AbstractStringSetting : Setting<string> {
        protected string settingName;
        protected string settingValue;

        public AbstractStringSetting(string settingName) {
            this.settingName = settingName;
        }
        public abstract bool init();
        public bool read(XmlDocument doc) {
            XmlNodeList nodes = doc.SelectNodes("/settings/" + settingName);
            if (nodes.Count > 0) {
                settingValue = nodes[0].InnerText;
                return true;
            } else {
                return init();
            }
        }
        public void save(XmlDocument doc, XmlElement settings) {
            XmlElement xe = doc.CreateElement(settingName);
            xe.AppendChild(doc.CreateTextNode(settingValue));
            settings.AppendChild(xe);
        }
        public string GetValue() {
            return settingValue;
        }
        public void SetValue(object value) {
            settingValue = (string) value;
        }
    }

    public class DbConnectionStringSetting : AbstractStringSetting {
        public DbConnectionStringSetting(string settingName) : base(settingName) {}
        public override bool init() {
            Option<string> userDbConnectionString = DbConnectionDialog.ShowDbConnectionDialog();
            userDbConnectionString.ForEach(cs => {
                settingValue = cs;
            });
            return userDbConnectionString.NonEmpty();
        }
    }

    public class DirSetting : AbstractStringSetting {
        public DirSetting(string settingName) : base(settingName) {}
        public override bool init() {
            var fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK) {
                settingValue = fbd.SelectedPath;
                return true;
            } else {
                return false;
            }
        }
    }

    public class StringSetting : AbstractStringSetting {
        public StringSetting(string settingName, string defaultValue = "") : base(settingName) {
            settingValue = defaultValue;
        }
        public override bool init() { 
            return true;
        }
    }

    public class Settings {
        public DbConnectionStringSetting dbConnectionString = new DbConnectionStringSetting("dbConnectionString");
        public DirSetting templatesLocation = new DirSetting("templatesLocation");
        public StringSetting gradeViewTags = new StringSetting("gradeViewTags");

        public string GetTemplateLocation(string templateName) {
            return templatesLocation + "/" + templateName;
        }

        private List<Setting<object>> settings;

        public Settings() {
            settings = new List<Setting<object>> {
                dbConnectionString,
                templatesLocation,
                gradeViewTags
            };
        }

        public void Save() {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));

            XmlElement settings = doc.CreateElement("settings");
            doc.AppendChild(settings);
            foreach (Setting<object> s in this.settings) {
                s.save(doc, settings);
            }

            XmlWriter xw = XmlWriter.Create(GetSettingsLocation());
            doc.WriteTo(xw);
            xw.Flush();
        }

        public static Option<Settings> Load() {
            Settings settings = new Settings();
            if (File.Exists(GetSettingsLocation())) {
                XmlDocument doc = new XmlDocument();
                doc.Load(GetSettingsLocation());

                foreach (Setting<object> s in settings.settings) {
                    if (!s.read(doc)) {
                        return new None<Settings>();
                    }
                }
            } else {
                foreach (Setting<object> s in settings.settings) {
                    if (!s.init()) {
                        return new None<Settings>();
                    }
                }
                settings.gradeViewTags.SetValue("");
            }
            return new Some<Settings>(settings);
        }

        private static string GetSettingsLocation() {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + 
                Path.DirectorySeparatorChar + 
                "grader.settings";
        }
    }
}
