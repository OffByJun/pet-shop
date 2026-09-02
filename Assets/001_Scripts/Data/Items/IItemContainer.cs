using System;
using System.Collections.Generic;

namespace _001_Scripts.Data.Items
{
    public interface IItemContainer
    {
        IReadOnlyList<ItemStack> Stacks { get; }
        event Action<ItemStack> ItemsChanged;
        int GetAmount(ItemBase item);
        bool TryAdd(ItemBase item, int amount);
        bool TryRemove(ItemBase item, int amount);
    }
}
