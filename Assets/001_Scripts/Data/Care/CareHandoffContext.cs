using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.Data
{

    /// <summary>Scene-boundary store. Domain data is immutable after each handoff.</summary>
    public static class CareHandoffContext
    {
        private static ICareConditionIdMapper mapper = new DefaultCareConditionIdMapper();
        private static CareVisitSnapshot current = Empty();

        public static ServiceOrder ActiveOrder => current.Order;
        public static string CustomerName => current.CustomerName;
        public static string PetName => current.PetName;
        public static string PetKind => current.PetKind;
        public static bool HasActiveVisit => current.HasConditions;

        public static void SetMapper(ICareConditionIdMapper value) =>
            mapper = value ?? throw new ArgumentNullException(nameof(value));

        public static void Set(string customerName, string petName, string petKind, string[] conditions) =>
            current = new CareVisitSnapshot(null, customerName, petName, petKind, conditions);

        public static void Set(ServiceOrder order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            var ids = new string[order.Requests.Count];
            for (var i = 0; i < order.Requests.Count; i++) ids[i] = mapper.Map(order.Requests[i].Condition);
            current = new CareVisitSnapshot(
                order,
                order.Customer.DisplayName,
                order.Pet.DisplayName,
                order.Pet.BaseAnimal == null ? string.Empty : order.Pet.BaseAnimal.DisplayName,
                ids);
        }

        public static bool HasCondition(string id) => current.HasCondition(id);
        public static void Clear() => current = Empty();

        private static CareVisitSnapshot Empty() =>
            new CareVisitSnapshot(null, string.Empty, "펫", string.Empty, Array.Empty<string>());
    }
}
