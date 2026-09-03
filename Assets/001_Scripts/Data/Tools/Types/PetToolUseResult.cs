using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;

namespace _001_Scripts.Data.Tools
{
    public sealed class PetToolUseResult
    {
        public PetToolDefinition Tool { get; }
        public ServiceRequestState ResolvedRequest { get; }
        public PetCareResult CareReward { get; }

        public PetToolUseResult(PetToolDefinition tool, ServiceRequestState resolvedRequest, PetCareResult careReward)
        {
            Tool = tool;
            ResolvedRequest = resolvedRequest;
            CareReward = careReward;
        }
    }
}
