﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using LibUtil;
using System.Windows.Forms;
using Grader.gui;
using MySql.Data.MySqlClient;

namespace Grader {
    
    public class Settings {
        public DbConnectionStringSetting dbConnectionString = new DbConnectionStringSetting();
        public DirSetting templatesLocation = new DirSetting("templatesLocation");
        public StringSetting gradeViewTags = new StringSetting("gradeViewTags");

        public string GetTemplateLocation(string templateName) {
            return templatesLocation.GetValue() + "/" + templateName;
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
            xw.Close();
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

        public static Option<string> XmlValue(XmlDocument doc, string path) {
            XmlNodeList nodes = doc.SelectNodes(path);
            if (nodes.Count > 0) {
                return new Some<string>(nodes[0].InnerText);
            } else {
                return new None<string>();
            }
        }
    }

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
            Option<string> settingOpt = Settings.XmlValue(doc, "/settings/" + settingName);
            if (settingOpt.NonEmpty()) {
                settingValue = settingOpt.Get();
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

    public class DbConnectionStringSetting : Setting<string> {
        string settingValue;

        string serverSetting;
        string portSetting;
        string userSetting;
        string passwordSetting;

        public DbConnectionStringSetting() { }
        public bool read(XmlDocument doc) {
            Option<string> serverSettingOpt = Settings.XmlValue(doc, "/settings/connection/server");
            Option<string> portSettingOpt = Settings.XmlValue(doc, "/settings/connection/port");
            Option<string> userSettingOpt = Settings.XmlValue(doc, "/settings/connection/user");
            if (serverSettingOpt.NonEmpty() && portSettingOpt.NonEmpty() && userSettingOpt.NonEmpty()) {
                DbConnectionDialog dcd = new DbConnectionDialog();
                dcd.Server = serverSettingOpt.Get();
                dcd.Port = portSettingOpt.Get();
                dcd.User = userSettingOpt.Get();
                dcd.FocusOnPassword = true;
                return ShowDbConnectionDialog(dcd);
            } else {
                return init();
            }
        }
        public bool init() {
            DbConnectionDialog dcd = new DbConnectionDialog();
            dcd.Port = "3306";
            return ShowDbConnectionDialog(dcd);
        }

        public bool ShowDbConnectionDialog(DbConnectionDialog dcd) {
            if (dcd.ShowDialog() == DialogResult.OK && dcd.User != "root") {
                if (!TestConnection(dcd)) {
                    dcd.Password = "";
                    return ShowDbConnectionDialog(dcd);
                } else {
                    settingValue = dcd.ConnectionString;
                    serverSetting = dcd.Server;
                    portSetting = dcd.Port;
                    userSetting = dcd.User;
                    passwordSetting = dcd.Password;
                    return true;
                }
            } else {
                return false;
            }
        }

        public bool TestConnection(DbConnectionDialog dcd) {
            var conn = new MySqlConnection(dcd.ConnectionString);
            try {
                conn.Open();
                conn.Close();
                return true;
            } catch (Exception) {
                MessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void save(XmlDocument doc, XmlElement settings) {
            XmlElement connection = doc.CreateElement("connection");

            XmlElement serverXe = doc.CreateElement("server");
            serverXe.AppendChild(doc.CreateTextNode(serverSetting));
            connection.AppendChild(serverXe);

            XmlElement portXe = doc.CreateElement("port");
            portXe.AppendChild(doc.CreateTextNode(portSetting));
            connection.AppendChild(portXe);

            XmlElement userXe = doc.CreateElement("user");
            userXe.AppendChild(doc.CreateTextNode(userSetting));
            connection.AppendChild(userXe);

            settings.AppendChild(connection);
        }

        public string GetValue() {
            return settingValue;
        }
        public void SetValue(object value) {
            settingValue = (string) value;
        }
    }

    public class DirSetting : AbstractStringSetting {
        public DirSetting(string settingName) : base(settingName) { }
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
        public StringSetting(string settingName, string defaultValue = "")
            : base(settingName) {
            settingValue = defaultValue;
        }
        public override bool init() {
            return true;
        }
    }

}
