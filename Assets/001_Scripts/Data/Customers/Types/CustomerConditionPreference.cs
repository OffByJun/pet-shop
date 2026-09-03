using System;
using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    [Serializable]
    public struct CustomerConditionPreference
    {
        public PetConditionDefinition Condition;
        [Min(0f)] public float Weight;
    }
}
