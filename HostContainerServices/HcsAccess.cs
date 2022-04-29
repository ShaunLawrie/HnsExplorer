using Microsoft.Windows.ComputeVirtualization;
using System.Text.Json;

namespace HnsExplorer.HostContainerServices
{
    public class HcsAccess
    {
        private IHcs _hcs;

        public HcsAccess()
        {
            _hcs = HcsFactory.GetHcs();
        }

        public IEnumerable<JsonElement> GetComputeSystems()
        {
            try
            {
                _hcs.EnumerateComputeSystems(null, out string computeSystems);
                return JsonSerializer.Deserialize<IEnumerable<JsonElement>>(computeSystems) ?? new List<JsonElement>();
            }
            catch (Exception ex)
            {
                var error = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(ex));
                return new List<JsonElement> { error };
            }
        }
    }
}
