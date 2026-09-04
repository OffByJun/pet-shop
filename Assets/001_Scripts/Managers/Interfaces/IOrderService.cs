using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Services;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;

namespace _001_Scripts.Managers.Interfaces
{
    /// <summary>손님 주문의 생성과 판정입니다. 접수 화면은 이 계약만 봅니다.</summary>
    public interface IOrderService : IService
    {
        bool CanCreateOrders { get; }

        ServiceOrder CreateOrder(CustomerTypeDefinition customer = null);
        ServiceOrder CreateNext();
        void Configure(ServiceOrderCatalog definition);
        void SetRandom(IServiceOrderRandom source);
        void SetEconomy(IServiceOrderEconomy service);
        bool TryApplyCare(ServiceOrder order, PetInstance pet, PetCareAction action, out PetCareResult careResult);
        bool TryFinalize(ServiceOrder order, out ServiceOrderCompletion completion);
    }
}
