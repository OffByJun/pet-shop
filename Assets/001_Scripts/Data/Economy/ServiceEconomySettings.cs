using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Items;
using UnityEngine;

namespace _001_Scripts.Data.Economy
{
    [Serializable]
    public struct ServicePriceTier
    {
        [SerializeField] private string tierId;
        [SerializeField, Min(0)] private int visitFee;
        [SerializeField, Min(0)] private int requiredCareUnitPrice;
        [SerializeField, Min(0)] private int optionalCareBonusUnitPrice;

        public string TierId => tierId;
        public int VisitFee => visitFee;
        public int RequiredCareUnitPrice => requiredCareUnitPrice;
        public int OptionalCareBonusUnitPrice => optionalCareBonusUnitPrice;
    }

    /// <summary>서비스 가격표입니다. 손님/주문 시스템은 구체 가격을 소유하지 않습니다.</summary>
    [CreateAssetMenu(fileName = "ServiceEconomySettings", menuName = "PetShop/Economy/Service Economy Settings")]
    public sealed class ServiceEconomySettings : ScriptableObject, IServiceOrderEconomy
    {
        [SerializeField] private ServicePriceTier defaultTier;
        [SerializeField] private ServicePriceTier[] tiers = Array.Empty<ServicePriceTier>();

        public IReadOnlyList<ServicePriceTier> Tiers => tiers;

        public ServiceReward CalculateReward(ServiceOrder order, ServiceOrderStatus result)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (result == ServiceOrderStatus.Failed) return ServiceReward.Empty;

            var tier = FindTier(order.Customer.EconomyTierId);
            var requiredFee = CalculateResolvedCare(order.RequiredRequests, tier.RequiredCareUnitPrice);
            var optionalBonus = CalculateResolvedCare(order.OptionalRequests, tier.OptionalCareBonusUnitPrice);
            var breakdown = new ServicePriceBreakdown(tier.VisitFee, requiredFee, optionalBonus);
            return new ServiceReward
            {
                Currency = breakdown.Total,
                PriceBreakdown = breakdown,
                BonusItems = Array.Empty<ItemStack>()
            };
        }

        private ServicePriceTier FindTier(string tierId)
        {
            if (tiers != null)
                for (var i = 0; i < tiers.Length; i++)
                    if (string.Equals(tiers[i].TierId, tierId, StringComparison.OrdinalIgnoreCase)) return tiers[i];
            return defaultTier;
        }

        private static int CalculateResolvedCare(IReadOnlyList<ServiceRequestState> requests, int unitPrice)
        {
            var total = 0;
            for (var i = 0; i < requests.Count; i++)
                if (requests[i].IsResolved)
                    total = checked(total + checked(requests[i].Condition.Severity * unitPrice));
            return total;
        }
    }
}
