using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.Data
{
    public sealed class DaySummary
    {
        public int DayNumber { get; }
        public int TotalCustomers { get; }
        public int FailedOrders { get; }
        public int CompletedOrders { get; }
        public int PerfectOrders { get; }
        public int ServiceRevenue { get; }
        public int ByproductRevenue { get; }
        public int TotalRevenue => ServiceRevenue + ByproductRevenue;

        internal DaySummary(
            int dayNumber,
            int totalCustomers,
            IReadOnlyList<ServiceOrderCompletion> completions,
            int byproductRevenue)
        {
            DayNumber = dayNumber;
            TotalCustomers = totalCustomers;
            ByproductRevenue = byproductRevenue;
            var failed = 0;
            var completed = 0;
            var perfect = 0;
            var serviceRevenue = 0;
            for (var i = 0; i < completions.Count; i++)
            {
                var completion = completions[i];
                serviceRevenue += completion.Reward.Currency;
                switch (completion.Result)
                {
                    case ServiceOrderStatus.Failed: failed++; break;
                    case ServiceOrderStatus.Completed: completed++; break;
                    case ServiceOrderStatus.Perfect: perfect++; break;
                }
            }
            FailedOrders = failed;
            CompletedOrders = completed;
            PerfectOrders = perfect;
            ServiceRevenue = serviceRevenue;
        }
    }
}
