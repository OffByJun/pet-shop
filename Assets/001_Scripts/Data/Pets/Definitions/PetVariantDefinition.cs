using System.Collections.Generic;
using _001_Scripts.Data.Progression;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    /// <summary>실제 콘텐츠로 출시되는 기본 동물과 속성의 허용 조합입니다.</summary>
    [CreateAssetMenu(fileName = "PetVariant", menuName = "PetShop/Pets/Pet Variant")]
    public sealed class PetVariantDefinition : ScriptableObject
    {
        [SerializeField] private string variantId;
        [SerializeField] private string displayName;
        [TextArea(2, 5)] [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private PetBase baseAnimal;
        [SerializeField] private PetAttributeDefinition attribute;
        [Tooltip("비어 있으면 처음부터 등장합니다.")]
        [SerializeField] private string requiredProgressionContentId;
        [SerializeField] private PetByproductRule[] byproducts = new PetByproductRule[0];

        public string VariantId => variantId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public PetBase BaseAnimal => baseAnimal;
        public PetAttributeDefinition Attribute => attribute;
        public string RequiredProgressionContentId => requiredProgressionContentId;
        public IReadOnlyList<PetByproductRule> Byproducts => byproducts;
        public bool IsConfigured => baseAnimal != null && attribute != null && byproducts != null && byproducts.Length > 0;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(variantId)) variantId = name;
            if (byproducts == null) return;
            for (var i = 0; i < byproducts.Length; i++)
            {
                var rule = byproducts[i];
                rule.MinAmount = Mathf.Max(1, rule.MinAmount);
                rule.MaxAmount = Mathf.Max(rule.MinAmount, rule.MaxAmount);
                rule.Chance = Mathf.Clamp01(rule.Chance);
                byproducts[i] = rule;
            }
        }
    }
}
