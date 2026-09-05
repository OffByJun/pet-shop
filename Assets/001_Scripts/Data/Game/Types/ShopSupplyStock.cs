using System;
using System.Collections.Generic;

namespace _001_Scripts.Data
{
    /// <summary>가게가 들고 있는 보급품 수량입니다. 하루가 바뀌어도 유지됩니다.</summary>
    public sealed class ShopSupplyStock
    {
        private readonly Dictionary<string, int> amounts = new Dictionary<string, int>(StringComparer.Ordinal);

        public int Get(ShopSupplyDefinition supply) =>
            supply != null && amounts.TryGetValue(supply.SupplyId, out var amount) ? amount : 0;

        public bool Has(ShopSupplyDefinition supply, int amount) => amount <= 0 || Get(supply) >= amount;

        public void Add(ShopSupplyDefinition supply, int amount)
        {
            if (supply == null || amount <= 0) return;
            amounts[supply.SupplyId] = Get(supply) + amount;
        }

        public bool TryConsume(ShopSupplyDefinition supply, int amount)
        {
            if (supply == null || amount <= 0) return true;
            var current = Get(supply);
            if (current < amount) return false;
            amounts[supply.SupplyId] = current - amount;
            return true;
        }

        /// <summary>새 게임 시작 시 정의된 기본 수량으로 채웁니다.</summary>
        public void Reset(IReadOnlyList<ShopSupplyDefinition> supplies)
        {
            amounts.Clear();
            if (supplies == null) return;
            for (var i = 0; i < supplies.Count; i++)
            {
                var supply = supplies[i];
                if (supply != null) amounts[supply.SupplyId] = supply.StartingStock;
            }
        }
    }
}
