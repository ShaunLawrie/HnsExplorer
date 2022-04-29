using System.Text.Json;
using System.Runtime.InteropServices;
using Microsoft.Windows.ComputeVirtualization;

namespace HnsExplorer.HostContainerServices
{
    public class HnsAccess
    {
        private IHns _hns;

        public HnsAccess()
        {
            _hns = HnsFactory.GetHns();
        }

        [DllImport("computenetwork.dll", ExactSpelling = true)]
        private static extern void HcnEnumerateLoadBalancers(string filter, [MarshalAs(UnmanagedType.LPWStr)] out string loadBalancersString, [MarshalAs(UnmanagedType.LPWStr)] out string errorRecord);
        [DllImport("computenetwork.dll", ExactSpelling = true)]
        private static extern void HcnOpenLoadBalancer(Guid lbguid, out IntPtr loadBalancerHandle, [MarshalAs(UnmanagedType.LPWStr)] out string errorRecord);
        [DllImport("computenetwork.dll", ExactSpelling = true)]
        private static extern void HcnCloseLoadBalancer(IntPtr loadBalancerHandle);
        [DllImport("computenetwork.dll", ExactSpelling = true)]
        private static extern void HcnQueryLoadBalancerProperties(IntPtr handle, string query, [MarshalAs(UnmanagedType.LPWStr)] out string properties, [MarshalAs(UnmanagedType.LPWStr)] out string errorRecord);

        private class HnsResponse {
            public bool Success { get; set; }
            public JsonElement Output { get; set; }
        }

        private class HnsResponseCollection
        {
            public bool Success { get; set; }
            public IEnumerable<JsonElement>? Output { get; set; }
        }

        public IEnumerable<JsonElement> GetActivities()
        {
            try
            {
                _hns.Call("GET", "/activities/", "", out string response);
                var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
                return hnsResponse?.Output ?? new List<JsonElement>();
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }

        public IEnumerable<JsonElement> GetNamespaces()
        {
            try
            {
                _hns.Call("GET", "/namespaces/", "", out string response);
                var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
                return hnsResponse?.Output ?? new List<JsonElement>();
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }

        public IEnumerable<JsonElement> GetNetworks()
        {
            try
            {
                _hns.Call("GET", "/networks/", "", out string response);
                var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
                return hnsResponse?.Output ?? new List<JsonElement>();
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }

        public IEnumerable<JsonElement> GetPolicyLists()
        {
            try
            {
                _hns.Call("GET", "/policylists/", "", out string response);
                var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
                return hnsResponse?.Output ?? new List<JsonElement>();
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }

        public IEnumerable<JsonElement> GetLoadBalancers()
        {
            try
            {
                HcnEnumerateLoadBalancers("", out string loadBalancers, out string errorRecord);
                var hnsResponse = JsonSerializer.Deserialize<List<string>>(loadBalancers);
                var loadBalancerProperties = new List<JsonElement>();
                if(hnsResponse is not null)
                {
                    foreach (var loadbalancer in hnsResponse)
                    {
                        HcnOpenLoadBalancer(new Guid(loadbalancer), out IntPtr handle, out string errorRecord1);
                        HcnQueryLoadBalancerProperties(handle, "", out string properties, out string errorRecord2);
                        HcnCloseLoadBalancer(handle);
                        loadBalancerProperties.Add(JsonSerializer.Deserialize<JsonElement>(properties));
                    }
                }
                return loadBalancerProperties;
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize($"Failed to get loadbalancers: {ex.Message}\n{ex.StackTrace}"));
                return new List<JsonElement> { error };
            }
        }

        public IEnumerable<JsonElement> GetEndpoints()
        {
            try
            {
                _hns.Call("GET", "/endpoints/", "", out string response);
                var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
                return hnsResponse?.Output ?? new List<JsonElement>();
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }

        public IEnumerable<JsonElement> GetEndpointStats(IEnumerable<string> endpointIds)
        {
            try
            {
                var endpointStats = new List<JsonElement>();
                foreach(var endpointId in endpointIds)
                {
                    _hns.Call("GET", $"/endpointstats/{endpointId}", "", out string response);
                    var hnsResponse = JsonSerializer.Deserialize<HnsResponse>(response);
                    if(hnsResponse?.Output != null)
                    {
                        endpointStats.Add(hnsResponse.Output);
                    }
                }
                return endpointStats;
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }
    }
}
