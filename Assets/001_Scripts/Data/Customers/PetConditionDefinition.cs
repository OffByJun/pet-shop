using _001_Scripts.Data.Pets;
using _001_Scripts.Data.Tools;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    /// <summary>펫에게 발생하고 특정 케어 행동으로 해결되는 상태입니다.</summary>
    [CreateAssetMenu(fileName = "PetCondition", menuName = "PetShop/Customers/Pet Condition")]
    public sealed class PetConditionDefinition : ScriptableObject
    {
        [SerializeField] private string conditionId;
        [SerializeField] private string displayName;
        [TextArea(2, 4)] [SerializeField] private string description;
        [SerializeField] private PetConditionCategory category;
        [SerializeField] private PetCareAction resolvedBy;
        [SerializeField] private PetToolCapability requiredCapabilities;
        [SerializeField] private PetToolInteractionMode interactionMode = PetToolInteractionMode.Hold;
        [SerializeField, Min(1)] private int severity = 1;
        [Tooltip("비어 있으면 처음부터 등장합니다.")]
        [SerializeField] private string requiredProgressionContentId;

        public string ConditionId => conditionId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public PetConditionCategory Category => category;
        public PetCareAction ResolvedBy => resolvedBy;
        public PetToolCapability RequiredCapabilities => requiredCapabilities;
        public PetToolInteractionMode InteractionMode => interactionMode;
        public int Severity => severity;
        public string RequiredProgressionContentId => requiredProgressionContentId;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(conditionId)) conditionId = name;
            severity = Mathf.Max(1, severity);
        }
    }
}
