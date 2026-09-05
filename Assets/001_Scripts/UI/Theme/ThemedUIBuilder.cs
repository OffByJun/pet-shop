using UnityEngine;
using UnityEngine.UI;

namespace _001_Scripts.UI.Theme
{
    /// <summary>런타임에 테마를 입힌 uGUI를 조립하는 도우미입니다.</summary>
    public static class ThemedUIBuilder
    {
        public static Canvas Overlay(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = .5f;
            return canvas;
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static Image Fill(string name, Transform parent, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                .GetComponent<Image>();
            image.rectTransform.SetParent(parent, false);
            image.color = color;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        public static Image Surface(string name, Transform parent, UITheme theme, Sprite sprite, Color color,
            Vector2 position, Vector2 size)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                .GetComponent<Image>();
            var rect = image.rectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.color = color;
            return image;
        }

        public static Text Label(string name, Transform parent, UITheme theme, string value, int size, Color color,
            TextAnchor anchor, Vector2 position, Vector2 rectSize, bool display = false)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text))
                .GetComponent<Text>();
            var rect = text.rectTransform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = rectSize;
            text.font = display ? theme.DisplayFont : theme.BodyFont;
            text.fontStyle = display ? FontStyle.Bold : FontStyle.Normal;
            text.fontSize = size;
            text.color = color;
            text.alignment = anchor;
            text.lineSpacing = 1.28f;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = value;
            return text;
        }

        /// <summary>주요/보조 두 단계 버튼입니다. 주요는 발바닥 아이콘을 답니다.</summary>
        public static Button Capsule(string name, Transform parent, UITheme theme, string label, bool primary,
            Vector2 position, Vector2 size)
        {
            var image = Surface(name, parent, theme,
                primary ? theme.CapsulePrimary : theme.CapsuleSoft, Color.white, position, size);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, .98f, .93f, 1f);
            colors.pressedColor = new Color(.90f, .85f, .76f, 1f);
            colors.disabledColor = new Color(1f, 1f, 1f, .45f);
            colors.fadeDuration = .08f;
            button.colors = colors;

            Label("Label", image.transform, theme, label,
                primary ? 22 : 17, primary ? theme.Ink : theme.InkSoft,
                TextAnchor.MiddleCenter, primary ? new Vector2(12f, 0f) : Vector2.zero,
                size - new Vector2(primary ? 70f : 24f, 6f), primary);

            if (!primary || theme.IconPaw == null) return button;
            var icon = new GameObject("Paw", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                .GetComponent<Image>();
            icon.rectTransform.SetParent(image.transform, false);
            icon.sprite = theme.IconPaw;
            icon.color = theme.Oak;
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, .5f);
            iconRect.pivot = new Vector2(0f, .5f);
            iconRect.sizeDelta = new Vector2(24f, 24f);
            iconRect.anchoredPosition = new Vector2(30f, 0f);
            return button;
        }

        /// <summary>정산 화면의 숫자 타일입니다.</summary>
        public static Text StatTile(string name, Transform parent, UITheme theme, string caption, Color tint,
            Vector2 position, Vector2 size)
        {
            var card = Surface(name, parent, theme, theme.Card, tint, position, size);
            Label("Caption", card.transform, theme, caption, 12, theme.InkFaint,
                TextAnchor.MiddleLeft, new Vector2(4f, size.y * .5f - 22f), new Vector2(size.x - 36f, 20f));
            return Label("Value", card.transform, theme, "-", 26, theme.Ink,
                TextAnchor.MiddleLeft, new Vector2(4f, -size.y * .5f + 26f), new Vector2(size.x - 36f, 32f), true);
        }
    }
}
