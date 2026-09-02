using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    [CreateAssetMenu(fileName = "PetCatalog", menuName = "PetShop/Pets/Pet Catalog")]
    public sealed class PetCatalog : ScriptableObject
    {
        [SerializeField] private PetBase[] baseAnimals = new PetBase[0];
        [SerializeField] private PetAttributeDefinition[] attributes = new PetAttributeDefinition[0];
        [SerializeField] private PetVariantDefinition[] variants = new PetVariantDefinition[0];

        public IReadOnlyList<PetBase> BaseAnimals => baseAnimals;
        public IReadOnlyList<PetAttributeDefinition> Attributes => attributes;
        public IReadOnlyList<PetVariantDefinition> Variants => variants;

        public PetVariantDefinition FindVariant(string variantId)
        {
            if (string.IsNullOrWhiteSpace(variantId) || variants == null) return null;
            for (var i = 0; i < variants.Length; i++)
                if (variants[i] != null && variants[i].VariantId == variantId) return variants[i];
            return null;
        }

        public bool IsAllowed(PetBase baseAnimal, PetAttributeDefinition attribute)
        {
            if (variants == null) return false;
            for (var i = 0; i < variants.Length; i++)
                if (variants[i] != null && variants[i].BaseAnimal == baseAnimal && variants[i].Attribute == attribute) return true;
            return false;
        }
    }
}
