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
        [Tooltip("평상 표정입니다.")]
        [SerializeField] private Sprite portrait;
        [Tooltip("인내심이 줄었을 때의 표정입니다. 비어 있으면 평상 표정을 씀니다.")]
        [SerializeField] private Sprite portraitUneasy;
        [Tooltip("인내심이 거의 끝났을 때의 표정입니다. 비어 있으면 앞 표정을 씀니다.")]
        [SerializeField] private Sprite portraitUpset;
        [SerializeField] private GameObject visualPrefab;
        [Tooltip("역할이 아니라 사람 이름입니다. 비우면 표시 이름을 씁니다.")]
        [SerializeField] private string characterName;
        [Tooltip("이 손님을 한 줄로 설명합니다. 접수대 배지에 쓰입니다.")]
        [SerializeField] private string tagline;
        [Tooltip("n번째 방문에서 쓰는 인사입니다. 없으면 기본 인사로 넘어갑니다. 토큰은 대사 테이블과 같습니다.")]
        [TextArea(1, 3)] [SerializeField] private string[] visitGreetings = new string[0];
        [Tooltip("지난번에 만족했을 때 하는 인사입니다.")]
        [TextArea(1, 3)] [SerializeField] private string returningHappyLine;
        [Tooltip("지난번에 실망했을 때 하는 인사입니다.")]
        [TextArea(1, 3)] [SerializeField] private string returningUpsetLine;

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
        public Sprite PortraitUneasy => portraitUneasy == null ? portrait : portraitUneasy;
        public Sprite PortraitUpset => portraitUpset == null ? PortraitUneasy : portraitUpset;
        public GameObject VisualPrefab => visualPrefab;
        public string CharacterName => string.IsNullOrWhiteSpace(characterName) ? DisplayName : characterName;
        public string Tagline => tagline;
        public string ReturningHappyLine => returningHappyLine;
        public string ReturningUpsetLine => returningUpsetLine;

        /// <summary>n번째 방문(1부터)에 맞는 전용 인사입니다. 없으면 빈 문자열입니다.</summary>
        public string GreetingForVisit(int visit)
        {
            var index = visit - 1;
            if (visitGreetings == null || index < 0 || index >= visitGreetings.Length) return string.Empty;
            return visitGreetings[index];
        }
        public float AppearanceWeight => appearanceWeight;
        public int MinimumRequiredRequests => minimumRequiredRequests;
        public int MaximumRequiredRequests => maximumRequiredRequests;
        public int MinimumOptionalCare => minimumOptionalCare;
        public int MaximumOptionalCare => maximumOptionalCare;
        public float ElementalPetChance => elementalPetChance;
        public float RareByproductChance => rareByproductChance;
        public string EconomyTierId => economyTierId;

        /// <summary>기분에 맞는 표정을 골라 줍니다. 기분 판정 기준은 ReceptionSettings에 있습니다.</summary>
        public Sprite PortraitFor(CustomerMood mood) => mood switch
        {
            CustomerMood.Upset => PortraitUpset,
            CustomerMood.Uneasy => PortraitUneasy,
            _ => Portrait
        };

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
