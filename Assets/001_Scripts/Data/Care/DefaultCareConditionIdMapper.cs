using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.Data
{
    public sealed class DefaultCareConditionIdMapper : ICareConditionIdMapper
    {
        public string Map(PetConditionDefinition condition) => condition.Category switch
        {
            PetConditionCategory.Contamination => "mud",
            PetConditionCategory.Injury => "wound",
            PetConditionCategory.Coat => "tangle",
            PetConditionCategory.ForeignObject => "crystal",
            PetConditionCategory.Growth => "long_fur",
            PetConditionCategory.Nails => "long_fur",
            _ => condition.ConditionId
        };
    }
}
