using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Progression
{
    [CreateAssetMenu(fileName = "ProgressionCatalog", menuName = "PetShop/Progression/Catalog")]
    public sealed class ProgressionCatalog : ScriptableObject
    {
        [SerializeField] private ProgressionStageDefinition[] stages = new ProgressionStageDefinition[0];
        [SerializeField] private ProgressionUnlockDefinition[] unlocks = new ProgressionUnlockDefinition[0];
        [Tooltip("기획 확정 전에는 후보를 여러 개 등록하고 런타임에서 선택할 수 있습니다.")]
        [SerializeField] private SettlementGoalDefinition[] endingCandidates = new SettlementGoalDefinition[0];

        public IReadOnlyList<ProgressionStageDefinition> Stages => stages;
        public IReadOnlyList<ProgressionUnlockDefinition> Unlocks => unlocks;
        public IReadOnlyList<SettlementGoalDefinition> EndingCandidates => endingCandidates;
    }
}
