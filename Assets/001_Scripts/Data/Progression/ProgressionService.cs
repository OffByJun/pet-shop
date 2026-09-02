using System;
using System.Collections.Generic;
using _001_Scripts.Data.Economy;

namespace _001_Scripts.Data.Progression
{
    public interface IProgressionContentAccess
    {
        bool IsContentUnlocked(string contentId);
    }

    public sealed class ProgressionService : IProgressionContentAccess
    {
        private readonly ProgressionCatalog catalog;
        private readonly IEconomyPurchaseService purchases;

        public ProgressionState State { get; }
        public event Action<ProgressionUnlockDefinition> Unlocked;
        public event Action<SettlementGoalDefinition> EndingReached;

        public ProgressionService(
            ProgressionCatalog catalog,
            IEconomyPurchaseService purchases,
            ProgressionState state = null)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
            State = state ?? new ProgressionState();
        }

        public ProgressionStageId CurrentStage
        {
            get
            {
                var endings = catalog.EndingCandidates;
                for (var i = 0; i < endings.Count; i++)
                    if (endings[i] != null && State.IsEndingCompleted(endings[i].GoalId)) return ProgressionStageId.Final;
                var stage = ProgressionStageId.Early;
                var unlocks = catalog.Unlocks;
                for (var i = 0; i < unlocks.Count; i++)
                    if (unlocks[i] != null && State.IsUnlocked(unlocks[i].UnlockId) && unlocks[i].Stage > stage)
                        stage = unlocks[i].Stage;
                return stage;
            }
        }

        public bool IsContentUnlocked(string contentId)
        {
            if (string.IsNullOrWhiteSpace(contentId)) return true;
            var unlocks = catalog.Unlocks;
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
            => definition != null &&
               !State.IsUnlocked(definition.UnlockId) &&
               HasAllPrerequisites(definition.Prerequisites) &&
               purchases.CanPurchase(definition.Quote);

        public bool TryUnlock(ProgressionUnlockDefinition definition)
        {
            if (!BelongsToCatalog(definition) || !CanUnlock(definition) || !purchases.TryPurchase(definition.Quote)) return false;
            if (!State.AddUnlock(definition.UnlockId)) return false;
            Unlocked?.Invoke(definition);
            return true;
        }

        public bool CanCompleteEnding(SettlementGoalDefinition goal)
            => goal != null &&
               !State.IsEndingCompleted(goal.GoalId) &&
               HasAllPrerequisites(goal.RequiredUnlocks) &&
               purchases.CanPurchase(goal.Quote);

        public bool TryCompleteEnding(SettlementGoalDefinition goal)
        {
            if (!IsEndingCandidate(goal) || !CanCompleteEnding(goal) || !purchases.TryPurchase(goal.Quote)) return false;
            if (!State.AddEnding(goal.GoalId)) return false;
            EndingReached?.Invoke(goal);
            return true;
        }

        private bool HasAllPrerequisites(IReadOnlyList<ProgressionUnlockDefinition> prerequisites)
        {
            for (var i = 0; i < prerequisites.Count; i++)
                if (prerequisites[i] == null || !State.IsUnlocked(prerequisites[i].UnlockId)) return false;
            return true;
        }

        private bool BelongsToCatalog(ProgressionUnlockDefinition definition)
        {
            if (definition == null) return false;
            var values = catalog.Unlocks;
            for (var i = 0; i < values.Count; i++) if (values[i] == definition) return true;
            return false;
        }

        private bool IsEndingCandidate(SettlementGoalDefinition goal)
        {
            if (goal == null) return false;
            var values = catalog.EndingCandidates;
            for (var i = 0; i < values.Count; i++) if (values[i] == goal) return true;
            return false;
        }
    }
}
