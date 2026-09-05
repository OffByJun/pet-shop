using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Progression;
using _001_Scripts.Data.Tools;
using UnityEngine;

namespace _001_Scripts.Data
{
    [CreateAssetMenu(menuName = "PetShop/Routine/Settings")]
    public sealed class ShopRoutineSettings : ScriptableObject
    {
        [SerializeField] private string mainMenuScene = "MainMenuScene";
        [SerializeField] private string routineScene = "ShopRoutineScene";
        [SerializeField] private string receptionScene = "CustomerReceptionScene";
        [SerializeField] private string careScene = "CarePlayScene";
        [SerializeField, Min(1)] private int baseStorageCapacity = 24;
        [SerializeField, Range(1f, 3f)] private float careDurationMultiplier = 1.35f;
        [SerializeField, Range(1, 5)] private int minimumCareRequestsPerVisit = 3;
        [SerializeField, Range(1, 5)] private int maximumCareRequestsPerVisit = 5;
        [SerializeField] private RoutineCareRule[] careRules = Array.Empty<RoutineCareRule>();
        [SerializeField] private ShopDecorationDefinition[] decorations = Array.Empty<ShopDecorationDefinition>();

        [Header("Supplies")]
        [Tooltip("가게에서 구매할 수 있는 보급품입니다.")]
        [SerializeField] private ShopSupplyDefinition[] supplies = Array.Empty<ShopSupplyDefinition>();

        [Header("Care grade")]
        [Tooltip("케어 중 쌓은 최고 콤보로 등급과 보상 배율을 정합니다. 높은 것부터 확인합니다.")]
        [SerializeField] private CareQualityTier[] careQualityTiers = Array.Empty<CareQualityTier>();

        [Header("Inspection minigame")]
        [Tooltip("하루 한 마리를 살펴볼 수 있는 진찰력입니다.")]
        [SerializeField, Min(1f)] private float inspectStamina = 100f;
        [Tooltip("반응이 시작되는 거리입니다. 스테이지 크기 기준 비율입니다.")]
        [SerializeField, Range(.05f, .8f)] private float inspectWarmRadius = .30f;
        [Tooltip("확신이 차오르기 시작하는 거리입니다.")]
        [SerializeField, Range(.01f, .4f)] private float inspectHotRadius = .085f;
        [Tooltip("돋보기를 1만큼 움직일 때 닳는 진찰력입니다.")]
        [SerializeField, Min(0f)] private float inspectStaminaCost = 26f;
        [Tooltip("뜨거운 자리에서 1초에 차오르는 확신입니다.")]
        [SerializeField, Min(.05f)] private float inspectConfidenceRate = 1.35f;
        [Tooltip("자리를 벗어나면 1초에 빠지는 확신입니다.")]
        [SerializeField, Min(0f)] private float inspectConfidenceDecay = .9f;

        [Header("Wash simulation")]
        [Tooltip("실제로 씻어낸 오염을 케어 진행도로 바꾸는 배율입니다.")]
        [SerializeField, Min(0f)] private float washProgressGain = 7f;
        [Tooltip("이 오염도 아래로 내려가야 세척을 끝낼 수 있습니다.")]
        [SerializeField, Range(0f, 1f)] private float washCleanThreshold = .18f;

        [Header("Daily goal")]
        [Tooltip("첫날 목표 매출입니다.")]
        [SerializeField, Min(0)] private int dailyGoalBase = 260;
        [Tooltip("하루가 지날 때마다 목표에 더해지는 금액입니다.")]
        [SerializeField, Min(0)] private int dailyGoalGrowth = 60;
        [Tooltip("목표를 넘겼을 때 받는 평판입니다.")]
        [SerializeField] private int goalHitReputation = 3;
        [Tooltip("목표에 못 미쳤을 때 깎이는 평판입니다.")]
        [SerializeField] private int goalMissReputation = -3;
        [Tooltip("목표 미달 시 부족분에 비례해 내는 유지비 비율입니다. .5면 부족분의 절반입니다.")]
        [SerializeField, Range(0f, 1f)] private float goalMissFeeRatio = .5f;

        [Header("Regulars")]
        [Tooltip("손님과 쌓인 관계 구간입니다. 방문 횟수가 많은 것부터 확인합니다.")]
        [SerializeField] private _001_Scripts.Data.Customers.CustomerBondTier[] bondTiers =
            Array.Empty<_001_Scripts.Data.Customers.CustomerBondTier>();

        [Header("Reputation")]
        [SerializeField] private int reputationPerPerfectOrder = 3;
        [SerializeField] private int reputationPerCompletedOrder = 1;
        [SerializeField] private int reputationPerFailedOrder = -2;
        [SerializeField] private int reputationPerUnservedOrder = -1;
        [Tooltip("점수가 높은 구간부터 차례로 확인합니다.")]
        [SerializeField] private ShopReputationTier[] reputationTiers = Array.Empty<ShopReputationTier>();
        public string MainMenuScene => mainMenuScene;
        public string RoutineScene => routineScene;
        public string ReceptionScene => receptionScene;
        public string CareScene => careScene;
        public int BaseStorageCapacity => baseStorageCapacity;
        public float CareDurationMultiplier => Mathf.Max(1f, careDurationMultiplier);
        public int MinimumCareRequestsPerVisit => Mathf.Clamp(minimumCareRequestsPerVisit, 1, 5);
        public int MaximumCareRequestsPerVisit => Mathf.Clamp(maximumCareRequestsPerVisit,
            MinimumCareRequestsPerVisit, 5);
        public IReadOnlyList<RoutineCareRule> CareRules => careRules;
        public IReadOnlyList<ShopDecorationDefinition> Decorations => decorations;
        public IReadOnlyList<ShopSupplyDefinition> Supplies => supplies;
        public int ReputationPerPerfectOrder => reputationPerPerfectOrder;
        public int ReputationPerCompletedOrder => reputationPerCompletedOrder;
        public int ReputationPerFailedOrder => reputationPerFailedOrder;
        public int ReputationPerUnservedOrder => reputationPerUnservedOrder;

