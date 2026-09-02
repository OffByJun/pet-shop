using System.Collections.Generic;
using _001_Scripts.Data.Pets;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    [CreateAssetMenu(fileName = "ServiceOrderCatalog", menuName = "PetShop/Customers/Service Order Catalog")]
    public sealed class ServiceOrderCatalog : ScriptableObject
    {
        [SerializeField] private CustomerTypeDefinition[] customerTypes = new CustomerTypeDefinition[0];
        [SerializeField] private PetVariantDefinition[] petVariants = new PetVariantDefinition[0];
        [SerializeField] private PetConditionDefinition[] conditions = new PetConditionDefinition[0];
        [SerializeField, Range(0f, 1f)] private float perfectOptionalCompletionRatio = 1f;

        public IReadOnlyList<CustomerTypeDefinition> CustomerTypes => customerTypes;
        public IReadOnlyList<PetVariantDefinition> PetVariants => petVariants;
        public IReadOnlyList<PetConditionDefinition> Conditions => conditions;
        public float PerfectOptionalCompletionRatio => perfectOptionalCompletionRatio;
    }
}
