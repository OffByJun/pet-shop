using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    public interface IServiceOrderRandom
    {
        float Value { get; }
        int Range(int minimumInclusive, int maximumExclusive);
    }

    public sealed class UnityServiceOrderRandom : IServiceOrderRandom
    {
        public float Value => Random.value;
        public int Range(int minimumInclusive, int maximumExclusive) => Random.Range(minimumInclusive, maximumExclusive);
    }
}
