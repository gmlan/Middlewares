using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Com.Gmlan.Scheduler.Model
{
    public class HostInfo
    {
        public static string IpAddress;
        public static string HostName;
        public static string MacAddress;
        public static string MachineKey;

        static HostInfo()
        {
            HostName = Dns.GetHostName();
            MacAddress = GetMacAddress()?.ToString();
            MachineKey = $"{HostName}_{Guid.NewGuid():N}";
            IpAddress = string.Join(",", Dns.GetHostAddresses(HostName).Select(m => m.ToString()));
        }

        private static PhysicalAddress GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
            }
            return null;
        }
    }
}
