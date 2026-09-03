using System;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.Pipes.Pipes;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;

namespace _001_Scripts.Core.Pipes
{
    /// <summary>기존 MessagePipeHub를 사용하는 게임 내부 요청/알림 진입점입니다. Unity 메인 스레드에서 호출합니다.</summary>
    public static class GamePipe
    {
        private static MessagePipeHub Prepare<T>() where T : struct, IPipeMsg
        {
            if (MessagePipeHub.IsShuttingDown) return null;
            MessagePipeHub.EnsureInstance();
            var hub = MessagePipeHub.Instance;
            if (hub != null) hub.Register<T>();
            return hub;
        }

        public static bool Publish<T>(in T message) where T : struct, IPipeMsg
        {
            var hub = Prepare<T>();
            if (hub == null) return false;
            hub.Publish(in message);
            return true;
        }

        public static IDisposable Subscribe<T>(Action<T> handler) where T : struct, IPipeMsg
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            return Prepare<T>()?.Subscribe(handler);
        }

        private static bool Send<TRequest, TResult>(in TRequest request, PipeReply<TResult> reply, out TResult result)
            where TRequest : struct, IPipeMsg
        {
            Publish(in request);
            result = reply.Value;
            return reply.Completed && reply.Succeeded;
        }

        public static bool TryCreateOrder(CustomerTypeDefinition customer, bool careRoom, out ServiceOrder result)
        {
            var reply = new PipeReply<ServiceOrder>();
            return Send(new CreateOrderRequest(customer, careRoom, reply), reply, out result);
        }

        public static bool TryApplyOrderCare(ServiceOrder order, PetInstance pet, PetCareAction action, out PetCareResult result)
        {
            var reply = new PipeReply<PetCareResult>();
            return Send(new ApplyOrderCareRequest(order, pet, action, reply), reply, out result);
        }

        public static bool TryFinalizeOrder(ServiceOrder order, out ServiceOrderCompletion result)
        {
            var reply = new PipeReply<ServiceOrderCompletion>();
            return Send(new FinalizeOrderRequest(order, reply), reply, out result);
        }

        public static bool TryGrantItem(ItemBase item, int amount)
        {
            var reply = new PipeReply<bool>();
            return Send(new GrantItemRequest(item, amount, reply), reply, out _);
        }

        public static bool TrySellItem(ItemBase item, int amount, out ItemSaleResult result)
        {
            var reply = new PipeReply<ItemSaleResult>();
            return Send(new SellItemRequest(item, amount, reply), reply, out result);
        }

        public static bool HasInventory()
        {
            var reply = new PipeReply<bool>();
            return Send(new InventoryAvailableQuery(reply), reply, out _);
        }

        public static bool TryGetBalance(out int result)
        {
            var reply = new PipeReply<int>();
            return Send(new CurrencyBalanceQuery(reply), reply, out result);
        }

        public static bool TryCreditCurrency(int amount)
        {
            var reply = new PipeReply<bool>();
            return Send(new CreditCurrencyRequest(amount, reply), reply, out _);
        }

        public static bool CanPurchase(ExpenseQuote quote)
        {
            var reply = new PipeReply<bool>();
            return Send(new CanPurchaseQuery(quote, reply), reply, out _);
        }

        public static bool TryPurchase(ExpenseQuote quote)
        {
            var reply = new PipeReply<bool>();
            return Send(new PurchaseRequest(quote, reply), reply, out _);
        }

        public static bool IsContentUnlocked(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId)) return true;
            var reply = new PipeReply<bool>();
            return Send(new ContentUnlockedQuery(contentId, reply), reply, out _);
        }

        public static bool TryUnlock(ProgressionUnlockDefinition definition)
        {
            var reply = new PipeReply<bool>();
            return Send(new UnlockProgressionRequest(definition, reply), reply, out _);
        }

        public static bool TryCompleteEnding(SettlementGoalDefinition goal)
        {
            var reply = new PipeReply<bool>();
            return Send(new CompleteEndingRequest(goal, reply), reply, out _);
        }

    }

}
