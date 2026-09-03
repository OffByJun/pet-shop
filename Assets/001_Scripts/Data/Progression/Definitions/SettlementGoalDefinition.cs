using System.Collections.Generic;
using _001_Scripts.Data.Economy;
using UnityEngine;

namespace _001_Scripts.Data.Progression
{
    [CreateAssetMenu(fileName = "SettlementGoal", menuName = "PetShop/Progression/Settlement Goal")]
    public sealed class SettlementGoalDefinition : ScriptableObject
    {
        [SerializeField] private string goalId;
        [SerializeField] private string displayName;
        [TextArea(2, 5)] [SerializeField] private string description;
        [SerializeField, Min(0)] private int cost;
        [SerializeField] private ProgressionUnlockDefinition[] requiredUnlocks = new ProgressionUnlockDefinition[0];
        [SerializeField] private bool continueBusinessAfterCompletion = true;

        public string GoalId => goalId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public int Cost => cost;
        public IReadOnlyList<ProgressionUnlockDefinition> RequiredUnlocks => requiredUnlocks;
        public bool ContinueBusinessAfterCompletion => continueBusinessAfterCompletion;
        public ExpenseQuote Quote => new ExpenseQuote(goalId, ExpenseCategory.FinalSettlement, cost);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(goalId)) goalId = name;
            cost = Mathf.Max(0, cost);
            continueBusinessAfterCompletion = true;
        }
    }
}
