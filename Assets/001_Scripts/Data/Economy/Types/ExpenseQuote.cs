using System;

namespace _001_Scripts.Data.Economy
{

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

}
