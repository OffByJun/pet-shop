using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.Pipes.Msgs
{
    public readonly struct CurrencyBalanceQuery : IPipeMsg
    {
        public readonly PipeReply<int> Reply;

        public CurrencyBalanceQuery(PipeReply<int> reply)
        {
            Reply = reply;
        }
    }

    public readonly struct CreditCurrencyRequest : IPipeMsg
    {
        public readonly int Amount;
        public readonly PipeReply<bool> Reply;

        public CreditCurrencyRequest(int amount, PipeReply<bool> reply)
        {
            Amount = amount;
            Reply = reply;
        }
    }

    public readonly struct CanPurchaseQuery : IPipeMsg
    {
        public readonly ExpenseQuote Quote;
        public readonly PipeReply<bool> Reply;

        public CanPurchaseQuery(ExpenseQuote quote, PipeReply<bool> reply)
        {
            Quote = quote;
            Reply = reply;
        }
    }

    public readonly struct PurchaseRequest : IPipeMsg
    {
        public readonly ExpenseQuote Quote;
        public readonly PipeReply<bool> Reply;

        public PurchaseRequest(ExpenseQuote quote, PipeReply<bool> reply)
        {
            Quote = quote;
            Reply = reply;
        }
    }

    public readonly struct CurrencyChanged : IPipeMsg
    {
        public readonly int Balance;
        public readonly int Delta;

        public CurrencyChanged(int balance, int delta)
        {
            Balance = balance;
            Delta = delta;
        }
    }

    public readonly struct PurchaseCompleted : IPipeMsg
    {
        public readonly ExpenseQuote Quote;

        public PurchaseCompleted(ExpenseQuote quote)
        {
            Quote = quote;
        }
    }
}
