using System;
using System.Collections.Generic;
using UnityEngine;

namespace PetShop.Care
{
    public enum CareKind { Wash, Brush, Treat, Remove, Trim }
    public enum CareToolKind { Sprayer, WashBrush, Comb, Medicine, Tweezers, Scissors }
    public enum CareInteractionStatus { WrongTool, Wetting, NeedsWater, Progressed, Resolved }

    public readonly struct CareInteractionResult
    {
        public CareInteractionStatus Status { get; }
        public CareConditionState Condition { get; }

        public CareInteractionResult(CareInteractionStatus status, CareConditionState condition)
        {
            Status = status;
            Condition = condition;
        }
    }

    public sealed class CareConditionState
    {
        private readonly HashSet<CareToolKind> acceptedTools;

        public string Id { get; }
        public string Name { get; }
        public CareKind Care { get; }
        public Rect Zone { get; }
        public bool NeedsWater { get; }
        public string Byproduct { get; }
        public float Remaining { get; private set; } = 1f;
        public float Wetness { get; private set; }
        public bool Resolved => Remaining <= .001f;

        public CareConditionState(
            string id, string name, CareKind care, Rect zone,
            IEnumerable<CareToolKind> tools, bool needsWater = false, string byproduct = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? string.Empty;
            Care = care;
            Zone = zone;
            NeedsWater = needsWater;
            Byproduct = byproduct ?? string.Empty;
            acceptedTools = new HashSet<CareToolKind>(tools ?? throw new ArgumentNullException(nameof(tools)));
        }

        public bool Accepts(CareToolKind tool) => acceptedTools.Contains(tool);
        public void ApplyWater(float amount) => Wetness = Mathf.Clamp01(Wetness + Mathf.Max(0f, amount));

        public bool ApplyProgress(float amount)
        {
            if (Resolved || amount <= 0f) return false;
            Remaining = Mathf.Clamp01(Remaining - amount);
            return Resolved;
        }
    }

    public interface ICareConditionSource
    {
        IReadOnlyList<CareConditionState> Create(Func<string, bool> includeCondition);
    }

    public sealed class DefaultCareConditionSource : ICareConditionSource
    {
        public IReadOnlyList<CareConditionState> Create(Func<string, bool> includeCondition)
        {
            var all = new List<CareConditionState>
            {
                new CareConditionState("mud", "진흙", CareKind.Wash, new Rect(.22f, .52f, .25f, .22f),
                    new[] { CareToolKind.Sprayer, CareToolKind.WashBrush }, true),
                new CareConditionState("tangle", "엉킨 털", CareKind.Brush, new Rect(.49f, .31f, .28f, .20f),
                    new[] { CareToolKind.Comb }, byproduct: "부드러운 털 x1"),
                new CareConditionState("wound", "작은 상처", CareKind.Treat, new Rect(.42f, .67f, .19f, .16f),
                    new[] { CareToolKind.Medicine }),
                new CareConditionState("crystal", "수정 조각", CareKind.Remove, new Rect(.69f, .51f, .15f, .22f),
                    new[] { CareToolKind.Tweezers }, byproduct: "수정 조각 x1"),
                new CareConditionState("long_fur", "긴 털", CareKind.Trim, new Rect(.13f, .29f, .18f, .20f),
                    new[] { CareToolKind.Scissors }, byproduct: "긴 털 뭉치 x1")
            };
            if (includeCondition != null) all.RemoveAll(item => !includeCondition(item.Id));
            return all;
        }
    }

    public sealed class CareSession
    {
        private readonly List<CareConditionState> conditions;
        private readonly List<string> byproducts = new List<string>();

        public IReadOnlyList<CareConditionState> Conditions => conditions;
        public IReadOnlyList<string> Byproducts => byproducts;
        public int RemainingCount { get; private set; }
        public bool IsCompleted => RemainingCount == 0;

        public CareSession(IReadOnlyList<CareConditionState> initialConditions)
        {
            if (initialConditions == null) throw new ArgumentNullException(nameof(initialConditions));
            conditions = new List<CareConditionState>(initialConditions);
            RemainingCount = conditions.Count;
        }

        public bool RegisterResolved(CareConditionState condition)
        {
            if (condition == null || !condition.Resolved || !conditions.Contains(condition)) return false;
            if (!string.IsNullOrWhiteSpace(condition.Byproduct) && !byproducts.Contains(condition.Byproduct))
                byproducts.Add(condition.Byproduct);
            RemainingCount = 0;
            for (var i = 0; i < conditions.Count; i++) if (!conditions[i].Resolved) RemainingCount++;
            return true;
        }

        public CareInteractionResult ApplyStroke(CareConditionState condition, CareToolKind tool, float distance)
        {
            if (condition == null || !conditions.Contains(condition))
                throw new ArgumentException("Condition does not belong to this session.", nameof(condition));
            if (!condition.Accepts(tool))
                return new CareInteractionResult(CareInteractionStatus.WrongTool, condition);

            if (condition.Care == CareKind.Wash && tool == CareToolKind.Sprayer)
            {
                condition.ApplyWater(distance / 380f);
                return new CareInteractionResult(CareInteractionStatus.Wetting, condition);
            }
            if (condition.NeedsWater && tool == CareToolKind.WashBrush && condition.Wetness < 1f)
                return new CareInteractionResult(CareInteractionStatus.NeedsWater, condition);

            var resolved = condition.ApplyProgress(distance / CareToolRules.Effort(tool));
            if (!resolved) return new CareInteractionResult(CareInteractionStatus.Progressed, condition);
            RegisterResolved(condition);
            return new CareInteractionResult(CareInteractionStatus.Resolved, condition);
        }
    }

    public static class CareToolRules
    {
        public static float Effort(CareToolKind tool) => tool switch
        {
            CareToolKind.Medicine => 300f,
            CareToolKind.Tweezers => 240f,
            CareToolKind.Scissors => 320f,
            _ => 420f
        };
    }
}
