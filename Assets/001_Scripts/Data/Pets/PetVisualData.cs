using System;
using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    [Serializable]
    public struct PetMaterialOverride
    {
        public string RendererPath;
        [Min(0)] public int MaterialIndex;
        public Material Material;
    }

    [Serializable]
    public struct PetVisualAttachment
    {
        public PetVisualSlot Slot;
        public GameObject Prefab;
        public Vector3 LocalPosition;
        public Vector3 LocalEulerAngles;
        public Vector3 LocalScale;
    }
}
