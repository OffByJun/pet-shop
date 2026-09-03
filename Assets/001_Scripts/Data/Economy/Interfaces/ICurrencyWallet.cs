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

}
