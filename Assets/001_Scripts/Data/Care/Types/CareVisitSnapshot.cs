using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.Data
{
    public sealed class CareVisitSnapshot
    {
        private readonly HashSet<string> conditionIds;

        public ServiceOrder Order { get; }
        public string CustomerName { get; }
        public string PetName { get; }
        public string PetKind { get; }
        public bool HasConditions => conditionIds.Count > 0;

        public CareVisitSnapshot(
            ServiceOrder order, string customerName, string petName, string petKind, IEnumerable<string> conditions)
        {
            Order = order;
            CustomerName = customerName ?? string.Empty;
            PetName = string.IsNullOrWhiteSpace(petName) ? "펫" : petName;
            PetKind = petKind ?? string.Empty;
            conditionIds = new HashSet<string>(conditions ?? Array.Empty<string>(), StringComparer.Ordinal);
        }

        public bool HasCondition(string id) => conditionIds.Contains(id);
    }
}
