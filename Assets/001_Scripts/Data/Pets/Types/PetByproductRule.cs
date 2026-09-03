using System;
using _001_Scripts.Data.Items;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    /// <summary>같은 속성이라도 변종마다 다른 부산물을 설정하는 규칙입니다.</summary>
    [Serializable]
    public struct PetByproductRule
    {
        public PetCareAction CareAction;
        public ItemBase Item;
        [Min(1)] public int MinAmount;
        [Min(1)] public int MaxAmount;
        [Range(0f, 1f)] public float Chance;

        public bool IsValid => Item != null && MinAmount > 0 && MaxAmount >= MinAmount && Chance > 0f;
    }
}
