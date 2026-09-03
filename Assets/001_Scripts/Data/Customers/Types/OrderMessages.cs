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
    public readonly struct CreateOrderRequest : IPipeMsg
    {
        public readonly CustomerTypeDefinition Customer;
        public readonly bool CareRoom;
        public readonly PipeReply<ServiceOrder> Reply;

        public CreateOrderRequest(CustomerTypeDefinition customer, bool careRoom, PipeReply<ServiceOrder> reply)
        {
            Customer = customer;
            CareRoom = careRoom;
            Reply = reply;
        }
    }

    public readonly struct ApplyOrderCareRequest : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly PetInstance Pet;
        public readonly PetCareAction Action;
        public readonly PipeReply<PetCareResult> Reply;

        public ApplyOrderCareRequest(ServiceOrder order, PetInstance pet, PetCareAction action, PipeReply<PetCareResult> reply)
        {
            Order = order;
            Pet = pet;
            Action = action;
            Reply = reply;
        }
    }

    public readonly struct FinalizeOrderRequest : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly PipeReply<ServiceOrderCompletion> Reply;

        public FinalizeOrderRequest(ServiceOrder order, PipeReply<ServiceOrderCompletion> reply)
        {
            Order = order;
            Reply = reply;
        }
    }

    public readonly struct OrderCreated : IPipeMsg
    {
        public readonly ServiceOrder Order;

        public OrderCreated(ServiceOrder order)
        {
            Order = order;
        }
    }

    public readonly struct OrderProgressed : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly int Resolved;

        public OrderProgressed(ServiceOrder order, int resolved)
        {
            Order = order;
            Resolved = resolved;
        }
    }

    public readonly struct OrderFinalized : IPipeMsg
    {
        public readonly ServiceOrder Order;
        public readonly ServiceOrderCompletion Completion;

        public OrderFinalized(ServiceOrder order, ServiceOrderCompletion completion)
        {
            Order = order;
            Completion = completion;
        }
    }
}
