using System;
using System.Collections.Generic;
using _001_Scripts.Data.Items;

namespace _001_Scripts.Data.Customers
{
    [Serializable]
    public struct ServiceReward
    {
        public int Currency;
        public ServicePriceBreakdown PriceBreakdown;
        public ItemStack[] BonusItems;

        public IReadOnlyList<ItemStack> Items => BonusItems ?? Array.Empty<ItemStack>();
        public static ServiceReward Empty => new ServiceReward { BonusItems = Array.Empty<ItemStack>() };
    }

    [Serializable]
    public readonly struct ServicePriceBreakdown
    {
        public int VisitFee { get; }
        public int RequiredCareFee { get; }
        public int OptionalCareBonus { get; }
        public int Total => VisitFee + RequiredCareFee + OptionalCareBonus;

        public ServicePriceBreakdown(int visitFee, int requiredCareFee, int optionalCareBonus)
        {
            VisitFee = visitFee;
            RequiredCareFee = requiredCareFee;
            OptionalCareBonus = optionalCareBonus;
        }
    }

}
