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
        /// <summary>선택 케어까지 끝낸 완벽 주문 보너스입니다.</summary>
        public int PerfectBonus { get; }
        /// <summary>케어 솜씨와 단골 관계로 얹히는 팁입니다.</summary>
        public int Tip { get; }
        public int Total => VisitFee + RequiredCareFee + OptionalCareBonus + PerfectBonus + Tip;

        public ServicePriceBreakdown(int visitFee, int requiredCareFee, int optionalCareBonus,
            int perfectBonus = 0, int tip = 0)
        {
            VisitFee = visitFee;
            RequiredCareFee = requiredCareFee;
            OptionalCareBonus = optionalCareBonus;
            PerfectBonus = perfectBonus;
            Tip = tip;
        }
    }

}
