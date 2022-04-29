using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace HnsExplorer.HostContainerServices
{
    public static class HostNetworkAccess
    {
        public enum RouteType
        {
            OTHER = 1,
            INVALID = 2,
            DIRECT = 3,
            INDIRECT = 4
        }
        public enum ForwardProto
        {
            OTHER = 1,
            LOCAL = 2,
            NETMGMT = 3,
            ICMP = 4,
            EGP = 5,
            GGP = 6,
            HELLO = 7,
            RIP = 8,
            IS_IS = 9,
            ES_IS = 10,
            CISCO = 11,
            BBN = 12,
            OSPF = 13,
            BGP = 14,
            AUTOSTATIC = 10002,
            STATIC = 10006,
            STATIC_NON_DOD = 1000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPFORWARDROW
        {
            public uint dwForwardDest;
            public uint dwForwardMask;
            public uint dwForwardPolicy;
            public uint dwForwardNextHop;
            public uint dwForwardIfIndex;
            public uint dwForwardType;
            public uint dwForwardProto;
            public uint dwForwardAge;
            public uint dwForwardNextHopAS;
            public int dwForwardMetric1;
            public int dwForwardMetric2;
            public int dwForwardMetric3;
            public int dwForwardMetric4;
            public int dwForwardMetric5;
        }

        [DllImport("iphlpapi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        private static extern int GetIpForwardTable(IntPtr pIpForwardTable, out int pdwSize, bool bOrder);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used during json serialization to avoid socket errors when trying to serialize IPAddress")]
        public class IpRoute
        {
            [JsonIgnore]
            public IPAddress ForwardDest { get; set; }
            [JsonIgnore]
            public IPAddress ForwardMask { get; set; }
            [JsonIgnore]
            public IPAddress ForwardNextHop { get; set; }
            [JsonIgnore]
            public IPAddress IfIpAddress { get; set; }
            [JsonPropertyName("ForwardDest")]
            private string ForwardDestString => ForwardDest.ToString();
            [JsonPropertyName("ForwardDest")]
            private string ForwardMaskString => ForwardMask.ToString();
            [JsonPropertyName("ForwardNextHop")]
            private string ForwardNextHopString => ForwardNextHop.ToString();
            [JsonPropertyName("IfIpAddress")]
            private string IfIpAddressString => IfIpAddress.ToString();
            public int IfIndex { get; set; }
            public ForwardProto ForwardProto { get; set; }
            public RouteType RouteType { get; set; }
            public int PrimaryMetric { get; set; }
            public string Error { get; set; }

            public IpRoute(uint forwardDest, uint forwardMask, uint forwardNextHop, int ifIndex, IPAddress ifIpAddress, uint routeType, uint forwardProto, int primaryMetric)
            {
                ForwardDest = new IPAddress(forwardDest);
                ForwardMask = new IPAddress(forwardMask);
                ForwardNextHop = new IPAddress(forwardNextHop);
                ForwardProto = (ForwardProto)forwardProto;
                IfIndex = ifIndex;
                IfIpAddress = ifIpAddress;
                RouteType = (RouteType)routeType;
                PrimaryMetric = primaryMetric;
                Error = string.Empty;
            }

            public IpRoute(string error)
            {
                ForwardDest = IPAddress.None;
                ForwardMask = IPAddress.None;
                ForwardNextHop = IPAddress.None;
                ForwardProto = (ForwardProto)1;
                IfIndex = -1;
                IfIpAddress = IPAddress.None;
                RouteType = (RouteType)1;
                PrimaryMetric = -1;
                Error = error;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Native call requires the out param, discard doesn't work")]
        public static IEnumerable<IpRoute> GetIpForwardTable()
        {
            int bufferSize = 1;
            IntPtr data = Marshal.AllocHGlobal(bufferSize);
            try
            {
                GetIpForwardTable(data, out bufferSize, true);
                data = Marshal.AllocHGlobal(bufferSize);
                var ret = GetIpForwardTable(data, out bufferSize, true);
                if (ret != 0)
                {
                    return new List<IpRoute>();
                }
                int length = Marshal.ReadInt32(data);
                var rowSize = Marshal.SizeOf<MIB_IPFORWARDROW>();

                var routeTable = new List<IpRoute>();
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();

                for (int i = 0; i < length; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_IPFORWARDROW>(data + sizeof(int) + (rowSize * i));
                    var index = (int)row.dwForwardIfIndex;
                    var iface = interfaces.Where(i => i.GetIPProperties().GetIPv4Properties()?.Index == index).First();
                    var ifIpAddress = iface.GetIPProperties().UnicastAddresses
                        .Where(u => u.Address.AddressFamily is AddressFamily.InterNetwork)
                        .First()?.Address;
                    if (ifIpAddress is null)
                    {
                        ifIpAddress = new IPAddress(0);
                    }
                    routeTable.Add(new IpRoute(
                        row.dwForwardDest,
                        row.dwForwardMask,
                        row.dwForwardNextHop,
                        index,
                        ifIpAddress,
                        row.dwForwardType,
                        row.dwForwardType,
                        row.dwForwardMetric1));
                }
                return routeTable;
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Serialize(ex);
                return new List<IpRoute> { new IpRoute(error) };
            }
            finally
            {
                Marshal.FreeHGlobal(data);
            }
        }
    }
}