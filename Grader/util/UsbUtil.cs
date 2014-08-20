using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace Grader.util {
    public static class UsbUtil {
        public static List<string> GetUsbSerialNumbers() {
            ManagementObjectSearcher theSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            List<string> serialNumbers = new List<string>();
            foreach (ManagementObject currentObject in theSearcher.Get()) {
                ManagementObject serialNumberQuery = new ManagementObject("Win32_PhysicalMedia.Tag='" + currentObject["DeviceID"] + "'");
                serialNumbers.Add(serialNumberQuery["SerialNumber"].ToString());
            }
            return serialNumbers;
        }
    }
}
