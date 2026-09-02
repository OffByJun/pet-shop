using System;
using System.Collections.Generic;
using _001_Scripts.Core;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Economy;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;
using UnityEngine;

namespace _001_Scripts.Data.Business
{
    /// <summary>손님 수를 기준으로 하루의 순차 서비스 흐름을 관리합니다.</summary>
    public sealed class BusinessDayService : GameBehaviour
    {
        [SerializeField] private BusinessDaySettings settings;
        [SerializeField] private ServiceOrderService orderService;
        [SerializeField] private PetToolService toolService;
        [Tooltip("ICurrencyWallet을 구현한 화폐 컴포넌트")]
        [SerializeField] private MonoBehaviour walletProvider;

        private readonly List<ServiceOrder> orders = new List<ServiceOrder>();
        private readonly List<ServiceOrderCompletion> completions = new List<ServiceOrderCompletion>();
        private IServiceOrderRandom random = new UnityServiceOrderRandom();
        private PetToolInteractionSession activeToolSession;
        private int currentCustomerIndex = -1;
        private int byproductRevenue;
        private ICurrencyWallet wallet;

        public BusinessDayStatus Status { get; private set; } = BusinessDayStatus.Closed;
        public int DayNumber { get; private set; }
        public int TotalCustomers => orders.Count;
        public int CurrentCustomerNumber => currentCustomerIndex < 0 ? 0 : currentCustomerIndex + 1;
        public ServiceOrder CurrentOrder => currentCustomerIndex >= 0 && currentCustomerIndex < orders.Count ? orders[currentCustomerIndex] : null;
        public PetInstance CurrentPet { get; private set; }
        public PetToolInteractionSession ActiveToolSession => activeToolSession;
        public bool CanSellByproducts => Status == BusinessDayStatus.EndOfDaySettlement;
        public IReadOnlyList<ServiceOrder> Orders => orders;

        public event Action<BusinessDayService> DayStarted;
        public event Action<ServiceOrder, int, int> CustomerArrived;
        public event Action<ServiceOrder, PetInstance> PetAccepted;
        public event Action<ServiceOrder, ServiceOrderCompletion, PetInstance> CustomerServiceCompleted;
        public event Action<BusinessDaySummary> EndOfDaySettlementStarted;
        public event Action<ItemSaleResult> ByproductSold;
        public event Action<BusinessDaySummary> DayEnded;

        private void Awake() => wallet = walletProvider as ICurrencyWallet;

        public bool StartBusiness()
        {
            if (Status != BusinessDayStatus.Closed || settings == null || orderService == null) return false;
            orders.Clear();
            completions.Clear();
            activeToolSession = null;
            CurrentPet = null;
            currentCustomerIndex = 0;
            byproductRevenue = 0;
            DayNumber++;

            var count = random.Range(settings.MinimumCustomers, settings.MaximumCustomers + 1);
            for (var i = 0; i < count; i++) orders.Add(orderService.CreateOrder());
            Status = BusinessDayStatus.CustomerArrived;
            DayStarted?.Invoke(this);
            RaiseCustomerArrived();
            return true;
        }

        public bool AcceptCurrentPet(PetInstance pet)
        {
            if (Status != BusinessDayStatus.CustomerArrived || CurrentOrder == null || pet == null) return false;
            if (pet.Variant != CurrentOrder.Pet) return false;
            CurrentPet = pet;
            Status = BusinessDayStatus.PetInCare;
            PetAccepted?.Invoke(CurrentOrder, pet);
            return true;
        }

        public bool TryApplyCare(PetCareAction action, out PetCareResult result)
        {
            result = null;
            return Status == BusinessDayStatus.PetInCare &&
                   orderService != null &&
                   orderService.TryApplyCare(CurrentOrder, CurrentPet, action, out result);
        }

