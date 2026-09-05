using _001_Scripts.Core.Pipes;
using _001_Scripts.Core.Pipes.Msgs;
using _001_Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Components
{
    /// <summary>Runtime choice panel for in-care incidents. It blocks tool input until resolved.</summary>
    public sealed class CareEventPanel : MonoBehaviour
    {
        [Tooltip("색과 글꼴을 화면 테마에서 가져옵니다. 비우면 기본값을 씁니다.")]
        [SerializeField] private _001_Scripts.UI.Theme.UITheme theme;

        /// <summary>런타임에 붙는 컴포넌트라 소유자가 테마를 넘겨 줍니다.</summary>
        public void SetTheme(_001_Scripts.UI.Theme.UITheme value) => theme = value;

        private CareUIComponent source;
        private RectTransform root;
        private Text title;
        private Text description;
        private readonly Button[] buttons = new Button[2];
        private readonly Text[] buttonLabels = new Text[2];
        private string renderedEventId;

        public void Initialize(CareUIComponent owner)
        {
            if (root != null) return;
            source = owner;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var overlay = new GameObject("Care Event Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root = (RectTransform)overlay.transform;
            root.SetParent(canvas.transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = theme != null
                ? new Color(theme.Ink.r * .55f, theme.Ink.g * .5f, theme.Ink.b * .42f, .62f)
                : (Color)new Color32(24, 37, 44, 165);

            var card = CreateImage("Event Card", root, new Color32(250, 246, 234, 255));
            card.anchorMin = card.anchorMax = new Vector2(.5f, .5f);
            card.sizeDelta = new Vector2(650f, 350f);
            card.anchoredPosition = Vector2.zero;

            var eyebrow = CreateText("Eyebrow", card, 15, TextAnchor.MiddleCenter, FontStyle.Bold,
                new Color32(105, 192, 166, 255));
            SetRect(eyebrow.rectTransform, .08f, .82f, .92f, .94f);
            eyebrow.text = "CARE EVENT";

            title = CreateText("Title", card, 30, TextAnchor.MiddleCenter, FontStyle.Bold,
                new Color32(39, 54, 61, 255));
            SetRect(title.rectTransform, .08f, .64f, .92f, .84f);
            description = CreateText("Description", card, 18, TextAnchor.MiddleCenter, FontStyle.Normal,
                new Color32(82, 88, 87, 255));
            SetRect(description.rectTransform, .10f, .43f, .90f, .66f);

            for (var i = 0; i < buttons.Length; i++)
            {
                var captured = i;
                var rect = CreateImage($"Choice {i + 1}", card,
                    i == 0 ? new Color32(207, 235, 224, 255) : new Color32(247, 220, 167, 255));
                rect.anchorMin = new Vector2(i == 0 ? .07f : .52f, .10f);
                rect.anchorMax = new Vector2(i == 0 ? .48f : .93f, .39f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                buttons[i] = rect.gameObject.AddComponent<Button>();
                buttons[i].targetGraphic = rect.GetComponent<Image>();
                buttons[i].onClick.AddListener(() =>
                    GamePipe.Publish(new CareInputRequest(source, CareInput.EventChoice, captured, 0f)));
                buttonLabels[i] = CreateText("Label", rect, 17, TextAnchor.MiddleCenter, FontStyle.Bold,
                    new Color32(47, 63, 66, 255));
                SetRect(buttonLabels[i].rectTransform, .05f, .05f, .95f, .95f);
            }

            root.gameObject.SetActive(false);
        }

        public void Render(CareEventEncounter encounter)
        {
            if (root == null) return;
            if (encounter == null || encounter.IsResolved)
            {
                root.gameObject.SetActive(false);
                renderedEventId = null;
                return;
            }

            if (renderedEventId == encounter.EventId && root.gameObject.activeSelf) return;
            renderedEventId = encounter.EventId;
            title.text = encounter.Title;
            description.text = encounter.Description;
            for (var i = 0; i < buttons.Length; i++)
            {
                var choice = encounter.Choices[i];
                buttonLabels[i].text = $"{choice.Label}\n<size=13>{choice.Hint} · 신뢰 상승</size>";
            }
            root.SetAsLastSibling();
            root.gameObject.SetActive(true);
        }

        private static RectTransform CreateImage(string objectName, Transform parent, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return rect;
        }

        private Text CreateText(string objectName, Transform parent, int size, TextAnchor alignment,
            FontStyle style, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            var text = go.GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = theme != null ? theme.BodyFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.fontStyle = style;
            text.color = color;
            text.supportRichText = true;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
