using System;

namespace _001_Scripts.Data.Economy
{
    public interface ICurrencyWallet
    {
        int Balance { get; }
        void Add(int amount);
        bool CanSpend(int amount);
        bool TrySpend(int amount);
    }

    /// <summary>저장 방식과 무관한 런타임 화폐 지갑입니다.</summary>
    public sealed class CurrencyWallet : ICurrencyWallet
    {
        public int Balance { get; private set; }
        public event Action<int, int> BalanceChanged;

        public CurrencyWallet(int initialBalance = 0)
        {
            if (initialBalance < 0) throw new ArgumentOutOfRangeException(nameof(initialBalance));
            Balance = initialBalance;
        }

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;
            Balance = checked(Balance + amount);
            BalanceChanged?.Invoke(Balance, amount);
        }

        public bool CanSpend(int amount) => amount >= 0 && Balance >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount)) return false;
            if (amount == 0) return true;
            Balance -= amount;
            BalanceChanged?.Invoke(Balance, -amount);
            return true;
        }
    }
}
