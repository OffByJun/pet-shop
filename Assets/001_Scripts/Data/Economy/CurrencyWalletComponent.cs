using System;
using _001_Scripts.Core;
using UnityEngine;

namespace _001_Scripts.Data.Economy
{
    /// <summary>씬과 저장 시스템에서 연결할 수 있는 화폐 지갑 컴포넌트입니다.</summary>
    public sealed class CurrencyWalletComponent : GameBehaviour, ICurrencyWallet
    {
        [SerializeField, Min(0)] private int balance;

        public int Balance => balance;
        public event Action<int, int> BalanceChanged;

        public void Add(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0) return;
            balance = checked(balance + amount);
            BalanceChanged?.Invoke(balance, amount);
        }

        public bool CanSpend(int amount) => amount >= 0 && balance >= amount;

        public bool TrySpend(int amount)
        {
            if (!CanSpend(amount)) return false;
            if (amount == 0) return true;
            balance -= amount;
            BalanceChanged?.Invoke(balance, -amount);
            return true;
        }

        private void OnValidate() => balance = Mathf.Max(0, balance);
    }
}
