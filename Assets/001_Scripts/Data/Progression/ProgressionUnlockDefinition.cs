using System.Collections.Generic;
using _001_Scripts.Data.Economy;
using UnityEngine;

namespace _001_Scripts.Data.Progression
{
    [CreateAssetMenu(fileName = "ProgressionUnlock", menuName = "PetShop/Progression/Unlock")]
    public sealed class ProgressionUnlockDefinition : ScriptableObject
    {
        [SerializeField] private string unlockId;
        [SerializeField] private string displayName;
        [TextArea(2, 5)] [SerializeField] private string description;
        [SerializeField] private ProgressionStageId stage;
        [SerializeField] private ExpenseCategory expenseCategory;
        [SerializeField, Min(0)] private int cost;
        [SerializeField] private ProgressionUnlockDefinition[] prerequisites = new ProgressionUnlockDefinition[0];
        [SerializeField] private ProgressionBenefit[] benefits = new ProgressionBenefit[0];

        public string UnlockId => unlockId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public ProgressionStageId Stage => stage;
        public int Cost => cost;
        public ExpenseCategory ExpenseCategory => expenseCategory;
        public IReadOnlyList<ProgressionUnlockDefinition> Prerequisites => prerequisites;
        public IReadOnlyList<ProgressionBenefit> Benefits => benefits;
        public ExpenseQuote Quote => new ExpenseQuote(unlockId, expenseCategory, cost);

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(unlockId)) unlockId = name;
            cost = Mathf.Max(0, cost);
        }
    }
}
