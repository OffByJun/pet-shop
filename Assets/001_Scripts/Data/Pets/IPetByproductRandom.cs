using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    public interface IPetByproductRandom
    {
        float Value { get; }
        int RangeInclusive(int minimum, int maximum);
    }

    public sealed class UnityPetByproductRandom : IPetByproductRandom
    {
        public float Value => Random.value;
        public int RangeInclusive(int minimum, int maximum) => Random.Range(minimum, maximum + 1);
    }
}
