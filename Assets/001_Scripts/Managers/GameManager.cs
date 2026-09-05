using System;
using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Core.World;
using _001_Scripts.Data;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Items;
using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;
using _001_Scripts.Managers.Interfaces;
using UnityEngine;

namespace _001_Scripts.Managers
{
    /// <summary>하루 진행, 손님 주문과 상점 진행도를 관리합니다.</summary>
    public sealed class GameManager : ServiceManagerBase<GameManager>, IDayService, IOrderService, IProgressionService
    {
        [SerializeField] private GameSettings settings;
        [SerializeField] private ShopRoutineSettings routineSettings;
        [SerializeField] private ServiceOrderCatalog orderCatalog;
        [SerializeField] private ProgressionCatalog progressionCatalog;
        [Tooltip("IServiceOrderEconomy를 구현한 경제 컴포넌트 또는 ScriptableObject")]
        [SerializeField] private UnityEngine.Object economyProvider;
        [SerializeField] private PetCareAction[] supportedActions =
        {
            PetCareAction.Wash, PetCareAction.Brush, PetCareAction.Treat,
            PetCareAction.Extract, PetCareAction.Trim, PetCareAction.Clip
        };
        [SerializeField, Min(1)] private int maximumGenerationAttempts = 20;

        private readonly List<ServiceOrder> orders = new List<ServiceOrder>();
        private readonly List<ServiceOrderCompletion> completions = new List<ServiceOrderCompletion>();
        private IServiceOrderRandom random = new UnityServiceOrderRandom();
        private ServiceOrderGenerator generator;
        private IServiceOrderEconomy economy;
        private PetToolInteractionSession activeToolSession;
        private int currentCustomerIndex = -1;
        private int byproductRevenue;
        private bool startingDay;
        private bool settlingCustomer;
        private bool sellingByproduct;
        private bool purchasingUnlock;
        private WorldContext world;

        public IWorldContext World => world;

        protected override void ProvideServices()
        {
            Provide<IDayService>();
            Provide<IOrderService>();
            Provide<IProgressionService>();
        }

