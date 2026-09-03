using System;

namespace _001_Scripts.Data.Economy
{
    public interface IEconomyPurchaseService
    {
        bool CanPurchase(ExpenseQuote quote);
        bool TryPurchase(ExpenseQuote quote);
    }
}
