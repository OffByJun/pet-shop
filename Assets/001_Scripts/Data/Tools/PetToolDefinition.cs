using _001_Scripts.Data.Customers;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace _001_Scripts.Data.Tools
{
    /// <summary>펫 종류가 아니라 처리 능력과 조작 방식만 선언하는 도구 정의입니다.</summary>
    [CreateAssetMenu(fileName = "PetTool", menuName = "PetShop/Tools/Pet Tool")]
    public sealed class PetToolDefinition : ScriptableObject
    {
        [SerializeField] private string toolId;
        [SerializeField] private string displayName;
        [TextArea(2, 4)] [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private PetToolCapability capabilities;
        [SerializeField] private PetToolInteractionSupport supportedInteractions;
        [SerializeField] private PetCareAction rewardAction;

        public string ToolId => toolId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject VisualPrefab => visualPrefab;
        public PetToolCapability Capabilities => capabilities;
        public PetToolInteractionSupport SupportedInteractions => supportedInteractions;
        public PetCareAction RewardAction => rewardAction;

        public bool CanProcess(PetConditionDefinition condition)
        {
            if (condition == null) return false;
            var handlesCapability = (capabilities & condition.RequiredCapabilities) != 0;
            var handlesInteraction = (supportedInteractions & condition.InteractionMode.ToSupport()) != 0;
            return handlesCapability && handlesInteraction;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(toolId)) toolId = name;
        }
    }
}
