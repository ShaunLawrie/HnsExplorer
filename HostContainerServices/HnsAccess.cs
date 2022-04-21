using System.Text.Json;
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
            _hns.Call("GET", "/activities/", "", out string response);
            var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
            return hnsResponse?.Output ?? new List<JsonElement>();
        }

        public IEnumerable<JsonElement> GetNamespaces()
        {
            _hns.Call("GET", "/namespaces/", "", out string response);
            var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
            return hnsResponse?.Output ?? new List<JsonElement>();
        }

        public IEnumerable<JsonElement> GetNetworks()
        {
            _hns.Call("GET", "/networks/", "", out string response);
            var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
            return hnsResponse?.Output ?? new List<JsonElement>();
        }

        public IEnumerable<JsonElement> GetPolicyLists()
        {
            _hns.Call("GET", "/policylists/", "", out string response);
            var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
            return hnsResponse?.Output ?? new List<JsonElement>();
        }

        public IEnumerable<JsonElement> GetEndpoints()
        {
            _hns.Call("GET", "/endpoints/", "", out string response);
            var hnsResponse = JsonSerializer.Deserialize<HnsResponseCollection>(response);
            return hnsResponse?.Output ?? new List<JsonElement>();
        }

        public IEnumerable<JsonElement> GetEndpointStats(IEnumerable<string> endpointIds)
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
    }
}
