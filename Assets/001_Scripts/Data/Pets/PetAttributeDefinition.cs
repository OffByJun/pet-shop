using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    /// <summary>여러 기본 동물에 재사용할 수 있는 속성 외형 묶음입니다.</summary>
    [CreateAssetMenu(fileName = "PetAttribute", menuName = "PetShop/Pets/Pet Attribute")]
    public sealed class PetAttributeDefinition : ScriptableObject
    {
        [SerializeField] private string attributeId;
        [SerializeField] private string displayName;
        [SerializeField] private PetElement element;
        [SerializeField] private PetMaterialOverride[] materialOverrides = new PetMaterialOverride[0];
        [SerializeField] private PetVisualAttachment[] attachments = new PetVisualAttachment[0];

        public string AttributeId => attributeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public PetElement Element => element;
        public IReadOnlyList<PetMaterialOverride> MaterialOverrides => materialOverrides;
        public IReadOnlyList<PetVisualAttachment> Attachments => attachments;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(attributeId)) attributeId = name;
            if (attachments == null) return;
            for (var i = 0; i < attachments.Length; i++)
            {
                var attachment = attachments[i];
                if (attachment.LocalScale == Vector3.zero) attachment.LocalScale = Vector3.one;
                attachments[i] = attachment;
            }
        }
    }
}
