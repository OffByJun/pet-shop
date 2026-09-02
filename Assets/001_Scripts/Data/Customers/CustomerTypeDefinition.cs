using System.Collections.Generic;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    [CreateAssetMenu(fileName = "CustomerType", menuName = "PetShop/Customers/Customer Type")]
    public sealed class CustomerTypeDefinition : CustomerBase
    {
        [SerializeField] private CustomerConditionPreference[] conditionPreferences = new CustomerConditionPreference[0];
        public IReadOnlyList<CustomerConditionPreference> ConditionPreferences => conditionPreferences;
    }
}