        protected override void SubscribeGamePipes()
        {
            EnsureWorld();
            Listen<CreateOrderRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                if (!CanCreateOrders) { request.Reply.Complete(false); return; }
                request.Reply.Complete(true, request.CareRoom ? CreateNext() : CreateOrder(request.Customer));
            });
            Listen<ApplyOrderCareRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                var success = TryApplyCare(request.Order, request.Pet, request.Action, out var result);
                request.Reply.Complete(success, result);
            });
            Listen<FinalizeOrderRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                var success = TryFinalize(request.Order, out var result);
                request.Reply.Complete(success, result);
            });
            Listen<ContentUnlockedQuery>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(IsContentUnlocked(request.ContentId));
            });
            Listen<UnlockProgressionRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(TryUnlock(request.Definition));
            });
            Listen<CompleteEndingRequest>(request =>
            {
                if (request.Reply == null || !request.Reply.TryClaim()) return;
                request.Reply.Complete(TryCompleteEnding(request.Goal));
            });
            Listen<ToolInteractionCancelled>(message =>
            {
                if (ReferenceEquals(activeToolSession, message.Session)) activeToolSession = null;
            });
            world.Activate();
        }

        protected override void OnManagerAwake()
        {
            EnsureWorld();
            if (orderCatalog != null) generator = new ServiceOrderGenerator(orderCatalog, random, IsContentUnlocked, IsConditionSupported);
            if (economy == null) economy = economyProvider as IServiceOrderEconomy;
        }

        private void EnsureWorld()
        {
            world ??= new WorldContext(new PetWorldSystem());
        }

        private void Update()
        {
            world?.Tick(Time.deltaTime);
            TickBusiness(Time.deltaTime);
        }

        public float RemainingBusinessSeconds { get; private set; }
        public bool IsClosingTime => RemainingBusinessSeconds <= 0f;
        public ProgressionCatalog ProgressionCatalog => progressionCatalog;

        public void TickBusiness(float seconds)
        {
            if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds) ||
                Status == DayStatus.Closed || Status == DayStatus.EndOfDaySettlement) return;
            RemainingBusinessSeconds = Mathf.Max(0f, RemainingBusinessSeconds - seconds);
            if (IsClosingTime && (Status == DayStatus.CustomerArrived || Status == DayStatus.WaitingForClose))
                BeginDaySettlement();
        }

        private void BeginDaySettlement()
        {
            if (Status == DayStatus.EndOfDaySettlement) return;
            Status = DayStatus.EndOfDaySettlement;
            GamePipe.Publish(new DaySettlementStarted(BuildSummary()));
        }

        protected override void OnDisable()
        {
            world?.Deactivate();
            activeToolSession = null;
            base.OnDisable();
        }

        protected override void OnManagerDestroying() => world?.Dispose();

        public DayStatus Status { get; private set; } = DayStatus.Closed;
        public int DayNumber { get; private set; }
        public int TotalCustomers => orders.Count;
        public int CurrentCustomerNumber => currentCustomerIndex < 0 ? 0 : currentCustomerIndex + 1;
        public ServiceOrder CurrentOrder => currentCustomerIndex >= 0 && currentCustomerIndex < orders.Count ? orders[currentCustomerIndex] : null;
        public PetInstance CurrentPet { get; private set; }
        public PetToolInteractionSession ActiveToolSession => activeToolSession;
        public bool CanSellByproducts => Status == DayStatus.EndOfDaySettlement;
        public IReadOnlyList<ServiceOrder> Orders => orders;

        public bool StartBusiness() => StartBusiness(0);

        /// <summary>extraCustomers는 평판처럼 바깥에서 정해지는 손님 유입 보정입니다.</summary>
        public bool StartBusiness(int extraCustomers)
        {
            if (startingDay) return false;
            startingDay = true;
            try
            {
                if (Status != DayStatus.Closed || !CanCreateOrders) return false;
                var count = settings == null ? 5 : random.Range(settings.MinimumCustomers, settings.MaximumCustomers + 1);
                count = Mathf.Max(1, count + extraCustomers);
                var generatedOrders = new List<ServiceOrder>(count);
                for (var i = 0; i < count; i++)
                {
                    generatedOrders.Add(CreateNext());
                }
                orders.Clear();
                completions.Clear();
                activeToolSession = null;
                CurrentPet = null;
                currentCustomerIndex = 0;
                byproductRevenue = 0;
                RemainingBusinessSeconds = settings == null ? 300f : settings.BusinessDurationSeconds;
                DayNumber++;

                orders.AddRange(generatedOrders);
                Status = DayStatus.CustomerArrived;
                startingDay = false;
                GamePipe.Publish(new DayStarted(DayNumber, TotalCustomers));
                RaiseCustomerArrived();
                return true;
            }
            finally
            {
                startingDay = false;
            }
        }

        public bool AcceptCurrentPet(PetInstance pet)
        {
            if (Status != DayStatus.CustomerArrived || CurrentOrder == null || pet == null) return false;
            if (World == null || !World.Contains(pet) || pet.Variant != CurrentOrder.Pet) return false;
            CurrentPet = pet;
            Status = DayStatus.PetInCare;
            GamePipe.Publish(new PetAccepted(CurrentOrder, pet));
            return true;
        }

        public bool TryApplyCare(PetCareAction action, out PetCareResult result)
        {
            result = null;
            return Status == DayStatus.PetInCare &&
                   TryApplyCare(CurrentOrder, CurrentPet, action, out result);
        }

        public bool TryBeginTool(
            PetToolDefinition tool,
            ServiceRequestState request,
            out PetToolInteractionSession session)
        {
            session = null;
            if (Status != DayStatus.PetInCare || activeToolSession != null) return false;
            if (!World.TryBeginTool(tool, CurrentOrder, CurrentPet, request, out session)) return false;
            activeToolSession = session;
            return true;
        }

        public bool ApplyToolInput(PetToolInteractionMode mode, float normalizedAmount)
            => Status == DayStatus.PetInCare &&
               activeToolSession != null &&
               World.TryApplyToolInput(activeToolSession, mode, normalizedAmount);

        public bool TryCompleteTool(out PetToolUseResult result)
        {
            result = null;
            if (Status != DayStatus.PetInCare || activeToolSession == null) return false;
            if (!World.TryCompleteTool(activeToolSession, out result)) return false;
            activeToolSession = null;
            return true;
        }

        public void CancelActiveTool()
        {
            if (activeToolSession == null) return;
            World.CancelTool(activeToolSession);
            activeToolSession = null;
        }

        /// <summary>플레이어가 완료 버튼을 눌렀을 때 호출합니다.</summary>
        public bool TryCompleteCurrentService(out ServiceOrderCompletion completion)
        {
            completion = null;
            if (Status == DayStatus.PetInCare && !TryReturnCurrentPet()) return false;
            return TryCollectPayment(out completion);
        }

        public bool TryReturnCurrentPet()
        {
            if (Status != DayStatus.PetInCare || CurrentOrder == null || activeToolSession != null) return false;
            if (CurrentOrder.ResolvedRequiredCount < CurrentOrder.RequiredCount) return false;
            Status = DayStatus.AwaitingPayment;
            return true;
        }

        public bool TryCollectPayment(out ServiceOrderCompletion completion)
        {
            completion = null;
            if (settlingCustomer) return false;
            settlingCustomer = true;
            try
            {
                if (Status != DayStatus.AwaitingPayment || CurrentOrder == null || activeToolSession != null) return false;
                if (!TryFinalize(CurrentOrder, out completion)) return false;
                if (!GamePipe.TryCreditCurrency(completion.Reward.Currency)) return false;
                completions.Add(completion);
                var completedPet = CurrentPet;
                Status = DayStatus.CustomerSettlement;
                CurrentPet = null;
                settlingCustomer = false;
                GamePipe.Publish(new CustomerServiceCompleted(CurrentOrder, completion, completedPet));
                return true;
            }
            finally
            {
                settlingCustomer = false;
            }
        }

        /// <summary>현재 손님 정산 화면을 닫고 다음 손님 또는 일일 정산으로 이동합니다.</summary>
        public bool ContinueAfterCustomerSettlement()
        {
            if (Status != DayStatus.CustomerSettlement) return false;
            currentCustomerIndex++;
            if (IsClosingTime) BeginDaySettlement();
            else if (currentCustomerIndex < orders.Count)
            {
                Status = DayStatus.CustomerArrived;
                RaiseCustomerArrived();
            }
            else
            {
                Status = DayStatus.WaitingForClose;
            }
            return true;
        }

        public bool SkipCurrentCustomer()
        {
            if (Status != DayStatus.CustomerArrived) return false;
            Status = DayStatus.CustomerSettlement;
            return ContinueAfterCustomerSettlement();
        }

        public bool TrySellByproduct(ItemBase item, int amount, out ItemSaleResult result)
        {
            result = default;
            if (sellingByproduct) return false;
            sellingByproduct = true;
            try
            {
                if (!CanSellByproducts || item == null || amount <= 0) return false;
                var total = checked(byproductRevenue + checked(item.BaseSellPrice * amount));
                if (!GamePipe.TrySellItem(item, amount, out result)) return false;
                byproductRevenue = total;
                sellingByproduct = false;
                GamePipe.Publish(new ByproductSold(result));
                return true;
            }
            finally
            {
                sellingByproduct = false;
            }
        }
        public bool FinishDay(out DaySummary summary)
        {
            summary = null;
            if (sellingByproduct || Status != DayStatus.EndOfDaySettlement) return false;
            summary = BuildSummary();
            Status = DayStatus.Closed;
            currentCustomerIndex = -1;
            GamePipe.Publish(new DayEnded(summary));
            return true;
        }

        public DaySummary BuildSummary()
            => new DaySummary(DayNumber, orders.Count, completions, byproductRevenue);

        public void SetRandom(IServiceOrderRandom source)
        {
            random = source ?? throw new ArgumentNullException(nameof(source));
            generator = null;
        }

        private void RaiseCustomerArrived() => GamePipe.Publish(new CustomerArrived(CurrentOrder, CurrentCustomerNumber, TotalCustomers));

        // 손님 주문
        public bool CanCreateOrders => orderCatalog != null;

        public void SetEconomy(IServiceOrderEconomy service) => economy = service;

        public ServiceOrder CreateOrder(CustomerTypeDefinition customer = null)
        {
            if (generator == null)
            {
                if (orderCatalog == null) throw new InvalidOperationException("ServiceOrderCatalog is not assigned.");
                generator = new ServiceOrderGenerator(orderCatalog, random, IsContentUnlocked, IsConditionSupported,
                    routineSettings == null ? 3 : routineSettings.MinimumCareRequestsPerVisit,
                    routineSettings == null ? 5 : routineSettings.MaximumCareRequestsPerVisit);
            }
            var order = generator.CreateOrder(customer);
            GamePipe.Publish(new OrderCreated(order));
            return order;
        }

        public ServiceOrder CreateNext()
        {
            for (var attempt = 0; attempt < maximumGenerationAttempts; attempt++)
            {
                var order = CreateOrder();
                if (CanEnterCareRoom(order)) return order;
            }
            throw new InvalidOperationException("Could not generate an order supported by the care room.");
        }

        private bool CanEnterCareRoom(ServiceOrder order)
        {
            for (var i = 0; i < order.Requests.Count; i++)
                if (!IsConditionSupported(order.Requests[i].Condition)) return false;
            return true;
        }

        private bool IsConditionSupported(PetConditionDefinition condition)
        {
            if (routineSettings == null) return Array.IndexOf(supportedActions, condition.ResolvedBy) >= 0;
            var rule = routineSettings.FindCare(condition);
            return rule != null && rule.DomainTool != null && rule.DomainTool.CanProcess(condition);
        }

        public void Configure(ServiceOrderCatalog definition)
        {
            orderCatalog = definition;
            generator = null;
        }

        public bool TryApplyCare(ServiceOrder order, PetInstance pet, PetCareAction action, out PetCareResult careResult)
        {
            careResult = null;
            if (order == null || order.IsFinalized || pet == null || pet.Variant != order.Pet) return false;
            if (PetToolCapabilityMap.FromCareAction(action) != PetToolCapability.None) return false;
            if (!World.TryCarePet(pet, action, out careResult)) return false;
            var resolved = order.ApplyCare(action);
            GamePipe.Publish(new OrderProgressed(order, resolved));
            return true;
        }

        public bool TryFinalize(ServiceOrder order, out ServiceOrderCompletion completion)
        {
            completion = null;
            if (economy == null) economy = economyProvider as IServiceOrderEconomy;
            if (order == null || economy == null) return false;
            completion = order.Finalize(economy);
            GamePipe.Publish(new OrderFinalized(order, completion));
            return true;
        }

        // 상점 진행도
        public ProgressionState State { get; } = new ProgressionState();

        public ProgressionStageId CurrentStage
        {
            get
            {
                if (progressionCatalog == null) return ProgressionStageId.Early;
                var endings = progressionCatalog.EndingCandidates;
                for (var i = 0; i < endings.Count; i++)
                    if (endings[i] != null && State.IsEndingCompleted(endings[i].GoalId)) return ProgressionStageId.Final;
                var stage = ProgressionStageId.Early;
                var unlocks = progressionCatalog.Unlocks;
                for (var i = 0; i < unlocks.Count; i++)
                    if (unlocks[i] != null && State.IsUnlocked(unlocks[i].UnlockId) && unlocks[i].Stage > stage)
                        stage = unlocks[i].Stage;
                return stage;
            }
        }

        public bool IsContentUnlocked(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId)) return true;
            if (progressionCatalog == null) return false;
            var unlocks = progressionCatalog.Unlocks;
            for (var i = 0; i < unlocks.Count; i++)
            {
                var unlock = unlocks[i];
                if (unlock == null || !State.IsUnlocked(unlock.UnlockId)) continue;
                var benefits = unlock.Benefits;
                for (var j = 0; j < benefits.Count; j++)
                    if (benefits[j].Type == ProgressionBenefitType.ContentPool &&
                        string.Equals(benefits[j].ContentId, contentId, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public bool CanUnlock(ProgressionUnlockDefinition definition)
            => Status == DayStatus.Closed && BelongsToCatalog(definition) &&
               !State.IsUnlocked(definition.UnlockId) &&
               HasAllPrerequisites(definition.Prerequisites) &&
               GamePipe.CanPurchase(definition.Quote);

        public bool TryUnlock(ProgressionUnlockDefinition definition)
        {
            if (purchasingUnlock) return false;
            purchasingUnlock = true;
            try
            {
                if (!BelongsToCatalog(definition) || !CanUnlock(definition) || !GamePipe.TryPurchase(definition.Quote)) return false;
                if (!State.AddUnlock(definition.UnlockId)) return false;
                purchasingUnlock = false;
                GamePipe.Publish(new ProgressionUnlocked(definition));
                return true;
            }
            finally
            {
                purchasingUnlock = false;
            }
        }

        public bool CanCompleteEnding(SettlementGoalDefinition goal)
            => goal != null &&
               !State.IsEndingCompleted(goal.GoalId) &&
               HasAllPrerequisites(goal.RequiredUnlocks) &&
               GamePipe.CanPurchase(goal.Quote);

        public bool TryCompleteEnding(SettlementGoalDefinition goal)
        {
            if (purchasingUnlock) return false;
            purchasingUnlock = true;
            try
            {
                if (!IsEndingCandidate(goal) || !CanCompleteEnding(goal) || !GamePipe.TryPurchase(goal.Quote)) return false;
                if (!State.AddEnding(goal.GoalId)) return false;
                purchasingUnlock = false;
                GamePipe.Publish(new EndingReached(goal));
                return true;
            }
            finally
            {
                purchasingUnlock = false;
            }
        }

        private bool HasAllPrerequisites(IReadOnlyList<ProgressionUnlockDefinition> prerequisites)
        {
            for (var i = 0; i < prerequisites.Count; i++)
                if (prerequisites[i] == null || !State.IsUnlocked(prerequisites[i].UnlockId)) return false;
            return true;
        }

        private bool BelongsToCatalog(ProgressionUnlockDefinition definition)
        {
            if (definition == null || progressionCatalog == null) return false;
            var values = progressionCatalog.Unlocks;
            for (var i = 0; i < values.Count; i++) if (values[i] == definition) return true;
            return false;
        }

        private bool IsEndingCandidate(SettlementGoalDefinition goal)
        {
            if (goal == null || progressionCatalog == null) return false;
            var values = progressionCatalog.EndingCandidates;
            for (var i = 0; i < values.Count; i++) if (values[i] == goal) return true;
            return false;
        }
    }
}
