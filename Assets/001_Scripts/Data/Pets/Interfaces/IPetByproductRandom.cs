using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    public interface IPetByproductRandom
    {
        float Value { get; }
        int RangeInclusive(int minimum, int maximum);
    }

}
