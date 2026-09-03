using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    public interface IServiceOrderRandom
    {
        float Value { get; }
        int Range(int minimumInclusive, int maximumExclusive);
    }

}
