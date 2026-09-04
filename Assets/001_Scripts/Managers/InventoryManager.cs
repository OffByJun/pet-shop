using System;
using System.Collections.Generic;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Managers.Interfaces;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>보유 아이템과 화폐를 관리하고 획득, 판매, 구매를 처리합니다.</summary>
    public sealed class InventoryManager : ServiceManagerBase<InventoryManager>, IInventoryService
    {
        [SerializeField, Min(1)] private int capacity = 24;
        [SerializeField, Min(0)] private int balance;

        private readonly List<ItemStack> stacks = new List<ItemStack>();
        private bool selling;

        protected override void ProvideServices()
        {
            Provide<IInventoryService>();
        }

        protected override void SubscribeGamePipes()
        {
            Listen<InventoryAvailableQuery>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(true);
            });
            Listen<GrantItemRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(TryGrant(request.Item, request.Amount));
            });
            Listen<SellItemRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                var success = TrySell(request.Item, request.Amount, out var result);
                request.Reply.Complete(success, result);
            });
            Listen<CurrencyBalanceQuery>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(true, Balance);
            });
            Listen<CreditCurrencyRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                if (request.Amount < 0 || balance > int.MaxValue - request.Amount) { request.Reply.Complete(false); return; }
                Add(request.Amount);
                request.Reply.Complete(true);
            });
            Listen<CanPurchaseQuery>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(CanPurchase(request.Quote));
            });
            Listen<PurchaseRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(TryPurchase(request.Quote));
            });
        }

        public IReadOnlyList<ItemStack> Stacks => stacks;
        public int Capacity => capacity;
        public int Balance => balance;

        public bool TryGrant(ItemBase item, int amount) => TryAdd(item, amount);

        public bool TrySell(ItemBase item, int amount, out ItemSaleResult result)
        {
            result = default;
            if (selling || item == null || amount <= 0 || GetAmount(item) < amount) return false;
            var totalPrice = checked(item.BaseSellPrice * amount);
            if (totalPrice < 0 || balance > int.MaxValue - totalPrice) return false;

            selling = true;
            try
            {
                if (!RemoveItems(item, amount)) return false;
                Add(totalPrice);
                result = new ItemSaleResult(item, amount, totalPrice);
                GamePipe.Publish(new InventoryChanged(new ItemStack(item, -amount), GetAmount(item)));
                GamePipe.Publish(new ItemSold(result));
                return true;
            }
            finally
            {
                selling = false;
            }
        }

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
                stack.Amount += added;
                stacks[i] = stack;
                remaining -= added;
            }
            while (remaining > 0 && stacks.Count < capacity)
            {
                var added = Mathf.Min(item.MaxStackSize, remaining); var stack = new ItemStack(item, added);
                stacks.Add(stack);
                remaining -= added;
            }
            GamePipe.Publish(new InventoryChanged(new ItemStack(item, amount), GetAmount(item)));
            return remaining == 0;
        }

        public bool TryRemove(ItemBase item, int amount)
        {
            if (!RemoveItems(item, amount)) return false;
            GamePipe.Publish(new InventoryChanged(new ItemStack(item, -amount), GetAmount(item)));
            return true;
        }

        private bool RemoveItems(ItemBase item, int amount)
        {
            if (item == null || amount <= 0 || GetAmount(item) < amount) return false;
            var remaining = amount;
            for (var i = stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (stacks[i].Item != item) continue;
                var stack = stacks[i]; var removed = Mathf.Min(stack.Amount, remaining);
                stack.Amount -= removed; remaining -= removed;
                if (stack.Amount == 0) stacks.RemoveAt(i); else stacks[i] = stack;
            }
            return true;
        }

        public bool CanPurchase(ExpenseQuote quote) => CanSpend(quote.Cost);

        public bool TryPurchase(ExpenseQuote quote)
        {
            if (!TrySpend(quote.Cost)) return false;
            GamePipe.Publish(new PurchaseCompleted(quote));
            return true;
        }

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;
            balance = checked(balance + amount);
            GamePipe.Publish(new CurrencyChanged(balance, amount));
        }

        public bool CanSpend(int amount) => amount >= 0 && balance >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount)) return false;
            if (amount == 0) return true;
            balance -= amount;
            GamePipe.Publish(new CurrencyChanged(balance, -amount));
            return true;
        }

        private void OnValidate()
        {
            capacity = Mathf.Max(1, capacity);
            balance = Mathf.Max(0, balance);
        }
    }
}
