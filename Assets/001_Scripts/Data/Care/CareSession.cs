using System;
using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data
{

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

}
