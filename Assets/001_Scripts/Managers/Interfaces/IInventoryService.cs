using _001_Scripts.Core.Services;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;

namespace _001_Scripts.Managers.Interfaces
{
    /// <summary>
    /// 소지품과 지갑입니다. 이미 나뉘어 있던 작은 계약들을 하나로 묶어 등록만 담당합니다.
    /// 사용하는 쪽은 필요한 작은 계약(IItemContainer 등)에만 의존해도 됩니다. (ISP)
    /// </summary>
    public interface IInventoryService : IService,
        IItemContainer,
        IItemAcquisitionService,
        IItemSellService,
        ICurrencyWallet,
        IEconomyPurchaseService
    {
    }
}
