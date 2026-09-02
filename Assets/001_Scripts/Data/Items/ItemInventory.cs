using System;
using System.Collections.Generic;
using _001_Scripts.Core;
using UnityEngine;

namespace _001_Scripts.Data.Items
{
    /// <summary>런타임 수량만 관리합니다. ItemBase asset 자체를 변경하지 않습니다.</summary>
    public sealed class ItemInventory : GameBehaviour, IItemContainer
    {
        [SerializeField, Min(1)] private int capacity = 24;
        private readonly List<ItemStack> stacks = new List<ItemStack>();
        public IReadOnlyList<ItemStack> Stacks => stacks;
        public int Capacity => capacity;
        public event Action<ItemStack> ItemsChanged;

        public int GetAmount(ItemBase item)
        {
            if (item == null) return 0;
            var amount = 0;
            for (var i = 0; i < stacks.Count; i++) if (stacks[i].Item == item) amount += stacks[i].Amount;
            return amount;
        }

        public bool TryAdd(ItemBase item, int amount)
        {
            if (item == null || amount <= 0) return false;
            var freeInExistingStacks = 0;
            for (var i = 0; i < stacks.Count; i++)
                if (stacks[i].Item == item) freeInExistingStacks += stacks[i].Space;
            var missing = Mathf.Max(0, amount - freeInExistingStacks);
            var requiredNewStacks = Mathf.CeilToInt(missing / (float)item.MaxStackSize);
            if (stacks.Count + requiredNewStacks > capacity) return false;

            var remaining = amount;
            for (var i = 0; i < stacks.Count && remaining > 0; i++)
            {
                if (stacks[i].Item != item || stacks[i].Space <= 0) continue;
                var stack = stacks[i]; var added = Mathf.Min(stack.Space, remaining);
                stack.Amount += added; stacks[i] = stack; remaining -= added; ItemsChanged?.Invoke(stack);
            }
            while (remaining > 0 && stacks.Count < capacity)
            {
                var added = Mathf.Min(item.MaxStackSize, remaining); var stack = new ItemStack(item, added);
                stacks.Add(stack); remaining -= added; ItemsChanged?.Invoke(stack);
            }
            return remaining == 0;
        }

        public bool TryRemove(ItemBase item, int amount)
        {
            if (item == null || amount <= 0 || GetAmount(item) < amount) return false;
            var remaining = amount;
            for (var i = stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (stacks[i].Item != item) continue;
                var stack = stacks[i]; var removed = Mathf.Min(stack.Amount, remaining);
                stack.Amount -= removed; remaining -= removed;
                if (stack.Amount == 0) stacks.RemoveAt(i); else stacks[i] = stack;
                ItemsChanged?.Invoke(new ItemStack(item, -removed));
            }
            return true;
        }

        private void OnValidate() => capacity = Mathf.Max(1, capacity);
    }
}
