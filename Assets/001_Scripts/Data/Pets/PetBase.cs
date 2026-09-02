using UnityEngine;

namespace _001_Scripts.Data.Pets
{
    /// <summary>체형, 모델, 애니메이션처럼 모든 변종이 공유하는 기본 동물 정의입니다.</summary>
    public abstract class PetBase : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string petBaseId;
        [SerializeField] private string displayName;
        [SerializeField] private PetSpecies species;
        [TextArea(2, 4)] [SerializeField] private string bodyRole;
        [Header("Visual")]
        [SerializeField] private GameObject modelPrefab;
        [SerializeField] private RuntimeAnimatorController animatorController;

        public string PetBaseId => petBaseId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public PetSpecies Species => species;
        public string BodyRole => bodyRole;
        public GameObject ModelPrefab => modelPrefab;
        public RuntimeAnimatorController AnimatorController => animatorController;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(petBaseId)) petBaseId = name;
        }
    }
}
