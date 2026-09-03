using System;
using System.Collections.Generic;
using _001_Scripts.Data.Customers;

namespace _001_Scripts.Data
{
    public interface ICareConditionIdMapper
    {
        string Map(PetConditionDefinition condition);
    }
}
