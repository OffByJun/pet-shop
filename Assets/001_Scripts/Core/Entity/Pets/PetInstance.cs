using System.Collections.Generic;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace _001_Scripts.Core.Entity
{
    [RequireComponent(typeof(PetVisualAssembler))]
    public sealed class PetInstance : GameEntity
    {
        [SerializeField] private PetVariantDefinition variant;
        [SerializeField] private PetVisualAssembler visualAssembler;
        private readonly HashSet<int> consumedByproductRules = new HashSet<int>();

        public PetVariantDefinition Variant => variant;
        public override string DefinitionId => variant == null ? string.Empty : variant.VariantId;
        public override string DisplayName => variant == null ? name : variant.DisplayName;

        private void Awake()
        {
            if (visualAssembler == null) visualAssembler = GetComponent<PetVisualAssembler>();
            if (visualAssembler != null && variant != null) visualAssembler.Build(variant);
        }

        public void Initialize(PetVariantDefinition definition)
        {
            variant = definition;
            consumedByproductRules.Clear();
            if (visualAssembler == null) visualAssembler = GetComponent<PetVisualAssembler>();
            if (visualAssembler != null) visualAssembler.Build(variant);
        }

        internal bool TryConsumeByproductRule(int ruleIndex)
            => ruleIndex >= 0 && consumedByproductRules.Add(ruleIndex);
    }
}
