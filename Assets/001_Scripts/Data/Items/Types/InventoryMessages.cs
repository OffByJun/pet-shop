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
    public readonly struct GrantItemRequest : IPipeMsg
    {
        public readonly ItemBase Item;
        public readonly int Amount;
        public readonly PipeReply<bool> Reply;

        public GrantItemRequest(ItemBase item, int amount, PipeReply<bool> reply)
        {
            Item = item;
            Amount = amount;
            Reply = reply;
        }
    }

    public readonly struct SellItemRequest : IPipeMsg
    {
        public readonly ItemBase Item;
        public readonly int Amount;
        public readonly PipeReply<ItemSaleResult> Reply;

        public SellItemRequest(ItemBase item, int amount, PipeReply<ItemSaleResult> reply)
        {
            Item = item;
            Amount = amount;
            Reply = reply;
        }
    }

    public readonly struct InventoryAvailableQuery : IPipeMsg
    {
        public readonly PipeReply<bool> Reply;

        public InventoryAvailableQuery(PipeReply<bool> reply)
        {
            Reply = reply;
        }
    }

    public readonly struct InventoryChanged : IPipeMsg
    {
        public readonly ItemStack Change;
        public readonly int TotalAmount;

        public InventoryChanged(ItemStack change, int totalAmount)
        {
            Change = change;
            TotalAmount = totalAmount;
        }
    }

    public readonly struct ItemSold : IPipeMsg
    {
        public readonly ItemSaleResult Result;

        public ItemSold(ItemSaleResult result)
        {
            Result = result;
        }
    }
}
