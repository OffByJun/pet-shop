using System;
using UnityEngine;
using UnityEngine.UI;

namespace PetShop.Care
{
    /// <summary>Passive uGUI care view. All referenced objects are serialized in the scene.</summary>
    public sealed class CareUIComponent : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text remainingText;
        [SerializeField] private Text messageText;
        [SerializeField] private Text byproductText;
        [SerializeField] private RectTransform stage;
        [SerializeField] private Button[] toolButtons;
        [SerializeField] private Image[] toolBackgrounds;
        [SerializeField] private Button[] conditionButtons;
        [SerializeField] private Text[] conditionNames;
        [SerializeField] private Text[] conditionCareLabels;
        [SerializeField] private Slider[] conditionProgress;
        [SerializeField] private RectTransform[] conditionMarks;
        [SerializeField] private Image[] conditionMarkImages;
        [SerializeField] private Text[] conditionMarkLabels;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private Text completionByproductText;

        public event Action<int> ToolRequested;
        public event Action<int> ConditionRequested;
        public event Action ResetRequested;

        private void Awake()
        {
            for (var i = 0; i < toolButtons.Length; i++)
            {
                var captured = i;
                toolButtons[i].onClick.AddListener(() => ToolRequested?.Invoke(captured));
            }
            for (var i = 0; i < conditionButtons.Length; i++)
            {
                var captured = i;
                conditionButtons[i].onClick.AddListener(() => ConditionRequested?.Invoke(captured));
            }
            resetButton.onClick.AddListener(() => ResetRequested?.Invoke());
        }

        public void Render(CareViewModel model)
        {
            titleText.text = CareHandoffContext.HasActiveVisit
                ? $"{CareHandoffContext.PetName}의 케어룸"
                : "포근포근 케어룸";
            remainingText.text = model.Completed
                ? "모든 케어 완료"
                : $"남은 상태 {model.RemainingCount} / {model.Conditions.Count}";
            messageText.text = model.Message;
            byproductText.text = model.Byproducts.Count == 0 ? "아직 없음" : string.Join(" · ", model.Byproducts);

            for (var i = 0; i < toolButtons.Length; i++)
                toolBackgrounds[i].color = i == (int)model.SelectedTool
                    ? new Color32(210, 238, 228, 255)
                    : new Color32(247, 244, 235, 255);

            for (var i = 0; i < conditionButtons.Length; i++)
            {
                var visible = i < model.Conditions.Count;
                conditionButtons[i].gameObject.SetActive(visible);
                conditionMarks[i].gameObject.SetActive(visible && !model.Conditions[i].Resolved);
                if (!visible) continue;

                var condition = model.Conditions[i];
                conditionNames[i].text = condition.Name;
                conditionCareLabels[i].text = condition.Resolved ? "해결" : CarePresentation.CareLabel(condition.Care);
                conditionProgress[i].value = 1f - condition.Remaining;
                PositionMark(conditionMarks[i], condition.Zone);
                conditionMarkImages[i].color = MarkColor(condition.Care, model.SelectedCondition == i);
                conditionMarkLabels[i].text = MarkGlyph(condition.Care);
            }

            completionPanel.SetActive(model.Completed);
            completionByproductText.text = model.Byproducts.Count == 0
                ? "획득한 부산물이 없습니다."
                : "획득 부산물  " + string.Join(" · ", model.Byproducts);
        }

        private void PositionMark(RectTransform mark, Rect zone)
        {
            var size = stage.rect.size;
            mark.anchorMin = Vector2.zero;
            mark.anchorMax = Vector2.zero;
            mark.pivot = Vector2.zero;
            mark.anchoredPosition = new Vector2(zone.x * size.x, (1f - zone.y - zone.height) * size.y);
            mark.sizeDelta = new Vector2(zone.width * size.x, zone.height * size.y);
        }

        private static Color MarkColor(CareKind care, bool selected)
        {
            var color = care switch
            {
                CareKind.Wash => new Color32(74, 158, 210, 220),
                CareKind.Brush => new Color32(242, 190, 83, 220),
                CareKind.Treat => new Color32(238, 126, 104, 220),
                CareKind.Remove => new Color32(131, 117, 181, 220),
                CareKind.Trim => new Color32(105, 192, 166, 220),
                _ => new Color32(80, 90, 100, 220)
            };
            if (selected) color.a = 255;
            return color;
        }

        private static string MarkGlyph(CareKind care) => care switch
        {
            CareKind.Wash => "泥",
            CareKind.Brush => "털",
            CareKind.Treat => "+",
            CareKind.Remove => "◆",
            CareKind.Trim => "긴",
            _ => "?"
        };

        public void Configure(
            Text title, Text remaining, Text message, Text byproducts, RectTransform stageRoot,
            Button[] tools, Image[] toolImages, Button[] conditions, Text[] names, Text[] careLabels,
            Slider[] progress, RectTransform[] marks, Image[] markImages, Text[] markLabels,
            Button reset, GameObject completion, Text completionByproducts)
        {
            titleText = title;
            remainingText = remaining;
            messageText = message;
            byproductText = byproducts;
            stage = stageRoot;
            toolButtons = tools;
            toolBackgrounds = toolImages;
            conditionButtons = conditions;
            conditionNames = names;
            conditionCareLabels = careLabels;
            conditionProgress = progress;
            conditionMarks = marks;
            conditionMarkImages = markImages;
            conditionMarkLabels = markLabels;
            resetButton = reset;
            completionPanel = completion;
            completionByproductText = completionByproducts;
        }
    }
}
