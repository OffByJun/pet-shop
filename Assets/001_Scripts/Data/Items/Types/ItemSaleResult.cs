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

}
