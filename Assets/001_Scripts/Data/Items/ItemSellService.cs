using System;

namespace _001_Scripts.Data.Items
{
    public readonly struct ItemSaleResult
    {
        public readonly ItemBase Item;
        public readonly int Amount;
        public readonly int TotalPrice;
        public ItemSaleResult(ItemBase item, int amount, int totalPrice) { Item = item; Amount = amount; TotalPrice = totalPrice; }
    }

    /// <summary>화폐 시스템은 콜백으로 주입합니다. 판매 규칙은 아이템 도메인에만 둡니다.</summary>
    public interface IItemSellService
    {
        bool TrySell(ItemBase item, int amount, out ItemSaleResult result);
    }

    public sealed class ItemSellService : IItemSellService
    {
        private readonly IItemContainer container;
        private readonly Action<int> addCurrency;
        public ItemSellService(IItemContainer container, Action<int> addCurrency)
        { this.container = container ?? throw new ArgumentNullException(nameof(container)); this.addCurrency = addCurrency ?? throw new ArgumentNullException(nameof(addCurrency)); }

        public bool TrySell(ItemBase item, int amount, out ItemSaleResult result)
        {
            result = default;
            if (item == null || amount <= 0 || container.GetAmount(item) < amount) return false;
            var totalPrice = checked(item.BaseSellPrice * amount);
            if (!container.TryRemove(item, amount)) return false;
            addCurrency(totalPrice);
            result = new ItemSaleResult(item, amount, totalPrice); return true;
        }
    }
}
