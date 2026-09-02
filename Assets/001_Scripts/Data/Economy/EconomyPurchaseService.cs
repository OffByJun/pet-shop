using System;

namespace _001_Scripts.Data.Economy
{
    public enum ExpenseCategory
    {
        ToolUpgrade,
        StoreEquipment,
        StorageExpansion,
        StoreExpansion,
        FinalSettlement
    }

    public readonly struct ExpenseQuote
    {
        public string ExpenseId { get; }
        public ExpenseCategory Category { get; }
        public int Cost { get; }

        public ExpenseQuote(string expenseId, ExpenseCategory category, int cost)
        {
            if (string.IsNullOrWhiteSpace(expenseId)) throw new ArgumentException("Expense id is required.", nameof(expenseId));
            if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
            ExpenseId = expenseId;
            Category = category;
            Cost = cost;
        }
    }

    public interface IEconomyPurchaseService
    {
        bool CanPurchase(ExpenseQuote quote);
        bool TryPurchase(ExpenseQuote quote);
    }

    /// <summary>비용 지불만 담당합니다. 해금과 엔딩 조건 판정은 호출한 성장 시스템의 책임입니다.</summary>
    public sealed class EconomyPurchaseService : IEconomyPurchaseService
    {
        private readonly ICurrencyWallet wallet;

        public event Action<ExpenseQuote> Purchased;

        public EconomyPurchaseService(ICurrencyWallet wallet)
            => this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

        public bool CanPurchase(ExpenseQuote quote) => wallet.CanSpend(quote.Cost);

        public bool TryPurchase(ExpenseQuote quote)
        {
            if (!wallet.TrySpend(quote.Cost)) return false;
            Purchased?.Invoke(quote);
            return true;
        }
    }
}