        public bool TryBeginTool(
            PetToolDefinition tool,
            ServiceRequestState request,
            out PetToolInteractionSession session)
        {
            session = null;
            if (Status != BusinessDayStatus.PetInCare || toolService == null || activeToolSession != null) return false;
            if (!toolService.TryBegin(tool, CurrentOrder, CurrentPet, request, out session)) return false;
            activeToolSession = session;
            return true;
        }

        public bool ApplyToolInput(PetToolInteractionMode mode, float normalizedAmount)
            => Status == BusinessDayStatus.PetInCare &&
               activeToolSession != null &&
               toolService.ApplyInput(activeToolSession, mode, normalizedAmount);

        public bool TryCompleteTool(out PetToolUseResult result)
        {
            result = null;
            if (Status != BusinessDayStatus.PetInCare || activeToolSession == null) return false;
            if (!toolService.TryComplete(activeToolSession, out result)) return false;
            activeToolSession = null;
            return true;
        }

        public void CancelActiveTool()
        {
            if (activeToolSession == null) return;
            toolService?.Cancel(activeToolSession);
            activeToolSession = null;
        }

        /// <summary>플레이어가 완료 버튼을 눌렀을 때 호출합니다.</summary>
        public bool TryCompleteCurrentService(out ServiceOrderCompletion completion)
        {
            completion = null;
            if (Status != BusinessDayStatus.PetInCare || CurrentOrder == null || activeToolSession != null) return false;
            if (!orderService.TryFinalize(CurrentOrder, out completion)) return false;
            completions.Add(completion);
            wallet?.Add(completion.Reward.Currency);
            var completedPet = CurrentPet;
            Status = BusinessDayStatus.CustomerSettlement;
            CustomerServiceCompleted?.Invoke(CurrentOrder, completion, completedPet);
            CurrentPet = null;
            return true;
        }

        /// <summary>현재 손님 정산 화면을 닫고 다음 손님 또는 일일 정산으로 이동합니다.</summary>
        public bool ContinueAfterCustomerSettlement()
        {
            if (Status != BusinessDayStatus.CustomerSettlement) return false;
            currentCustomerIndex++;
            if (currentCustomerIndex < orders.Count)
            {
                Status = BusinessDayStatus.CustomerArrived;
                RaiseCustomerArrived();
            }
            else
            {
                Status = BusinessDayStatus.EndOfDaySettlement;
                EndOfDaySettlementStarted?.Invoke(BuildSummary());
            }
            return true;
        }

        public bool TrySellByproduct(IItemSellService sellService, ItemBase item, int amount, out ItemSaleResult result)
        {
            result = default;
            if (!CanSellByproducts || sellService == null || !sellService.TrySell(item, amount, out result)) return false;
            byproductRevenue = checked(byproductRevenue + result.TotalPrice);
            ByproductSold?.Invoke(result);
            return true;
        }

        public bool TrySellByproduct(IItemContainer inventory, ItemBase item, int amount, out ItemSaleResult result)
        {
            result = default;
            if (inventory == null || wallet == null) return false;
            return TrySellByproduct(new ItemSellService(inventory, wallet.Add), item, amount, out result);
        }

        public bool FinishDay(out BusinessDaySummary summary)
        {
            summary = null;
            if (Status != BusinessDayStatus.EndOfDaySettlement) return false;
            summary = BuildSummary();
            Status = BusinessDayStatus.Closed;
            currentCustomerIndex = -1;
            DayEnded?.Invoke(summary);
            return true;
        }

        public BusinessDaySummary BuildSummary()
            => new BusinessDaySummary(DayNumber, orders.Count, completions, byproductRevenue);

        public void SetRandom(IServiceOrderRandom source) => random = source ?? throw new ArgumentNullException(nameof(source));
        public void SetWallet(ICurrencyWallet value) => wallet = value ?? throw new ArgumentNullException(nameof(value));

        private void RaiseCustomerArrived() => CustomerArrived?.Invoke(CurrentOrder, CurrentCustomerNumber, TotalCustomers);
    }
}
