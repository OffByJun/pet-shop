using System;

namespace _001_Scripts.Data.Items
{
    [Serializable]
    public struct ItemStack
    {
        public ItemBase Item;
        public int Amount;
        public ItemStack(ItemBase item, int amount) { Item = item; Amount = amount; }
        public bool IsEmpty => Item == null || Amount <= 0;
        public int Space => Item == null ? 0 : Math.Max(0, Item.MaxStackSize - Amount);
    }
}
