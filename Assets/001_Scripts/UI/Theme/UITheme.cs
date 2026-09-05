using UnityEngine;

namespace _001_Scripts.UI.Theme
{
    /// <summary>화면 전체가 쓰는 색·글자 크기·스프라이트입니다. 값은 이 에셋에서만 조정합니다.</summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "PetShop/UI/Theme")]
    public sealed class UITheme : ScriptableObject
    {
        [Header("Fonts")]
        [Tooltip("제목용 글꼴입니다. 비우면 OS 글꼴로 대체합니다.")]
        [SerializeField] private Font displayFont;
        [Tooltip("본문용 글꼴입니다. 비우면 OS 글꼴로 대체합니다.")]
        [SerializeField] private Font bodyFont;
        [Tooltip("글꼴이 비어 있을 때 쓸 OS 글꼴 이름입니다.")]
        [SerializeField] private string osFontFallback = "Malgun Gothic";

        [Header("Ink")]
        [SerializeField] private Color ink = new Color32(45, 65, 60, 255);
        [SerializeField] private Color inkSoft = new Color32(99, 121, 110, 255);
        [SerializeField] private Color inkFaint = new Color32(147, 166, 154, 255);
        [SerializeField] private Color inkOnPrimary = new Color32(45, 65, 60, 255);

        [Header("Surfaces")]
        [SerializeField] private Color paper = new Color32(251, 245, 231, 255);
        [SerializeField] private Color paperWarm = new Color32(253, 248, 236, 245);
        [SerializeField] private Color memo = new Color32(251, 243, 222, 255);
        [SerializeField] private Color oak = new Color32(194, 146, 94, 255);
        [SerializeField] private Color sage = new Color32(184, 215, 195, 255);
        [SerializeField] private Color sageDeep = new Color32(127, 169, 141, 255);
        [SerializeField] private Color blush = new Color32(221, 128, 105, 255);
        [SerializeField] private Color gold = new Color32(217, 166, 74, 255);
        [SerializeField] private Color shadow = new Color32(88, 59, 37, 46);

        [Header("Care")]
        [SerializeField] private Color careWash = new Color32(91, 157, 201, 255);
        [SerializeField] private Color careBrush = new Color32(224, 172, 79, 255);
        [SerializeField] private Color careTreat = new Color32(221, 128, 105, 255);
        [SerializeField] private Color careRemove = new Color32(138, 124, 184, 255);
        [SerializeField] private Color careTrim = new Color32(111, 187, 162, 255);

        [Header("Type scale")]
        [SerializeField] private int titleSize = 44;
        [SerializeField] private int headingSize = 26;
        [SerializeField] private int bodySize = 19;
        [SerializeField] private int smallSize = 15;
        [SerializeField] private int microSize = 12;

        [Header("Sprites")]
        [SerializeField] private Sprite card;
        [SerializeField] private Sprite capsulePrimary;
        [SerializeField] private Sprite capsuleSoft;
        [SerializeField] private Sprite capsuleSage;
        [SerializeField] private Sprite capsuleMuted;
        [SerializeField] private Sprite chip;
        [SerializeField] private Sprite circle;
        [SerializeField] private Sprite ring;
        [SerializeField] private Sprite iconWash;
        [SerializeField] private Sprite iconBrush;
        [SerializeField] private Sprite iconTreat;
        [SerializeField] private Sprite iconRemove;
        [SerializeField] private Sprite iconTrim;
        [SerializeField] private Sprite iconPaw;
        [SerializeField] private Sprite iconStar;
        [SerializeField] private Sprite iconCheck;

        public Color Ink => ink;
        public Color InkSoft => inkSoft;
        public Color InkFaint => inkFaint;
        public Color InkOnPrimary => inkOnPrimary;
        public Color Paper => paper;
        public Color PaperWarm => paperWarm;
        public Color Memo => memo;
        public Color Oak => oak;
        public Color Sage => sage;
        public Color SageDeep => sageDeep;
        public Color Blush => blush;
        public Color Gold => gold;
        public Color ShadowColor => shadow;
        public Color CareWash => careWash;
        public Color CareBrush => careBrush;
        public Color CareTreat => careTreat;
        public Color CareRemove => careRemove;
        public Color CareTrim => careTrim;
        public int TitleSize => titleSize;
        public int HeadingSize => headingSize;
        public int BodySize => bodySize;
        public int SmallSize => smallSize;
        public int MicroSize => microSize;
        public Sprite Card => card;
        public Sprite CapsulePrimary => capsulePrimary;
        public Sprite CapsuleSoft => capsuleSoft;
        public Sprite CapsuleSage => capsuleSage;
        public Sprite CapsuleMuted => capsuleMuted;
        public Sprite Chip => chip;
        public Sprite Circle => circle;
        public Sprite Ring => ring;
        public Sprite IconWash => iconWash;
        public Sprite IconBrush => iconBrush;
        public Sprite IconTreat => iconTreat;
        public Sprite IconRemove => iconRemove;
        public Sprite IconTrim => iconTrim;
        public Sprite IconPaw => iconPaw;
        public Sprite IconStar => iconStar;
        public Sprite IconCheck => iconCheck;

        /// <summary>제목용 글꼴입니다. 에셋이 비어 있으면 OS 글꼴을 만들어 씁니다.</summary>
        public Font DisplayFont => displayFont != null ? displayFont : Fallback();
        public Font BodyFont => bodyFont != null ? bodyFont : Fallback();

        private Font cachedFallback;

        private Font Fallback()
        {
            if (cachedFallback != null) return cachedFallback;
            if (!string.IsNullOrWhiteSpace(osFontFallback))
                cachedFallback = Font.CreateDynamicFontFromOSFont(osFontFallback, 32);
            if (cachedFallback == null)
                cachedFallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return cachedFallback;
        }

        public Color CareColor(_001_Scripts.Data.CareKind kind) => kind switch
        {
            _001_Scripts.Data.CareKind.Wash => careWash,
            _001_Scripts.Data.CareKind.Brush => careBrush,
            _001_Scripts.Data.CareKind.Treat => careTreat,
            _001_Scripts.Data.CareKind.Remove => careRemove,
            _001_Scripts.Data.CareKind.Trim => careTrim,
            _ => inkSoft
        };

        public Sprite CareIcon(_001_Scripts.Data.CareKind kind) => kind switch
        {
            _001_Scripts.Data.CareKind.Wash => iconWash,
            _001_Scripts.Data.CareKind.Brush => iconBrush,
            _001_Scripts.Data.CareKind.Treat => iconTreat,
            _001_Scripts.Data.CareKind.Remove => iconRemove,
            _001_Scripts.Data.CareKind.Trim => iconTrim,
            _ => iconPaw
        };
    }
}
