using _001_Scripts.Data.Customers;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.Core.Entity
{
    /// <summary>Animates pre-authored customer and carrier RectTransforms; it creates nothing at runtime.</summary>
    public sealed class ReceptionCustomerActor : CustomerInstance
    {
        [System.Serializable]
        private struct CustomerColorEntry
        {
            public CustomerArchetype archetype;
            public Color color;

            public CustomerColorEntry(CustomerArchetype archetype, Color color)
            {
                this.archetype = archetype;
                this.color = color;
            }
        }

        [SerializeField] private RectTransform customerRoot;
        [SerializeField] private RectTransform carrierRoot;
        [SerializeField] private RectTransform deskPetAnchor;
        [SerializeField] private Image customerBody;
        [SerializeField] private Image petBody;
        [SerializeField] private Vector2 hiddenLeftPosition = new Vector2(-260f, -40f);
        [SerializeField] private Vector2 counterPosition = new Vector2(245f, -40f);
        [SerializeField, Min(.1f)] private float moveDuration = .8f;
        [SerializeField] private CustomerColorEntry[] customerColors =
        {
            new CustomerColorEntry(CustomerArchetype.Adventurer, new Color32(92, 139, 181, 255)),
            new CustomerColorEntry(CustomerArchetype.Wizard, new Color32(128, 105, 170, 255)),
            new CustomerColorEntry(CustomerArchetype.Merchant, new Color32(195, 140, 70, 255)),
            new CustomerColorEntry(CustomerArchetype.Noble, new Color32(168, 99, 126, 255))
        };
        [SerializeField] private Color defaultCustomerColor = new Color32(89, 158, 120, 255);
        [SerializeField] private Color defaultPetColor = new Color32(224, 176, 113, 255);
        [Tooltip("초상화 스프라이트가 있을 때 감추는 대체 도형입니다.")]
        [SerializeField] private GameObject[] placeholderShapes = new GameObject[0];

        private Vector2 carrierHome;
        private CustomerBase portraitSource;
        private Sprite appliedPortrait;
        private float moveStarted;
        private bool entering;
        private bool exiting;
        private bool handingOff;

        public bool HasArrived => !entering && !exiting && customerRoot != null &&
                                  Vector2.Distance(customerRoot.anchoredPosition, counterPosition) < 1f;
        public bool HasExited => !entering && !exiting && customerRoot != null &&
                                 Vector2.Distance(customerRoot.anchoredPosition, hiddenLeftPosition) < 1f;

        private void Awake()
        {
            if (carrierRoot != null) carrierHome = carrierRoot.anchoredPosition;
        }

        public void Enter(ServiceOrder order)
        {
            if (order == null) throw new System.ArgumentNullException(nameof(order));
            Initialize(order.Customer);
            portraitSource = order.Customer;
            appliedPortrait = null;
            if (customerRoot == null) return;
            ApplyOrderColors(order);
            ApplyOrderSprites(order);
            SetMood(CustomerMood.Calm);
            customerRoot.anchoredPosition = hiddenLeftPosition;
            if (carrierRoot != null) carrierRoot.anchoredPosition = carrierHome;
            handingOff = false;
            entering = true;
            exiting = false;
            moveStarted = Time.unscaledTime;
        }

        public void Exit()
        {
            entering = false;
            exiting = true;
            moveStarted = Time.unscaledTime;
        }

        public void BeginHandoff()
        {
            handingOff = true;
            moveStarted = Time.unscaledTime;
        }

        private void Update()
        {
            if (customerRoot == null) return;
            if (entering)
            {
                customerRoot.anchoredPosition = Vector2.Lerp(hiddenLeftPosition, counterPosition, Ease());
                if (Ease() >= 1f) entering = false;
            }
            else if (exiting)
            {
                customerRoot.anchoredPosition = Vector2.Lerp(counterPosition, hiddenLeftPosition, Ease());
                if (Ease() >= 1f) exiting = false;
            }

            if (handingOff && carrierRoot != null && deskPetAnchor != null)
            {
                var world = Vector3.Lerp(customerRoot.TransformPoint(carrierHome), deskPetAnchor.position, Ease());
                carrierRoot.position = world;
            }
        }

        private float Ease()
        {
            var t = Mathf.Clamp01((Time.unscaledTime - moveStarted) / moveDuration);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private void ApplyOrderColors(ServiceOrder order)
        {
            if (customerBody != null)
            {
                customerBody.color = defaultCustomerColor;
                for (var i = 0; i < customerColors.Length; i++)
                    if (customerColors[i].archetype == order.Customer.Archetype)
                    {
                        customerBody.color = customerColors[i].color;
                        break;
                    }
            }
            if (petBody != null) petBody.color = defaultPetColor;
        }

        /// <summary>손님 초상화와 펫 아이콘을 주문 데이터에서 가져옵니다.</summary>
        private void ApplyOrderSprites(ServiceOrder order)
        {
            var portrait = order.Customer.Portrait;
            if (customerBody != null)
            {
                customerBody.sprite = portrait;
                customerBody.preserveAspect = portrait != null;
                if (portrait != null) customerBody.color = Color.white;
            }
            for (var i = 0; i < placeholderShapes.Length; i++)
                if (placeholderShapes[i] != null) placeholderShapes[i].SetActive(portrait == null);

            if (petBody == null) return;
            var icon = order.Pet == null ? null : order.Pet.Icon;
            petBody.sprite = icon;
            petBody.preserveAspect = icon != null;
            petBody.color = icon == null ? defaultPetColor : Color.white;
        }

        /// <summary>기분에 맞춰 손님 표정을 바꿉니다.</summary>
        public void SetMood(CustomerMood mood)
        {
            if (customerBody == null || portraitSource == null) return;
            var portrait = portraitSource.PortraitFor(mood);
            if (portrait == null || ReferenceEquals(portrait, appliedPortrait)) return;
            appliedPortrait = portrait;
            customerBody.sprite = portrait;
        }

        public void Configure(RectTransform customer, RectTransform carrier, RectTransform deskAnchor, Image body, Image pet, GameObject[] placeholders = null)
        {
            placeholderShapes = placeholders ?? new GameObject[0];
            customerRoot = customer;
            carrierRoot = carrier;
            deskPetAnchor = deskAnchor;
            customerBody = body;
            petBody = pet;
        }
    }
}
