using System;
using System.Collections.Generic;
using _001_Scripts.Core.Entity;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data;
using _001_Scripts.Data.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _001_Scripts.Managers
{
    /// <summary>씬 이동과 하루 사이의 화면만 소유합니다. 주문/지갑/케어 규칙은 기존 서비스가 소유합니다.</summary>
    public sealed class ShopRoutineManager : SinManagerBase<ShopRoutineManager>
    {
        [SerializeField] private ShopRoutineSettings settings;
        [SerializeField] private GameManager game;
        [SerializeField] private InventoryManager inventory;
        private readonly HashSet<string> ownedDecorations = new HashSet<string>(StringComparer.Ordinal);
        private readonly ShopSupplyStock stock = new ShopSupplyStock();
        private readonly ShopReputation reputation = new ShopReputation();
        private readonly _001_Scripts.Data.Customers.CustomerRelationshipBook relationships =
            new _001_Scripts.Data.Customers.CustomerRelationshipBook();
        private bool loading;
        private bool purchasingDecoration;
        private bool purchasingSupply;
        private bool stockSeeded;
        private int settledDay = -1;
        public ShopRoutineSettings Settings => settings;
        public GameManager Game => game;
        public InventoryManager Inventory => inventory;
        public bool IsLoading => loading;
        public bool IsImproving { get; private set; }
        public DaySummary LastSummary { get; private set; }
        public ShopDecorationDefinition Decoration { get; private set; }
        public ShopSupplyStock Stock => stock;
        public ShopReputation Reputation => reputation;
        public ShopReputationTier ReputationTier => settings.TierFor(reputation.Points);
        public _001_Scripts.Data.Customers.CustomerRelationshipBook Relationships => relationships;

        /// <summary>오늘 넘겨야 하는 매출입니다.</summary>
        public int DailyGoal => settings.DailyGoalFor(Mathf.Max(1, game.DayNumber));
        /// <summary>지금까지 번 돈입니다. 정산 전에도 진행도를 보여 줍니다.</summary>
        public int DailyEarned => game.BuildSummary().TotalRevenue;
        public float DailyGoalProgress => DailyGoal <= 0 ? 1f : Mathf.Clamp01(DailyEarned / (float)DailyGoal);
        public bool DailyGoalMet => DailyEarned >= DailyGoal;
        /// <summary>마지막 정산에서 목표 미달로 낸 유지비입니다.</summary>
        public int LastMissFee { get; private set; }
        public bool LastGoalMet { get; private set; } = true;

        /// <summary>지금 손님과 쌓인 관계입니다.</summary>
        public _001_Scripts.Data.Customers.CustomerRelationship RelationshipWith(
            _001_Scripts.Data.Customers.CustomerBase customer) => relationships.For(customer);

        public _001_Scripts.Data.Customers.CustomerBondTier BondWith(
            _001_Scripts.Data.Customers.CustomerBase customer) => settings.BondFor(relationships.For(customer));
        public float CareSpeedMultiplier => Mathf.Max(.1f, 1f + Benefit(ProgressionBenefitType.ProcessingSpeed));

        protected override void SubscribeGamePipes()
        {
            // DayEnded only fires when the player leaves the settlement screen, so the numbers
            // themselves are settled the moment that screen opens (see Update).
            Listen<DayEnded>(message =>
            {
                LastSummary = message.Summary;
                IsImproving = true;
            });
            Listen<ProgressionUnlocked>(_ => ApplyBenefits());
        }

        private void Start()
        {
            SeedStock();
            ApplyBenefits();
        }

        /// <summary>정산 화면이 열리는 순간 하루 결과를 확정합니다. 화면과 숫자가 어긋나지 않게 합니다.</summary>
        private void Update()
        {
            if (game == null || game.Status != DayStatus.EndOfDaySettlement) return;
            if (settledDay == game.DayNumber) return;
            settledDay = game.DayNumber;
            var summary = game.BuildSummary();
            LastSummary = summary;
            reputation.Apply(summary, settings);
            SettleDailyGoal(summary);
        }

        /// <summary>새 게임의 첫 하루에만 기본 보급품을 채웁니다.</summary>
        private void SeedStock()
        {
            if (stockSeeded) return;
            stockSeeded = true;
            stock.Reset(settings.Supplies);
        }

        public bool StartDay()
        {
            SeedStock();
            if (loading || !game.StartBusiness(ReputationTier.ExtraCustomers)) return false;
            IsImproving = false;
            CareHandoffContext.Clear();
            Load(settings.ReceptionScene);
            return true;
        }

        public bool AcceptPet()
        {
            if (loading || game.Status != DayStatus.CustomerArrived || game.IsClosingTime) return false;
            var go = new GameObject("Current customer pet");
            go.transform.SetParent(transform);
            var pet = go.AddComponent<PetInstance>();
            pet.Initialize(game.CurrentOrder.Pet);
            game.World.Register(pet);
            if (game.AcceptCurrentPet(pet))
            {
                var order = game.CurrentOrder;
                if (order != null)
                {
                    var relationship = relationships.For(order.Customer);
                    relationship.RecordArrival();
                    order.RecordRelationshipTip(settings.BondFor(relationship).TipRatio);
                }
                return true;
            }
            Destroy(go);
            return false;
        }

        public void EnterCare()
        {
            if (!loading && game.Status == DayStatus.PetInCare) Load(settings.CareScene);
        }

        public bool ReturnPet() => !loading && game.TryReturnCurrentPet();

        public bool CollectPayment()
        {
            if (loading) return false;
            var pet = game.CurrentPet;
            var order = game.CurrentOrder;
            if (!game.TryCollectPayment(out _)) return false;
            if (order != null) relationships.For(order.Customer).RecordResult(order.Status);
            if (pet != null) { game.World.Unregister(pet); Destroy(pet.gameObject); }
            return true;
        }

        public bool ContinueAfterPayment()
        {
            if (loading || !game.ContinueAfterCustomerSettlement()) return false;
            CareHandoffContext.Clear();
            if (game.Status == DayStatus.CustomerArrived) Load(settings.ReceptionScene);
            return true;
        }

        /// <summary>목표 달성 여부를 평판과 유지비로 정산합니다.</summary>
        private void SettleDailyGoal(DaySummary summary)
        {
            var goal = settings.DailyGoalFor(Mathf.Max(1, summary.DayNumber));
            var earned = summary.TotalRevenue;
            LastGoalMet = earned >= goal;
            LastMissFee = 0;
            if (LastGoalMet)
            {
                reputation.Add(settings.GoalHitReputation);
                return;
            }
            reputation.Add(settings.GoalMissReputation);
            var fee = settings.MissFeeFor(goal - earned);
            // Never push the shop below zero; an empty till is punishment enough.
            LastMissFee = Mathf.Min(fee, inventory.Balance);
            if (LastMissFee > 0) inventory.TrySpend(LastMissFee);
        }

        /// <summary>예약 손님을 모두 맞이했다면 남은 영업 시간을 건너뛸 수 있습니다.</summary>
        public bool CanCloseEarly => !loading && game.Status == DayStatus.WaitingForClose;

        /// <summary>아직 손님이 남았는데 접는 경우입니다. 남은 예약은 모두 미응대가 됩니다.</summary>
        public bool CanGiveUpDay => !loading &&
            (game.Status == DayStatus.CustomerArrived || game.Status == DayStatus.WaitingForClose);

        /// <summary>오늘 못 받을 손님 수입니다. 접기 전에 대가를 보여 줄 때 씁니다.</summary>
        public int RemainingCustomers => Mathf.Max(0, game.TotalCustomers - game.CurrentCustomerNumber + 1);

        /// <summary>손님이 남아 있어도 하루를 접습니다. 남은 손님은 미응대로 처리됩니다.</summary>
        public bool GiveUpDay()
        {
            if (!CanGiveUpDay) return false;
            game.TickBusiness(Mathf.Max(1f, game.RemainingBusinessSeconds));
            return true;
        }

        /// <summary>남은 시간을 흘려보내고 바로 하루 정산으로 넘어갑니다.</summary>
        public bool CloseEarly()
        {
            if (!CanCloseEarly) return false;
            // The clock is what opens settlement, so run it out rather than forcing the status.
            game.TickBusiness(Mathf.Max(1f, game.RemainingBusinessSeconds));
            return true;
        }

        public bool FinishDay() => !loading && game.FinishDay(out _);

        /// <summary>영업을 접고 시작 메뉴로 돌아갑니다. 다음 시작은 완전히 새 하루로 시작합니다.</summary>
        public bool ReturnToMainMenu()
        {
            if (loading) return false;
            CareHandoffContext.Clear();
            var persistentRoot = transform.root.gameObject;
            loading = true;
            var operation = SceneManager.LoadSceneAsync(settings.MainMenuScene);
            operation.completed += _ => Destroy(persistentRoot);
            return true;
        }

        public void SellByproducts()
        {
            // A sale mutates the stack list; use a snapshot to keep iteration stable.
            var items = new List<_001_Scripts.Data.Items.ItemStack>(inventory.Stacks);
            foreach (var stack in items) game.TrySellByproduct(stack.Item, stack.Amount, out _);
        }

        /// <summary>보급품을 한 묶음 구매합니다. 개선 단계에서만 가능합니다.</summary>
        public bool PurchaseSupply(ShopSupplyDefinition supply)
        {
            if (!IsImproving || loading || purchasingSupply || supply == null) return false;
            var registered = false;
            foreach (var entry in settings.Supplies) if (entry == supply) registered = true;
            if (!registered) return false;
            purchasingSupply = true;
            try
            {
                if (!inventory.TryPurchase(supply.Quote)) return false;
                stock.Add(supply, supply.PackSize);
                return true;
            }
            finally { purchasingSupply = false; }
        }

        /// <summary>해당 케어를 지금 할 수 있을 만큼 보급품이 남아 있는지 확인합니다.</summary>
        public bool HasSupplyFor(RoutineCareRule rule) =>
            rule == null || rule.Supply == null || stock.Has(rule.Supply, rule.SupplyCost);

        /// <summary>케어 한 건을 끝낼 때 보급품을 소모합니다.</summary>
        public bool ConsumeSupplyFor(RoutineCareRule rule) =>
            rule == null || rule.Supply == null || stock.TryConsume(rule.Supply, rule.SupplyCost);

        public bool PurchaseUpgrade(ProgressionUnlockDefinition unlock)
            => IsImproving && !loading && game.TryUnlock(unlock);

        public bool Owns(ShopDecorationDefinition decoration)
            => decoration != null && ownedDecorations.Contains(decoration.DecorationId);

        public bool SelectDecoration(ShopDecorationDefinition decoration)
        {
            if (!IsImproving || loading || purchasingDecoration || decoration == null) return false;
            var registered = false;
            foreach (var entry in settings.Decorations) if (entry == decoration) registered = true;
            if (!registered) return false;
            purchasingDecoration = true;
            try
            {
                if (!Owns(decoration))
                {
                    if (!inventory.TryPurchase(decoration.Quote)) return false;
                    ownedDecorations.Add(decoration.DecorationId);
                }
                Decoration = decoration;
                return true;
            }
            finally { purchasingDecoration = false; }
        }

        private float Benefit(ProgressionBenefitType type)
        {
            var value = 0f;
            if (game.ProgressionCatalog == null) return value;
            foreach (var unlock in game.ProgressionCatalog.Unlocks)
                if (unlock != null && game.State.IsUnlocked(unlock.UnlockId))
                    foreach (var benefit in unlock.Benefits) if (benefit.Type == type) value += benefit.Value;
            return value;
        }

        private void ApplyBenefits() => inventory.SetCapacity(settings.BaseStorageCapacity + Mathf.RoundToInt(Benefit(ProgressionBenefitType.StorageCapacity)));

        private void Load(string scene)
        {
            loading = true;
            try
            {
                var operation = SceneManager.LoadSceneAsync(scene);
                operation.completed += _ => loading = false;
            }
            catch { loading = false; throw; }
        }
    }
}
