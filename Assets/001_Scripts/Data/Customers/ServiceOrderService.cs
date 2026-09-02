using System;
using _001_Scripts.Core;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;
using _001_Scripts.Data.Progression;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    /// <summary>주문 생성, 케어 반영, 결과 확정을 묶는 게임플레이 진입점입니다.</summary>
    public sealed class ServiceOrderService : GameBehaviour
    {
        [SerializeField] private ServiceOrderCatalog catalog;
        [SerializeField] private PetCareService petCareService;
        [Tooltip("IServiceOrderEconomy를 구현한 경제 컴포넌트 또는 ScriptableObject")]
        [SerializeField] private UnityEngine.Object economyProvider;

        private ServiceOrderGenerator generator;
        private IServiceOrderEconomy economy;
        private IProgressionContentAccess progressionAccess;

        public event Action<ServiceOrder> OrderCreated;
        public event Action<ServiceOrder, int> OrderProgressed;
        public event Action<ServiceOrder, ServiceOrderCompletion> OrderFinalized;

        private void Awake()
        {
            if (catalog != null) generator = new ServiceOrderGenerator(catalog, null, progressionAccess);
            if (economy == null) economy = economyProvider as IServiceOrderEconomy;
        }

        public void SetEconomy(IServiceOrderEconomy service) => economy = service;
        public void SetProgressionAccess(IProgressionContentAccess access)
        {
            progressionAccess = access;
            generator = catalog == null ? null : new ServiceOrderGenerator(catalog, null, progressionAccess);
        }

        public ServiceOrder CreateOrder(CustomerTypeDefinition customer = null)
        {
            if (generator == null)
            {
                if (catalog == null) throw new InvalidOperationException("ServiceOrderCatalog is not assigned.");
                generator = new ServiceOrderGenerator(catalog, null, progressionAccess);
            }
            var order = generator.CreateOrder(customer);
            OrderCreated?.Invoke(order);
            return order;
        }

        public bool TryApplyCare(ServiceOrder order, PetInstance pet, PetCareAction action, out PetCareResult careResult)
        {
            careResult = null;
            if (order == null || order.IsFinalized || pet == null || pet.Variant != order.Pet || petCareService == null) return false;
            if (PetToolCapabilityMap.FromCareAction(action) != PetToolCapability.None) return false;
            if (!petCareService.TryCare(pet, action, out careResult)) return false;
            var resolved = order.ApplyCare(action);
            OrderProgressed?.Invoke(order, resolved);
            return true;
        }

        public bool TryFinalize(ServiceOrder order, out ServiceOrderCompletion completion)
        {
            completion = null;
            if (order == null || economy == null) return false;
            completion = order.Finalize(economy);
            OrderFinalized?.Invoke(order, completion);
            return true;
        }
    }
}
