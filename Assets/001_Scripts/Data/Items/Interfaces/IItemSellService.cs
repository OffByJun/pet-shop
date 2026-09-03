using System;

namespace _001_Scripts.Data.Items
{
    /// <summary>화폐 시스템은 콜백으로 주입합니다. 판매 규칙은 아이템 도메인에만 둡니다.</summary>
    public interface IItemSellService
    {
        bool TrySell(ItemBase item, int amount, out ItemSaleResult result);
    }
}
