using UnityEngine;

namespace _001_Scripts.Data.Customers
{
    /// <summary>외형과 주문 생성 성향을 공유하는 손님 타입 정의입니다.</summary>
    public abstract class CustomerBase : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string customerTypeId;
        [SerializeField] private string displayName;
        [SerializeField] private CustomerArchetype archetype;
        [SerializeField] private Sprite portrait;
        [SerializeField] private GameObject visualPrefab;

        [Header("Order Tendency")]
        [SerializeField, Min(0.01f)] private float appearanceWeight = 1f;
        [SerializeField, Min(1)] private int minimumRequiredRequests = 1;
        [SerializeField, Min(1)] private int maximumRequiredRequests = 2;
        [SerializeField, Min(0)] private int minimumOptionalCare;
        [SerializeField, Min(0)] private int maximumOptionalCare = 1;
        [SerializeField, Range(0f, 1f)] private float elementalPetChance;
        [SerializeField, Range(0f, 1f)] private float rareByproductChance;
        [SerializeField] private string economyTierId = "standard";

        public string CustomerTypeId => customerTypeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public CustomerArchetype Archetype => archetype;
        public Sprite Portrait => portrait;
        public GameObject VisualPrefab => visualPrefab;
        public float AppearanceWeight => appearanceWeight;
        public int MinimumRequiredRequests => minimumRequiredRequests;
        public int MaximumRequiredRequests => maximumRequiredRequests;
        public int MinimumOptionalCare => minimumOptionalCare;
        public int MaximumOptionalCare => maximumOptionalCare;
        public float ElementalPetChance => elementalPetChance;
        public float RareByproductChance => rareByproductChance;
        public string EconomyTierId => economyTierId;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(customerTypeId)) customerTypeId = name;
            appearanceWeight = Mathf.Max(0.01f, appearanceWeight);
            minimumRequiredRequests = Mathf.Max(1, minimumRequiredRequests);
            maximumRequiredRequests = Mathf.Max(minimumRequiredRequests, maximumRequiredRequests);
            minimumOptionalCare = Mathf.Max(0, minimumOptionalCare);
            maximumOptionalCare = Mathf.Max(minimumOptionalCare, maximumOptionalCare);
            if (string.IsNullOrWhiteSpace(economyTierId)) economyTierId = "standard";
        }
    }
}
