using System.Text.Json;
using HnsExplorer.HostContainerServices;
using HnsExplorer.Extensions;

namespace HnsExplorer.Data
{
    public class HnsDatasource
    {

        private readonly HnsAccess HnsAccess = new();
        private readonly HcsAccess HcsAccess = new();

        public int NumberOfStepsLoaded { get; private set; }
        public int NumberOfStepsTotal { get; private set; }

        public string SummaryOutput { get; private set; }
        public string LoadingState { get; private set; }
        public string ExportDataSnapshot { get; private set; }

        public TreeNode RoutesNode { get; private set; }
        public TreeNode ActivitiesNode { get; private set; }
        public TreeNode OrphansNode { get; private set; }
        public TreeNode NamespacesNode { get; private set; }

        public HnsDatasource()
        {
            NumberOfStepsLoaded = 0;
            NumberOfStepsTotal = 20;
            LoadingState = "Initialised";
            SummaryOutput = "No data";
            ExportDataSnapshot = "{}";
            RoutesNode = new TreeNode();
            ActivitiesNode = new TreeNode();
            OrphansNode = new TreeNode();
            NamespacesNode = new TreeNode();
        }

        private void UpdateLoadingState(string message)
        {
            LoadingState = message;
            if(NumberOfStepsLoaded < NumberOfStepsTotal)
            {
                NumberOfStepsLoaded++;
            }
        }
        public void Reset()
        {
            NumberOfStepsLoaded = 0;
            LoadingState = "Reload required";
            SummaryOutput = "No data";
            ExportDataSnapshot = "{}";
            RoutesNode = new TreeNode();
            ActivitiesNode = new TreeNode();
            OrphansNode = new TreeNode();
            NamespacesNode = new TreeNode();
        }