        public float WashProgressGain => washProgressGain;
        public float WashCleanThreshold => washCleanThreshold;

        /// <summary>설정값으로 살펴보기 상태를 하나 만듭니다.</summary>
        public CareInspection CreateInspection() => new CareInspection(
            inspectStamina, inspectWarmRadius, inspectHotRadius,
            inspectStaminaCost, inspectConfidenceRate, inspectConfidenceDecay);

        public int GoalHitReputation => goalHitReputation;
        public int GoalMissReputation => goalMissReputation;

        /// <summary>n일차의 목표 매출입니다.</summary>
        public int DailyGoalFor(int dayNumber) =>
            dailyGoalBase + dailyGoalGrowth * Mathf.Max(0, dayNumber - 1);

        /// <summary>목표에 못 미친 만큼 내는 유지비입니다.</summary>
        public int MissFeeFor(int shortfall) =>
            shortfall <= 0 ? 0 : Mathf.RoundToInt(shortfall * goalMissFeeRatio);

        /// <summary>방문 기록에 맞는 관계 구간을 돌려줍니다.</summary>
        public _001_Scripts.Data.Customers.CustomerBondTier BondFor(
            _001_Scripts.Data.Customers.CustomerRelationship relationship)
        {
            var best = default(_001_Scripts.Data.Customers.CustomerBondTier);
            if (relationship == null) return best;
            var found = false;
            for (var i = 0; i < bondTiers.Length; i++)
            {
                var tier = bondTiers[i];
                if (relationship.Visits < tier.MinimumVisits) continue;
                if (relationship.HappyVisits < tier.MinimumHappyVisits) continue;
                if (found && tier.MinimumVisits <= best.MinimumVisits) continue;
                best = tier;
                found = true;
            }
            return best;
        }

        /// <summary>최고 콤보에 해당하는 케어 등급을 돌려줍니다.</summary>
        public CareQualityTier CareQualityFor(int bestCombo)
        {
            var best = default(CareQualityTier);
            var found = false;
            for (var i = 0; i < careQualityTiers.Length; i++)
            {
                var tier = careQualityTiers[i];
                if (bestCombo < tier.MinimumBestCombo) continue;
                if (found && tier.MinimumBestCombo <= best.MinimumBestCombo) continue;
                best = tier;
                found = true;
            }
            return found ? best : CareQualityTier.Default;
        }

        /// <summary>점수에 해당하는 평판 구간을 돌려줍니다.</summary>
        public ShopReputationTier TierFor(int points)
        {
            var best = default(ShopReputationTier);
            var found = false;
            for (var i = 0; i < reputationTiers.Length; i++)
            {
                var tier = reputationTiers[i];
                if (points < tier.MinimumPoints) continue;
                if (found && tier.MinimumPoints <= best.MinimumPoints) continue;
                best = tier;
                found = true;
            }
            return best;
        }
        public RoutineCareRule FindCare(PetConditionDefinition condition)
        {
            foreach (var rule in careRules) if (rule.Condition == condition) return rule;
            return null;
        }
    }

    /// <summary>케어 솜씨 등급입니다. 보상 배율과 정산 문구를 함께 정합니다.</summary>
    [Serializable]
    public struct CareQualityTier
    {
        [SerializeField] private string label;
        [Tooltip("이 콤보 이상이면 이 등급입니다.")]
        [SerializeField, Min(0)] private int minimumBestCombo;
        [Tooltip("1이면 기본 요금, 1.2면 20% 더 받습니다.")]
        [SerializeField, Min(0f)] private float payoutMultiplier;

        public string Label => string.IsNullOrWhiteSpace(label) ? "기본" : label;
        public int MinimumBestCombo => minimumBestCombo;
        public float PayoutMultiplier => payoutMultiplier <= 0f ? 1f : payoutMultiplier;

        public static CareQualityTier Default => new CareQualityTier();
    }

    [Serializable]
    public sealed class RoutineCareRule
    {
        [SerializeField] private PetConditionDefinition condition;
        [SerializeField] private CareKind presentationKind;
        [SerializeField] private Rect zone;
        [SerializeField] private CareToolKind tool;
        [SerializeField] private bool needsWater;
        [SerializeField, Min(1)] private float effort = 420f;
        [SerializeField, Min(1)] private float waterEffort = 380f;
        [SerializeField] private PetToolDefinition domainTool;
        [Header("Supply")]
        [Tooltip("이 케어를 한 건 끝낼 때 소모되는 보급품입니다. 비우면 소모하지 않습니다.")]
        [SerializeField] private ShopSupplyDefinition supply;
        [SerializeField, Min(0)] private int supplyCost = 1;
        public PetConditionDefinition Condition => condition;
        public ShopSupplyDefinition Supply => supply;
        public int SupplyCost => Mathf.Max(0, supplyCost);
        public CareToolKind Tool => tool;
        public PetToolDefinition DomainTool => domainTool;
        public float Effort => Mathf.Max(1f, effort);
        public float WaterEffort => Mathf.Max(1f, waterEffort);
        public CareConditionState CreateState() => new CareConditionState(condition.ConditionId,
            condition.DisplayName, presentationKind, zone,
            needsWater ? new[] { CareToolKind.Sprayer, tool } : new[] { tool }, needsWater,
            requiredPasses: Mathf.Clamp(condition.Severity + 2, 3, 4));
    }
}
