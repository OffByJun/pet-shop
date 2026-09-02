using System.Collections.Generic;
using _001_Scripts.Core;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    [RequireComponent(typeof(PetVisualAssembler))]
    public sealed class PetInstance : GameBehaviour
    {
        [SerializeField] private PetVariantDefinition variant;
        [SerializeField] private PetVisualAssembler visualAssembler;
        private readonly HashSet<int> consumedByproductRules = new HashSet<int>();

        public PetVariantDefinition Variant => variant;

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