        public void Load()
        {
            UpdateLoadingState("Loading activities...");
            var activitiesData = HnsAccess.GetActivities();
            UpdateLoadingState("Loading namespaces...");
            var namespaceData = HnsAccess.GetNamespaces();
            UpdateLoadingState("Loading networks...");
            var networkData = HnsAccess.GetNetworks();
            UpdateLoadingState("Loading policies...");
            var policyData = HnsAccess.GetPolicyLists();
            UpdateLoadingState("Loading endpoints...");
            var endpointData = HnsAccess.GetEndpoints();
            UpdateLoadingState("Loading compute...");
            var computeData = HcsAccess.GetComputeSystems();
            UpdateLoadingState("Loading routes...");
            var routeData = HostNetworkAccess.GetIpForwardTable();

            LoadingState = "Building summary...";
            SummaryOutput = $"Activities: {activitiesData.Count()}{Environment.NewLine}";
            SummaryOutput += $"Namespaces: {namespaceData.Count()}{Environment.NewLine}";
            SummaryOutput += $"Networks: {networkData.Count()}{Environment.NewLine}";
            SummaryOutput += $"Network policies: {policyData.Count()}{Environment.NewLine}";
            SummaryOutput += $"Network endpoints: {endpointData.Count()}{Environment.NewLine}";
            SummaryOutput += $"Compute systems: {computeData.Count()}{Environment.NewLine}";
            SummaryOutput += $"Host routes: {routeData.Count()}{Environment.NewLine}";

            UpdateLoadingState("Building routes output...");
            var hostRouteList = $"{"Destination",16} {"Netmask",16} {"Gateway",16} {"Interface",16} {"IfIndex",8} {"Type",8} {"Protocol",14} {"Metric",7}{Environment.NewLine}";
            hostRouteList += $"------------------------------------------------------------------------------------------------------------{Environment.NewLine}";
            foreach (var row in routeData)
            {
                hostRouteList += $"{row.ForwardDest,16} {row.ForwardMask,16} {row.ForwardNextHop,16} {row.IfIpAddress,16} {row.IfIndex,8} {row.RouteType,8} {row.ForwardProto,14} {row.PrimaryMetric,7}{row.Error}{Environment.NewLine}";
            }
            RoutesNode = new TreeNode
            {
                Text = "HostRoutes",
                Tag = hostRouteList
            };

            ActivitiesNode = new TreeNode
            {
                Text = "Activities",
                Tag = SummaryOutput
            };

            UpdateLoadingState("Building activities tree...");
            var orphanedActivities = ActivitiesNode.Nodes.InsertNestedChildren(activitiesData, "ID", "parentId", "Activity", "Allocators.Tag");

            UpdateLoadingState("Inserting network children...");
            var orphanedNetworks = ActivitiesNode.Nodes.InsertNestedChildren(networkData, "ID", "Resources.ID", "Network", "Name,' ',ManagementIP");

            UpdateLoadingState("Inserting endpoint children...");
            var orphanedEndpoints = ActivitiesNode.Nodes.InsertNestedChildren(endpointData, "ID", "Resources.ID", "Endpoint", "Name,' ',IPAddress");

            UpdateLoadingState("Inserting policy children...");
            var orphanedPolicies = ActivitiesNode.Nodes.InsertNestedChildren(policyData, "ID", "Resources.ID", "Policy", "Policies.Type,' ',Policies.SourceVIP,' ->',Policies.VIPs,' ',Policies.ExternalPort,':',Policies.InternalPort");

            UpdateLoadingState("Getting endpointstats...");
            var endpointIds = new List<string>();
            foreach (var item in endpointData)
            {
                var id = item.GetJsonDataAsString("ID");
                endpointIds.Add(id);
            }
            var endpointStatsData = HnsAccess.GetEndpointStats(endpointIds);
            UpdateLoadingState("Inserting endpointstats children...");
            ActivitiesNode.Nodes.InsertNestedChildren(endpointStatsData, "InstanceId", "EndpointId", "Endpoint Stats", "Name");

            UpdateLoadingState("Inserting virtualmachine children...");
            var orphanedCompute = ActivitiesNode.Nodes.InsertChildrenWithMatchingParentReference(computeData, endpointData, "Id", "ID", "VirtualMachine", "Virtual Machine", "Owner");
            UpdateLoadingState("Inserting container children...");
            orphanedCompute = ActivitiesNode.Nodes.InsertChildrenWithMatchingParentReference(orphanedCompute, endpointData, "Id", "ID", "SharedContainers", "Container", "Owner");

            UpdateLoadingState("Building orphan tree...");
            OrphansNode = new TreeNode("Orphaned Data");
            OrphansNode.Nodes.InsertChildren(orphanedActivities, "ID", "Activities", "Allocators.Tag");
            OrphansNode.Nodes.InsertChildren(orphanedNetworks, "ID", "Network", "Name");
            OrphansNode.Nodes.InsertChildren(orphanedEndpoints, "ID", "Endpoint", "Name");
            OrphansNode.Nodes.InsertChildren(orphanedPolicies, "ID", "Endpoint", "Name");
            OrphansNode.Nodes.InsertChildren(orphanedCompute, "ID", "Compute", "Name");

            UpdateLoadingState("Building namespaces tree...");
            NamespacesNode = new TreeNode
            {
                Text = "Namespaces",
                Tag = $"{namespaceData.Count()} namespaces"
            };
            NamespacesNode.Nodes.InsertChildren(namespaceData, "ID", "Namespace", "CompartmentId");

            UpdateLoadingState("Building data snapshot...");
            var allData = new Dictionary<string, IEnumerable<JsonElement>>
            {
                { "Namespaces", namespaceData },
                { "Network", networkData },
                { "Policies", policyData },
                { "Endpoints", endpointData },
                { "Compute", computeData },
                { "Activities", activitiesData },
                { "EndpointStats", endpointStatsData }
            };
            var routeString = JsonSerializer.Serialize(routeData);
            var routeJsonElement = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(routeString);
            if(routeJsonElement is not null)
            {
                allData.Add("Routes", routeJsonElement);
            }
            ExportDataSnapshot = JsonSerializer.Serialize(allData);
            UpdateLoadingState("Done");
        }
    }
}
